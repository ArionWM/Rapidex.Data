using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Rapidex.Data;

[DataContract]
public class DbFieldMetadata : IDbFieldMetadata
{
    protected Type type { get; set; }

    [YamlMember(Order = -9999)]
    [JsonPropertyOrder(-9999)]
    public virtual string Name { get; set; }

    public string NavigationName => this.Name;

    [JsonPropertyName("type")]
    [YamlMember(Alias = "type", Order = -9998)]
    [JsonPropertyOrder(-9998)]
    public virtual string TypeName { get; set; }

    public virtual string Caption { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    [YamlIgnore]
    public virtual bool IsPersisted { get; set; } = true;

    [System.Text.Json.Serialization.JsonIgnore]
    [YamlIgnore]
    public virtual bool SkipDirectLoad { get; set; } = false;

    [System.Text.Json.Serialization.JsonIgnore]
    [YamlIgnore]
    public virtual bool SkipDirectSet { get; set; } = false;

    [System.Text.Json.Serialization.JsonIgnore]
    [YamlIgnore]
    public virtual bool SkipDbVersioning { get; set; }

    //?? Ui ile ilgili bir şey olmamalı?
    [System.Text.Json.Serialization.JsonIgnore]
    [YamlIgnore]
    public virtual bool Invisible { get; set; }

    public virtual bool IsSealed { get; set; } = false;

    [System.Text.Json.Serialization.JsonIgnore]
    [YamlIgnore]
    public virtual Type Type { get { return this.type; } set { this.SetType(value); } }

    [System.Text.Json.Serialization.JsonIgnore]
    [YamlIgnore]
    public virtual Type BaseType { get; set; }

    [YamlIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    //[YamlMember(Order = -9996)]
    //[JsonPropertyOrder(-9996)]
    public virtual DbFieldType? DbType { get; set; }




    [YamlIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual DbFieldProperties DbProperties { get; set; } = new DbFieldProperties();

    [YamlIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual EntityFieldValueGetterDelegate ValueGetterUpper { get; set; }

    [YamlIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual EntityFieldValueGetterDelegate ValueGetterLower { get; set; }

    [YamlIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ValueSetterDelegate ValueSetter { get; set; }

    [YamlIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public IDbEntityMetadata ParentMetadata { get; set; }

    public DbFieldMetadata()
    {
        this.ValueGetterLower = this.GetLowerValueFromEntity;
        this.ValueGetterUpper = this.GetUpperValueFromEntity;
        this.ValueSetter = this.SetValue;
    }

    public DbFieldMetadata(IDbFieldMetadata from) : this()
    {
        this.Name = from.Name;
        this.BaseType = from.BaseType;
        this.DbType = from.DbType;
        //this.IsPrimaryKey = from.IsPrimaryKey;
        this.SkipDbVersioning = from.SkipDbVersioning;
    }

    protected void SetType(Type type)
    {
        this.type = type;

        if (this.BaseType == null)
            this.BaseType = type;

        if (this.TypeName.IsNullOrEmpty())
            this.TypeName = type.Name.CamelCase();
    }

    public override string ToString()
    {
        return $"{this.Name}:{this.Type.Name}";
    }

    protected virtual object GetLowerValueFromEntity(IEntity entity, string fieldName)
    {
        object value = entity.GetValue(this.Name);
        if (value is IDataType dt)
        {
            value = dt?.GetValueLower();
        }
        return value;
    }

    protected virtual object GetUpperValueFromEntity(IEntity entity, string fieldName)
    {
        return entity.GetValue(this.Name);
    }

    protected virtual void SetValue(IEntity entity, string fieldName, object value, bool applyToEntity)
    {
        if (applyToEntity)
            entity.SetValue(this.Name, value);
    }

    public virtual void Setup(IDbEntityMetadata parentMetadata)
    {
        this.ParentMetadata = parentMetadata;
    }

    public virtual void GetDefinitionData(IDbSchemaScope scope, ref ItemDefinitionExtraData data, bool placeOptions)
    {
        data.Type = this.TypeName;
        data.Name = this.Name;
    }
}
