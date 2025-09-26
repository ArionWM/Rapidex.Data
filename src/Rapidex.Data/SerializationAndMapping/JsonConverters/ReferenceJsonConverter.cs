using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rapidex.Data.SerializationAndMapping.JsonConverters;
internal class ReferenceJsonConverter<T> : JsonConverter<ReferenceBase<T>>
    where T : ReferenceBase<T>, new()
{
    public override ReferenceBase<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.Number)
        {
            // Handle direct ID reference
            long id = reader.GetInt64();
            T reference = new T();
            reference.TargetId = id;
            return reference;
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                JsonElement root = doc.RootElement;
                T reference = new T();

                if (root.TryGetProperty("value", out JsonElement valueElement))
                {
                    if (valueElement.ValueKind == JsonValueKind.Number)
                    {
                        reference.TargetId = valueElement.GetInt64();
                    }
                    else if (valueElement.ValueKind == JsonValueKind.String)
                    {
                        if (long.TryParse(valueElement.GetString(), out long longValue))
                        {
                            reference.TargetId = longValue;
                        }
                    }
                }

                return reference;
            }
        }

        throw new JsonException($"Unexpected token {reader.TokenType} when parsing Reference.");
    }

    public override void Write(Utf8JsonWriter writer, ReferenceBase<T> value, JsonSerializerOptions options)
    {
        JsonConvertersHelper.WriteReference(writer, value, options);
    }

    public static void Register()
    {
        ReferenceJsonConverter<T> conv = new ReferenceJsonConverter<T>();
        JsonHelper.Register(conv);
    }
}

internal class EntityReferenceJsonConverter : ReferenceJsonConverter<Reference<IEntity>>
{

    public static new void Register()
    {
        EntityReferenceJsonConverter conv = new EntityReferenceJsonConverter();
        JsonHelper.Register(conv);
    }
}
