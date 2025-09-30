using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rapidex.Data.SerializationAndMapping.JsonConverters;
internal class DateTimeStartEndJsonConverter : JsonConverter<DateTimeStartEnd>
{
    public override DateTimeStartEnd? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                JsonElement root = doc.RootElement;
                DateTimeStartEnd dateRange = new DateTimeStartEnd();

                if (root.TryGetProperty("start", out JsonElement startElement))
                {
                    if (startElement.ValueKind == JsonValueKind.String)
                    {
                        if (DateTimeOffset.TryParse(startElement.GetString(), out DateTimeOffset start))
                        {
                            dateRange.Start = start;
                        }
                    }
                }

                if (root.TryGetProperty("end", out JsonElement endElement))
                {
                    if (endElement.ValueKind == JsonValueKind.String)
                    {
                        if (DateTimeOffset.TryParse(endElement.GetString(), out DateTimeOffset end))
                        {
                            dateRange.End = end;
                        }
                    }
                }

                return dateRange;
            }
        }

        throw new JsonException($"Unexpected token {reader.TokenType} when parsing DateTimeStartEnd.");
    }

    public override void Write(Utf8JsonWriter writer, DateTimeStartEnd value, JsonSerializerOptions options)
    {
        if (value.IsNullOrEmpty())
        {
            writer.WriteNullValue();
            return;
        }

        IDataType _this = value;
        IDbFieldMetadata fm = _this?.FieldMetadata;
        string startFieldName = fm?.Name + "Start";
        string endFieldName = fm?.Name + "End";

        DateTimeOffset startTime = _this.Parent.NotNull().GetValue<DateTimeOffset>(startFieldName);
        DateTimeOffset endTime = _this.Parent.NotNull().GetValue<DateTimeOffset>(endFieldName);


        writer.WriteStartObject();
        writer.WriteString("start", startTime.ToString("o"));
        writer.WriteString("end", endTime.ToString("o"));
        writer.WriteEndObject();
    }

    public static void Register()
    {
        DateTimeStartEndJsonConverter conv = new DateTimeStartEndJsonConverter();
        JsonHelper.Register(conv);
    }
}
