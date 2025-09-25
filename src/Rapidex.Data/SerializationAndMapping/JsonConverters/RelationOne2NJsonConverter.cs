using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rapidex.Data.SerializationAndMapping.JsonConverters;
internal class RelationOne2NJsonConverter : JsonConverter<RelationOne2N>
{
    public override RelationOne2N? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, RelationOne2N value, JsonSerializerOptions options)
    {
        JsonConvertersHelper.WriteRelation(writer, value, options);
    }

    public static void Register()
    {
        RelationOne2NJsonConverter conv = new RelationOne2NJsonConverter();
        JsonHelper.Register(conv);
    }
}
