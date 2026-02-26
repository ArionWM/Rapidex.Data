
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace Rapidex.Data;

public class Image : BlobFieldBase<Image>, ILazy, ILazyBlob //TODO: Image -> Blob
{
    public override string TypeName => "image";


    public string ContentType { get; set; }


    public override IValidationResult Validate()
    {
        throw new NotImplementedException();
    }

    public override IDbFieldMetadata SetupMetadata(IDbMetadataContainer container, IDbFieldMetadata self, ObjDictionary values)
    {
        var fm = base.SetupMetadata(container, self, values);
        fm.TypeName = this.TypeName;
        return fm;
    }

    public static implicit operator byte[](Image value)
    {
        ByteArrayContent streamResult = value.GetContent();
        return streamResult?.Data;
    }
}
