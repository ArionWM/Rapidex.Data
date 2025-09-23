using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.SqlServer;
internal class DbSqlServerAuthorizationChecker : IDbAuthorizationChecker
{
    DbSqlServerConnection _connection;
    public DbSqlServerAuthorizationChecker(string connectionString)
    {
        _connection = new DbSqlServerConnection(connectionString);
    }
    public bool CanCreateDatabase()
    {
        DataTable result = _connection.Execute("SELECT ISNULL(IS_SRVROLEMEMBER('dbcreator'), 0);");
        if (result.Rows.Count == 0)
            return false;

        var resultValue = result.Rows[0][0];
        return resultValue != null && (int)resultValue == 1;
    }
    public bool CanCreateSchema()
    {
        
        string sql = @"
            DECLARE @result INT = 0;
            
            -- HAS_PERMS_BY_NAME ile kontrol et
            SET @result = ISNULL(HAS_PERMS_BY_NAME(DB_NAME(), 'DATABASE', 'CREATE SCHEMA'), 0);
            
            -- Eğer hala 0 ise, db_owner veya db_ddladmin rollerini kontrol et
            IF @result = 0
            BEGIN
                IF IS_ROLEMEMBER('db_owner') = 1 OR IS_ROLEMEMBER('db_ddladmin') = 1
                    SET @result = 1;
            END
            
            SELECT @result;";
        
        DataTable dataTable = _connection.Execute(sql);
        if (dataTable.Rows.Count == 0)
            return false;
        var result = dataTable.Rows[0][0];
        return result != null && (int)result == 1;
    }
    public bool CanCreateTable(string schemaName)
    {
        string sql = @$"
            DECLARE @result INT = 0;
            
            -- Schema var mı kontrol et
            IF EXISTS (SELECT 1 FROM sys.schemas WHERE name = '{schemaName}')
            BEGIN
                -- Schema'da CREATE TABLE yetkisi var mı kontrol et
                SET @result = ISNULL(HAS_PERMS_BY_NAME('{schemaName}', 'SCHEMA', 'CREATE TABLE'), 0);
                
                -- Eğer hala NULL ise, db_owner veya db_ddladmin rollerini kontrol et
                IF @result = 0
                BEGIN
                    IF IS_ROLEMEMBER('db_owner') = 1 OR IS_ROLEMEMBER('db_ddladmin') = 1
                        SET @result = 1;
                END
            END
            ELSE
            BEGIN
                -- Schema yoksa, schema oluşturma yetkisi var mı kontrol et
                SET @result = ISNULL(HAS_PERMS_BY_NAME(DB_NAME(), 'DATABASE', 'CREATE SCHEMA'), 0);
                
                -- Eğer schema oluşturma yetkisi varsa, table da oluşturabilir
                IF @result = 0
                BEGIN
                    IF IS_ROLEMEMBER('db_owner') = 1 OR IS_ROLEMEMBER('db_ddladmin') = 1
                        SET @result = 1;
                END
            END
            
            SELECT @result;";


        DataTable dataTable = _connection.Execute(sql);
        if (dataTable.Rows.Count == 0)
            return false;
        var result = dataTable.Rows[0][0];
        return result != null && (int)result == 1;
    }

    public string GetCurrentUserId()
    {
        DataTable dataTable = _connection.Execute("SELECT SUSER_SNAME();");
        if (dataTable.Rows.Count == 0)
            return null;

        var result = dataTable.Rows[0][0];
        return result?.ToString();
    }
    public void Dispose()
    {
        _connection?.Dispose();
        _connection = null;
    }

}