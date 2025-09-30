using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rapidex.Data.SerializationAndMapping.JsonConverters;
internal class TagsJsonConverter : JsonConverter<Tags>
{
    public override Tags? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.String)
        {
            // Handle simple string value (comma-separated tags)
            string value = reader.GetString();
            Tags tags = new Tags();
            if (!string.IsNullOrEmpty(value))
            {
                string[] tagArray = value.Split(new[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(t => t.Trim())
                                       .ToArray();
                tags.Add(tagArray);
            }
            return tags;
        }

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            Tags tags = new Tags();
            List<string> tagList = new List<string>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    // Handle tag info objects with name and color
                    using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
                    {
                        JsonElement root = doc.RootElement;
                        if (root.TryGetProperty("name", out JsonElement nameElement))
                        {
                            tagList.Add(nameElement.GetString());
                        }
                    }
                }
                else if (reader.TokenType == JsonTokenType.String)
                {
                    // Handle simple string array
                    tagList.Add(reader.GetString());
                }
            }

            if (tagList.Count > 0)
                tags.Add(tagList.ToArray());

            return tags;
        }

        throw new JsonException($"Unexpected token {reader.TokenType} when parsing Tags.");
    }

    public override void Write(Utf8JsonWriter writer, Tags value, JsonSerializerOptions options)
    {
        string[] tags = value.Get();
        IDataType _this = value;

        var tagInfos = HasTags.GetTagInfo(_this.Parent._Schema, _this.FieldMetadata.ParentMetadata, tags);

        writer.WriteStartArray();

        foreach (var tagInfo in tagInfos)
        {
            writer.WriteStartObject();
            writer.WriteString("name", tagInfo.Name);
            writer.WriteString("color", tagInfo.Color);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }

    public static void Register()
    {
        TagsJsonConverter conv = new TagsJsonConverter();
        JsonHelper.Register(conv);
    }
}
