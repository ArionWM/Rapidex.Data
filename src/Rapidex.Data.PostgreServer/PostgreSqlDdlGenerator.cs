using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Rapidex.Data.PostgreServer;

public class PostgreSqlDdlGenerator
{
    public string SwitchDatabase(string databaseName)
    {
        // PostgreSQL does not support USE; connection string must specify database
        return $"-- Connect to database {databaseName} using your connection string";
    }

    public string IsDatabaseAvailable(string databaseName)
    {
        return $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'";
    }

    public string IsSchemaAvailable(string schemaName)
    {
        return $"SELECT schema_name FROM information_schema.schemata WHERE schema_name = '{schemaName}'";
    }

    public string IsTableAvailable(string schemaName, string tableName)
    {
        return $"SELECT 1 FROM information_schema.tables WHERE table_schema = '{schemaName}' AND table_name = '{tableName}'";
    }

    public string IsColumnAvailable(string schemaName, string tableName, string columnName)
    {
        return $"SELECT 1 FROM information_schema.columns WHERE table_schema = '{schemaName}' AND table_name = '{tableName}' AND column_name = '{columnName}'";
    }

    public string CreateSchema(string schemaName)
    {
        return $"DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = '{schemaName}') THEN EXECUTE 'CREATE SCHEMA {schemaName}'; END IF; END $$;";
    }

    public string CreateDatabase01(string databaseName, string logonName)
    {
        return $"CREATE DATABASE {databaseName} OWNER {logonName};";
    }

    public string CreateDatabase02(string databaseName, string logonName)
    {
        // Ownership is set in CREATE DATABASE; this is a placeholder
        return $"-- Ownership already set in CREATE DATABASE for {databaseName}";
    }

    public string DropDatabase01(string databaseName)
    {
        // No single-user mode in PostgreSQL
        return $"-- No single-user mode in PostgreSQL for {databaseName}";
    }

    public string DropDatabase02(string databaseName)
    {
        return $"DROP DATABASE IF EXISTS {databaseName};";
    }

    public string DropSchema(string schemaName)
    {
        return $"DROP SCHEMA IF EXISTS {schemaName} CASCADE;";
    }

    protected string CreateField(IDbFieldMetadata fm, bool isPrimaryKey)
    {
        // You need to implement PostgreSqlHelper.GetDataTypeName(fm)
        var dataType = PostgreHelper.GetDataTypeName(fm);
        var nullable = fm.DbProperties.IsNullable && !isPrimaryKey ? "" : "NOT NULL";
        var pk = isPrimaryKey ? "PRIMARY KEY" : "";

        string fieldName = PostgreHelper.CheckObjectName(fm.Name);

        return $"\"{fieldName}\" {dataType} {nullable} {pk}".Trim();
    }

    public string CreateTable(string schemaName, IDbEntityMetadata em)
    {
        string tableName = PostgreHelper.TableName(schemaName, em.TableName);
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"CREATE TABLE {tableName} (");

        List<string> fields = new List<string>
        {
            this.CreateField(em.PrimaryKey, true)
        };

        foreach (var field in em.Fields.Values)
        {
            if (field == em.PrimaryKey || !field.IsPersisted)
                continue;

            fields.Add(this.CreateField(field, false));
        }

