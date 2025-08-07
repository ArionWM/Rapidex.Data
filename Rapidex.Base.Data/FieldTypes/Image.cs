
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace Rapidex.Data
{
    public class Image : BlobFieldBase<Image>, ILazy, ILazyBlob
    {
        public override string TypeName => "image";


        public string ContentType { get; set; }


        public override IValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        public override IDbFieldMetadata SetupMetadata(IDbEntityMetadataManager containerManager, IDbFieldMetadata self, ObjDictionary values)
        {
            var fm = base.SetupMetadata(containerManager, self, values);
            fm.TypeName = this.TypeName;
            return fm;
        }

        


        public static implicit operator byte[](Image value)
        {
            using StreamContent streamResult = value.GetContent();
            if (streamResult.Stream == null)
                return new byte[0];

            byte[] buffer = new byte[streamResult.Stream.Length];
            streamResult.Stream.Read(buffer, 0, buffer.Length);
            return buffer;
        }
    }
}
