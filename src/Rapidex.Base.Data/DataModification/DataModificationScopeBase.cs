using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.DataModification;
internal abstract class DataModificationScopeBase : IDbDataModificationScope
{
    public IDbSchemaScope ParentScope { get; protected set; }
    public IDbDataModificationPovider DmProvider { get; protected set; }

    public DataModificationScopeBase(IDbSchemaScope parentScope, IDbDataModificationPovider dmProvider)
    {
        this.ParentScope = parentScope;
        this.DmProvider = dmProvider;

        this.Initialize();
    }

    protected virtual void Initialize()
    {

    }

    protected IDbEntityLoader SelectLoader(IDbEntityMetadata em)
    {
        //TODO: Select loader 
        //DbEntityInMemoryCacheLoader cacheLoader = new DbEntityInMemoryCacheLoader();
        //cacheLoader.Setup(this.DmProvider);

        return this.DmProvider;
    }


    public IQuery GetQuery(IDbEntityMetadata em)
    {
        em.NotNull("Metadata can't be null");
        return new Rapidex.Data.Query.DbQuery(this.ParentScope, em);
    }

    public IQuery<T> GetQuery<T>() where T : IConcreteEntity
    {
        return new Rapidex.Data.Query.Query<T>(this.ParentScope);
    }

    public IEntityLoadResult Load(IQueryLoader queryLoader)
    {
        var em = queryLoader.EntityMetadata;

        //TODO: count
        //TODO: SelectLoader'a query i verelim, critere bakarak;
        //1- Count ya da 2- Id list kullansın
        //3- count ya da id sayısı X'in üzerinde ise doğrudan yükle
        // - değil ise id listesi ile yükle (Cache)

        IDbEntityLoader entityLoader = this.SelectLoader(em);

        IEntityLoadResult loadedResult = entityLoader.Load(queryLoader);

        if (queryLoader.Paging.IsPagingSet())
        {
            loadedResult.StartIndex = queryLoader.Paging.StartIndex;
            loadedResult.PageSize = queryLoader.Paging.PageSize;
            loadedResult.PageIndex = loadedResult.StartIndex / queryLoader.Paging.PageSize;

            if (queryLoader.Paging.IsPagingSet() && queryLoader.Paging.IncludeTotalItemCount)
            {
                IQueryAggregate totalCounter = (IQueryAggregate)queryLoader.Clone();
                totalCounter.Alias = queryLoader.Alias;
                totalCounter.ClearPaging();
                //totalCounter.Page(int.MaxValue, 0);
                loadedResult.TotalItemCount = totalCounter.Count();
                loadedResult.IncludeTotalItemCount = true;
                loadedResult.PageCount = (long)Math.Ceiling(Convert.ToDecimal(loadedResult.TotalItemCount) / Convert.ToDecimal(loadedResult.PageSize.Value));

            }
        }

        return loadedResult;

    }

    public ILoadResult<DataRow> LoadRaw(IQueryLoader queryLoader)
    {
        return this.DmProvider.LoadRaw(queryLoader);
    }


    public IEntity Find(IDbEntityMetadata em, long id)
    {
        if (em.OnlyBaseSchema && this.ParentScope.SchemaName != DatabaseConstants.DEFAULT_SCHEMA_NAME)
            return this.ParentScope.ParentDbScope.Find(em, id);

        DbEntityId eid = new DbEntityId(id, -1);
        IDbEntityLoader loader = this.SelectLoader(em);
        IEntityLoadResult result = loader.Load(em, new DbEntityId[] { eid });
        return result.FirstOrDefault();
    }

    public virtual IEntity New(IDbEntityMetadata em)
    {


        em.NotNull();

        if (em.OnlyBaseSchema && this.ParentScope.SchemaName != DatabaseConstants.DEFAULT_SCHEMA_NAME) //?? acaba?
            throw new InvalidOperationException($"Entity '{em.Name}' only create for base schema");

        IEntity entity = Database.EntityFactory.Create(em, this.ParentScope, true);

        entity = entity.PublishOnNew().Result ?? entity;

        return entity;
    }


