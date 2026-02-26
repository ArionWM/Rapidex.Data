using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Rapidex.Data;

public abstract class DbConcreteEntityBase : IConcreteEntity, IIntEntity, IDbDefinition
{
#pragma warning disable IDE1006 // Naming Styles
    [System.Text.Json.Serialization.JsonIgnore]
    protected Type _Type { get; set; }
    protected ObjDictionary _Values { get; }
#pragma warning restore IDE1006 // Naming Styles

    [System.Text.Json.Serialization.JsonIgnore]
    IDbEntityMetadata IEntity._Metadata { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    object IEntity._VirtualId { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public IDbSchemaScope _Schema { get; set; }

    public LoadSource _loadSource { get; set; } = LoadSource.Unknown;

    public object this[string columnName]
    {
        get { return this.GetValue(columnName); }
        set { this.SetValue(columnName, value); }
    }

    public string _TypeName { get; set; }
    public string _DbName { get; set; }
    public string _SchemaName { get; set; }
    public bool _IsNew { get; set; } = true;

    bool IEntity._IsDeleted { get; set; }

    public long Id { get; set; } = DatabaseConstants.DEFAULT_EMPTY_ID;
    public virtual string ExternalId { get; set; }
    public int DbVersion { get; set; }


    protected DbConcreteEntityBase()
    {
        this._Type = this.GetType();
        this._TypeName = this._Type.Name;
        this._Values = new ObjDictionary();
    }

    public T GetValue<T>(string fieldName)
    {
        object val = this.GetValue(fieldName);
        if (val == null)
            return default(T);
        return (T)val;
    }

    public void SetValue<T>(string fieldName, T value)
    {
        var prop = this._Type.GetPropertyCached(fieldName);
        if (prop == null)
        {
            this._Values.Set(fieldName, value);
        }
        else
        {
            if (prop.CanWrite)
                prop.SetValue(this, value);
        }
    }

    public object GetValue(string fieldName)
    {
        //TODO: EntityMetadata'daki setter'lar?
        var prop = this._Type.GetPropertyCached(fieldName);
        if (prop == null)
            return this._Values.Get(fieldName);

        return prop.GetValue(this, null);
    }

    public void SetValue(string fieldName, object value)
    {
        var prop = this._Type.GetPropertyCached(fieldName);
        if (prop == null)
        {
            var em = this.GetMetadata();
            var fm = em.Fields[fieldName];

            var evalue = EntityMapper.EnsureValueType(fm, this, value);
            this._Values.Set(fieldName, evalue);
        }
        else
        {
            if (prop.CanWrite)
            {
                var evalue = EntityMapper.EnsureValueType(prop, this, value);
                prop.SetValue(this, evalue);
            }
        }
    }

    public ObjDictionary GetAllValues()
    {
        return this._Values;
    }

    public object GetId()
    {
        return this.Id;
    }

    public void SetId(object id)
    {
        this.Id = (long)id;
        this.SetValue(nameof(this.Id), id);
    }

    public virtual void OnDeserialized()
    {
     
    }


}
