using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;

public struct EntitySerializationOptions
{
    public static EntitySerializationOptions Default { get; }

    public bool IncludeBaseFields { get; set; } = true;
    public bool IncludeNestedEntities { get; set; } = false;
    public bool IncludeTypeName { get; set; } = false;
    public bool IncludePictureField { get; set; } = false;

    static EntitySerializationOptions()
    {
        Default = new EntitySerializationOptions()
        {
            IncludeBaseFields = true,
            IncludeNestedEntities = false,
            IncludeTypeName = false
        };
    }

    public EntitySerializationOptions()
    {

    }
}
