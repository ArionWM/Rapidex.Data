
using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data
{
    public class Text : BasicBaseDataType<string, Text>
    {

        public class TextDbFieldMetadata : DbFieldMetadata
        {
            public TextDbFieldMetadata()
            {

            }

            public TextDbFieldMetadata(IDbFieldMetadata source)
            {
                this.Name = source.Name;
                this.Caption = source.Caption;
                this.Type = typeof(Text);
                this.BaseType = typeof(string);
                this.DbType = DbFieldType.String;
                this.DbProperties.Length = -1; //Max
                //this.DbProperties.Scale = 8;
                this.SkipDbVersioning = source.SkipDbVersioning;
            }

            public override void Setup(IDbEntityMetadata parentMetadata)
            {
                //Entity'e bir de text type sütunu ekliyoruz.
                base.Setup(parentMetadata);

                DbFieldMetadata typeCm = new DbFieldMetadata();
                typeCm.Name = Text.GetTypeFieldName(this);
                typeCm.Caption = this.Caption;
                typeCm.Type = typeof(string);
                typeCm.BaseType = typeof(string);
                typeCm.DbType = DbFieldType.String;
                typeCm.SkipDbVersioning = this.SkipDbVersioning;
                typeCm.DbProperties.Length = 10;
                typeCm.Invisible = true;

                parentMetadata.AddFieldIfNotExist(typeCm);
            }
        }


        public override string TypeName => "text";

        private string prematureTextType { get; set; }

        public string Type
        {
            get
            {
                return this.GetTextType();
            }
            set
            {
                this.SetTextType(value);
            }
        }

        public override IDbFieldMetadata SetupMetadata(IDbEntityMetadataManager containerManager, IDbFieldMetadata self, ObjDictionary values)
        {
            //self.DbProperties.Length = -1; //Max
            //return base.SetupMetadata(containerManager, self, values);

            return new TextDbFieldMetadata(self);
        }

        public override IValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        protected string GetTextType()
        {
            IDataType _this = this;
            if (_this.FieldMetadata == null)
                return this.prematureTextType;

            string typeFieldName = GetTypeFieldName(this);

            string typeValue = _this.Parent.NotNull().GetValue<string>(typeFieldName);
            return typeValue;
        }

        protected void SetTextType(string TextCode)
        {
            IDataType _this = this;
            if (_this.FieldMetadata == null)
            {
                this.prematureTextType = TextCode;
            }
            else
            {
                this.prematureTextType = null;
                string typeFieldName = GetTypeFieldName(this);
                _this.Parent.NotNull().SetValue(typeFieldName, TextCode);
            }
        }

        public override object Clone()
        {
            Text clone = new Text();
            clone.Value = this.Value;
            return clone;
        }
        
        public static implicit operator Text(string value)
        {
            return new Text() { Value = value };
        }

        public static implicit operator string(Text value)
        {
            return value?.Value;
        }

        protected static string GetTypeFieldName(Text text)
        {
            IDataType _this = text;
            return GetTypeFieldName(_this.FieldMetadata);
        }

        protected static string GetTypeFieldName(IDbFieldMetadata TextFm)
        {
            TextFm.NotNull();

            return TextFm.Name + "Type";
        }
    }
}
