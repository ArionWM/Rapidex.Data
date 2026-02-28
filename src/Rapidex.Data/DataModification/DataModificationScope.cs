using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rapidex.Data.Exceptions;

namespace Rapidex.Data.DataModification;



internal class DataModificationScope : DataModificationReadScopeBase, IDbDataModificationScope
{
    #region wrapper
    internal class DataModificationScopeInternalWrapper : IDbDataModificationScope
    {
        private readonly DataModificationScope scope;
        public bool IsFinalized => this.scope.IsFinalized;
        public bool IsFinalizing => this.scope.IsFinalizing;
        public IDbDataModificationStaticHost Parent => this.scope.Parent;
        public IDbInternalTransactionScope CurrentTransaction => this.scope.CurrentTransaction;

        public IDbSchemaScope ParentSchema => this.Parent.ParentSchema;

        public DataModificationScopeInternalWrapper(DataModificationScope scope)
        {
            this.scope = scope;
        }
        public void Add(IQueryUpdater updater)
        {
            this.scope.Add(updater);
        }
        public void Attach(IEntity entity, bool checkIntegrity = true)
        {
            this.scope.Attach(entity, checkIntegrity);
        }
        public void DeAttach(IEntity entity)
        {
            this.scope.DeAttach(entity);
        }
        public IEntity New(IDbEntityMetadata em)
        {
            return this.scope.NewInternal(em);
        }
        public void Save(IEntity entity)
        {
            this.scope.SaveInternal(entity);
        }
        public void Save(IEnumerable<IEntity> entities)
        {
            this.scope.SaveInternal(entities);
        }
        public void Delete(IEntity entity)
        {
            this.scope.DeleteInternal(entity);
        }
        public void Rollback()
        {
            throw new NotSupportedException();
        }
        public Task RollbackAsync()
        {
            throw new NotSupportedException();
        }
        public Task<IEntityUpdateResult> CommitChangesAsync()
        {
            throw new NotSupportedException();
        }
        public IEntityUpdateResult CommitChanges()
        {
            throw new NotSupportedException();
        }

        (bool Found, string? Desc) IDbDataModificationScope.FindAndAnalyse(IDbEntityMetadata em, long id)
        {
            return ((IDbDataModificationScope)this.scope).FindAndAnalyse(em, id);
        }

        public IQuery GetQuery(IDbEntityMetadata em)
        {
            return this.scope.GetQuery(em);
        }

        public IQuery<T> GetQuery<T>() where T : IConcreteEntity
        {
            return this.scope.GetQuery<T>();
        }

        public IEntity Find(IDbEntityMetadata em, long id)
        {
            return this.scope.Find(em, id);

        }

        public IEntity[] Find(IDbEntityMetadata em, params long[] ids)
        {
            return this.scope.Find(em, ids);
        }

        public IEntityLoadResult Load(IQueryLoader loader)
        {
            return this.scope.Load(loader);
        }

        public ILoadResult<DataRow> LoadRaw(IQueryLoader loader)
        {
            return this.scope.LoadRaw(loader);
        }

        public void Dispose()
        {

        }
    }
    #endregion

    private readonly int debugTracker;
    public bool IsFinalized { get; protected set; } = false;
    public bool IsFinalizing { get; protected set; } = false;
    public IDbDataModificationStaticHost Parent { get; }
    public IDbInternalTransactionScope CurrentTransaction { get; protected set; }
    protected virtual IDbChangesCollection ChangesCollection { get; set; }

    public DataModificationScope(IDbDataModificationStaticHost parentHost) : base(parentHost.ParentSchema)
    {
        this.Parent = parentHost;
        this.debugTracker = RandomHelper.Random(1000000);
    }

    public override string ToString()
    {
        return $"{this.debugTracker}";
    }

    protected override void Initialize()
    {
        base.Initialize();

        this.ChangesCollection = new DbChangesCollection(this);
        this.CurrentTransaction = this.DmProvider.BeginTransaction();
    }

    protected virtual void ApplyFinalized()
    {
        this.IsFinalized = true;
        this.IsFinalizing = false;
        this.Parent.UnRegister(this);
    }

