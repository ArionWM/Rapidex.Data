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
        throw new NotImplementedException();
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
