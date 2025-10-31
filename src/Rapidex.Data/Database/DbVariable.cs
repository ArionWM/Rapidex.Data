using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using YamlDotNet.Core.Tokens;

namespace Rapidex.Data;

public class DbVariable : DbVariableType
{
    object value;

    public string FieldName { get; set; }
    public string ParameterName { get; set; }
    public object Value { get { return this.value ?? DBNull.Value; } set { this.value = value; } }


    public DbVariable()
    {
    }

    public DbVariable(DbFieldType dbType) : base(dbType)
    {
    }

    public DbVariable(DbFieldType dbType, int lenght) : base(dbType, lenght)
    {
    }

    public DbVariable(DbFieldType dbType, int lenght, int scale) : base(dbType, lenght, scale)
    {
    }

    public override string ToString()
    {
        return $"{this.FieldName}: {this.Value}";
    }

    public static DbVariable Get(string name, object value)
    {
        DbVariable dbVar = new DbVariable();
        if (value == null || value == DBNull.Value)
        {
            dbVar.DbType = DbFieldType.String;
            dbVar.Value = DBNull.Value;
            return dbVar;
        }

        dbVar.ParameterName = name;
        dbVar.Value = value;

        var dbType = DataDbTypeConverter.GetDbType(value.GetType());
        dbVar.DbType = dbType.DbType;
        return dbVar;
    }

    public static DbVariable[] Get(IDictionary<string, object> values)
    {
        var dbValues = values.Select(v => DbVariable.Get(v.Key, v.Value));
        return dbValues.ToArray();
    }
    }
