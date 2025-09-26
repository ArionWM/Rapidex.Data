using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rapidex.Data.SerializationAndMapping.JsonConverters;
internal class ImageJsonConverter : JsonConverter<Image>
{
    public override Image? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        //Similar with ReferenceJsonConverter

        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.Number)
        {
            // Handle direct ID reference
            long id = reader.GetInt64();
            Image reference = new Image();
            reference.TargetId = id;
            return reference;
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                JsonElement root = doc.RootElement;
                Image reference = new Image();

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

    public override void Write(Utf8JsonWriter writer, Image value, JsonSerializerOptions options)
    {
        if (value.TargetId.IsEmptyId())
        {
            writer.WriteNullValue();
            return;
        }


        IDataType _this = value;
        string fieldName = _this.FieldMetadata.Name;
        IEntity owner = _this.Parent;
        IDbEntityMetadata em = owner.GetMetadata();
        IDbSchemaScope dbScope = owner._Schema;

        string nav = BlobFieldHelper.GetFileDescriptorIdForFieldFile(owner, em, fieldName);
        //$"{dbScope.SchemaName}.{em.Name}.{owner.GetId()}.{fieldName}";

        writer.WriteStartObject();

        writer.WriteNumber("value", value.Value);

        ILazy _lazy = (ILazy)value;
        IEntity ent = (IEntity)_lazy.GetContent();
        string caption = ent?.Caption();

        writer.WriteString("text", caption);

        writer.WriteString(CommonConstants.FIELD_ID, nav);
        writer.WriteString(CommonConstants.DATA_FIELD_ID, nav);

        writer.WriteEndObject();
    }

    public static void Register()
    {
        ImageJsonConverter conv = new ImageJsonConverter();
        JsonHelper.Register(conv);
    }

}
