using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rapidex.Data.DataModification;
internal class DataModificationScope : DataModificationReadScopeBase, IDbDataModificationScope
{
    private readonly int _debugTracker;
    public bool IsFinalized { get; protected set; } = false;
    public IDbDataModificationStaticHost Parent { get; }
    public IDbInternalTransactionScope CurrentTransaction { get; protected set; }
    protected virtual IDbChangesCollection ChangesCollection { get; set; }

    public DataModificationScope(IDbDataModificationStaticHost parentHost) : base(parentHost.ParentSchema)
    {
        this.Parent = parentHost;
        this._debugTracker = RandomHelper.Random(1000000);
    }

    protected override void Initialize()
    {
        base.Initialize();

        this.ChangesCollection = new DbChangesCollection();
        this.CurrentTransaction = this.DmProvider.BeginTransaction();
    }

    protected virtual void ApplyFinalized()
    {
        this.IsFinalized = true;
        this.Parent.UnRegister(this);
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

        this.ChangesCollection.CheckNewEntities();

        var types = this.ChangesCollection.SplitForTypesAndDependencies();

        foreach (var _scope in types)
        {
            result.MergeWith(this.InsertOrUpdate(_scope));
        }

        result.Success = true;

        return result;

    }

    public void Delete(IEntity entity)
    {
        this.ChangesCollection.Delete(entity);
    }

    public virtual void Add(IQueryUpdater updater)
    {
        this.ChangesCollection.Add(updater);
    }


    public virtual IEntity New(IDbEntityMetadata em)
    {
        this.CheckActive();

        em.NotNull();

        if (em.OnlyBaseSchema && this.ParentSchema.SchemaName != DatabaseConstants.DEFAULT_SCHEMA_NAME) //?? acaba?
            throw new InvalidOperationException($"Entity '{em.Name}' only create for base schema");

        IEntity entity = Database.EntityFactory.Create(em, this.ParentSchema, true);

        entity = entity.PublishOnNew().Result ?? entity;

        return entity;
    }

    protected Exception AnalyseException(Exception ex)
    {
        //var tex = Common.ExceptionManager.Translate(ex);
        //tex.Log();
        //throw tex;

        switch (ex)
        {
            case EntityNotFoundException enfx:
                var em = this.ParentSchema.ParentDbScope.Metadata.Get(enfx.EntityName);
                long id = enfx.EntityId.As<long>();
                IDbDataModificationStaticHost host = this.Parent;
                var findResult = host.FindAndAnalyseInScopes(em, id);
                if (findResult.Found)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Entity not found in this scope, but found in other (or upper) scopes");

                    if(id < 0)
                    {
                        sb.AppendLine("You can commit prior scope or create / save entity in this scope");
                    }
                    
                    sb.AppendLine(ex.Message);
                    return new EntityNotFoundException(enfx.EntityName, enfx.EntityId, sb.ToString());
                }
                else
                {
                    return enfx;
                }
                break;
        }

        return ex;
    }

    public virtual void Save(IEntity entity)
    {
        this.CheckActive();

        try
        {
            if (entity is not IPartialEntity)
                entity.EnsureDataTypeInitialization();

            IEntity retEntity = entity.PublishOnBeforeSave()
                .Result;

            if (retEntity != null)
                entity = retEntity;

            this.ChangesCollection.Add(entity);
        }
        catch (EntityNotFoundException enf)
        {
            Exception rex = this.AnalyseException(enf);
            throw rex;
        }
    }

    public virtual void Save(IEnumerable<IEntity> entities)
    {
        this.CheckActive();

        //TODO: Validate 

        try
        {
            List<IEntity> _entities = new List<IEntity>(entities);

            foreach (var entity in _entities)
            {
                IEntity _entity = entity;
                if (_entity is not IPartialEntity)
                    _entity.EnsureDataTypeInitialization();

                IEntity retEntity = _entity.PublishOnBeforeSave()
                    .Result;

                if (retEntity != null)
                    _entity = retEntity;

                this.ChangesCollection.Add(_entity);
            }

        }
        catch (EntityNotFoundException enf)
        {
            Exception rex = this.AnalyseException(enf);
            throw rex;
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
        this.CheckActive();

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
        if (this.IsFinalized)
            return;

        this.CommitChanges();

        this.ChangesCollection?.Clear();
    }

    (bool Found, string? Desc) IDbDataModificationScope.FindAndAnalyse(IDbEntityMetadata em, long id)
    {
        return this.ChangesCollection.FindAndAnalyse(em, id);
    }
}
