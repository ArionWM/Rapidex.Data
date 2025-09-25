using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rapidex.Data.SerializationAndMapping.JsonConverters;
internal class RelationN2NJsonConverter : JsonConverter<RelationN2N>
{
    public override RelationN2N? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, RelationN2N value, JsonSerializerOptions options)
    {
        JsonConvertersHelper.WriteRelation(writer, value, options);
    }

    public static void Register()
    {
        RelationN2NJsonConverter conv = new RelationN2NJsonConverter();
        JsonHelper.Register(conv);
    }
}
