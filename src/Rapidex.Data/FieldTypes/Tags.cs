using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data
{
    public class Tags : BasicBaseDataType<string>
    {
        public override string TypeName => "tags";

        public override IDbFieldMetadata SetupMetadata(IDbMetadataContainer container, IDbFieldMetadata self, ObjDictionary values)
        {
            self.DbProperties.Length = -1; //Max
            return base.SetupMetadata(container, self, values);
        }

        //public override object GetSerializationData(EntitySerializationOptions options)
        //{
        //    string[] tags = this.Get();
        //    IDataType _this = this;

        //    var tagInfos = HasTags.GetTagInfo(_this.Parent._Schema, _this.FieldMetadata.ParentMetadata, tags);
        //    return tagInfos;

        //}

        public override IValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            Tags clone = new Tags();
            clone.Value = this.Value;
            return clone;
        }

        public static implicit operator Tags(string value)
        {
            return new Tags() { Value = value };
        }

        public static implicit operator string(Tags mail)
        {
            return mail?.Value;
        }

        public override void SetValue(IEntity entity, string fieldName, object value, bool applyToEntity)
        {
            if (value.IsNullOrEmpty())
                value = null;

            if (value is string)
            {
                string strVal = (string)value;
                strVal = strVal?.Trim();

                string[] tagParts = strVal.Split('|', ',', ';').DistinctWithTrimElements();
                strVal = '|' + tagParts.Select(tstr => HasTags.SplitTag(tstr).Name).Join("|") + '|';
                value = strVal;
            }

            base.SetValue(entity, fieldName, value, applyToEntity);

            IDataType _this = this;

            if (value.IsNOTNullOrEmpty())
            {
                //Job yapısına buradan verilebilir mi?
                HasTags.CheckEntityTags(_this.Parent._Schema, _this.FieldMetadata.ParentMetadata, (string)value);
            }
        }

        public override IPartialEntity[] SetValue(IEntity entity, string fieldName, ObjDictionary value)
        {


            return base.SetValue(entity, fieldName, value);
        }

        /// <summary>
        /// Tags string dizisinin başında ve sonunda '|' karakteri olmasını garanti eder
        /// </summary>
        /// <param name="tags">| ile ayrılmış tag'ler</param>
        protected void Set(string tags)
        {
            IDataType _this = this;
            this.SetValue(_this.Parent, _this.FieldMetadata.Name, tags, true);
        }

        public void Add(string tag)
        {
            string value = this.Value;
            if (value.IsNullOrEmpty())
            {
                value = tag;
            }
            else
            {
                value += "|" + tag;
            }

            this.Set(value);

            //TagRecord güncellenecek?
        }

        public void Add(string[] tags)
        {
            if (tags.IsNullOrEmpty())
            {
                return;
            }

            string value = this.Value;
            if (value.IsNOTNullOrEmpty() && !value.EndsWith('|'))
            {
                value += '|';
            }

            value += tags.Join("|");
            this.Set(value);
        }

        public void Remove(string tag)
        {
            if (this.Value.IsNullOrEmpty())
            {
                return;
            }
            List<string> tags = Value.Split('|').DistinctWithTrimElements().ToList();
            tags.Remove(tag);
            string value = tags.Join("|");
            this.Set(value);
        }

        public bool IsContains(string tag)
        {
            if (this.Value.IsNullOrEmpty())
            {
                return false;
            }

            string search = '|' + tag + '|';
            return this.Value.Contains(search);
        }

        public string[] Get()
        {
            if (this.Value.IsNullOrEmpty())
            {
                return new string[0];
            }
            return Value.Split('|').DistinctWithTrimElements().ToArray();
        }
    }
}
