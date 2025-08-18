using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Rapidex;
using YamlDotNet.Serialization;

namespace Rapidex.Data
{
    public class Enumeration : Reference, IEnumeration
    {
        public const string FIELD_IS_ARCHIVED = "IsArchived";

        public override string TypeName => "enum";


        public static void EnumDefinitionDataFiller(IDbSchemaScope scope, ItemDefinitionExtraData data, ReferenceDbFieldMetadata rfm, bool placeOptions)
        {
            scope.NotNull();

            data.Type = "enum";

            if (placeOptions)
            {
                var results = scope.GetQuery(rfm.ReferencedEntityMetadata.NotNull())
                      .Eq(FIELD_IS_ARCHIVED, false)
                      .Load().Result;

                foreach (IEntity entity in results)
                {
                    Dictionary<string, object> entityData = new Dictionary<string, object>();
                    entityData.Set("id", entity.GetId());
                    entityData.Set("value", entity.GetId());
                    entityData.Set("text", entity["name"]);
                    entityData.Set("hint", entity["description"]);
                    entityData.Set("icon", entity["icon"]);

                    object colorVal = entity.GetValue("color");
                    if (colorVal is IDataType dt)
                        colorVal = dt.GetValueLower();
                    entityData.Set("color", colorVal);

                    data.Options.Add(entityData);
                }


            }
        }

        public override IDbFieldMetadata SetupMetadata(IDbMetadataContainer container, IDbFieldMetadata self, ObjDictionary values)
        {
            IDbFieldMetadata fm = base.SetupMetadata(container, self, values);

            ReferenceDbFieldMetadata rfm = (ReferenceDbFieldMetadata)fm;
            rfm.DefinitionDataCallback = Enumeration.EnumDefinitionDataFiller;
            rfm.Type = this.GetType();
            rfm.TypeName = "enum";
            return fm;
        }



        public override void SetValue(IEntity entity, string fieldName, object value, bool applyToEntity)
        {
            if (value is Enum @enum)
            {
                value = Convert.ToInt32(@enum).As<long>();
            }

            if (value is string strEnum)
            {
                var em = entity._Metadata.Parent.Parent.Metadata.Get(this.ReferencedEntity);
                if (em != null)
                {
                    //ConcreteTypeName kullanmıyoruz, ConcreteTypeName sadece ConcreteEntity sınıfından türeyenler için
                    Type enumType = Rapidex.Common.Assembly.FindType(em.Name) ?? Type.GetType(em.Name);
                    if (enumType != null && enumType.IsEnum && Enum.TryParse(enumType, strEnum, true, out object enumValue))
                    {
                        value = Convert.ToInt32(enumValue).As<long>();
                    }

                }
            }

            //TODO: Bulamaz ise (String) enum'nin caption field'ı ile aramalı -> Reference

            base.SetValue(entity, fieldName, value, applyToEntity);
        }

        public override object GetSerializationData(EntitySerializationOptions options)
        {
            return base.GetSerializationData(options);
        }

        public override object SetWithSerializationData(string memberName, object value)
        {
            if (value is IDictionary<string, object> data)
            {
                this.Value = data["value"].As<long>();

            }
            else
            {
                this.Value = value.As<long>();
            }

            return null;
        }
    }

    /// <summary>
    /// reference\<T\> ile benzer yapıya sahiptir
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Enumeration<T> : Enumeration where T : System.Enum
    {
        public override Type BaseType => typeof(Enumeration<>);
        public override string TypeName => "enumConcrete";


        //[YamlMember(Alias = "enum", Order = 100)]
        //[JsonPropertyOrder(100)]
        //public string ReferencedEnumName => typeof(T).Name;

        public T EnumValue { get { return (T)Enum.ToObject(typeof(T), Convert.ToInt32(this.Value)); } }

        public override IDbFieldMetadata SetupMetadata(IDbMetadataContainer container, IDbFieldMetadata self, ObjDictionary values)
        {
            container.AddEnum<T>();

            this.ReferencedEntity = typeof(T).Name;

            values.Set("reference", this.ReferencedEntity);
            IDbFieldMetadata fm = base.SetupMetadata(container, self, values);
            fm.Type = this.GetType();
            return fm;
        }

        public override void SetValue(IEntity entity, string fieldName, object value, bool applyToEntity)
        {

            if (value is string strEnum)
            {
                if (Enum.TryParse(typeof(T), strEnum, true, out object enumValue))
                {
                    value = Convert.ToInt32(enumValue).As<long>();
                }
            }

            base.SetValue(entity, fieldName, value, applyToEntity);
        }

        public override object Clone()
        {
            Enumeration<T> clone = new Enumeration<T>();
            clone.ReferencedEntity = this.ReferencedEntity;
            clone.TargetId = this.TargetId;
            return clone;

        }

        public override string ToString()
        {
            T val = this;
            return val.ToString();
        }

        public static implicit operator Enumeration<T>(System.Enum enumVal)
        {
            Enumeration<T> reference = new Enumeration<T>();
            if (enumVal == null)
            {
                reference.TargetId = DatabaseConstants.DEFAULT_EMPTY_ID;
            }
            else
            {
                reference.TargetId = Convert.ToInt32(enumVal).As<long>();
                reference.ReferencedEntity = enumVal.GetType().Name;
            }

            return reference;
        }

        public static implicit operator Enumeration<T>(T enumVal)
        {
            Enumeration<T> reference = new Enumeration<T>();
            if (enumVal == null)
            {
                reference.TargetId = DatabaseConstants.DEFAULT_EMPTY_ID;
            }
            else
            {
                reference.TargetId = Convert.ToInt32(enumVal).As<long>();
                reference.ReferencedEntity = enumVal.GetType().Name;
            }

            return reference;
        }

        public static implicit operator System.Enum(Enumeration<T> reference)
        {
            if (reference == null)
                return null;

            if (0L == reference.Value)
                return null;


            System.Enum en = (System.Enum)System.Enum.ToObject(typeof(T), reference.TargetId);
            return en;
        }

        public static implicit operator T(Enumeration<T> reference)
        {
            if (reference == null)
                return default(T);

            if (0L == reference.Value)
                return default(T);


            System.Enum en = (System.Enum)System.Enum.ToObject(typeof(T), reference.TargetId);
            return (T)en;
        }
    }
}
