using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rapidex.Data.SerializationAndMapping.JsonConverters;
internal class EnumerationJsonConverter : JsonConverter<Enumeration>
{
    public override Enumeration? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, Enumeration value, JsonSerializerOptions options)
    {
        JsonConvertersHelper.WriteReference(writer, value, options);

    }

    public static void Register()
    {
        EnumerationJsonConverter conv = new EnumerationJsonConverter();
        JsonHelper.Register(conv);
    }
}
