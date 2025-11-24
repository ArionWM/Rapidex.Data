using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.PostgreServer;
internal class PostgreSqlServerAuthorizationChecker : IDbAuthorizationChecker
{
    PostgreSqlServerConnection connection;

    public PostgreSqlServerAuthorizationChecker(string connectionString)
    {
        connection = new PostgreSqlServerConnection(connectionString);
    }

    public bool CanCreateDatabase()
    {
        DataTable result = connection.Execute("SELECT rolcreatedb FROM pg_roles WHERE rolname = current_user;");
        if (result.Rows.Count == 0)
            return false;

        var resultValue = result.Rows[0][0];
        return resultValue != null && (bool)resultValue;
    }

    public bool CanCreateSchema()
    {
        DataTable result = connection.Execute("SELECT has_database_privilege(current_user, current_database(), 'CREATE');");
        if (result.Rows.Count == 0)
            return false;

        var resultValue = result.Rows[0][0];
        return resultValue != null && (bool)resultValue;
    }

    public bool CanCreateTable(string schemaName)
    {
        schemaName = schemaName.ToLowerInvariant();
        DataTable result = connection.Execute($"SELECT has_schema_privilege(current_user, '{schemaName}', 'CREATE');");
        if (result.Rows.Count == 0)
            return false;

        var resultValue = result.Rows[0][0];
        return resultValue != null && (bool)resultValue;
    }

    public string GetCurrentUserId()
    {
        DataTable result = connection.Execute("SELECT current_user;");
        if (result.Rows.Count == 0)
            return null;

        return result.Rows[0][0]?.ToString();
    }

    public void Dispose()
    {
        connection?.Dispose();
        connection = null;
    }
}