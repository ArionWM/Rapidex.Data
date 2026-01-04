using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rapidex.Data.SerializationAndMapping.JsonConverters;
internal class RapidexEntityDataConverterFactory : JsonConverterFactory
{
    readonly static ConcurrentDictionary<Type, Type?> ConverterTypes = new();
    readonly static ConcurrentDictionary<Type, JsonConverter> Converters = new();


    public RapidexEntityDataConverterFactory()
    {
        
    }

    public override bool CanConvert(Type typeToConvert)
    {
        if (ConverterTypes.ContainsKey(typeToConvert))
        {
            bool canConvert = ConverterTypes[typeToConvert] != null;
            return canConvert;
        }

        if (JsonHelper.TypedJsonConverters.TryGetValue(typeToConvert, out JsonConverter? conv1))
        {
            ConverterTypes[typeToConvert] = conv1.GetType();
            Converters[typeToConvert] = conv1;
            return true;
        }

        IList<Type> baseTypes = TypeHelper.GetBaseTypesChainCached(typeToConvert, !typeToConvert.IsValueType);
        for (int i = 0; i < baseTypes.Count; i++)
        {
            if (JsonHelper.TypedJsonConverters.TryGetValue(baseTypes[i], out JsonConverter? conv2))
            {
                ConverterTypes[typeToConvert] = conv2.GetType();
                Converters[typeToConvert] = conv2;
                return true;
            }
        }

        ConverterTypes[typeToConvert] = null;
        return false;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (Converters.TryGetValue(typeToConvert, out JsonConverter? converter))
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
