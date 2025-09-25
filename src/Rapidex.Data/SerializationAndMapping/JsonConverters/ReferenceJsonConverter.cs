using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rapidex.Data.SerializationAndMapping.JsonConverters;
internal class ReferenceJsonConverter<T> : JsonConverter<ReferenceBase<T>>
    where T : ReferenceBase<T>, new()
{
    public override ReferenceBase<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, ReferenceBase<T> value, JsonSerializerOptions options)
    {
        JsonConvertersHelper.WriteReference(writer, value, options);
    }

    public static void Register()
    {
        ReferenceJsonConverter<T> conv = new ReferenceJsonConverter<T>();
        JsonHelper.Register(conv);
    }
}

internal class EntityReferenceJsonConverter : ReferenceJsonConverter<Reference<IEntity>>
{

    public static new void Register()
    {
        EntityReferenceJsonConverter conv = new EntityReferenceJsonConverter();
        JsonHelper.Register(conv);
    }
}
