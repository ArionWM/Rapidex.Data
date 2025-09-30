using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rapidex.Data.SerializationAndMapping.JsonConverters;
internal class DataTypeDefaultJsonConverter : JsonConverter<IDataType>
{
    public override IDataType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        object value = reader.GetValueAsOriginalType(node =>
        {
            throw new InvalidOperationException($"Unsupported data type: {node}");
        });

        Type undType = typeToConvert.StripNullable();
        undType.ShouldSupportTo<IDataType>();

        IDataType dt = (IDataType)Activator.CreateInstance(undType);
        dt.SetValuePremature(value);

        return dt;
    }

    public override void Write(Utf8JsonWriter writer, IDataType value, JsonSerializerOptions options)
    {
        var lowerValue = value.GetValueLower();
        if (lowerValue.IsNullOrEmpty())
        {
            writer.WriteNullValue();
            return;
        }

        JsonSerializer.Serialize(writer, lowerValue, lowerValue.GetType(), options);
    }

    public static void Register()
    {
        DataTypeDefaultJsonConverter conv = new DataTypeDefaultJsonConverter();
        JsonHelper.Register(conv);
    }
}
