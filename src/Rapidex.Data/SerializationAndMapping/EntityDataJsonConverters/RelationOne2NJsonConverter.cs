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
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        RelationOne2N relation = new RelationOne2N();

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            // Handle array of entities or entity references
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    // Deserialize each entity in the relation
                    IEntity entity = JsonSerializer.Deserialize<IEntity>(ref reader, options);
                    if (entity != null)
                    {
                        relation.Add(entity);
                    }
                }
                else if (reader.TokenType == JsonTokenType.Number)
                {
                    // Handle direct ID references
                    long id = reader.GetInt64();
                    // Create a partial entity reference with just the ID
                    // This would need proper entity creation based on relation metadata
                    throw new NotImplementedException();
                }
            }
        }
        else 
        {
           throw new JsonException($"Unexpected token {reader.TokenType} when parsing RelationOne2N. Array required.");
        }

        return relation;
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
