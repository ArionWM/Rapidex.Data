using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.PostgreServer;
internal class PostgreTestHelper : IDataUnitTestHelper
{
    public void DropEverythingInDatabase(string connectionString)
    {
        string schemasSql = "select sch.schema_name" +
            " from information_schema.schemata as sch" +
            " where sch.schema_name not in ('information_schema', 'pg_catalog', 'public')" +
            " and sch.schema_name not like 'pg_toast%'" +
            " and sch.schema_name not like 'pg_temp_%'";
        NpgsqlConnectionStringBuilder npgsqlConnectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        using PostgreSqlServerConnection pConnection = new PostgreSqlServerConnection(npgsqlConnectionStringBuilder.ConnectionString);
        DataTable schemas = pConnection.Execute(schemasSql).Result;
        foreach (DataRow row in schemas.Rows)
        {
            string schemaName = row["schema_name"].ToString();
            string sql1 = $"ALTER SCHEMA \"{schemaName}\" OWNER TO {npgsqlConnectionStringBuilder.Username}";
            pConnection.Execute(sql1).Wait();
            string sql2 = $"DROP SCHEMA \"{schemaName}\" CASCADE";
            pConnection.Execute(sql2).Wait();
        }
    }

    public void DropAllTablesInDatabase(string connectionString)
    {
        NpgsqlConnectionStringBuilder npgsqlConnectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);

        // Check if database exists before proceeding
        string databaseName = npgsqlConnectionStringBuilder.Database;

        // Create a temporary connection to check database existence
        using (var tempConnection = new PostgreSqlServerConnection(npgsqlConnectionStringBuilder.ConnectionString))
        {
            string checkDbSql = $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'";
            DataTable dbCheckResult = tempConnection.Execute(checkDbSql).Result;

            // If database doesn't exist, return early
            if (dbCheckResult.Rows.Count == 0)
            {
                return;
            }
        }

        string schemasSql = "select sch.schema_name" +
            " from information_schema.schemata as sch" +
            " where sch.schema_name not in ('information_schema', 'pg_catalog', 'public')" +
            " and sch.schema_name not like 'pg_toast%'" +
            " and sch.schema_name not like 'pg_temp_%'";

        using PostgreSqlServerConnection pConnection = new PostgreSqlServerConnection(npgsqlConnectionStringBuilder.ConnectionString);


        DataTable schemas = pConnection.Execute(schemasSql).Result;
        foreach (DataRow row in schemas.Rows)
        {
            string schemaName = row["schema_name"].ToString();
            string sql1 = $"ALTER SCHEMA \"{schemaName}\" OWNER TO {npgsqlConnectionStringBuilder.Username}";
            pConnection.Execute(sql1).Wait();
            string sql2 = $"DROP SCHEMA \"{schemaName}\" CASCADE";
            pConnection.Execute(sql2).Wait();
        }
    }
}
