using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.SqlServer;

internal class DbSqlInternalTransactionScope : IDbInternalTransactionScope
{
    protected DbSqlServerConnection Connection { get; }
    public bool Live { get; protected set; } = true;

    public DbSqlInternalTransactionScope(DbSqlServerConnection connection)
    {
        this.Connection = connection;
        this.Connection.BeginTransaction();
    }

    public async Task Commit()
    {
        if (this.Live)
            await this.Connection.Transaction.CommitAsync();
        else
            ;
        this.Live = false;
    }

    public async Task Rollback()
    {
        try
        {
            if (this.Live)
                await this.Connection.Transaction.RollbackAsync();
            else
                ;
        }
        finally
        {
            this.Live = false;
        }
    }
}
