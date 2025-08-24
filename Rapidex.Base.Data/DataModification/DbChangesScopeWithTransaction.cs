using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.DataModification;
internal class DbChangesScopeWithTransaction : DbDChangesScope, IDbChangesScopeWithTransaction, IDisposable
{
    protected DbDataModificationManager Manager { get; }

    public IDbInternalTransactionScope InternalTransactionScope { get; }

    public DbChangesScopeWithTransaction(DbDataModificationManager manager, IDbInternalTransactionScope internalTransactionScope)
    {
        this.Manager = manager;
        this.InternalTransactionScope = internalTransactionScope;
    }

    public async Task Commit()
    {
        this.Manager.Enter(this);
        await this.Manager.CommitOrApplyChanges();
        this.Manager.Leave(this);
    }

    public async Task Rollback()
    {
        await this.Manager.Rollback();
    }

    public void Dispose()
    {
        this.Commit().Wait();
    }

}