    protected virtual void CheckIntegrity(IEntity entity)
    {
        var em = entity.GetMetadata();
        if (em.OnlyBaseSchema && this.ParentSchema.SchemaName != DatabaseConstants.DEFAULT_SCHEMA_NAME) //?? acaba?
            throw new EntityAttachScopeException("OnlyBase", $"Entity '{em.Name}' only create for base schema");

        var dict = entity.GetAllValues();
        foreach (string fieldName in dict.Keys)
        {
            var fm = em.Fields.Get(fieldName);
            if (fm == null)
                throw new EntityAttachScopeException("FieldNotFound", $"Field '{fieldName}' not found in entity '{em.Name}'");
        }
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

    protected void PublishAfterSave(IDbChangesCollection scope)
    {
        foreach (var entity in scope.ChangedEntities)
        {
            entity.PublishOnAfterSave();
        }
    }

    protected void PublishAfterDelete(IDbChangesCollection scope)
    {
        foreach (var entity in scope.DeletedEntities)
        {
            entity.PublishOnAfterDelete();
        }
    }

    protected void PublishAfterCommit(IDbChangesCollection scope)
    {
        foreach (var entity in scope.ChangedEntities)
        {
            entity.PublishOnAfterCommit();
        }
    }
    protected async Task<IEntityUpdateResult> InsertOrUpdateAsync(IDbChangesCollection scope)
    {
        EntityUpdateResult result = new EntityUpdateResult();

        //bool bulkOperationRequired = this.IsBulkOperationRequired(scope);

        IDbEntityMetadata em = scope.ChangedEntities.FirstOrDefault()?.GetMetadata() ?? scope.DeletedEntities.FirstOrDefault()?.GetMetadata();

        if (scope.ChangedEntities.Any())
        {
            IDbEntityUpdater[] savers = this.SelectUpdaters(em, scope.ChangedEntities);

            foreach (var saver in savers)
            {
                result.MergeWith(await saver.InsertOrUpdate(em, scope.ChangedEntities));
            }

            this.PublishAfterSave(scope);
        }

        if (scope.DeletedEntities.Any())
        {
            IDbEntityUpdater[] savers = this.SelectUpdaters(em, scope.DeletedEntities);

            foreach (var saver in savers)
            {
                result.MergeWith(await saver.Delete(em, scope.DeletedEntities.Select(ent => (long)ent.GetId())));
            }

            this.PublishAfterDelete(scope);
        }

        if (scope.BulkUpdates.Any())
        {

            IDbEntityUpdater[] savers = this.SelectUpdaters(em, scope.BulkUpdates);
            foreach (var saver in savers)
            {
                foreach (IQueryUpdater updater in scope.BulkUpdates)
                {
                    result.MergeWith(await saver.BulkUpdate(em, updater));
                }
            }
        }

        return result;

    }

    protected virtual async Task<IEntityUpdateResult> CommitOrApplyChangesInternal()
    {
        EntityUpdateResult result = new EntityUpdateResult();

        await this.ChangesCollection.PrepareCommit();


        var types = this.ChangesCollection.SplitForTypesAndDependencies();

        foreach (var _scope in types)
        {
            result.MergeWith(await this.InsertOrUpdateAsync(_scope));
        }

        result.Success = true;

        return result;

    }

    protected void DeleteInternal(IEntity entity)
    {
        IEntity resEntity = entity.PublishOnBeforeDelete().Result;
        if (resEntity != null)
            entity = resEntity;

        this.ChangesCollection.Delete(entity);

        Database.Cache.RemoveEntity(entity).Wait();
    }

    public void Delete(IEntity entity)
    {
        this.CheckActive();

        this.DeleteInternal(entity);
    }

    protected virtual void AddInternal(IQueryUpdater updater)
    {
        this.ChangesCollection.Add(updater);
    }

    public virtual void Add(IQueryUpdater updater)
    {
        this.CheckActive();

        this.AddInternal(updater);
    }

    protected virtual IEntity NewInternal(IDbEntityMetadata em)
    {
        em.NotNull();

        if (em.OnlyBaseSchema && this.ParentSchema.SchemaName != DatabaseConstants.DEFAULT_SCHEMA_NAME) //?? acaba?
            throw new InvalidOperationException($"Entity '{em.Name}' only create for base schema");

        IEntity entity = Database.EntityFactory.Create(em, this.ParentSchema, true);

        entity = entity.PublishOnNew().Result ?? entity;

        return entity;
    }


    public virtual IEntity New(IDbEntityMetadata em)
    {
        this.CheckActive();

        return this.NewInternal(em);
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

                    if (id < 0)
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
        }

        return ex;
    }

