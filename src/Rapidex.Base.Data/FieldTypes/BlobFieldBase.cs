using Rapidex.Data.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Text;
using YamlDotNet.Core.Tokens;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Rapidex.Data
{
    /// <summary>
    /// Resim gibi içeriği byte[] olarak tutulacak ve BlobRepository'e kaydedilecek veri tipleri için kullanılır.
    /// Bu veri tipi, BlobRepository'de (BlobRecord) ile kaydedilen veriye referans verir (id'si ile).
    /// Ancak BlobRepository'ler farklı türlerde olabilir. Bu neden ile bu veri tipi, sistemden aktif repository'yi alır.
    /// </summary>
    /// <typeparam name="TThis"></typeparam>
    public abstract class BlobFieldBase<TThis> : ReferenceBase<TThis>, IDataType<long>, ILazyBlob
        where TThis : BlobFieldBase<TThis>, new()
    {
        public override bool? SkipDirectLoad => false;



        public new virtual StreamContent GetContent()
        {
            if (this.TargetId.IsEmptyId())
                return new StreamContent();

            IDataType _this = this;
            IEntity _parent = this.GetParent().NotNull("Parent not set");
            IResult<StreamContent> fsr = _parent._Scope.Blobs.Get(this.TargetId);
            return fsr.Content;
        }

        public override IDbFieldMetadata SetupMetadata(IDbMetadataContainer container, IDbFieldMetadata self, ObjDictionary values)
        {
            values.Set("reference", typeof(BlobRecord).Name);
            IDbFieldMetadata fm = base.SetupMetadata(container, self, values);
            fm.Type = this.GetType();
            return fm;
        }

        public override void SetValue(IEntity entity, string fieldName, object value, bool applyToEntity)
        {
            //Mime type'ı bilsek iyi olur..
            object _value = value;

            if (value is string || value is IDictionary<string, object> dict)
            {
                this.SetWithSerializationData(fieldName, value);
                return;
            }

            if (_value is byte[] byteArray)
            {
                string _contentType = "application/octet-stream";

                this.Set(byteArray, "unknown", _contentType);
                value = this.TargetId;
            }

            base.SetValue(entity, fieldName, value, applyToEntity);
        }

        public override IPartialEntity[] SetValue(IEntity entity, string fieldName, ObjDictionary value)
        {
            throw new NotImplementedException();
        }

        public virtual BlobRecord SetContent(Stream stream, string name, string contentType)
        {
            IDataType _this = this;
            IEntity _parent = this.GetParent().NotNull("Parent not set");
            BlobRecord blobRec = null;
            //Doğrudan kaydediliyor !!!
            long blobId = this.TargetId;
            if (stream == null)
            {
                if (blobId > 0)
                {
                    //Sil ..
                    _parent._Scope.Blobs.Delete(blobId);
                }
                this.TargetId = DatabaseConstants.DEFAULT_EMPTY_ID;
            }
            else
            {
                //güncelle ya da ekle
                IResult<BlobRecord> bres = _parent._Scope.Blobs.Set(stream, name, contentType, this.TargetId)
                    ;
                blobRec = bres.Content;
                this.TargetId = blobRec.Id;
            }

            return blobRec;
        }

        public virtual BlobRecord SetContent(byte[] data, string name, string contentType)
        {
            using MemoryStream memoryStream = new MemoryStream(data);
            return this.SetContent(memoryStream, name, contentType);
        }

        public override object GetSerializationData(EntitySerializationOptions options)
        {
            if (this.TargetId.IsEmptyId())
                return null;

            IDataType _this = this;
            string fieldName = _this.FieldMetadata.Name;
            IEntity owner = _this.Parent;
            IDbEntityMetadata em = owner.GetMetadata();
            IDbSchemaScope dbScope = owner._Scope;

            string nav = GetFileDescriptorIdForFieldFile(owner, em, fieldName);
            //$"{dbScope.SchemaName}.{em.Name}.{owner.GetId()}.{fieldName}";

            ObjDictionary data = (ObjDictionary)base.GetSerializationData(options);
            data["id"] = nav;

            return data;
        }

        public override object SetWithSerializationData(string memberName, object value)
        {
            //Bkz: BlobsAndFiles.md

            object _value = value;
            string _contentType = "application/octet-stream";
            string _contentStr = null;
            string _name = "unknown";

            if (value is string str)
            {
                str = str.Trim();

                if (str.IsNullOrEmpty())
                {
                    if (this.GetParent().IsNOTNullOrEmpty())
                        this.SetEmpty();
                    this.SetValue(null, memberName, null);
                    return null;
                }

                if (str.StartsWith('{'))
                {
                    _value = str.FromJson<Dictionary<string, object>>();
                }
                else
                {
                    if (str.IsBase64() || str.StartsWith("data:"))
                    {
                        _contentStr = str;
                    }
                    else
                    {
                        //Bu bir blobId'mi?
                        BlobInfo bInfo = BlobRecordHelper.ParseBlobId(null, str);
                        this.TargetId = bInfo.Id.As<long>();
                        if (this.TargetId.IsPersistedRecordId())
                        {
                            return null; //??
                        }
                        else
                        {
                            return null; //??
                        }
                    }

                    //TODO: blobId çalışılacak burası düzenlenecek Serialization_02_RawJsonDeserialization dan gelen veri düzenlenecek ..
                }
            }

            if (value is IDictionary<string, object> dict)
            {
                _contentType = dict.Get("contentType").As<string>();
                _contentStr = dict.Get("content").As<string>();
                _name = dict.Get("name").As<string>();
            }

            if (_contentStr.StartsWith("data:"))
            {
                var parts = _contentStr.Split(';');
                _contentType = parts[0].Split(':')[1];
                _contentStr = parts[1].Split(',')[1];
            }

            byte[] _contentByte = _contentStr.IsNullOrEmpty()
                ? new byte[0]
                : Convert.FromBase64String(_contentStr);

            BlobRecord brec = this.Set(_contentByte, _name, _contentType);

            return brec.CreateArray();
            //return base.SetWithSerializationData(memberName, value);
        }

        public override object Clone()
        {
            TThis clone = new TThis();
            clone.TargetId = this.TargetId;
            return clone;
        }

        public static string GetFileDescriptorIdForFieldFile(IEntity owner, IDbEntityMetadata em, string fieldName)
        {
            IDbSchemaScope dbScope = owner._Scope;
            string nav = $"{dbScope.SchemaName}.{em.Name}.{owner.GetId()}.fields.{fieldName}";
            return nav;
        }

    }
}
