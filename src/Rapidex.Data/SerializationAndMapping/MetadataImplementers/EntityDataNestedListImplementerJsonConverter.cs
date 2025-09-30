using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rapidex.Data.SerializationAndMapping.MetadataImplementers;

internal class EntityDataNestedListImplementerJsonConverter : JsonConverter<EntityDataNestedListImplementer>
{
    public static JsonSerializerOptions InternalJsonSerializerOptions { get; private set; }

    static void CheckOptions()
    {
        if (InternalJsonSerializerOptions == null)
        {
            // Kendini çağırmasına engel olmak için
            InternalJsonSerializerOptions = new JsonSerializerOptions();
            InternalJsonSerializerOptions.SetDefaultOptions(typeof(EntityDataNestedListImplementerJsonConverter));
        }
    }

    public static void Register()
    {
        EntityDataNestedListImplementerJsonConverter conv = new EntityDataNestedListImplementerJsonConverter();
        JsonHelper.Register(conv);
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(EntityDataNestedListImplementer);
    }

    public override EntityDataNestedListImplementer? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        CheckOptions();

        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                //setupAction: actions.setup şeklinde ifadeler için
                string content = reader.GetString();
                EntityDataNestedListImplementer imp = content;
                return imp;
            case JsonTokenType.StartObject:
                JsonNode node = JsonNode.Parse(ref reader);
                JsonObject obj = node.ShouldSupportTo<JsonObject>();

                //DataNestedListImplementer dobj = JsonSerializer.Deserialize<DataNestedListImplementer>(obj);

                EntityDataNestedListImplementer dobj = obj.Deserialize<EntityDataNestedListImplementer>(InternalJsonSerializerOptions);
                return dobj;
            case JsonTokenType.StartArray:
                JsonArray array = JsonNode.Parse(ref reader) as JsonArray;

                EntityDataNestedListImplementer imp2 = new EntityDataNestedListImplementer();
                foreach (JsonObject item in array)
                {
                    var itemObj = item.Deserialize<EntityDataItemImplementer>(JsonHelper.JsonSerializerOptions);
                    //imp2.Items = imp2.Items ?? new();
                    imp2.Add(itemObj);
                }
                return imp2;
            default:
                throw new InvalidOperationException($"Unexpected token {reader.TokenType} when parsing ActionDefinitionImplementer.");
        }
    }

    public override void Write(Utf8JsonWriter writer, EntityDataNestedListImplementer value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}