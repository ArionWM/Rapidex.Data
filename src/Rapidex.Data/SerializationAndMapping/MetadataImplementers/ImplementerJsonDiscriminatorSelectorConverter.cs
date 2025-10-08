using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Rapidex.Data.SerializationAndMapping;

namespace Rapidex.Data.SerializationAndMapping.MetadataImplementers;

[Obsolete("", true)]
internal class ImplementerJsonDiscriminatorSelectorConverter : JsonConverter<IImplementer>
{
    //https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to#use-default-system-converter
    //private readonly static JsonConverter<IUiLayoutItem> s_defaultConverter = (JsonConverter<IUiLayoutItem>)JsonSerializerOptions.Default.GetConverter(typeof(IUiLayoutItem));

    public static void Register()
    {
        ImplementerJsonDiscriminatorSelectorConverter conv = new ImplementerJsonDiscriminatorSelectorConverter();
        JsonHelper.Register(conv);
    }

    public override bool CanConvert(Type typeToConvert)
    {
        bool ok = typeToConvert == typeof(IImplementer);
        return ok;
    }

    public override IImplementer? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonNode node = JsonNode.Parse(ref reader, JsonHelper.DefaultJsonNodeOptions);
        if (node is JsonObject obj)
        {
            Type impType = MetadataImplementerContainer.FindType(node);
            return obj.Deserialize(impType, DefaultMetadataImplementHost.JsonSerializerOptionsWithExcludes)
                .ShouldSupportTo<IImplementer>();
        }

        throw new NotSupportedException("Invalid JSON");
    }

    public override void Write(Utf8JsonWriter writer, IImplementer value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
