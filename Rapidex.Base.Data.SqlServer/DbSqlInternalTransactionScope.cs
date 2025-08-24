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
        await this.Connection.Transaction.CommitAsync();
        this.Live = false;
    }

    public async Task Rollback()
    {
        try
        {
            await this.Connection.Transaction.RollbackAsync();
        }
        finally
        {
            this.Live = false;
        }
    }
}
