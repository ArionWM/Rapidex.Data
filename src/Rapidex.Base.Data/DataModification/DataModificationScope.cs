using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rapidex.Data.DataModification;
internal class DataModificationScope : DataModificationScopeBase, IDbDataModificationScope
{
    protected bool IsFinalized { get; set; } = false;
    protected IDbDataModificationStaticHost parentHost;
    public IDbInternalTransactionScope CurrentTransaction { get; protected set; }
    protected virtual IDbChangesCollection ChangesCollection { get; set; }

    public DataModificationScope(IDbDataModificationStaticHost parentHost) : base(parentHost.ParentScope)
    {
        this.parentHost = parentHost;
    }

    protected override void Initialize()
    {
        base.Initialize();

        this.ChangesCollection = new DbChangesCollection();
        //this.DmProvider = this.ParentScope.DbProvider.GetDataModificationProvider();
        this.CurrentTransaction = this.DmProvider.BeginTransaction();
    }

    protected IDbChangesCollection GetChangesCollection()
    {
        return this.ChangesCollection;

    }

    protected virtual void ApplyFinalized()
    {
        this.IsFinalized = true;
        this.parentHost.UnRegister(this);
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

    public virtual void Add(IQueryUpdater updater)
    {
        this.GetChangesCollection().Add(updater);
    }



    public virtual IEntity New(IDbEntityMetadata em)
    {
        if (this.IsFinalized)
            throw new InvalidOperationException("This scope is finalized");

        em.NotNull();

        if (em.OnlyBaseSchema && this.ParentScope.SchemaName != DatabaseConstants.DEFAULT_SCHEMA_NAME) //?? acaba?
            throw new InvalidOperationException($"Entity '{em.Name}' only create for base schema");

        IEntity entity = Database.EntityFactory.Create(em, this.ParentScope, true);

        entity = entity.PublishOnNew().Result ?? entity;

        return entity;
    }

    public virtual void Save(IEntity entity)
    {
        if (this.IsFinalized)
            throw new InvalidOperationException("This scope is finalized");

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
        if (this.IsFinalized)
            throw new InvalidOperationException("This scope is finalized");

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



    public void Rollback()
    {
        IDbInternalTransactionScope _its = this.CurrentTransaction;
        _its.NotNull("No transaction available");

        _its.Rollback();

        this.ApplyFinalized();
    }

    public IEntityUpdateResult CommitChanges()
    {
        if (this.IsFinalized)
            throw new InvalidOperationException("This scope is finalized");

        IDbInternalTransactionScope _its = this.CurrentTransaction;
        try
        {
            var result = this.CommitOrApplyChangesInternal();
            _its?.Commit();
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
            this.ApplyFinalized();
        }
    }

    public void Dispose()
    {
        this.ChangesCollection?.Clear();

        if (this.IsFinalized)
            return;

        this.CommitChanges();
    }
}
