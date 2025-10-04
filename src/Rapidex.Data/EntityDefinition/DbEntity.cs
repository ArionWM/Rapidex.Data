using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Rapidex.Data
{
    public class DbEntity : IIntEntity, IJsonOnDeserialized
    {
#pragma warning disable IDE1006 // Naming Styles
        protected ObjDictionary _Values { get; }
#pragma warning restore IDE1006 // Naming Styles

        [System.Text.Json.Serialization.JsonIgnore]
        IDbEntityMetadata IEntity._Metadata { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        object IEntity._virtualId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public IDbSchemaScope _Schema { get; set; }

        public object this[string columnName]
        {
            get { return this.GetValue(columnName); }
            set { this.SetValue(columnName, value); }
        }

        public bool _IsNew { get; set; }
        public bool _IsDeleted { get; set; }
        public string _TypeName { get; set; }
        public string _DbName { get; set; }
        public string _SchemaName { get; set; }


        public long Id
        {
            get
            {
                return this._Values[DatabaseConstants.FIELD_ID].As<long>();
            }
            set
            {
                this._Values[DatabaseConstants.FIELD_ID] = value;
            }
        }

        public string ExternalId
        {
            get
            {
                return this._Values[DatabaseConstants.FIELD_EXTERNAL_ID].As<string>();
            }
            set
            {
                this._Values[DatabaseConstants.FIELD_EXTERNAL_ID] = value;
            }
        }

        public int DbVersion
        {
            get
            {
                return this._Values[DatabaseConstants.FIELD_VERSION].As<int>();
            }
            set
            {
                this._Values[DatabaseConstants.FIELD_VERSION] = value;
            }
        }

        public DbEntity()
        {
            this._Values = new ObjDictionary();
        }

        public T GetValue<T>(string columnName)
        {
            columnName.NotEmpty();
            var val = this.GetValue(columnName);
            if (val == null)
                return default(T);

            //TODO: orjinal türünde bir değişken elde ederek atamalıyız. 
            //Zira object türünde dict içinden geldiği için halen "object" değişkeninde
            //tutuluyor. Bu durumda "T" türüne dönüşüm yaparken hata alabiliriz (implicit'ler çalışmıyor).
            return (T)val;
        }

        public void SetValue<T>(string columnName, T value)
        {
            this._Values.Set(columnName, value);
        }

        public object GetValue(string columnName)
        {
            return this._Values.Get(columnName);
        }

        public void SetValue(string fieldName, object value)
        {
            if (this._Schema.IsNullOrEmpty())
            {
                //Deattached entity
                this._Values.Set(fieldName, value);
            }
            else
            {
                var em = this.GetMetadata();
                var fm = em.Fields[fieldName];

                var evalue = EntityMapper.EnsureValueType(fm, this, value);
                this._Values.Set(fieldName, evalue);
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
            this.Id = id.As<long>();
            this.SetValue(nameof(this.Id), id);
        }

        public virtual void OnDeserialized()
        {
            this.EnsureDataTypeInitialization();
        }
    }

    public class PartialEntity : DbEntity, IPartialEntity
    {
        public bool _IsDeleted { get; set; } = false;

        public string[] GetFieldNames()
        {
            return this._Values.Keys.ToArray();
        }
    }
}
