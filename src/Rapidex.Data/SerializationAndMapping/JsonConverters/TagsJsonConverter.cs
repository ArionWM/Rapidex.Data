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
        throw new NotImplementedException();
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
