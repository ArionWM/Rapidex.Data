using MoreLinq.Extensions;
using Rapidex.Data.DataModification;
using Rapidex.Data.DataModification.Loaders;

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Rapidex.Data;

internal class DbDataModificationManager : IDbDataModificationManager
{
    protected ThreadLocal<IDbChangesScope> dbChangesScope;
    protected ThreadLocal<IDbTransactionScope> currentTransaction;

    //public int BulkOperationThreshold { get; set; } = int.MaxValue;

    public IDbSchemaScope ParentScope { get; protected set; }

    public IDbTransactionScope CurrentTransaction { get { return currentTransaction.Value; } } //protected set { currentTransaction.Value = value; } 

    public IDbDataModificationPovider DmProvider { get; protected set; }

    public DbDataModificationManager(IDbSchemaScope parentScope, IDbDataModificationPovider dmProvider)
    {
        ParentScope = parentScope;
        DmProvider = dmProvider;
        dbChangesScope = new ThreadLocal<IDbChangesScope>();
        currentTransaction = new ThreadLocal<IDbTransactionScope>();
    }

    protected IDbEntityLoader SelectLoader(IDbEntityMetadata em)
    {

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

    public async Task<IEntityLoadResult> Load(IQueryLoader loader)
    {
        IQueryLoader idsLoader = (IQueryLoader)loader.Clone();
        idsLoader.Alias = loader.Alias;
        var em = loader.EntityMetadata;

        //TODO: count
        //TODO: SelectLoader'a query i verelim, critere bakarak;
        //1- Count ya da 2- Id list kullansın
        //3- count ya da id sayısı X'in üzerinde ise doğrudan yükle
        // - değil ise id listesi ile yükle (Cache)

        IDbEntityLoader multiEntLoader = this.SelectLoader(em);

        IEntityLoadResult loadedResult = null;
        loadedResult = multiEntLoader.Load(loader);

        if (loader.Paging.IsPagingSet())
        {
            loadedResult.StartIndex = loader.Paging.StartIndex;
            loadedResult.PageSize = loader.Paging.PageSize;
            loadedResult.PageIndex = loadedResult.StartIndex / loader.Paging.PageSize;

            if (loader.Paging.IsPagingSet() && loader.Paging.IncludeTotalItemCount)
            {
                IQueryAggregate totalCounter = (IQueryAggregate)loader.Clone();
                totalCounter.Alias = loader.Alias;
                totalCounter.ClearPaging();
                //totalCounter.Page(int.MaxValue, 0);
                loadedResult.TotalItemCount = await totalCounter.Count();
                loadedResult.IncludeTotalItemCount = true;
                loadedResult.PageCount = (long)Math.Ceiling(Convert.ToDecimal(loadedResult.TotalItemCount) / Convert.ToDecimal(loadedResult.PageSize.Value));

            }
        }

        return loadedResult;

    }

    public async Task<ILoadResult<DataRow>> LoadRaw(IQueryLoader loader)
    {
        return this.DmProvider.LoadRaw(loader);
    }


    public async Task<IEntity> Find(IDbEntityMetadata em, long id)
    {
        if (em.OnlyBaseSchema && this.ParentScope.SchemaName != DatabaseConstants.DEFAULT_SCHEMA_NAME)
            return await this.ParentScope.ParentDbScope.Find(em, id);

        DbEntityId eid = new DbEntityId(id, -1);
        IDbEntityLoader loader = this.SelectLoader(em);
        IEntityLoadResult result = loader.Load(em, new DbEntityId[] { eid });
        return result.FirstOrDefault();
    }

    public IEntity New(IDbEntityMetadata em)
    {
        em.NotNull();

        if (em.OnlyBaseSchema && this.ParentScope.SchemaName != DatabaseConstants.DEFAULT_SCHEMA_NAME) //?? acaba?
            throw new InvalidOperationException($"Entity '{em.Name}' only create for base schema");

        IEntity entity = Database.EntityFactory.Create(em, this.ParentScope, true);

        entity = entity.PublishOnNew().Result ?? entity;

        return entity;
    }

    public IDbTransactionScope Begin(string transactionName = null)
    {
        //CurrentTransaction ???

        this.currentTransaction.Value = this.DmProvider.BeginTransaction(transactionName);
        return this.currentTransaction.Value;
    }

    protected IDbChangesScope GetChangesScope()
    {
        if (this.CurrentTransaction != null)
            this.dbChangesScope.Value = this.CurrentTransaction;

        if (this.dbChangesScope.Value == null)
            this.dbChangesScope.Value = new DbDChangesScope();

        return this.dbChangesScope.Value;

    }

    //Async olduğunda transaction ayrı bir thread'da kalıyor. Scope şeklinde 
    public void Save(IEntity entity)
    {
        //TODO: Validate 

        if (entity is not IPartialEntity)
            entity.EnsureDataTypeInitialization();

        IEntity retEntity = entity.PublishOnBeforeSave()
            .Result;

        if (retEntity != null)
            entity = retEntity;

        this.GetChangesScope().Add(entity);
    }

    public void Save(IEnumerable<IEntity> entities)
    {
        //TODO: Validate 

        List<IEntity> _entities = new List<IEntity>(entities);

        IDbChangesScope cScope = this.GetChangesScope();
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
        this.GetChangesScope().Add(updater);
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

    protected async Task<IEntityUpdateResult> InsertOrUpdate(IDbChangesScope scope)
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


    public async Task<IEntityUpdateResult> CommitOrApplyChanges()
    {
        EntityUpdateResult result = new EntityUpdateResult();
        try
        {
            IDbChangesScope scope = this.GetChangesScope();
            scope.CheckNewEntities();

            var types = scope.SplitForTypesAndDependencies();

            foreach (var _scope in types)
            {
                result.MergeWith(await this.InsertOrUpdate(_scope));
            }

            this.CurrentTransaction?.Commit();

            result.Success = true;

            return result;
        }
        catch (Exception ex)
        {
            var tex = Common.ExceptionManager.Translate(ex);
            tex.Log();

            this.CurrentTransaction?.Rollback();

            throw tex;
        }
        finally
        {
            this.dbChangesScope.Value = null;
            this.currentTransaction.Value = null;
        }
    }

    public async Task Rollback()
    {
        this.dbChangesScope.Value = null;
        this.CurrentTransaction?.Rollback();
    }

    public IIntSequence Sequence(string name)
    {
        return this.DmProvider.Sequence(name);
    }

    public void Delete(IEntity entity)
    {
        this.GetChangesScope().Delete(entity);
    }
}