    protected virtual void SaveInternal(IEntity entity)
    {

        try
        {
            if (!entity.IsAttached() || entity._Schema != this.ParentSchema)
            {
                this.Attach(entity);
            }

            entity.EnsureDataTypeInitialization();

            IEntity retEntity = entity.PublishOnBeforeSave()
                .Result;

            if (retEntity != null) entity = retEntity;

            IValidationResult entityValidationResult = entity.PublishForValidate().Result;
            if (!entityValidationResult.Success)
                throw new DataValidationException(entityValidationResult);

            this.ChangesCollection.Add(entity);
        }
        catch (EntityNotFoundException enf)
        {
            Exception rex = this.AnalyseException(enf);
            throw rex;
        }
    }

    public virtual void Save(IEntity entity)
    {
        this.CheckActive();

        this.SaveInternal(entity);
    }

    protected virtual void SaveInternal(IEnumerable<IEntity> entities)
    {
        //TODO: Validate 

        try
        {
            List<IEntity> _entities = new List<IEntity>(entities);

            foreach (var entity in _entities)
            {
                IEntity _entity = entity;

                if (!entity.IsAttached() || entity._Schema != this.ParentSchema)
                {
                    this.Attach(entity);
                }
                else
                {
                    _entity.EnsureDataTypeInitialization();
                }

                IEntity retEntity = _entity.PublishOnBeforeSave()
                    .Result;

                if (retEntity != null)
                    _entity = retEntity;

                IValidationResult entityValidationResult = _entity.PublishForValidate().Result;
                if (!entityValidationResult.Success)
                    throw new DataValidationException(entityValidationResult);

                this.ChangesCollection.Add(_entity);
            }

        }
        catch (EntityNotFoundException enf)
        {
            Exception rex = this.AnalyseException(enf);
            throw;
        }
    }

    public virtual void Save(IEnumerable<IEntity> entities)
    {
        this.CheckActive();

        this.SaveInternal(entities);
    }

    public void Rollback()
    {
        this.RollbackAsync().Wait();
    }

    public async Task RollbackAsync()
    {
        this.IsFinalizing = true;
        IDbInternalTransactionScope _its = this.CurrentTransaction;
        _its.NotNull("No transaction available");

        await _its.Rollback();

        this.ApplyFinalized();
    }

    public async Task<IEntityUpdateResult> CommitChangesAsync()
    {
        this.CheckActive();

        this.IsFinalizing = true;

        IDbInternalTransactionScope _its = this.CurrentTransaction;
        try
        {
            var result = await this.CommitOrApplyChangesInternal();
            _its?.Commit();

            this.PublishAfterCommit(this.ChangesCollection);

            return result;
        }
        catch (Exception ex)
        {
            var tex = Common.ExceptionManager.Translate(ex);
            tex.Log();

            _its?.Rollback();

            throw;
        }
        finally
        {
            this.ApplyFinalized();
        }
    }

    public IEntityUpdateResult CommitChanges()
    {
        return this.CommitChangesAsync().GetAwaiter().GetResult();
    }

    public override void Dispose()
    {
        if (this.IsFinalizing)
            throw new InvalidOperationException("Scope already finalizing, can't dispose yet");

        if (!this.IsFinalized)
            this.CommitChanges();

        this.ChangesCollection?.Clear();
        this.ChangesCollection = null;

        this.CurrentTransaction = null;

        base.Dispose();
    }

    (bool Found, string? Desc) IDbDataModificationScope.FindAndAnalyse(IDbEntityMetadata em, long id)
    {
        return this.ChangesCollection.FindAndAnalyse(em, id);
    }

    public void Attach(IEntity entity, bool checkIntegrity = true)
    {

        entity._Schema = this.ParentSchema;
        entity._SchemaName = this.ParentSchema.SchemaName;

        if (checkIntegrity)
            this.CheckIntegrity(entity);

        entity.EnsureDataTypeInitialization();
    }

    public void DeAttach(IEntity entity)
    {

    }
}
