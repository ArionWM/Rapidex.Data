using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rapidex.SerializationAndMapping;

public class AutoStringToBasicConverter : JsonConverter<object>
{
    public override bool CanConvert(Type typeToConvert)
    {
        // see https://stackoverflow.com/questions/1749966/c-sharp-how-to-determine-whether-a-type-is-a-number
        switch (Type.GetTypeCode(typeToConvert))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
            case TypeCode.Boolean:
                return true;
            default:
                return false;
        }
    }
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();

            if (s.IsNullOrEmpty())
            {
                return Activator.CreateInstance(typeToConvert);
            }

            Type undType = Nullable.GetUnderlyingType(typeToConvert);
            if (undType != null)
                typeToConvert = undType;

            switch (Type.GetTypeCode(typeToConvert))
            {
                case TypeCode.Byte:
                    return byte.Parse(s);
                case TypeCode.SByte:
                    return sbyte.Parse(s);
                case TypeCode.UInt16:
                    return ushort.Parse(s);
                case TypeCode.UInt32:
                    return uint.Parse(s);
                case TypeCode.UInt64:
                    return ulong.Parse(s);
                case TypeCode.Int16:
                    return short.Parse(s);
                case TypeCode.Int32:
                    return int.Parse(s);
                case TypeCode.Int64:
                    return long.Parse(s);
                case TypeCode.Decimal:
                    return decimal.Parse(s);
                case TypeCode.Double:
                    return double.Parse(s);
                case TypeCode.Single:
                    return float.Parse(s);
                case TypeCode.Boolean:
                    return bool.Parse(s);

                default:
                    throw new NotSupportedException($"unsupported type {typeToConvert}");

            }
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            switch (Type.GetTypeCode(typeToConvert))
            {
                case TypeCode.Byte:
                    return reader.TryGetByte(out byte valByte) ? valByte : reader.GetByte();
                case TypeCode.SByte:
                    return reader.TryGetByte(out byte valSByte) ? valSByte : reader.GetSByte();
                case TypeCode.UInt16:
                    return reader.TryGetUInt16(out ushort valUInt16) ? valUInt16 : reader.GetUInt16();
                case TypeCode.UInt32:
                    return reader.TryGetUInt32(out uint valUInt32) ? valUInt32 : reader.GetUInt32();
                case TypeCode.UInt64:
                    return reader.TryGetUInt64(out ulong valUInt64) ? valUInt64 : reader.GetUInt64();
                case TypeCode.Int16:
                    return reader.TryGetInt16(out short valInt16) ? valInt16 : reader.GetInt16();
                case TypeCode.Int32:
                    return reader.TryGetInt32(out int valInt32) ? valInt32 : reader.GetInt32();
                case TypeCode.Int64:
                    return reader.TryGetInt64(out long val64) ? val64 : reader.GetInt64();
                case TypeCode.Decimal:
                    return reader.TryGetDecimal(out decimal valDecimal) ? valDecimal : reader.GetDecimal();
                case TypeCode.Double:
                    return reader.TryGetDouble(out double valDouble) ? valDouble : reader.GetDouble();
                case TypeCode.Single:
                    return reader.TryGetSingle(out float valSingle) ? valSingle : reader.GetSingle();
                case TypeCode.Boolean:
                    if (reader.TryGetByte(out byte valBool))
                    {
                        return valBool != 0;
                    }
                    else
                    {
                        return reader.GetByte() != 0;
                    }


                default:
                    throw new NotSupportedException($"unsupported type {typeToConvert}");
            }
        }

        if (reader.TokenType == JsonTokenType.True)
        {
            return true;
        }

        if (reader.TokenType == JsonTokenType.False)
        {
            return false;
        }

        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        using (JsonDocument document = JsonDocument.ParseValue(ref reader))
        {
            throw new InvalidOperationException($"unable to parse {document.RootElement.ToString()} ({reader.TokenType})");
        }
    }


    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        if (value.IsNullOrEmpty())
        {
            writer.WriteNullValue();
            return;
        }

        TypeCode tcode = Type.GetTypeCode(value.GetType());
        switch (tcode)
        {
            case TypeCode.Byte:
                writer.WriteNumberValue((byte)value);
                return;
            case TypeCode.SByte:
                writer.WriteNumberValue((sbyte)value);
                return;
            case TypeCode.UInt16:
                writer.WriteNumberValue((ushort)value);
                return;
            case TypeCode.UInt32:
                writer.WriteNumberValue((uint)value);
                return;
            case TypeCode.UInt64:
                writer.WriteNumberValue((ulong)value);
                return;
            case TypeCode.Int16:
                writer.WriteNumberValue((short)value);
                return;
            case TypeCode.Int32:
                writer.WriteNumberValue((int)value);
                return;
            case TypeCode.Int64:
                writer.WriteNumberValue((long)value);
                return;
            case TypeCode.Decimal:
                writer.WriteNumberValue((decimal)value);
                return;
            case TypeCode.Double:
                writer.WriteNumberValue((double)value);
                return;
            case TypeCode.Single:
                writer.WriteNumberValue((float)value);
                return;
            case TypeCode.Boolean:
                writer.WriteBooleanValue((bool)value);
                return;
            default:
                throw new Exception($"unable to parse {value} to number");
                break;
        }




        //JsonSerializer.Serialize(writer, value, options);



        //var str = value.ToString();             // I don't want to write int/decimal/double/...  for each case, so I just convert it to string . You might want to replace it with strong type version.
        //if (int.TryParse(str, out var i))
        //{
        //    writer.WriteNumberValue(i);
        //}
        //else if (double.TryParse(str, out var d))
        //{
        //    writer.WriteNumberValue(d);
        //}
        //else
        //{
        //    throw new Exception($"unable to parse {str} to number");
        //}
    }
}
