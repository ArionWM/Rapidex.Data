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
    public void DropAllTablesInDatabase(string connectionString)
    {

        string schemasSql = "select sch.schema_name" +
            " from information_schema.schemata as sch" +
            " where sch.schema_name not in ('information_schema', 'pg_catalog', 'public')" +
            " and sch.schema_name not like 'pg_toast%'" +
            " and sch.schema_name not like 'pg_temp_%'";

        NpgsqlConnectionStringBuilder npgsqlConnectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);

        PostgreSqlServerConnection pConnection = new PostgreSqlServerConnection(npgsqlConnectionStringBuilder.ConnectionString);

        DataTable schemas = pConnection.Execute(schemasSql);
        foreach (DataRow row in schemas.Rows)
        {
            string schemaName = row["schema_name"].ToString();
            string sql1 = $"ALTER SCHEMA \"{schemaName}\" OWNER TO {npgsqlConnectionStringBuilder.Username}";
            pConnection.Execute(sql1);
            string sql2 = $"DROP SCHEMA \"{schemaName}\" CASCADE";
            pConnection.Execute(sql2);
        }
    }
}
