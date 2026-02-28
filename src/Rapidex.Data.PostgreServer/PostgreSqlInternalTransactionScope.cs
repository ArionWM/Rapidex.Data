using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.PostgreServer;
internal class PostgreSqlInternalTransactionScope : IDbInternalTransactionScope
{
    protected PostgreSqlServerConnection Connection { get; }
    public bool Live { get; protected set; } = true;

    public PostgreSqlInternalTransactionScope(PostgreSqlServerConnection connection)
    {
        this.Connection = connection;
        this.Connection.BeginTransaction();
    }

    public async Task Commit()
    {
        await this.Connection.CommitTransaction();
        this.Live = false;
    }

    public async Task Rollback()
    {
        try
        {
           await this.Connection.RollbackTransaction();
        }
        finally
        {
            this.Live = false;
        }
    }
}

