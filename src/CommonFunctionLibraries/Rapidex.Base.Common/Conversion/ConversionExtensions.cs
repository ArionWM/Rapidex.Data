using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Rapidex
{
    //Şimdilik basit tutuldu bk: Procore / SafeValueConverter
    public static class ConversionExtensions
    {

        public static void Register<T>(this RapidexTypeConverter converter) where T : IBaseConverter
        {
            T inst = Activator.CreateInstance<T>();
            converter.Register(inst);
        }


        public static T As<T>(this object value)
        {
            if (value == null || value is DBNull)
            {
                return default(T);
            }

            if (value is JsonElement jelement)
            {
                value = jelement.GetValueAsOriginalType();
            }

            Type targetType = typeof(T);

            return (T)value.As(targetType);
        }

        public static object As(this object value, Type targetType)
        {
            if (value is JsonElement jelement)
            {
                value = jelement.GetValueAsOriginalType();
            }

            return Common.Converter.Convert(value, targetType);
        }


        public static object GetValueAsOriginalType(this JsonElement jsonElement)
        {
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.Undefined:
                case JsonValueKind.Null:
                    return null;
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Number:
                    return jsonElement.GetDecimal();
                case JsonValueKind.String:
                    return jsonElement.GetString();
                case JsonValueKind.Object:
                    JsonElement valueElement;

                    if (jsonElement.GetPropertyCount() == 1)
                    {
                        if (jsonElement.TryGetProperty("value", out valueElement))
                        {
                            return GetValueAsOriginalType(valueElement);
                        }
                        else
                        {
                            goto case default;
                        }
                    }
                    else
                    {
                        object ds = jsonElement.Deserialize<Dictionary<string, object>>(JsonHelper.JsonSerializerOptions);
                        return ds;

                    }
                case JsonValueKind.Array:
                    return jsonElement.EnumerateArray().Select(GetValueAsOriginalType).ToArray();


                default:
                    throw new NotSupportedException($"Usupported value format: {jsonElement.ValueKind}"); //Try?? JsonSerializer.Deserialize<object>(fieldElement.GetRawText(), options)
            }
        }

        public static object GetValueAsOriginalType(this JsonNode jsonNode)
        {
            JsonValue jsonValue = jsonNode?.AsValue();
            if (jsonNode == null)
            {
                return null;
            }

            JsonValueKind vkind = jsonValue.GetValueKind();
            switch (vkind)
            {
                case JsonValueKind.Undefined:
                case JsonValueKind.Null:
                    return null;
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Number:
                    return jsonNode.GetValue<decimal>();
                case JsonValueKind.String:
                    return jsonNode.GetValue<string>();
                //case JsonValueKind.Array:
                //    return jsonElement.EnumerateArray().Select(GetValueAsOriginalType).ToArray();
                //case JsonValueKind.Object:
                //    return jsonElement.EnumerateObject().ToDictionary(x => x.Name, x => GetValueAsOriginalType(x.Value));
                default:
                    throw new NotSupportedException($"{vkind}");
            }
        }

        //https://josef.codes/custom-dictionary-string-object-jsonconverter-for-system-text-json/
        public static object GetValueAsOriginalType(this Utf8JsonReader jsonReader, Func<JsonNode, Type> getObjectType)
        {
            switch (jsonReader.TokenType)
            {

                case JsonTokenType.Null:
                    return null;
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.Number:
                    return jsonReader.GetDecimal();
                case JsonTokenType.String:
                    return jsonReader.GetString();
                case JsonTokenType.StartObject:
                    JsonNode node = JsonObject.Parse(ref jsonReader);
                    Type type = getObjectType.NotNull().Invoke(node);
                    TypeInfo tInfo = type.GetTypeInfo();
                    return JsonSerializer.Deserialize(node, tInfo, JsonHelper.JsonSerializerOptions); // JsonHelper.JsonSerializerOptions);

                default:
                    throw new NotSupportedException($"Usupported value format: {jsonReader.TokenType}");
            }
        }

    }
}
