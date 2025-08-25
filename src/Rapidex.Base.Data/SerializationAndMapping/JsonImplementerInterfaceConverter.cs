using Rapidex.Data.Metadata.Implementers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rapidex.Data;
internal class JsonImplementerInterfaceConverter : JsonConverter<IImplementer>
{
    //https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to#use-default-system-converter
    //private readonly static JsonConverter<IUiLayoutItem> s_defaultConverter = (JsonConverter<IUiLayoutItem>)JsonSerializerOptions.Default.GetConverter(typeof(IUiLayoutItem));

    public static void Register()
    {
        JsonImplementerInterfaceConverter conv = new JsonImplementerInterfaceConverter();
        JsonHelper.Register(conv);
    }

    public override bool CanConvert(Type typeToConvert)
    {
        bool ok = typeToConvert == typeof(IImplementer);
        return ok;
    }

    public override IImplementer? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonNode node = JsonObject.Parse(ref reader);
        if (node is JsonObject obj)
        {
            Type impType = MetadataImplementerContainer.FindType(node);
            return obj.Deserialize(impType, options).ShouldSupportTo<IImplementer>();
        }

        throw new NotSupportedException("Invalid JSON");
    }

    public override void Write(Utf8JsonWriter writer, IImplementer value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
