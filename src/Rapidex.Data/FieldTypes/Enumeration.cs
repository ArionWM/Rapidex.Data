using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.AccessControl;
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
                var results = scope.GetQuery(rfm.ReferencedEntityMetadata.EnsureIsNotPremature(scope.ParentDbScope))
                      .Eq(FIELD_IS_ARCHIVED, false)
                      .Load();

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

        protected bool TryFindEnumConcreteType(IDbMetadataContainer container, bool checkMetadata, ref Type enumType)
        {
            enumType = null;
            var em = container.Get(this.ReferencedEntity);

            if (em != null && em.ConcreteTypeName.IsNOTNullOrEmpty())
            {
                enumType = Type.GetType(em.ConcreteTypeName);
            }

            if (enumType == null)
            {
                enumType = Rapidex.Common.Assembly.FindType(em.Name, true) ?? Type.GetType(em.Name);
            }

            bool finded = enumType != null && enumType.IsEnum;

            if (finded && checkMetadata)
            {
                //Enum metadata'ları kontrol ediliyor
                container.AddIfNotExist(enumType);
            }

            return finded;
        }

        public override IDbFieldMetadata SetupMetadata(IDbMetadataContainer container, IDbFieldMetadata self, ObjDictionary values)
        {
            IDbFieldMetadata fm = base.SetupMetadata(container, self, values);

            Type enumType = null;
            this.TryFindEnumConcreteType(container, true, ref enumType);

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
                Type enumType = null;

                if (this.TryFindEnumConcreteType(this.GetParent().GetMetadata().Parent, true, ref enumType)) //TODO: Çok maaliyetli
                {
                    if (enumType.IsEnum && Enum.TryParse(enumType, strEnum, true, out object enumValue))
                    {
                        value = Convert.ToInt32(enumValue).As<long>();
                    }
                }

                //var em = entity._Metadata.Parent.DbScope.Metadata.Get(this.ReferencedEntity);
                //if (em != null)
                //{
                //    if (em.IsPremature)
                //        throw new InvalidOperationException($"Entity metadata '{em.Name}' is premature and cannot be used for value setting.");

                //    //ConcreteTypeName kullanmıyoruz, ConcreteTypeName sadece ConcreteEntity sınıfından türeyenler için




                //}
            }

            //TODO: Bulamaz ise (String) enum'nin caption field'ı ile aramalı -> Reference

            base.SetValue(entity, fieldName, value, applyToEntity);
        }

    }

    public class ConcreteEnumTypeConverter : System.ComponentModel.TypeConverter, ICustomTypeConverter
    {
        static Type[] SupportedTypes = new Type[]
            {
                    typeof(int),
                    typeof(long),
                    typeof(string),
                    typeof(Enum),
            };


        public bool CanConvert(Type fromType, Type toType)
        {
            Type supportedGenTypeDef = typeof(Enumeration<>);

            if (fromType.IsGenericType)
            {
                Type fromGenTypeDef = fromType.GetGenericTypeDefinition();

                if (fromGenTypeDef == supportedGenTypeDef)
                {
                    if (Array.IndexOf(SupportedTypes, toType) >= 0)
                        return true;

                    //Enumeration<Abc> to Abc
                    var genA = fromType.GetGenericArguments().FirstOrDefault();

                    if (toType.IsEnum && genA != null && genA == toType)
                        return true;
                }


                Type toGenTypeDef = toType.GetGenericTypeDefinition();
                if (toGenTypeDef == supportedGenTypeDef)
                {
                    if (Array.IndexOf(SupportedTypes, fromType) >= 0)
                        return true;

                    //Abc to Enumeration<Abc>
                    var genA = toType.GetGenericArguments().FirstOrDefault();

                    if (toType.IsEnum && genA != null && genA == fromType)
                        return true;
                }
            }

            return false;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            if (Array.IndexOf(SupportedTypes, sourceType) >= 0)
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value)
        {
            //Convert to switch-case if more types are supported
            if (value == null) return null;

            Type propertyType = context?.PropertyDescriptor?.PropertyType;
            if (propertyType == null || !propertyType.IsGenericType || propertyType.GetGenericTypeDefinition() != typeof(Enumeration<>))
                return base.ConvertFrom(context, culture, value);

            Type enumType = propertyType.GetGenericArguments()[0];
            if (enumType == null) return null;

            switch (value)
            {
                case Enum @enum:
                    if (@enum.GetType() == enumType)
                        return @enum;
                    break;
                case int:
                case uint:
                case long:
                case ulong:
                case short:
                case ushort:
                case byte:
                    return (Enumeration)Enum.ToObject(enumType, value);
                case string strVal:
                    if (Enum.TryParse(enumType, strVal, true, out object enumValue))
                    {
                        return (Enumeration)Enum.ToObject(enumType, enumValue);
                    }
                    break;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType)
        {
            if (Array.IndexOf(SupportedTypes, destinationType) >= 0)
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        //Create ConvertTo method using switch-case if more types are supported
        public override object? ConvertTo(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object? value, Type destinationType)
        {
            if (value == null) return null;


            switch (value)
            {
                case Enumeration enumBase:
                    switch (destinationType)
                    {
                        case Type t when t.IsSupportTo<Enumeration>():
                            return enumBase;
                        case Type t when t.IsSupportTo<Enum>():
                            object enumRes = Enum.ToObject(destinationType, enumBase.Value);
                            return enumRes;
                        case Type t when t == typeof(int):
                            return Convert.ToInt32(enumBase.Value);
                        case Type t when t == typeof(long):
                            return Convert.ToInt64(enumBase.Value);
                        case Type t when t == typeof(string):
                            return enumBase.ToString();
                    }
                    break;
                case Enum @enum:
                    switch (destinationType)
                    {
                        case Type t when t.IsSupportTo<Enumeration>():
                            Enumeration enobject = (Enumeration)Activator.CreateInstance(destinationType);
                            enobject.SetValuePremature(value);
                            return enobject;
                        case Type t when t == typeof(int):
                            return Convert.ToInt32(@enum);
                        case Type t when t == typeof(long):
                            return Convert.ToInt64(@enum);
                        case Type t when t == typeof(string):
                            return @enum.ToString();
                    }
                    break;
            }


            return base.ConvertTo(context, culture, value, destinationType);
        }


    }

    /// <summary>
    /// reference\<T\> ile benzer yapıya sahiptir
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Enumeration<T> : Enumeration where T : System.Enum
    {

        [System.Text.Json.Serialization.JsonIgnore]
        public override Type BaseType => typeof(Enumeration<>);
        public override string TypeName => "enumConcrete";

        public T EnumValue { get { return (T)Enum.ToObject(typeof(T), Convert.ToInt64(this.Value)); } }

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

        public override void SetValuePremature(object value)
        {
            switch (value)
            {
                case string strEnum:
                    if (Enum.TryParse(typeof(T), strEnum, true, out object enumValue))
                    {
                        this.Value = Convert.ToInt32(enumValue).As<long>();
                        return;
                    }
                    break;
                case int:
                case uint:
                case long:
                case ulong:
                case short:
                case ushort:
                case byte:
                    this.Value = Convert.ToInt64(value);
                    return;
            }

            base.SetValuePremature(value);
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

        public static implicit operator long(Enumeration<T> reference)
        {
            if (reference == null)
                return 0;

            if (0L == reference.Value)
                return 0;

            return reference.TargetId;
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
