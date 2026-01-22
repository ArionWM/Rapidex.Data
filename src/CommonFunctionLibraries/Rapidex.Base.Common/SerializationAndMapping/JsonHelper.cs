using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Options;
using Rapidex.SerializationAndMapping;

namespace Rapidex;

public static class JsonHelper
{
    private static object OptionRegisterLocker = new object();
    private static DefaultJsonTypeInfoResolver DefaultJsonTypeInfoResolver;
    internal static List<JsonConverter> JsonConverters { get; private set; } = new();
    internal static Dictionary<Type, JsonConverter> TypedJsonConverters { get; private set; } = new();
    public static JsonSerializerOptions DefaultJsonSerializerOptions { get; private set; }
    public static JsonDocumentOptions DefaultJsonDocumentOptions { get; private set; } = new JsonDocumentOptions()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip,
    };

    public static JsonNodeOptions DefaultJsonNodeOptions { get; private set; } = new JsonNodeOptions()
    {
        PropertyNameCaseInsensitive = true,
    };


    static JsonHelper()
    {

    }

    private static void CheckInitialized()
    {
        if (DefaultJsonSerializerOptions == null)
            throw new InvalidOperationException("JsonHelper is not initialized. Please call JsonHelper.Start() in application startup.");
    }

    public static void Setup()
    {

    }

    public static void Start()
    {
        JsonHelper.DefaultJsonSerializerOptions = new();
        JsonHelper.DefaultJsonSerializerOptions.SetDefaultOptions();
    }

    public static void SetDefaultOptions(this JsonSerializerOptions options, params Type[] excludedConverters)
    {
        if (!options.IsReadOnly)
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.PropertyNameCaseInsensitive = true;
            options.WriteIndented = true;
            options.AllowTrailingCommas = true;
            options.ReadCommentHandling = JsonCommentHandling.Skip;

            options.Converters.Add(new JsonStringEnumConverter());
            options.Converters.Add(new AutoStringToBasicConverter());

            foreach (JsonConverter converter in JsonConverters)
            {
                Type ctype = converter.GetType();
                if (excludedConverters != null && excludedConverters.Any(c => c == ctype))
                    continue;

                options.Converters.Add(converter);
            }


            //if (defaultJsonTypeInfoResolver == null)
            //{
            //    defaultJsonTypeInfoResolver = new DefaultJsonTypeInfoResolver();
            //    AddDerivedTypes(defaultJsonTypeInfoResolver);
            //}
            //options.TypeInfoResolver = defaultJsonTypeInfoResolver;
        }
    }

    public static void Register(JsonConverter converter)
    {
        lock (OptionRegisterLocker)
        {
            if (JsonConverters.Any(c => c.GetType() == converter.GetType()))
                return;

            if (converter.Type != null)
                TypedJsonConverters.Set(converter.Type, converter);

            JsonConverters.Add(converter);
        }
    }



    public static void MsDeserializationCorrection(Dictionary<string, object> data)
    {
        foreach (var item in data)
        {
            if (item.Value is JsonElement jsonElement)
            {
                data[item.Key] = jsonElement.GetValueAsOriginalType();
            }
        }

    }

    //TODO: Remove for confusion
    public static T FromJson<T>(this string json)
    {
        CheckInitialized();

        json = json?.Trim();
        if (json.IsNullOrEmpty())
            return default(T);

        //??
        if (json.StartsWith("{") && json.EndsWith("}"))
            if (json == "{}")
                return default(T);

        if (json.StartsWith("[") && json.EndsWith("]"))
            if (json == "[]")
                return default(T);

        return JsonSerializer.Deserialize<T>(json, DefaultJsonSerializerOptions);
    }

    public static object FromJson(this string json, Type targetType)
    {
        CheckInitialized();

        json = json?.Trim();
        if (json.IsNullOrEmpty())
            return null;

        //??
        if (json.StartsWith("{") && json.EndsWith("}"))
            if (json == "{}")
                return null;

        if (json.StartsWith("[") && json.EndsWith("]"))
            if (json == "[]")
                return null;

        return JsonSerializer.Deserialize(json, targetType, DefaultJsonSerializerOptions);
    }

    public static Dictionary<string, object> FromJsonToDictionary(ref Utf8JsonReader reader)
    {
        CheckInitialized();

        var options = new JsonSerializerOptions
        {
            Converters = { new JsonToDictionaryConverter(floatFormat: FloatFormat.Double, unknownNumberFormat: UnknownNumberFormat.Error, objectFormat: ObjectFormat.Dictionary) },
            WriteIndented = true,
        };
        dynamic d = JsonSerializer.Deserialize<dynamic>(ref reader, options);
        return d;
    }

    public static Dictionary<string, object> FromJsonToDictionary(this string json)
    {
        CheckInitialized();

        json = json?.Trim();
        if (json.IsNullOrEmpty())
            return null;

        //??
        if (json.StartsWith("{") && json.EndsWith("}"))
            if (json == "{}")
                return null;

        if (json.StartsWith("[") && json.EndsWith("]"))
            if (json == "[]")
                return null;

        var options = new JsonSerializerOptions
        {
            Converters = { new JsonToDictionaryConverter(floatFormat: FloatFormat.Double, unknownNumberFormat: UnknownNumberFormat.Error, objectFormat: ObjectFormat.Dictionary) },
            WriteIndented = true,
        };
        dynamic d = JsonSerializer.Deserialize<dynamic>(json, options);
        return d;
    }

    public static IEnumerable<Dictionary<string, object>> FromJsonToListOfDictionary(this string json)
    {
        CheckInitialized();

        json = json?.Trim();
        if (json.IsNullOrEmpty())
            return null;
        //??
        if (json.StartsWith("{") && json.EndsWith("}"))
            if (json == "{}")
                return null;
        if (json.StartsWith("[") && json.EndsWith("]"))
            if (json == "[]")
                return null;
        var options = new JsonSerializerOptions
        {
            Converters = { new JsonToDictionaryConverter(floatFormat: FloatFormat.Double, unknownNumberFormat: UnknownNumberFormat.Error, objectFormat: ObjectFormat.Dictionary) },
            WriteIndented = true,
        };

        dynamic d = JsonSerializer.Deserialize<IEnumerable<Dictionary<string, object>>>(json, options);
        return d;
    }


    public static string ToJson<T>(this T obj)
    {
        CheckInitialized();

        return JsonSerializer.Serialize(obj, DefaultJsonSerializerOptions);
    }


}
