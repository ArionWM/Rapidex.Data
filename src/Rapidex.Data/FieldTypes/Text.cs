
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Rapidex.Data
{
    public class Text : BasicBaseDataType<string>, IEmptyCheckObject
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

        private TextType PrematureTextType { get; set; }

        public TextType Type
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

        bool IEmptyCheckObject.IsEmpty => this.Value.IsNullOrEmpty();

        public override IDbFieldMetadata SetupMetadata(IDbMetadataContainer container, IDbFieldMetadata self, ObjDictionary values)
        {
            //self.DbProperties.Length = -1; //Max
            //return base.SetupMetadata(container, self, values);

            return new TextDbFieldMetadata(self);
        }

        public override IValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        protected TextType GetTextType()
        {
            IDataType _this = this;
            if (_this.FieldMetadata == null)
                return this.PrematureTextType;

            string typeFieldName = GetTypeFieldName(this);

            string typeValue = _this.Parent.NotNull().GetValue<string>(typeFieldName);
            if (typeValue.IsNOTNullOrEmpty() && Enum.TryParse<TextType>(typeValue, out TextType textType))
            {
                return textType;
            }

            return TextType.Plain;
        }

        protected void SetTextType(TextType textType)
        {
            IDataType _this = this;
            if (_this.FieldMetadata == null)
            {
                this.PrematureTextType = textType;
            }
            else
            {
                this.PrematureTextType =  TextType.Plain;
                string typeFieldName = GetTypeFieldName(this);
                _this.Parent.NotNull().SetValue(typeFieldName, textType.ToString());
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
