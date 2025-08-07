using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace Rapidex.Data
{
    [JsonDerivedBase]
    public abstract class BasicBaseDataType : IDataType
    {
        IEntity IDataType.Parent { get; set; }
        IDbFieldMetadata IDataType.FieldMetadata { get; set; }

        public virtual bool? SkipDirectLoad { get; set; }
        public virtual bool? SkipDirectSet { get; set; }
        public virtual bool? SkipDbVersioning { get; set; }

        public abstract string TypeName { get; }
        public abstract Type BaseType { get; }



        public abstract IValidationResult Validate();

        public abstract object Clone();

        public abstract object GetValueUpper(IEntity entity, string fieldName);

        public abstract object GetValueLower();

        public abstract void SetValue(IEntity entity, string fieldName, object value, bool applyToEntity);
        public abstract IPartialEntity[] SetValue(IEntity entity, string fieldName, ObjDictionary value);
        public abstract void SetValuePremature(object value);

        public abstract object GetSerializationData(EntitySerializationOptions options);
        public abstract object SetWithSerializationData(string memberName, object value);

        protected virtual IEntity GetParent()
        {
            return ((IDataType)this).Parent;
        }

        public virtual void SetupInstance(IEntity entity, IDbFieldMetadata fm)
        {
            IDataType _this = this;
            _this.Parent = entity;
            _this.FieldMetadata = fm;

        }

        public virtual IDbFieldMetadata SetupMetadata(IDbEntityMetadataManager containerManager, IDbFieldMetadata self, ObjDictionary values)
        {
            if (self.BaseType != null && !self.DbType.HasValue)
            {
                var vt = DataDbTypeConverter.GetDbType(self.BaseType);
                self.DbType = vt.DbType;
            }

            return self;
        }

        public static object GetValueUpperSt(IEntity entity, string fieldName)
        {
            object value = entity.GetValue(fieldName);

            if (entity is IPartialEntity && value == null)
            {
                return null;
            }

            if (value is IDataType dt)
            {
                return dt;
            }

            //EnsureXXX sonucunda buraya gelmemesi gerekir. Entity içerisinde hep Upper value olmalı
            var em = entity.GetMetadata();
            var fm = em.Fields.Get(fieldName);

            object evalue = EntityMapper.EnsureValueType(fm, entity, value);
            return evalue;
        }


    }

    //[TypeConverter(typeof(BbdTypeConverter<>))]
    public abstract class BasicBaseDataType<TBasicDataType, TThis>
        : BasicBaseDataType, IDataType<TBasicDataType>
        where TThis : BasicBaseDataType<TBasicDataType, TThis>, new()
    {
        public class BasicBaseDataTypeConverter : ConverterBase
        {
            public override Type FromType => typeof(TThis);

            public override Type ToType => typeof(TBasicDataType);

            public override object Convert(object from, Type toType)
            {
                TThis _from = (TThis)from;
                return _from.Value.As(toType);
            }

            public override bool TryConvert(object from, Type toType, out object to)
            {
                TThis _from = (TThis)from;
                if (_from.Value == null)
                {
                    to = null;
                    return true;
                }
                if (_from.Value.GetType() == toType)
                {
                    to = _from.Value;
                    return true;
                }
                return Common.Converter.TryConvert(_from.Value, toType, out to);
            }
        }


        public TBasicDataType Value { get; set; }

        public override Type BaseType => typeof(TBasicDataType);



        public override object GetValueUpper(IEntity entity, string fieldName)
        {
            return GetValueUpperSt(entity, fieldName);
        }

        public override object GetValueLower()
        {
            return this.Value;
        }

        public override void SetValue(IEntity entity, string fieldName, object value, bool applyToEntity)
        {
            if (value == null)
            {
                this.Value = default(TBasicDataType);
                if (applyToEntity)
                    entity.SetValue(fieldName, this);
                return;
            }

            if (value is TBasicDataType typedValue)
            {
                this.Value = typedValue;
                if (applyToEntity)
                    entity.SetValue(fieldName, this);
                return;
            }

            object ovalue;
            if(Common.Converter.TryConvert(value, typeof(TBasicDataType), out ovalue))
            {
                this.Value = (TBasicDataType)ovalue;
                if (applyToEntity)
                    entity.SetValue(fieldName, this);

                return;
            }

            throw new InvalidOperationException($"Value '{value}' is not of type " + typeof(TBasicDataType).Name);

        }





        public override void SetValuePremature(object value)
        {
            this.Value = value.As<TBasicDataType>();
        }

        public override IDbFieldMetadata SetupMetadata(IDbEntityMetadataManager containerManager, IDbFieldMetadata self, ObjDictionary values)
        {
            BasicBaseDataTypeConverter converter = new BasicBaseDataTypeConverter();
            Common.Converter.Register(converter);

            self.BaseType = typeof(TBasicDataType);
            return base.SetupMetadata(containerManager, self, values);
        }

        public override object GetSerializationData(EntitySerializationOptions options)
        {
            return this.Value;
        }

        public override object SetWithSerializationData(string memberName, object value)
        {
            value.ShouldSupportTo<TBasicDataType>();
            this.Value = value.As<TBasicDataType>();
            return this;
        }

        public override IPartialEntity[] SetValue(IEntity entity, string fieldName, ObjDictionary value)
        {
            throw new NotSupportedException($"This DataType contains basic data set. Not support ObjDictionary");
        }
    }


}
