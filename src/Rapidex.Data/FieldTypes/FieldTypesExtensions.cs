using MimeTypes;
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



        public static void Set(this ILazyBlob blob, byte[] value, string name, string contentType)
        {
            blob.SetContent(value, name, contentType);
        }

        public static void LoadFromFile(this ILazyBlob blob, string filePath, string contentType = null)
        {
            if (contentType.IsNullOrEmpty())
            {
                string ext = Path.GetExtension(filePath).ToLowerInvariant().TrimStart('.');
                contentType = MimeTypeMap.GetMimeType(ext);
            }

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                blob.SetContent(data, Path.GetFileName(filePath), contentType);
            }
        }


        public static void SetEmpty(this ILazyBlob blob)
        {
            blob.SetContent(null, null, null);
        }
    }
}