        sb.AppendLine(string.Join(",\n", fields));
        sb.AppendLine(");");
        return sb.ToString();
    }



    public string GetTableStructure(string schemaName, IDbEntityMetadata em)
    {
        schemaName = PostgreHelper.CheckObjectName(schemaName);
        string tableName = PostgreHelper.CheckObjectName(em.TableName);
        return $"SELECT * FROM information_schema.columns WHERE table_schema = '{schemaName}' AND table_name = '{tableName}'";
    }

    public string DropTable(string schemaName, string tableName)
    {
        return $"DROP TABLE IF EXISTS {schemaName}.{tableName} CASCADE;";
    }

    public string CreateSequenceIfNotExists(string schemaName, string sequenceName, long minValue, long currentValue)
    {
        schemaName = PostgreHelper.CheckObjectName(schemaName);
        sequenceName = PostgreHelper.CheckObjectName(sequenceName);
        return $"DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM information_schema.sequences WHERE sequence_schema = '{schemaName}' AND sequence_name = '{sequenceName}') THEN EXECUTE 'CREATE SEQUENCE {schemaName}.{sequenceName} START WITH {currentValue} INCREMENT BY 1 MINVALUE {minValue}'; END IF; END $$;";
    }

    public string GetNextSequenceValue(string schemaName, string sequenceName)
    {
        schemaName = PostgreHelper.CheckObjectName(schemaName);
        sequenceName = PostgreHelper.CheckObjectName(sequenceName);
        return $"SELECT nextval('{schemaName}.{sequenceName}');";
    }

    public string GetNextNSequenceValues(string schemaName, string sequenceName, int numberCount)
    {
        schemaName = PostgreHelper.CheckObjectName(schemaName);
        sequenceName = PostgreHelper.CheckObjectName(sequenceName);
        return $@"
            SELECT nextval('{schemaName}.{sequenceName}') AS SequenceValue
            FROM generate_series(1, {numberCount});
            ";
    }

    public string GetCurrentSequenceValue(string schemaName, string sequenceName)
    {
        schemaName = PostgreHelper.CheckObjectName(schemaName);
        sequenceName = PostgreHelper.CheckObjectName(sequenceName);
        return $"SELECT last_value FROM {schemaName}.{sequenceName};";
    }

    public string RelocateSequence(string schemaName, string sequenceName, long startAt)
    {
        schemaName = PostgreHelper.CheckObjectName(schemaName);
        sequenceName = PostgreHelper.CheckObjectName(sequenceName);
        return $"ALTER SEQUENCE {schemaName}.{sequenceName} RESTART WITH {startAt};";
    }

    public string Insert(string schemaName, string tableName, DbVariable[] fieldNames)
    {
        schemaName = PostgreHelper.CheckObjectName(schemaName);
        tableName = PostgreHelper.CheckObjectName(tableName);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"INSERT INTO {schemaName}.{tableName} (");

        List<string> fields = fieldNames.Select(f => $"\"{PostgreHelper.CheckObjectName(f.FieldName)}\"").ToList();
        List<string> values = fieldNames.Select((f, i) => $"{f.ParameterName}").ToList();

        sb.AppendLine(string.Join(",\n", fields));
        sb.AppendLine(")");
        sb.AppendLine("VALUES (");
        sb.AppendLine(string.Join(",\n", values));
        sb.AppendLine(");");

        return sb.ToString();
    }

    public string Insert(string schemaName, string tableName, DbVariable[] fieldNames, List<DbVariable[]> dbVariables)
    {
        schemaName = PostgreHelper.CheckObjectName(schemaName);
        tableName = PostgreHelper.CheckObjectName(tableName);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"INSERT INTO {schemaName}.{tableName} (");

        List<string> fields = fieldNames.Select(f => $"\"{PostgreHelper.CheckObjectName(f.FieldName)}\"").ToList();
        sb.AppendLine(string.Join(",\n", fields));
        sb.AppendLine(")");
        sb.AppendLine("VALUES");

        List<string> values = new List<string>();
        foreach (DbVariable[] line in dbVariables)
        {
            var valueList = line.Select(li => $"{li.ParameterName}");
            values.Add("(" + string.Join(", ", valueList) + ")");
        }

        sb.AppendLine(string.Join(",\n", values));
        sb.AppendLine(";");

        return sb.ToString();
    }

    public string Update(string schemaName, IDbEntityMetadata em, DbVariable id, DbVariable[] fields, params string[] excludes)
    {
        schemaName = PostgreHelper.CheckObjectName(schemaName);
        string tableName = PostgreHelper.CheckObjectName(em.TableName);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"UPDATE {schemaName}.{tableName} SET");

        List<string> fieldValues = fields.Where(f => !excludes.Contains(f.FieldName)).Select(f => $"\"{PostgreHelper.CheckObjectName(f.FieldName)}\" = {f.ParameterName}").ToList();
        sb.AppendLine(string.Join(",\n", fieldValues));
        sb.AppendLine($"WHERE id = {id.ParameterName};");
        sb.AppendLine($"SELECT COUNT(*) FROM {schemaName}.{tableName} WHERE id = {id.ParameterName};");

        return sb.ToString();
    }

    public string Delete(string schemaName, IDbEntityMetadata em, IEnumerable<long> ids)
    {
        schemaName = PostgreHelper.CheckObjectName(schemaName);
        string tableName = PostgreHelper.CheckObjectName(em.TableName);
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"DELETE FROM {schemaName}.{tableName}");
        sb.AppendLine("WHERE id IN (");
        sb.AppendLine(string.Join(", ", ids));
        sb.AppendLine(");");
        return sb.ToString();
    }
}
