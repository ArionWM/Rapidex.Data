using Rapidex.Data.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Text;
using YamlDotNet.Core.Tokens;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Rapidex.Data;

/// <summary>
/// Resim gibi içeriği byte[] olarak tutulacak ve BlobRepository'e kaydedilecek veri tipleri için kullanılır.
/// Bu veri tipi, BlobRepository'de (BlobRecord) ile kaydedilen veriye referans verir (id'si ile).
/// Ancak BlobRepository'ler farklı türlerde olabilir. Bu neden ile bu veri tipi, sistemden aktif repository'yi alır.
/// </summary>
/// <typeparam name="TThis"></typeparam>
public abstract class BlobFieldBase<TThis> : ReferenceBase, IDataType<long>, ILazyBlob
    where TThis : BlobFieldBase<TThis>, new()
{
    public override bool? SkipDirectLoad => false;



    public new virtual StreamContent GetContent()
    {
        if (this.TargetId.IsEmptyId())
            return new StreamContent();

        IDataType _this = this;
        IEntity _parent = this.GetParent().NotNull("Parent not set");
        IResult<StreamContent> fsr = _parent._Schema.Blobs.Get(this.TargetId);
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
            throw new NotImplementedException();
            //this.SetWithSerializationData(fieldName, value);
            //return;
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
                _parent._Schema.Blobs.Delete(blobId);
            }
            this.TargetId = DatabaseConstants.DEFAULT_EMPTY_ID;
        }
        else
        {
            //güncelle ya da ekle
            IResult<BlobRecord> bres = _parent._Schema.Blobs.Set(stream, name, contentType, this.TargetId)
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

    public override object Clone()
    {
        TThis clone = new TThis();
        clone.TargetId = this.TargetId;
        return clone;
    }

    

}


public static class BlobFieldHelper
{
    public static string GetFileDescriptorIdForFieldFile(IEntity owner, IDbEntityMetadata em, string fieldName)
    {
        IDbSchemaScope dbScope = owner._Schema;
        string nav = $"{dbScope.SchemaName}.{em.Name}.{owner.GetId()}.fields.{fieldName}";
        return nav;
    }
}
