using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rapidex.Data.SerializationAndMapping.JsonConverters;
internal static class JsonConvertersHelper
{
    public static void WriteRelation(Utf8JsonWriter writer, ILazy value, JsonSerializerOptions options)
    {
        bool? useNestedEntitiesRef = EntityJsonConverter.UseNestedEntities;
        try
        {
            bool useNestedEntities = useNestedEntitiesRef ?? true;
            EntityJsonConverter.UseNestedEntities = false;

            IEntityLoadResult? loadResult = (IEntityLoadResult)((ILazy)value).GetContent();
            if (loadResult.IsNullOrEmpty())
            {
                writer.WriteNullValue();
                return;
            }

            if (!useNestedEntities)
            {
                writer.WriteStartObject();
                writer.WriteString("_description", "Nested content available");
                writer.WriteEndObject();
                return;
            }

            writer.WriteStartArray();
            foreach (IEntity ent in loadResult)
            {
                JsonSerializer.Serialize(writer, ent, ent.GetType(), options);
            }
            writer.WriteEndArray();
        }
        finally
        {
            EntityJsonConverter.UseNestedEntities = useNestedEntitiesRef;
        }

    }

    public static void WriteReference(Utf8JsonWriter writer, IReference value, JsonSerializerOptions options)
    {
        if (value.IsNullOrEmpty())
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        writer.WritePropertyName("value");
        writer.WriteNumberValue(value.TargetId);

        ILazy _this = (ILazy)value;
        IEntity ent = (IEntity)_this.GetContent();
        string caption = ent?.Caption();

        writer.WritePropertyName("text");
        writer.WriteStringValue(caption);

        writer.WriteEndObject();
    }
}
