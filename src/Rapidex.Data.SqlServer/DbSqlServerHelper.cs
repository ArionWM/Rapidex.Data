using System;
using System.Collections.Generic;
using Microsoft.Data;
using Microsoft.Data.SqlTypes;
using System.Runtime.CompilerServices;
using System.Text;
using System.Data;

namespace Rapidex.Data.SqlServer;

internal static class DbSqlServerHelper
{
    public const SqlDbType JSON = (SqlDbType)35;
    public const SqlDbType VECTOR = (SqlDbType)36;

    //SELECT * FROM sys.messages WHERE language_id = 1033 and text like '%permission%'
    public static int[] SQL_Errors_Permission = new int[] { 229, 230, 262, 297, 300 };
    public static SqlDbType Convert(DbFieldType dbType)
    {
        switch (dbType)
        {
            case DbFieldType.Int32:
                return SqlDbType.Int;

            case DbFieldType.Int64:
                return SqlDbType.BigInt;

            case DbFieldType.String:
                return SqlDbType.NVarChar;

            case DbFieldType.DateTime:
            case DbFieldType.DateTime2:
                return SqlDbType.DateTime2;

            case DbFieldType.DateTimeOffset:
                return SqlDbType.DateTimeOffset;

            case DbFieldType.Guid:
                return SqlDbType.UniqueIdentifier;

            case DbFieldType.Decimal:
                return SqlDbType.Decimal;

            case DbFieldType.Double:
                return SqlDbType.Float;

            case DbFieldType.Binary:
                return SqlDbType.Image;

            case DbFieldType.Int16:
                return SqlDbType.SmallInt;

            case DbFieldType.Boolean:
                return SqlDbType.Bit;

            case DbFieldType.Vector:
                return DbSqlServerHelper.VECTOR;

            default:
                throw new NotSupportedException($"Not supported dbtype {dbType}");
        }
    }

    public static DbFieldType Convert(SqlDbType dbType)
    {
        switch (dbType)
        {
            case SqlDbType.Int:
                return DbFieldType.Int32;

            case SqlDbType.BigInt:
                return DbFieldType.Int64;

            case SqlDbType.NVarChar:
                return DbFieldType.String;

            case SqlDbType.DateTime:
                return DbFieldType.DateTime;

            case SqlDbType.DateTime2:
                return DbFieldType.DateTime;

            case SqlDbType.UniqueIdentifier:
                return DbFieldType.Guid;

            case SqlDbType.Decimal:
                return DbFieldType.Decimal;

            case SqlDbType.Float:
                return DbFieldType.Double;

            case SqlDbType.Image:
                return DbFieldType.Binary;

            case SqlDbType.SmallInt:
                return DbFieldType.Int16;

            case SqlDbType.Bit:
                return DbFieldType.Boolean;

            default:
                throw new NotSupportedException($"Not supported dbtype {dbType}");
        }
    }


    public static string GetDataTypeName(DbVariableType variableType)
    {
        SqlDbType sqlDbType = Convert(variableType.DbType);

        switch (sqlDbType)
        {
            case SqlDbType.Int:
                return "int";

            case SqlDbType.BigInt:
                return "bigint";

            case SqlDbType.NVarChar:
                int lenght = variableType.Lenght;
                if (lenght == 0)
                    lenght = 255;
                return $"nvarchar({((lenght > 4000 || lenght == -1) ? "max" : lenght.ToString())})";

            case SqlDbType.DateTime:
                return "datetime";

            case SqlDbType.DateTime2:
                return "datetime2";

            case SqlDbType.DateTimeOffset:
                return "datetimeoffset";

            case SqlDbType.UniqueIdentifier:
                return "uniqueidentifier";

            case SqlDbType.Decimal:
                return $"decimal({variableType.Lenght},{variableType.Scale})";

            case SqlDbType.Float:
                return "float";

            case SqlDbType.Image:
                return "image";

            case SqlDbType.SmallInt:
                return "smallint";

            case SqlDbType.Bit:
                return "bit";

            case DbSqlServerHelper.VECTOR:
                int lenght2 = variableType.Lenght;
                if (lenght2 == 0)
                    lenght2 = 1024;
                return string.Format("vector({0})", lenght2);

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

        if (value is string strValue && strValue.IsNullOrEmpty())
        {
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

    public static string DbName(string schema, string objectName)
    {
        return $"[{schema}].[{objectName}]";
    }

    public static string CreateTableAlias(this IDbEntityMetadata em)
    {
        //TODO: Tekilliği garanti edilmeli
        return $"{em.Name.AbbrFromFirstLetters()}{RandomHelper.RandomNumeric(6)}";
    }

    private static bool NeedQuote(DbVariable var)
    {
        switch (var.DbType)
        {
            case DbFieldType.String:
            case DbFieldType.DateTime:
            case DbFieldType.DateTime2:
            case DbFieldType.DateTimeOffset:
            case DbFieldType.Guid:
                return true;
            default:
                return false;
        }
    }

    

    public static string CreateSqlLog(int debugId, string sql, params DbVariable[] parameters)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"-- Executing ({debugId})");
        if (parameters.IsNOTNullOrEmpty())
        {
            sb.AppendLine("-- Parameters:");
            foreach (var param in parameters)
            {
                if (NeedQuote(param))
                    sb.AppendLine($" declare {param.ParameterName} = '{param.Value}' -- ({param.DbType}, {param.Value?.GetType().Name})");
                else
                    sb.AppendLine($" declare {param.ParameterName} {GetDataTypeName(param)} = {param.Value} -- ({param.DbType}, {param.Value?.GetType().Name})");
            }
        }

        sb.AppendLine($"-- SQL: ({debugId})");
        sb.AppendLine(sql);

        return sb.ToString();

    }
}
