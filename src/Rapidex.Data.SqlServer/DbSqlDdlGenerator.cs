using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;

namespace Rapidex.Data.SqlServer;

public class DbSqlDdlGenerator
{
    public string SwitchDatabase(string databaseName)
    {
        return $"USE [{databaseName}]";
    }

    public string IsDatabaseAvailable(string databaseName)
    {
        return $"SELECT DB_ID('{databaseName}') AS DatabaseId";
    }

    public string IsSchemaAvailable(string schemaName)
    {
        //select 1 from information_schema.schemata where schema_name='my-schema'
        return $"SELECT SCHEMA_ID('{schemaName}') AS SchemaId";
    }

    public string IsTableAvailable(string schemaName, string tableName)
    {
        //select 1 from information_schema.tables where table_schema='my-schema' and table_name='my-table'
        return $"SELECT OBJECT_ID('{schemaName}.{tableName}') AS TableId";
    }

    public string IsColumnAvailable(string schemaName, string tableName, string columnName)
    {
        //select 1 from information_schema.columns where table_schema='my-schema' and table_name='my-table' and column_name='my-column'
        return $"SELECT COLUMN_ID('{schemaName}.{tableName}', '{columnName}') AS ColumnId";
    }

    public string CreateSchema(string schemaName)
    {
        return $"IF (NOT EXISTS (SELECT * FROM sys.schemas WHERE name = '{schemaName}')) BEGIN EXEC ('CREATE SCHEMA {schemaName} AUTHORIZATION dbo') END";
    }

