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

[Obsolete("", true)]
internal class DbDataModificationManager : IDbDataModificationScope
{
    protected ThreadLocal<IDbChangesCollection> dbChangesScope;
    protected ThreadLocal<IDbChangesScopeWithTransaction> dbChangesScopeWithTransaction;

    public IDbSchemaScope ParentSchema { get; protected set; }

    public IDbDataModificationPovider DmProvider { get; protected set; }

    public IDbDataModificationStaticHost Parent => throw new NotImplementedException();

    public bool IsFinalized => throw new NotImplementedException();

    public DbDataModificationManager(IDbSchemaScope parentScope, IDbDataModificationPovider dmProvider)
    {
        ParentSchema = parentScope;
        DmProvider = dmProvider;
        dbChangesScope = new ThreadLocal<IDbChangesCollection>();
        dbChangesScopeWithTransaction = new ThreadLocal<IDbChangesScopeWithTransaction>();
        //currentInternalTransaction = new ThreadLocal<IDbInternalTransactionScope>();
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
        return new Rapidex.Data.Query.DbQuery(this.ParentSchema, em);
    }

    public IQuery<T> GetQuery<T>() where T : IConcreteEntity
    {
        return new Rapidex.Data.Query.Query<T>(this.ParentSchema);
    }

    public IEntityLoadResult Load(IQueryLoader loader)
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
                loadedResult.TotalItemCount =  totalCounter.Count();
                loadedResult.IncludeTotalItemCount = true;
                loadedResult.PageCount = (long)Math.Ceiling(Convert.ToDecimal(loadedResult.TotalItemCount) / Convert.ToDecimal(loadedResult.PageSize.Value));

            }
        }

        return loadedResult;

    }

    public ILoadResult<DataRow> LoadRaw(IQueryLoader loader)
    {

                return this.DmProvider.LoadRaw(loader);

    }


    public IEntity Find(IDbEntityMetadata em, long id)
    {
        if (em.OnlyBaseSchema && this.ParentSchema.SchemaName != DatabaseConstants.DEFAULT_SCHEMA_NAME)
            return  this.ParentSchema.ParentDbScope.Find(em, id);

        DbEntityId eid = new DbEntityId(id, -1);
        IDbEntityLoader loader = this.SelectLoader(em);
        IEntityLoadResult result = loader.Load(em, new DbEntityId[] { eid });
        return result.FirstOrDefault();
    }

    public IEntity New(IDbEntityMetadata em)
    {
        em.NotNull();

        if (em.OnlyBaseSchema && this.ParentSchema.SchemaName != DatabaseConstants.DEFAULT_SCHEMA_NAME) //?? acaba?
            throw new InvalidOperationException($"Entity '{em.Name}' only create for base schema");

        IEntity entity = Database.EntityFactory.Create(em, this.ParentSchema, true);

        entity = entity.PublishOnNew().Result ?? entity;

        return entity;
    }

    public IDbChangesScopeWithTransaction BeginTransaction()
    {
        throw new NotImplementedException();
        //CurrentTransaction ???

        //if (this.IsTransactionAvailable)
        //    throw new InvalidOperationException("Transaction already active");

        //if (this.dbChangesScopeWithTransaction.Value != null)
        //    throw new InvalidOperationException("Transaction already active");

        //if (this.dbChangesScope.Value != null && !this.dbChangesScope.Value.IsEmpty)
        //    throw new InvalidOperationException("Scope already has modified entities. Commit or discard changes before starting a new transaction.");

        //IDbInternalTransactionScope its = this.DmProvider.BeginTransaction();

        ////this.currentInternalTransaction.Value = this.DmProvider.BeginTransaction();
        //this.dbChangesScope.Value = this.dbChangesScopeWithTransaction.Value = new DbChangesScopeWithTransaction(this, its);
        //return this.dbChangesScopeWithTransaction.Value;
    }

    protected IDbChangesCollection GetChangesScope()
    {
        if (this.dbChangesScope.Value == null)
            this.dbChangesScope.Value = new DbChangesCollection();

        return this.dbChangesScope.Value;

    }

    //olduğunda transaction ayrı bir thread'da kalıyor. Scope şeklinde 
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

        IDbChangesCollection cScope = this.GetChangesScope();
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


    public IEntityUpdateResult CommitOrApplyChanges()
    {
        IDbInternalTransactionScope _its = this.dbChangesScopeWithTransaction.Value?.InternalTransactionScope;
        EntityUpdateResult result = new EntityUpdateResult();
        try
        {
            IDbChangesCollection scope = this.GetChangesScope();
            scope.CheckNewEntities();

            var types = scope.SplitForTypesAndDependencies();

            foreach (var _scope in types)
            {
                result.MergeWith( this.InsertOrUpdate(_scope));
            }

             _its?.Commit();
            //this.CurrentTransaction?.Commit();

            result.Success = true;

            return result;
        }
        catch (Exception ex)
        {
            var tex = Common.ExceptionManager.Translate(ex);
            tex.Log();

            _its?.Rollback();

            throw tex;
        }
        finally
        {
            this.dbChangesScope.Value = null;
            this.dbChangesScopeWithTransaction.Value = null;
        }
    }

    public void Rollback()
    {
        IDbInternalTransactionScope _its = this.dbChangesScopeWithTransaction.Value?.InternalTransactionScope;
        _its.NotNull("No transaction available");
        this.dbChangesScope.Value = null;
        this.dbChangesScopeWithTransaction.Value = null;
         _its.Rollback();
    }

    public IIntSequence Sequence(string name)
    {
        return this.DmProvider.Sequence(name);
    }

    public void Delete(IEntity entity)
    {
        this.GetChangesScope().Delete(entity);
    }

    public IEntityUpdateResult CommitChanges()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    (bool Found, string? Desc) IDbDataModificationScope.FindAndAnalyse(IDbEntityMetadata em, long id)
    {
        throw new NotImplementedException();
    }
}
