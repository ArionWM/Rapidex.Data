using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rapidex.Data.SerializationAndMapping.JsonConverters;
internal class OneWayPasswordJsonConverter : JsonConverter<OneWayPassword>
{
    public override OneWayPassword? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, OneWayPassword value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(OneWayPassword.CRIPTO_TEXT_DUMMY);
    }

    public static void Register()
    {
        OneWayPasswordJsonConverter conv = new OneWayPasswordJsonConverter();
        JsonHelper.Register(conv);
    }
}
