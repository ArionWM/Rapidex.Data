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

    public void Commit()
    {
        this.Connection.Transaction.Commit();
        this.Live = false;
    }

    public void Rollback()
    {
        try
        {
            this.Connection.Transaction.Rollback();
        }
        finally
        {
            this.Live = false;
        }
    }
}
