using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rapidex.Data.SerializationAndMapping.JsonConverters;
internal class CurrencyJsonConverter : JsonConverter<Currency>
{
    public override Currency? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.Number)
        {
            // Handle simple numeric value
            decimal value = reader.GetDecimal();
            Currency currency = new Currency();
            currency.Value = value;
            return currency;
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                JsonElement root = doc.RootElement;
                Currency currency = new Currency();

                if (root.TryGetProperty("value", out JsonElement valueElement))
                {
                    currency.Value = valueElement.GetDecimal();
                }

                if (root.TryGetProperty("type", out JsonElement typeElement))
                {
                    currency.Type = typeElement.GetString();
                }

                return currency;
            }
        }

        throw new JsonException($"Unexpected token {reader.TokenType} when parsing Currency.");
    }

    public override void Write(Utf8JsonWriter writer, Currency value, JsonSerializerOptions options)
    {
        if (value.IsNullOrEmpty())
        {
            writer.WriteNullValue();
            return;
        }
        writer.WriteStartObject();
        writer.WriteNumber("value", value.Value);
        writer.WriteString("type", value.Type);
        writer.WriteEndObject();

    }
}
