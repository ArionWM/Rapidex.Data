using Npgsql;
using System;

namespace Rapidex.Data.PostgreServer;

internal class PostgreSqlServerExceptionTranslator : IExceptionTranslator
{
    public Exception Translate(Exception ex)
    {
        if (ex is TranslatedException)
        {
            return ex;
        }

        var pgException = ex as PostgresException;
        if (pgException == null)
            return null;

        // See: https://www.postgresql.org/docs/current/errcodes-appendix.html
        switch (pgException.SqlState)
        {
            case "23503": // foreign_key_violation
                return new TranslatedException(ExceptionTarget.ApplicationSupport, "Foreign key violation.", ex);
            case "23505": // unique_violation
                return new TranslatedException(ExceptionTarget.ApplicationSupport, "Violation of unique or primary key constraint.", ex);
            case "23502": // not_null_violation
                return new TranslatedException(ExceptionTarget.ApplicationSupport, "Null value in column violates not-null constraint.", ex);
            case "3D000": // invalid_catalog_name (database does not exist)
                return new TranslatedException(ExceptionTarget.ITDepartment_ApplicationManagement, "Database does not exist. Check the connection string and database name.", ex);
            case "28P01": // invalid_password
                return new TranslatedException(ExceptionTarget.ITDepartment_ApplicationManagement, "Invalid username or password for PostgreSQL server.", ex);
            case "08001": // sqlclient_unable_to_establish_sqlconnection
            case "08006": // connection_failure
                return new TranslatedException(ExceptionTarget.ITDepartment_Infrastructure, "Could not connect to PostgreSQL server. Check network and server status.", ex);
            case "42P01": // undefined_table
                return new TranslatedException(ExceptionTarget.ApplicationSupport, "Target table does not exist.", ex);
            case "42703": // undefined_column
                return new TranslatedException(ExceptionTarget.ApplicationSupport, $"Column not found in database/table: \r\n{ex.Message}", ex);
            case "42501": // insufficient_privilege
                return new TranslatedException(ExceptionTarget.ITDepartment_ApplicationManagement, "Insufficient privileges to access the requested object.", ex);
            case "40001": // serialization_failure (deadlock)
                return new TranslatedException(ExceptionTarget.ApplicationSupport, "Deadlock detected or serialization failure.", ex);
            case "42P07": // duplicate_table
                return new TranslatedException(ExceptionTarget.ApplicationSupport, "Table already exists in the database.", ex);
            case "42710": // duplicate_object
                return new TranslatedException(ExceptionTarget.ApplicationSupport, "Object already exists in the database.", ex);
            default:
                return new TranslatedException(ExceptionTarget.ApplicationSupport, "An unknown PostgreSQL error occurred.", ex);
        }
    }
}
