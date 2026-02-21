using Rapidex.Data.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
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

    protected ByteArrayContent UnAttachedData { get; set; }
    protected BlobRecord AttachedData { get; set; }



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

    //public virtual BlobRecord SetContent(Stream stream, string name, string contentType)
    //{
    //    IDataType _this = this;
    //    IEntity _parent = this.GetParent().NotNull("Parent not set");
    //    BlobRecord blobRec = null;



    //    //Doğrudan kaydediliyor !!!
    //    long blobId = this.TargetId;
    //    if (stream == null)
    //    {
    //        if (blobId > 0)
    //        {
    //            //Sil ..
    //            _parent._Schema.Blobs.Delete(blobId);
    //        }
    //        this.TargetId = DatabaseConstants.DEFAULT_EMPTY_ID;
    //    }
    //    else
    //    {
    //        //güncelle ya da ekle
    //        IResult<BlobRecord> bres = _parent._Schema.Blobs.Set(stream, name, contentType, this.TargetId);
    //        blobRec = bres.Content;
    //        this.TargetId = blobRec.Id;
    //    }

    //    return blobRec;
    //}

    public new virtual ByteArrayContent GetContent()
    {
        if (this.UnAttachedData.IsNOTNullOrEmpty())
            return this.UnAttachedData;

        if (this.TargetId.IsEmptyId())
            return null;

        IEntity _parent = this.GetParent()
            .NotNull("Parent not set");

        if (!_parent.IsAttached())
            throw new InvalidOperationException("Cannot get content of a blob field when the parent entity is not attached to a schema.");

        this.AttachedData = this.AttachedData ?? _parent._Schema.Find<BlobRecord>(this.TargetId);
        if (this.AttachedData == null)
            throw new InvalidOperationException($"Blob record with id {this.TargetId} not found in the schema {_parent._Schema.SchemaName}");

        ByteArrayContent content = this.AttachedData.GetByteArrayContent();

        return content;
    }

    public virtual void SetContent(byte[] data, string name, string contentType)
    {
        this.UnAttachedData = new ByteArrayContent(name, contentType, data);
        //this.AttachedData = null;

        /*
        Dolu idim (id var), boşaldım -> 
        UnAttachedData var, UnAttachedData.Data null, attachedData var / ya da yok (önemsiz), id var
        BlobRecord sil

        Dolu idim, değiştim (id var) -> 
        UnAttachedData var, attachedData var / ya da yok (önemsiz), id var
        BlobRecord yükle / güncelle / kaydet

        Boş idim, doldum (id var)-> 
        UnAttachedData var, attachedData var / ya da yok (önemsiz), id var
        BlobRecord yüklenmemiş, yükle ve güncelle

        Boş idim, doldum (id yok)-> 
        UnAttachedData var, attachedData yok, id yok
        BlobRecord ekle
         */
    }

    public override void PrepareCommit(IEntity entity, IDbDataModificationScope parentDms, DataUpdateType updateType)
    {
        base.PrepareCommit(entity, parentDms, updateType);

        IEntity _parent = this.GetParent()
            .NotNull("Parent not set");

        if (!_parent.IsAttached())
            throw new InvalidOperationException("Cannot get content of a blob field when the parent entity is not attached to a schema.");

        var schemaScope = _parent._Schema;

        switch (updateType)
        {
            case DataUpdateType.Update:
                if (this.UnAttachedData != null)
                {
                    if (this.UnAttachedData.Data.IsNullOrEmpty())
                    {
                        //Delete blob if exists and set TargetId to empty
                        if (this.TargetId.IsPersistedRecordId())
                        {
                            BlobRecord brec = schemaScope.Find<BlobRecord>(this.TargetId);
                            brec.NotNull();

                            parentDms.Delete(brec);
                            this.TargetId = DatabaseConstants.DEFAULT_EMPTY_ID;
                        }
                    }
                    else
                    {
                        BlobRecord brec;
                        if (this.TargetId.IsEmptyId())
                        {
                            brec = parentDms.New<BlobRecord>();
                            this.TargetId = brec.Id;
                        }
                        else
                        {
                            brec = schemaScope.Find<BlobRecord>(this.TargetId);
                            brec.NotNull();
                        }

                        brec.Data = this.UnAttachedData.Data;
                        brec.ContentType = this.UnAttachedData.ContentType;
                        brec.Name = this.UnAttachedData.Name;
                        parentDms.Save(brec);
                    }
                }
                break;
            case DataUpdateType.Delete:
                if (this.TargetId.IsPersistedRecordId())
                {
                    parentDms.GetQuery<BlobRecord>()
                        .EnterUpdateMode()
                        .IdEq(this.TargetId)
                        .Delete(parentDms);
                }
                break;
        }

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
