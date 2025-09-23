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
        if (this.Live)
            this.Connection.Transaction.Commit();
        else
            ;
        this.Live = false;
    }

    public void Rollback()
    {
        try
        {
            if (this.Live)
                this.Connection.Transaction.Rollback();
            else
                ;
        }
        finally
        {
            this.Live = false;
        }
    }
}