    protected abstract IDbChangesCollection GetChangesCollection();

    //olduğunda transaction ayrı bir thread'da kalıyor. Scope şeklinde 
    public virtual void Save(IEntity entity)
    {
        //TODO: Validate 

        if (entity is not IPartialEntity)
            entity.EnsureDataTypeInitialization();

        IEntity retEntity = entity.PublishOnBeforeSave()
            .Result;

        if (retEntity != null)
            entity = retEntity;

        this.GetChangesCollection().Add(entity);
    }

    public virtual void Save(IEnumerable<IEntity> entities)
    {
        //TODO: Validate 

        List<IEntity> _entities = new List<IEntity>(entities);

        IDbChangesCollection cScope = this.GetChangesCollection();
        foreach (var entity in _entities)
        {
            IEntity _entity = entity;
            if (_entity is not IPartialEntity)
                _entity.EnsureDataTypeInitialization();

            IEntity retEntity = _entity.PublishOnBeforeSave()
                .Result;

            if (retEntity != null)
                _entity = retEntity;

            cScope.Add(_entity);
        }
    }

    public void Add(IQueryUpdater updater)
    {
        this.GetChangesCollection().Add(updater);
    }

    protected IDbEntityUpdater[] SelectUpdaters(IDbEntityMetadata em, IEnumerable<IEntity> entities)
    {
        //TODO: Locate to caches, birinci cache
        //Ancak transaction tamamlanmaz ise?

        return new IDbEntityUpdater[] { this.DmProvider };
    }

    protected IDbEntityUpdater[] SelectUpdaters(IDbEntityMetadata em, IEnumerable<IQueryUpdater> updaters)
    {
        //TODO: Locate to caches, birinci cache
        //Ancak transaction tamamlanmaz ise?

        return new IDbEntityUpdater[] { this.DmProvider };
    }

    //protected bool IsBulkOperationRequired(IDbChangesScope scope)
    //{
    //    return scope.ChangedEntities.Count() > this.BulkOperationThreshold;
    //}

    protected IEntityUpdateResult InsertOrUpdate(IDbChangesCollection scope)
    {
        EntityUpdateResult result = new EntityUpdateResult();

        //bool bulkOperationRequired = this.IsBulkOperationRequired(scope);

        IDbEntityMetadata em = scope.ChangedEntities.FirstOrDefault()?.GetMetadata() ?? scope.DeletedEntities.FirstOrDefault()?.GetMetadata();

        if (scope.ChangedEntities.Any())
        {
            IDbEntityUpdater[] savers = this.SelectUpdaters(em, scope.ChangedEntities);

            //Cache nasıl güncellenecek?

            foreach (var saver in savers)
            {
                result.MergeWith(saver.InsertOrUpdate(em, scope.ChangedEntities));
            }
        }

        if (scope.DeletedEntities.Any())
        {
            IDbEntityUpdater[] savers = this.SelectUpdaters(em, scope.DeletedEntities);

            foreach (var saver in savers)
            {
                result.MergeWith(saver.Delete(em, scope.DeletedEntities.Select(ent => (long)ent.GetId())));
            }
        }

        if (scope.BulkUpdates.Any())
        {

            IDbEntityUpdater[] savers = this.SelectUpdaters(em, scope.BulkUpdates);
            foreach (var saver in savers)
            {
                foreach (IQueryUpdater updater in scope.BulkUpdates)
                {
                    result.MergeWith(saver.BulkUpdate(em, updater));
                }
            }
        }

        return result;

    }



    protected virtual void ApplyFinalized()
    {

    }

    protected virtual IEntityUpdateResult CommitOrApplyChangesInternal()
    {
        EntityUpdateResult result = new EntityUpdateResult();

        IDbChangesCollection scope = this.GetChangesCollection();
        scope.CheckNewEntities();

        var types = scope.SplitForTypesAndDependencies();

        foreach (var _scope in types)
        {
            result.MergeWith(this.InsertOrUpdate(_scope));
        }

        result.Success = true;

        return result;

    }




    public void Delete(IEntity entity)
    {
        this.GetChangesCollection().Delete(entity);
    }
}
