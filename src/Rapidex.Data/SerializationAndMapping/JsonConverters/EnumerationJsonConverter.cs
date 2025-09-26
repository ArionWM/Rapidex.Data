using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rapidex.Data.SerializationAndMapping.JsonConverters;
internal class EnumerationJsonConverter : JsonConverter<Enumeration>
{
    public override Enumeration? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        //typeToConvert; enumeration or enumeration<T>
        Type dataFieldType = typeToConvert.StripNullable();

        Enumeration enumeration = (Enumeration)Activator.CreateInstance(dataFieldType);

        if (reader.TokenType == JsonTokenType.Number)
        {
            // Handle direct ID reference
            long id = reader.GetInt64();
            enumeration.TargetId = id;
            return enumeration;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            // Handle direct ID reference as string
            string str = reader.GetString();
            if (long.TryParse(str, out long id))
            {
                enumeration.TargetId = id;
                return enumeration;
            }
            else
            {
                Type? concreteEnumType = typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Enumeration<>) ? typeToConvert.GetGenericArguments()[0] : null;
                if (concreteEnumType != null)
                {
                    if (concreteEnumType.IsEnum && Enum.TryParse(concreteEnumType, str, true, out object enumValue))
                    {
                        enumeration.Value = Convert.ToInt32(enumValue);
                        return enumeration;
                    }
                }

                //Query on referenced entity for caption field?
                throw new NotImplementedException("yet");
            }

        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                JsonElement root = doc.RootElement;
                if (root.TryGetProperty("value", out JsonElement valueElement))
                {
                    if (valueElement.ValueKind == JsonValueKind.Number)
                    {
                        enumeration.Value = valueElement.GetInt32();
                    }
                    else if (valueElement.ValueKind == JsonValueKind.String)
                    {
                        if (int.TryParse(valueElement.GetString(), out int intValue))
                        {
                            enumeration.Value = intValue;
                        }
                    }
                }

                return enumeration;
            }
        }

        throw new JsonException($"Unexpected token {reader.TokenType} when parsing Enumeration.");
    }

    public override void Write(Utf8JsonWriter writer, Enumeration value, JsonSerializerOptions options)
    {
        JsonConvertersHelper.WriteReference(writer, value, options);
    }

    public static void Register()
    {
        EnumerationJsonConverter conv = new EnumerationJsonConverter();
        JsonHelper.Register(conv);
    }
}
