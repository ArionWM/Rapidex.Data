using Rapidex.SerializationAndMapping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Rapidex;

public static class JsonHelper
{
    static DefaultJsonTypeInfoResolver defaultJsonTypeInfoResolver;
    public static List<JsonConverter> jsonConverters = new List<JsonConverter>();
    public static JsonSerializerOptions JsonSerializerOptions { get; private set; } = new JsonSerializerOptions();

    static JsonHelper()
    {
        //JsonHelper.JsonSerializerOptions.SetDefaultOptions();
    }

    public static void Setup()
    {
        JsonHelper.JsonSerializerOptions.SetDefaultOptions();
    }

    public static void SetDefaultOptions(this JsonSerializerOptions options, params Type[] excludedConverters)
    {
        if (!options.IsReadOnly)
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.PropertyNameCaseInsensitive = true;
            options.WriteIndented = true;
            options.AllowTrailingCommas = true;
            options.ReadCommentHandling = JsonCommentHandling.Skip;

            options.Converters.Add(new JsonStringEnumConverter());
            options.Converters.Add(new AutoStringToBasicConverter());

            foreach (JsonConverter converter in jsonConverters)
            {
                Type ctype = converter.GetType();
                if (excludedConverters != null && excludedConverters.Any(c => c == ctype))
                    continue;

                options.Converters.Add(converter);
            }


            if (defaultJsonTypeInfoResolver == null)
            {
                defaultJsonTypeInfoResolver = new DefaultJsonTypeInfoResolver();
                AddDerivedTypes(defaultJsonTypeInfoResolver);
            }

            // Configure either a DefaultJsonTypeInfoResolver or some JsonSerializerContext and add the required modifier:
            options.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
        }
    }

    public static void Register(JsonConverter converter)
    {
        if (jsonConverters.Any(c => c.GetType() == converter.GetType()))
            return;

        jsonConverters.Add(converter);
        JsonSerializerOptions.Converters.Add(converter);
    }

    public static void AddDerivedTypes(IJsonTypeInfoResolver tiResolver, Type[] types)
    {
        foreach (var type in types)
        {
            tiResolver.WithAddedModifier(JsonHelper.AddPolymorphismOptions(
                type.BaseType,
                new()
                {
                    DerivedTypes = { new(type.BaseType), new(type) },
                }
            ));

            Type[] subTypes = Common.Assembly.FindDerivedClassTypes(type);
            if (subTypes.IsNOTNullOrEmpty())
            {
                AddDerivedTypes(tiResolver, subTypes);
            }
        }
    }

    public static void AddDerivedTypes(IJsonTypeInfoResolver tiResolver)
    {
        Type[] types = Common.Assembly.FindTypesHasAttribute<JsonDerivedBaseAttribute>();
        AddDerivedTypes(tiResolver, types);
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

    public static T FromJson<T>(this string json)
    {
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

        return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions);
    }

    public static object FromJson(this string json, Type targetType)
    {
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

        return JsonSerializer.Deserialize(json, targetType, JsonSerializerOptions);
    }

    public static Dictionary<string, object> FromJsonToDictionary(this string json)
    {
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


    public static string ToJson<T>(this T obj)
    {
        return JsonSerializer.Serialize<T>(obj, JsonSerializerOptions);
    }

    //https://stackoverflow.com/questions/77857543/how-can-i-add-jsonderivedtype-without-attributes-in-runtime-in-system-text-json
    public static Action<JsonTypeInfo> AddPolymorphismOptions(Type baseType, JsonPolymorphismOptions options) => (typeInfo) =>
    {
        if (baseType.IsSealed)
            throw new ArgumentException($"Cannot add JsonPolymorphismOptions to sealed base type {baseType.FullName}");
        if (typeInfo.Type.IsAssignableTo(baseType) && !typeInfo.Type.IsSealed)
        {
            typeInfo.PolymorphismOptions = new()
            {
                IgnoreUnrecognizedTypeDiscriminators = options.IgnoreUnrecognizedTypeDiscriminators,
                TypeDiscriminatorPropertyName = options.TypeDiscriminatorPropertyName,
                UnknownDerivedTypeHandling = options.UnknownDerivedTypeHandling,
            };
            foreach (var derivedType in options.DerivedTypes.Where(t => !t.DerivedType.IsAbstract && t.DerivedType.IsAssignableTo(typeInfo.Type)))
                typeInfo.PolymorphismOptions.DerivedTypes.Add(derivedType);
        }
    };


}
