using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Entities
{
    public class BlobRecord : DbConcreteEntityBase
    {
        public string Name { get; set; }
        public string ContentType { get; set; }
        public string ContentChecksum { get; set; }
        public long Length { get; set; }
        public byte[] Data { get; set; }
    }

    public struct BlobInfo
    {
        public static BlobInfo Empty => new BlobInfo()
        {
            Id = null,
            OwnerName = null,
            FieldOrFileName = null,
            Location = BlobLocation.Field
        };

        public enum BlobLocation
        {
            EntityList,
            Field
        }

        public IEntity Owner { get; set; }
        public string OwnerName { get; set; }
        public string Id { get; set; }

        public BlobLocation Location { get; set; }

        public string SchemaName { get; set; }
        public string FieldOrFileName { get; set; }




        public string ContentType { get; set; }
        public string Tag { get; set; }



    }

    public static class BlobRecordHelper
    {
        //See: GetFileDescriptorIdForFieldFile

        public static async Task<IEntity> GetBlobRecordOwner(IDbSchemaScope dbScope, string blobIdOrInfo)
        {

            BlobInfo binfo = ParseBlobId(null, blobIdOrInfo);
            if (binfo.OwnerName.IsNullOrEmpty())
                throw new InvalidOperationException($"Blob info invalid: {blobIdOrInfo}");

            dbScope.NotNull();
            var parentEntity = await dbScope.Find(binfo.OwnerName, binfo.Id.As<long>());
            return parentEntity;

            //var parts = blobIdOrInfo.Replace('/', '.').Split('.');
            //if (parts.Length == 4)
            //{
            //    //"schema.parentEntityName.parentEntityId.fieldName"
            //    //$"{dbScope.SchemaName}.{em.Name}.{parent.GetId()}.{fieldName}";
            //    string schemaName = parts[0];
            //    string parentEntityName = parts[1];
            //    long parentEntityId = parts[2].As<long>();
            //    string fieldName = parts[3];
            //    var parentEntity = dbScope.Find(parentEntityName, parentEntityId);
            //    return parentEntity;
            //}

            //throw new InvalidOperationException($"Blob info invalid: {blobIdOrInfo}");
        }

        public static BlobInfo ParseBlobId(IEntity owner, string blobIdOrInfo)
        {
            //See: BlobsAndFiles.md

            /*
             1= |Bir entity alanı içerisindeki Blob veri (dosya)|`<schemaName>.<OwnerEntityName>.<OwnerEntityId>.<FieldName>` ya da `<schemaName>.<OwnerEntityName>.<OwnerEntityId>.fields.<FieldName>`|
             2= |Bir entity'nin dosya listesi içerisindeki dosya|`<schemaName>.<OwnerEntityName>.<OwnerEntityId>.files.<fileNameAndExtension>`|

            */

            if (blobIdOrInfo.IsNullOrEmpty())
                return BlobInfo.Empty;

            BlobInfo blobInfo = new BlobInfo();
            blobInfo.Owner = owner;
            blobInfo.OwnerName = owner?._Metadata?.Name;
            var parts = blobIdOrInfo.Replace('/', '.').Split('.');

            if (parts.Length == 6)
            {
                //2 no'lu seçenek
                blobInfo.Location = BlobInfo.BlobLocation.EntityList;
                blobInfo.SchemaName = parts[0];
                blobInfo.OwnerName = parts[1];
                blobInfo.Id = parts[2];
                blobInfo.FieldOrFileName = parts[4] + '.' + parts[5];
            }

            if (parts.Length == 5)
            {
                //Base.Contact.11000.fields.Picture
                //1 no'lu seçenek
                blobInfo.Location = BlobInfo.BlobLocation.Field;
                blobInfo.SchemaName = parts[0];
                blobInfo.OwnerName = parts[1];
                blobInfo.Id = parts[2];
                blobInfo.FieldOrFileName = parts[4];
            }

            if (parts.Length == 4)
            {
                //1 no'lu seçenek
                blobInfo.Location = BlobInfo.BlobLocation.Field;
                blobInfo.SchemaName = parts[0];
                blobInfo.OwnerName = parts[1];
                blobInfo.Id = parts[2];
                blobInfo.FieldOrFileName = parts[3];
            }

            if (parts.Length == 2)
            {
                //1 no'lu seçenek
                blobInfo.Location = BlobInfo.BlobLocation.Field;
                blobInfo.Id = parts[0];
                blobInfo.FieldOrFileName = parts[1];
            }

            if (parts.Length == 1)
            {
                //1 no'lu seçenek
                blobInfo.Location = BlobInfo.BlobLocation.Field;
                blobInfo.SchemaName = owner?._SchemaName;
                blobInfo.FieldOrFileName = parts[0];
            }

            return blobInfo;
        }

        public static Task<BlobRecord> GetBlobRecord(IDbSchemaScope dbScope, IEntity parentEntity, string blobIdOrInfo)
        {
            if (long.TryParse(blobIdOrInfo, out long blobId))
            {
                return dbScope.Find<BlobRecord>(blobId);
            }

            var parts = blobIdOrInfo.Replace('/', '.').Split('.');

            BlobInfo blobInfo = ParseBlobId(parentEntity, blobIdOrInfo);


            var id = parentEntity.GetValue(blobInfo.FieldOrFileName).EnsureLowerValue().As<long>();
            if (id == 0)
            {
                throw new InvalidOperationException($"Blob id invalid");
            }

            return dbScope.Find<BlobRecord>(id);

        }
    }
}
