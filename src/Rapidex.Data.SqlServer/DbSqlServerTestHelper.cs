using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.SqlServer;
internal class DbSqlServerTestHelper : IDataUnitTestHelper
{
    public void DropEverythingInDatabase(string connectionString)
    {
        SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
        using DbSqlServerConnection connection = new DbSqlServerConnection(sqlConnectionStringBuilder.ConnectionString);
        
        connection.Execute($"USE [master]");
        connection.Execute($"IF EXISTS(select * FROM master..sysdatabases where [name] ='{sqlConnectionStringBuilder.InitialCatalog}') ALTER DATABASE [{sqlConnectionStringBuilder.InitialCatalog}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE ");
        connection.Execute($"IF EXISTS(select * FROM master..sysdatabases where [name] ='{sqlConnectionStringBuilder.InitialCatalog}') DROP DATABASE [{sqlConnectionStringBuilder.InitialCatalog}]");
        connection.Execute($"IF NOT EXISTS(select * FROM master..sysdatabases where [name] ='{sqlConnectionStringBuilder.InitialCatalog}') CREATE DATABASE [{sqlConnectionStringBuilder.InitialCatalog}]");
        //ALTER DATABASE [databasename] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    }

    public void DropAllTablesInDatabase(string connectionString)
    {
        SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
        using DbSqlServerConnection connection = new DbSqlServerConnection(sqlConnectionStringBuilder.ConnectionString);

        string sql1 = $"IF EXISTS(select * FROM master..sysdatabases where [name] ='{sqlConnectionStringBuilder.InitialCatalog}') EXEC sp_msforeachtable \"ALTER TABLE ? NOCHECK CONSTRAINT all\"";
        connection.Execute(sql1);

        string sql2 = $"IF EXISTS(select * FROM master..sysdatabases where [name] ='{sqlConnectionStringBuilder.InitialCatalog}') EXEC sp_msforeachtable \"DROP TABLE ?\"";
        connection.Execute(sql2);
    }
}
