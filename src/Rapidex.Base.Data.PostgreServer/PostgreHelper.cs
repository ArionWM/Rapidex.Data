using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;

namespace Rapidex.Data.PostgreServer;

internal static class PostgreHelper
{
    public const NpgsqlDbType Jsonb = NpgsqlDbType.Jsonb;
    public const NpgsqlDbType Vector = (NpgsqlDbType)9001; // Placeholder for custom vector type, adjust as needed

    public static NpgsqlDbType Convert(DbFieldType dbType)
    {
        switch (dbType)
        {
            case DbFieldType.Int32:
                return NpgsqlDbType.Integer;
            case DbFieldType.Int64:
                return NpgsqlDbType.Bigint;
            case DbFieldType.String:
                return NpgsqlDbType.Varchar;
            case DbFieldType.DateTime:
            case DbFieldType.DateTime2:
                return NpgsqlDbType.TimestampTz;
            case DbFieldType.Guid:
                return NpgsqlDbType.Uuid;
            case DbFieldType.Decimal:
                return NpgsqlDbType.Numeric;
            case DbFieldType.Double:
                return NpgsqlDbType.Double;
            case DbFieldType.Binary:
                return NpgsqlDbType.Bytea;
            case DbFieldType.Int16:
                return NpgsqlDbType.Smallint;
            case DbFieldType.Boolean:
                return NpgsqlDbType.Boolean;
            case DbFieldType.Vector:
                return PostgreHelper.Vector;
            default:
                throw new NotSupportedException($"Not supported dbtype {dbType}");
        }
    }

    public static DbFieldType Convert(NpgsqlDbType dbType)
    {
        switch (dbType)
        {
            case NpgsqlDbType.Integer:
                return DbFieldType.Int32;
            case NpgsqlDbType.Bigint:
                return DbFieldType.Int64;
            case NpgsqlDbType.Text:
            case NpgsqlDbType.Varchar:
                return DbFieldType.String;
            case NpgsqlDbType.Timestamp:
            case NpgsqlDbType.TimestampTz:
                return DbFieldType.DateTime;
            case NpgsqlDbType.Uuid:
                return DbFieldType.Guid;
            case NpgsqlDbType.Numeric:
                return DbFieldType.Decimal;
            case NpgsqlDbType.Double:
                return DbFieldType.Double;
            case NpgsqlDbType.Bytea:
                return DbFieldType.Binary;
            case NpgsqlDbType.Smallint:
                return DbFieldType.Int16;
            case NpgsqlDbType.Boolean:
                return DbFieldType.Boolean;
            default:
                throw new NotSupportedException($"Not supported dbtype {dbType}");
        }
    }

    public static string GetDataTypeName(DbVariableType variableType)
    {
        var npgsqlDbType = Convert(variableType.DbType);

        switch (npgsqlDbType)
        {
            case NpgsqlDbType.Integer:
                return "integer";
            case NpgsqlDbType.Bigint:
                return "bigint";
            case NpgsqlDbType.Text:
                return "text";
            case NpgsqlDbType.Varchar:
                int length = variableType.Lenght;
                if (length == 0)
                    length = 255;
                if (length == -1 || length > 4000)
                    return $"text";
                else
                    return $"varchar({length})";
            case NpgsqlDbType.Timestamp:
                return "timestamp";
            case NpgsqlDbType.TimestampTz:
                return "timestamptz";
            case NpgsqlDbType.Uuid:
                return "uuid";
            case NpgsqlDbType.Numeric:
                return $"numeric({variableType.Lenght},{variableType.Scale})";
            case NpgsqlDbType.Double:
                return "double precision";
            case NpgsqlDbType.Bytea:
                return "bytea";
            case NpgsqlDbType.Smallint:
                return "smallint";
            case NpgsqlDbType.Boolean:
                return "boolean";
            case PostgreHelper.Vector:
                int length2 = variableType.Lenght;
                if (length2 == 0)
                    length2 = 1024;
                return $"vector({length2})";
            case NpgsqlDbType.Jsonb:
                return "jsonb";
            default:
                throw new NotSupportedException($"Not supported dbtype {variableType.DbType}");
        }
    }

    public static DbVariableType GetDataType(IDbFieldMetadata fm)
    {
        if (!fm.DbType.HasValue)
            throw new NotSupportedException($"DbType is not set ({fm.Name})");

        DbVariableType dbVariableType = new DbVariableType(fm.DbType.Value, fm.DbProperties.Length, fm.DbProperties.Scale);
        return dbVariableType;
    }

    public static string GetDataTypeName(IDbFieldMetadata fm)
    {
        DbVariableType dbVariableType = GetDataType(fm);
        return GetDataTypeName(dbVariableType);
    }

    internal static object CheckValue(object value)
    {
        if (value == null)
            return DBNull.Value;

        if (value is string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
                return DBNull.Value;
        }

        if (value is DateTimeOffset dto)
        {
            if (dto == DateTimeOffset.MinValue || dto == DateTimeOffset.MaxValue)
                return DBNull.Value;

            if (dto.Offset != TimeSpan.Zero)
            {
                //Her zaman UTC+0
                return dto.ToOffset(TimeSpan.Zero);
            }

            // PostgreSQL stores timestamps as UTC by default
            return dto;
        }

        return value;
    }

    public static DbVariable GetData(IDbFieldMetadata fm, object value)
    {
        if (!fm.DbType.HasValue)
            throw new NotSupportedException($"DbType is not set ({fm.Name})");

        DbVariable dbVariable = new DbVariable(fm.DbType.Value, fm.DbProperties.Length, fm.DbProperties.Scale);
        dbVariable.FieldName = fm.Name;
        dbVariable.ParameterName = GetVariableParameterName(fm.Name);
        dbVariable.Value = CheckValue(value);
        return dbVariable;
    }

    public static string GetVariableParameterName(string fieldName)
    {
        return $"@{fieldName}";
    }

    public static string TableName(string schema, string objectName)
    {
        schema = CheckObjectName(schema);
        objectName = CheckObjectName(objectName);
        return $"{schema}.{objectName}";
    }

    public static string CreateTableAlias(this IDbEntityMetadata em)
    {
        //TODO: Ensure uniqueness
        return $"{em.Name.AbbrFromFirstLetters()}{RandomHelper.RandomNumeric(6)}";
    }

    public static void Execute(this NpgsqlConnection connection, string sql)
    {
        if (connection == null) throw new ArgumentNullException(nameof(connection));
        if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentException("SQL command cannot be null or empty.", nameof(sql));
        using (var command = connection.CreateCommand())
        {
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
    }

    public static string CheckObjectName(string objectName)
    {
        objectName.NotEmpty();
        objectName = objectName.ToLowerInvariant();

        return objectName;
    }
}
