using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Rapidex.Data.SerializationAndMapping.JsonConverters;

namespace Rapidex.Data.SerializationAndMapping.MetadataImplementers;

internal class EntityDataListImplementerJsonConverter : JsonConverter<EntityDataListImplementer>
{
    public static JsonSerializerOptions InternalJsonSerializerOptions { get; private set; }

    static void CheckOptions()
    {
        if (InternalJsonSerializerOptions == null)
        {
            // Kendini çağırmasına engel olmak için
            InternalJsonSerializerOptions = new JsonSerializerOptions();
            InternalJsonSerializerOptions.SetDefaultOptions(typeof(EntityDataListImplementerJsonConverter), typeof(RapidexEntityDataConverterFactory));
        }
    }

    public static void Register()
    {
        EntityDataListImplementerJsonConverter conv = new EntityDataListImplementerJsonConverter();
        JsonHelper.Register(conv);
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(EntityDataListImplementer);
    }

    public override EntityDataListImplementer? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        CheckOptions();

        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                //setupAction: actions.setup şeklinde ifadeler için
                string content = reader.GetString();
                EntityDataListImplementer imp = content;
                return imp;
            case JsonTokenType.StartObject:
                JsonNode node = JsonNode.Parse(ref reader, JsonHelper.DefaultJsonNodeOptions);
                JsonObject obj = node.As<JsonObject>();

                //DataListImplementer dobj = JsonSerializer.Deserialize<DataListImplementer>(obj);

                EntityDataListImplementer dobj = obj.Deserialize<EntityDataListImplementer>(InternalJsonSerializerOptions);
                return dobj;

            default:
                throw new InvalidOperationException($"Unexpected token {reader.TokenType} when parsing ActionDefinitionImplementer.");
        }
    }

    public override void Write(Utf8JsonWriter writer, EntityDataListImplementer value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
