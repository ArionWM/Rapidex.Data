using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rapidex.Data.SerializationAndMapping.JsonConverters;
internal class TextJsonConverter : JsonConverter<Text>
{
    public override Text? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.String)
        {
            // Handle simple string value
            string value = reader.GetString();
            Text text = new Text();
            text.Value = value;
            text.Type = TextType.Plain; // Default type
            return text;
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                JsonElement root = doc.RootElement;
                Text text = new Text();

                if (root.TryGetProperty("value", out JsonElement valueElement))
                {
                    text.Value = valueElement.GetString();
                }

                if (root.TryGetProperty("type", out JsonElement typeElement))
                {
                    string typeString = typeElement.GetString();
                    if (Enum.TryParse<TextType>(typeString, true, out TextType textType))
                    {
                        text.Type = textType;
                    }
                }

                return text;
            }
        }

        throw new JsonException($"Unexpected token {reader.TokenType} when parsing Text.");
    }

    public override void Write(Utf8JsonWriter writer, Text value, JsonSerializerOptions options)
    {
        if (value.IsNullOrEmpty())
        {
            writer.WriteNullValue();
            return;
        }
        writer.WriteStartObject();
        writer.WriteString("value", value.Value);
        writer.WriteString("type", value.Type.ToString());
        writer.WriteEndObject();
    }

    public static void Register()
    {
        TextJsonConverter conv = new TextJsonConverter();
        JsonHelper.Register(conv);
    }
}
