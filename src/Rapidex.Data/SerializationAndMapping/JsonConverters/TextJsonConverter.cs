using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rapidex.Data.SerializationAndMapping.JsonConverters;
internal class TextJsonConverter : JsonConverter<Text>
{
    public override Text? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, Text value, JsonSerializerOptions options)
    {
        if (value.IsNullOrEmpty())
        {
            writer.WriteNullValue();
            return;
        }
        writer.WriteStartObject();
        writer.WriteString("value", value.Value);
        writer.WriteString("type", value.Type.ToString());
        writer.WriteEndObject();
    }

    public static void Register()
    {
        TextJsonConverter conv = new TextJsonConverter();
        JsonHelper.Register(conv);
    }
}