    public string CreateDatabase01(string databaseName, string logonName)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"CREATE DATABASE [{databaseName}]");
        return sb.ToString();
    }

    public string CreateDatabase02(string databaseName, string logonName)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"ALTER AUTHORIZATION ON DATABASE::[{databaseName}] TO {logonName}"); //logonName assign to db dbo user
        return sb.ToString();
    }

    public string DropDatabase01(string databaseName)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;");
        return sb.ToString();
    }

    public string DropDatabase02(string databaseName)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"DROP DATABASE [{databaseName}]");
        return sb.ToString();
    }

    public string DropSchema(string databaseName)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"DROP SCHEMA [{databaseName}]");
        return sb.ToString();
    }

    protected string CreateField(IDbFieldMetadata fm, bool isPrimaryKey)
    {
        if (isPrimaryKey)
        {
            return $"[{fm.Name}] {DbSqlServerHelper.GetDataTypeName(fm)} PRIMARY KEY";
        }

        return $"[{fm.Name}] {DbSqlServerHelper.GetDataTypeName(fm)}";
    }

    public string CreateTable(string schemaName, IDbEntityMetadata em)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"CREATE TABLE [{schemaName}].[{em.TableName}]");
        sb.AppendLine("(");

        List<string> fields = new List<string>();

        fields.Add(this.CreateField(em.PrimaryKey, true));

        foreach (var field in em.Fields.Values)
        {
            if (field == em.PrimaryKey || !field.IsPersisted)
                continue;

            fields.Add(this.CreateField(field, field == em.PrimaryKey));
            //sb.AppendLine(this.CreateField(field));
        }

        sb.AppendLine(fields.Join(", \r\n"));

        sb.AppendLine(")");
        return sb.ToString();
    }

    public string CreateTableTypeDeclaration(string schemaName, IDbEntityMetadata em)
    {
        string name = $"{schemaName}.{em.TableName}";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"IF TYPE_ID(N'{name}') IS NOT NULL drop type {name};");

        sb.AppendLine($"CREATE TYPE {name} as TABLE");
        sb.AppendLine("(");

        List<string> fields = new List<string>();

        fields.Add(this.CreateField(em.PrimaryKey, true));

        var sortedFields = em.Fields.Values.OrderBy(f => f.Name);
        foreach (var field in sortedFields)
        {
            if (!field.IsPersisted)
                continue;

            if (field == em.PrimaryKey)
                continue;

            fields.Add(this.CreateField(field, field == em.PrimaryKey));
            //sb.AppendLine(this.CreateField(field));
        }

        sb.AppendLine(fields.Join(", \r\n"));

        sb.AppendLine(")");
        return sb.ToString();
    }

    public string GetTableStructure(string schemaName, IDbEntityMetadata em)
    {
        string sql = $"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='{schemaName}' and table_name = '{em.TableName}'";
        return sql;
    }

    public string DropTable(string schemaName, string tableName)
    {
        return $"DROP TABLE [{schemaName}].[{tableName}]";
    }

    public string CreateSequenceIfNotExists(string schemaName, string sequenceName, long minValue, long currentValue)
    {
        string sql = $"IF NOT EXISTS(SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{schemaName}].[{sequenceName}]') AND type = 'SO') CREATE SEQUENCE [{schemaName}].[{sequenceName}] AS [bigint] START WITH {currentValue} INCREMENT BY 1 MINVALUE {minValue} ";
        return sql;
    }

    public string GetNextSequenceValue(string schemaName, string sequenceName)
    {
        string sql = $"SELECT NEXT VALUE FOR [{schemaName}].[{sequenceName}]";
        return sql;
    }

    public string GetNextNSequenceValues(string schemaName, string sequenceName, int numberCount)
    {
        //TODO: Use sys.sp_sequence_get_range

        string sql = $@"
DECLARE
    @firstNum AS SQL_VARIANT,
    @lastNum AS SQL_VARIANT;

EXECUTE sys.sp_sequence_get_range
    @sequence_name = N'[{schemaName}].[{sequenceName}]',
    @range_size = {numberCount},
    @range_first_value = @firstNum OUTPUT,
    @range_last_value = @lastNum OUTPUT;

-- The following statement returns the output values
SELECT @firstNum AS FirstVal,
       @lastNum AS LastVal;

";

        return sql;
    }

    public string GetCurrentSequenceValue(string schemaName, string sequenceName)
    {
        string sql = $"SELECT CURRENT_VALUE FROM sys.sequences WHERE name = '{sequenceName}' AND SCHEMA_NAME(schema_id) = '{schemaName}'";
        return sql;
    }

    public string RelocateSequence(string schemaName, string sequenceName, long startAt)
    {
        string sql = $"ALTER SEQUENCE [{schemaName}].[{sequenceName}] RESTART WITH {startAt}";
        return sql;
    }

    //TODO: Template
    public string Insert(string schemaName, string tableName, DbVariable[] fieldNames)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"INSERT INTO [{schemaName}].[{tableName}]");
        sb.AppendLine("(");

        List<string> fields = new List<string>();
        List<string> values = new List<string>();

        foreach (var dbVariable in fieldNames)
        {
            fields.Add($"[{dbVariable.FieldName}]");
            values.Add($"{dbVariable.ParameterName}");
        }

        sb.AppendLine(fields.Join(", \r\n"));
        sb.AppendLine(")");
        sb.AppendLine("VALUES");
        sb.AppendLine("(");
        sb.AppendLine(values.Join(", \r\n"));
        sb.AppendLine(")");

        return sb.ToString();

    }

    public string InsertWithValueTable(string schemaName, string tableName, DataTable variableTable)
    {
        variableTable.TableName = $"{schemaName}.{tableName}";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"INSERT INTO [{schemaName}].[{tableName}]");
        sb.AppendLine("(");

        List<string> fields = new List<string>();


        foreach (DataColumn column in variableTable.Columns)
        {
            fields.Add($"[{column.ColumnName}]");
        }

        sb.AppendLine(fields.Join(", \r\n"));
        sb.AppendLine(")");


        sb.AppendLine("select ");
        sb.AppendLine(fields.Join(", \r\n"));
        sb.AppendLine(" from @table ");

        return sb.ToString();

    }

    //TODO: Template
    public string Insert(string schemaName, string tableName, DbVariable[] fieldNames, List<DbVariable[]> dbVariables)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"INSERT INTO [{schemaName}].[{tableName}]");
        sb.AppendLine("(");

        List<string> fields = new List<string>();
        List<string> values = new List<string>();

        foreach (var dbVariable in fieldNames)
        {
            fields.Add($"[{dbVariable.FieldName}]");
        }

        sb.AppendLine(fields.Join(", \r\n"));
        sb.AppendLine(")");
        sb.AppendLine("VALUES");

        foreach (DbVariable[] line in dbVariables)
        {
            StringBuilder valSb = new StringBuilder();
            valSb.AppendLine("\r\n(");
            valSb.AppendLine(line.Select(li => li.ParameterName).Join(", \r\n"));
            valSb.AppendLine(")");

            values.Add(valSb.ToString() + "\r\n");
        }

        sb.AppendLine(values.Join(", "));

        return sb.ToString();
    }


    //TODO: aliases
    //TODO: Template
    public string Update(string schemaName, IDbEntityMetadata em, DbVariable id, DbVariable[] fields, params string[] excludes)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"UPDATE [{schemaName}].[{em.TableName}]");
        sb.AppendLine("SET");

        List<string> fieldValues = new List<string>();

        foreach (var dbVariable in fields)
        {
            if (excludes.Contains(dbVariable.FieldName))
                continue;

            fieldValues.Add($"[{dbVariable.FieldName}] = {dbVariable.ParameterName}");
        }

        sb.AppendLine(fieldValues.Join(", \r\n"));
        sb.AppendLine("WHERE");
        sb.AppendLine($"[Id] = {id.ParameterName}");

        sb.AppendLine("select @@ROWCOUNT");

        return sb.ToString();
    }

    public string Delete(string schemaName, IDbEntityMetadata em, IEnumerable<long> ids)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"DELETE FROM [{schemaName}].[{em.TableName}]");
        sb.AppendLine("WHERE Id in (");

        sb.AppendLine(ids.Select(i => i.ToString()).Join(", "));
        sb.AppendLine(")");



        return sb.ToString();
    }
}
