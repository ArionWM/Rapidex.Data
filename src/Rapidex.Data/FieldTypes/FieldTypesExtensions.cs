using Rapidex.Data;
using Rapidex.Data.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Xml.Linq;

namespace Rapidex.Data
{
    public static class FieldTypesExtensions
    {
        public static IDataType CloneFor(this IDataType dt, IEntity entity, IDbFieldMetadata fm)
        {
            IDataType clone = (IDataType)dt.Clone();
            clone.SetupInstance(entity, fm);
            return clone;
        }

        //public static void Add(this RelationOne2N rel, IEnumerable<IEntity> entities)
        //{
        //    foreach (var entity in entities)
        //    {
        //        rel.Add(entity);
        //    }
        //}

        public static BlobRecord Set(this ILazyBlob blob, byte[] value, string name, string contentType)
        {
            using (var ms = value == null ? new MemoryStream() : new MemoryStream(value))
            {
                return blob.SetContent(ms, name, contentType);
            }
        }

        public static BlobRecord Set(this ILazyBlob blob, Stream stream, string name, string contentType)
        {
            return blob.SetContent(stream, name, contentType);
        }

        public static void SetEmpty(this ILazyBlob blob)
        {
            blob.SetContent(null, null, null);
        }
    }
}
