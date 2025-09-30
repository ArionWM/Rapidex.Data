using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rapidex.Data.SerializationAndMapping.JsonConverters;
internal class RapidexEntityDataConverterFactory : JsonConverterFactory
{
    readonly static Dictionary<Type, Type?> converterTypes = new();
    readonly static Dictionary<Type, JsonConverter> converters = new();


    public RapidexEntityDataConverterFactory()
    {
        
    }

    public override bool CanConvert(Type typeToConvert)
    {
        if (converterTypes.ContainsKey(typeToConvert))
        {
            bool canConvert = converterTypes[typeToConvert] != null;
            return canConvert;
        }

        if (JsonHelper.TypedJsonConverters.TryGetValue(typeToConvert, out JsonConverter? conv1))
        {
            converterTypes[typeToConvert] = conv1.GetType();
            converters[typeToConvert] = conv1;
            return true;
        }

        IList<Type> baseTypes = TypeHelper.GetBaseTypesChainCached(typeToConvert, !typeToConvert.IsValueType);
        for (int i = 0; i < baseTypes.Count; i++)
        {
            if (JsonHelper.TypedJsonConverters.TryGetValue(baseTypes[i], out JsonConverter? conv2))
            {
                converterTypes[typeToConvert] = conv2.GetType();
                converters[typeToConvert] = conv2;
                return true;
            }
        }

        converterTypes[typeToConvert] = null;
        return false;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (converters.TryGetValue(typeToConvert, out JsonConverter? converter))
        {
            return converter;
        }

        return null;
    }

    public static void Register()
    {
        RapidexEntityDataConverterFactory conv = new RapidexEntityDataConverterFactory();
        JsonHelper.Register(conv);
    }
}
