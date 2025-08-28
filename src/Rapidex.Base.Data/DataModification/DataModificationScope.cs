using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rapidex.Data.DataModification;
internal class DataModificationScope : DataModificationScopeBase, IDbDataModificationTransactionedScope
{
    protected bool IsFinalized { get; set; } = false;
    public IDbInternalTransactionScope CurrentTransaction { get; protected set; }
    protected virtual IDbChangesCollection ChangesCollection { get; set; }

    public DataModificationScope(IDbSchemaScope parentScope, IDbDataModificationPovider dmProvider) : base(parentScope, dmProvider)
    {
    }

    protected override void Initialize()
    {
        base.Initialize();

        this.ChangesCollection = new DbChangesCollection();
        this.CurrentTransaction = this.DmProvider.BeginTransaction();
    }

    protected override IDbChangesCollection GetChangesCollection()
    {
        return this.ChangesCollection;

    }

    protected override void ApplyFinalized()
    {
        base.ApplyFinalized();
        this.IsFinalized = true;
    }

    public override IEntity New(IDbEntityMetadata em)
    {
        if (this.IsFinalized)
            throw new InvalidOperationException("This scope is finalized");

        return base.New(em);
    }

    public override void Save(IEntity entity)
    {
        if (this.IsFinalized)
            throw new InvalidOperationException("This scope is finalized");

        base.Save(entity);
    }

    public override void Save(IEnumerable<IEntity> entities)
    {
        if (this.IsFinalized)
            throw new InvalidOperationException("This scope is finalized");

        base.Save(entities);
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
            var result = base.CommitOrApplyChangesInternal();
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

    public virtual void Dispose()
    {
        this.ChangesCollection?.Clear();

        if (this.IsFinalized)
            return;

        this.CommitChanges();
    }
}
