using FluentAssertions;
using Rapidex;
using Rapidex.Data.Metadata.Columns;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Rapidex.Data
{
    public class DateTimeStartEnd : BasicBaseDataType
    {
        public class VirtualDateTimeStartEndDbFieldMetadata : Metadata.Columns.VirtualDbFieldMetadata
        {
            [JsonIgnore]
            [YamlIgnore]
            public IDbFieldMetadata StartFm { get; set; }

            [JsonIgnore]
            [YamlIgnore]
            public IDbFieldMetadata EndFm { get; set; }

            public VirtualDateTimeStartEndDbFieldMetadata()
            {

            }

            public VirtualDateTimeStartEndDbFieldMetadata(IDbFieldMetadata source)
            {
                this.Name = source.Name;
                this.Caption = source.Caption;
                this.Type = source.Type;
                this.TypeName = source.TypeName;
                this.BaseType = source.BaseType;
                this.DbType = DbFieldType.Binary; //Kullanılmayan değer
                this.SkipDirectLoad = true;
                this.SkipDirectSet = false;
                this.SkipDbVersioning = source.SkipDbVersioning;

                //DateTimeStartEnd _source = (DateTimeStartEnd)source;

                this.ValueSetter = (entity, fieldName, value, applyToEntity) =>
                {
                    //_source.SetValue(entity, fieldName, value);
                    source.ValueSetter(entity, fieldName, value, applyToEntity);
                };

                this.ValueGetterUpper = (entity, fieldName) =>
                {
                    //return _source.GetValueUpper(entity, fieldName);
                    return source.ValueGetterUpper(entity, fieldName);
                };

                this.ValueGetterLower = (entity, fieldName) =>
                {
                    //return _source.GetValueUpper(entity, fieldName);
                    return source.ValueGetterLower(entity, fieldName);
                };
            }

            public override void Setup(IDbEntityMetadata parentMetadata)
            {
                base.Setup(parentMetadata);

                DbFieldMetadata startFm = new DbFieldMetadata();
                startFm.Name = this.Name + "Start";
                startFm.Caption = this.Caption;
                startFm.Type = typeof(DateTimeOffset);
                startFm.BaseType = typeof(DateTimeOffset);
                startFm.DbType = DbFieldType.DateTime2;
                startFm.SkipDbVersioning = this.SkipDbVersioning;

                this.StartFm = startFm;

                parentMetadata.AddFieldIfNotExist(startFm); //AddIfNotExists

                DbFieldMetadata endFm = new DbFieldMetadata();
                endFm.Name = this.Name + "End";
                endFm.Caption = this.Caption;
                endFm.Type = typeof(DateTimeOffset);
                endFm.BaseType = typeof(DateTimeOffset);
                endFm.DbType = DbFieldType.DateTime2;
                endFm.SkipDbVersioning = this.SkipDbVersioning;

                this.EndFm = endFm;

                parentMetadata.AddFieldIfNotExist(endFm);
            }
        }

        private object prematureValue = null;

        public override string TypeName => "datetimestartend";
        public override Type BaseType => typeof(DateTimeOffset);

        public DateTimeOffset Start
        {
            get
            {
                IDataType _this = this;
                VirtualDateTimeStartEndDbFieldMetadata fm = (VirtualDateTimeStartEndDbFieldMetadata)_this.FieldMetadata;

                DateTimeOffset val = (DateTimeOffset)fm.StartFm.ValueGetterLower(_this.Parent, null);
                return val;
            }
            set
            {
                IDataType _this = this;
                VirtualDateTimeStartEndDbFieldMetadata fm = (VirtualDateTimeStartEndDbFieldMetadata)_this.FieldMetadata;
                fm.StartFm.ValueSetter(_this.Parent, null, value, true);
            }
        }

        public DateTimeOffset End
        {
            get
            {
                IDataType _this = this;
                VirtualDateTimeStartEndDbFieldMetadata fm = (VirtualDateTimeStartEndDbFieldMetadata)_this.FieldMetadata;

                DateTimeOffset val = (DateTimeOffset)fm.EndFm.ValueGetterLower(_this.Parent, null);
                return val;
            }
            set
            {
                IDataType _this = this;
                VirtualDateTimeStartEndDbFieldMetadata fm = (VirtualDateTimeStartEndDbFieldMetadata)_this.FieldMetadata;
                fm.EndFm.ValueSetter(_this.Parent, null, value, true);
            }
        }

        public override IDbFieldMetadata SetupMetadata(IDbEntityMetadataManager containerManager, IDbFieldMetadata self, ObjDictionary values)
        {
            VirtualDateTimeStartEndDbFieldMetadata dm = new VirtualDateTimeStartEndDbFieldMetadata(self);
            return dm;
        }

        public override void SetupInstance(IEntity entity, IDbFieldMetadata fm)
        {
            base.SetupInstance(entity, fm);

            if (this.prematureValue.IsNOTNullOrEmpty())
            {
                this.SetValue(entity, fm.Name, this.prematureValue, true);
                this.prematureValue = null;
            }
        }

        public override IValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            DateTimeStartEnd dt = new DateTimeStartEnd();
            return dt;
        }


        public override object GetValueUpper(IEntity entity, string fieldName)
        {
            return GetValueUpperSt(entity, fieldName);
        }

        public override object GetValueLower()
        {
            object sData = this.GetSerializationData(EntitySerializationOptions.Default);

            return sData.ToJson();
        }

        public override void SetValue(IEntity entity, string fieldName, object value, bool applyToEntity)
        {
            object _value = value;

            string startFieldName = fieldName + "Start";
            string endFieldName = fieldName + "End";

            if(value.IsNullOrEmpty())
            {
                entity.SetValue(startFieldName, null);
                entity.SetValue(endFieldName, null);
                return;
            }

            if (value is string str)
            {
                _value = str.FromJson<Dictionary<string, object>>();
            }

            if (_value is IDictionary<string, object> dict)
            {
                DateTimeOffset? start = dict.Get("start", false).As<DateTimeOffset?>();
                DateTimeOffset? end = dict.Get("end", false).As<DateTimeOffset?>();

                if (start.HasValue && start.Value == DateTimeOffset.MinValue)
                    start = null;

                if (end.HasValue && end.Value == DateTimeOffset.MinValue)
                    end = null;

                entity.SetValue(startFieldName, start);
                entity.SetValue(endFieldName, end);
                return;
            }

            throw new NotSupportedException($"Unknown type: {value?.GetType()?.Name}");
        }

        public override IPartialEntity[] SetValue(IEntity entity, string fieldName, ObjDictionary dict)
        {
            DateTimeOffset start = dict.Get("start", false).As<DateTimeOffset>();
            DateTimeOffset end = dict.Get("end", false).As<DateTimeOffset>();
            entity.SetValue(fieldName + "Start", start == DateTimeOffset.MinValue ? null : start);
            entity.SetValue(fieldName + "End", end == DateTimeOffset.MinValue ? null : end);

            return null;
        }

        public override void SetValuePremature(object value)
        {
            this.prematureValue = value;
        }

        public override object GetSerializationData(EntitySerializationOptions options)
        {
            IDataType _this = this;
            IDbFieldMetadata fm = _this?.FieldMetadata;
            string startFieldName = fm?.Name + "Start";
            string endFieldName = fm?.Name + "End";

            DateTimeOffset startTime = _this.Parent.NotNull().GetValue<DateTimeOffset>(startFieldName);
            DateTimeOffset endTime = _this.Parent.NotNull().GetValue<DateTimeOffset>(endFieldName);

            ObjDictionary keyValuePairs = new ObjDictionary();
            keyValuePairs["start"] = startTime == DateTimeOffset.MinValue ? null : startTime;
            keyValuePairs["end"] = endTime == DateTimeOffset.MinValue ? null : endTime;
            return keyValuePairs;
        }

        public override object SetWithSerializationData(string memberName, object value)
        {
            if (this.GetParent() == null || memberName.IsNullOrEmpty())
            {
                this.SetValuePremature(value);
                return this;
            }

            if (value is IDictionary<string, object> dict)
            {
                this.SetValue(GetParent(), memberName, dict, true);
                return this;
            }
            else
                throw new InvalidOperationException($"Value '{value}' is not of type " + typeof(IDictionary<string, object>).Name);

        }
    }
}
