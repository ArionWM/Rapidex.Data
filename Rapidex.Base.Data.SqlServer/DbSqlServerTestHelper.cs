using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.SqlServer;
internal class DbSqlServerTestHelper : IDataUnitTestHelper
{
    public void DropAllTablesInDatabase(string connectionString)
    {
        SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
        DbSqlServerConnection connection = new DbSqlServerConnection(sqlConnectionStringBuilder.ConnectionString);

        string sql1 = $"IF EXISTS(select * FROM master..sysdatabases where [name] ='{sqlConnectionStringBuilder.InitialCatalog}') EXEC sp_msforeachtable \"ALTER TABLE ? NOCHECK CONSTRAINT all\"";
        connection.Execute(sql1);

        string sql2 = $"IF EXISTS(select * FROM master..sysdatabases where [name] ='{sqlConnectionStringBuilder.InitialCatalog}') EXEC sp_msforeachtable \"DROP TABLE ?\"";
        connection.Execute(sql2);
    }
}
