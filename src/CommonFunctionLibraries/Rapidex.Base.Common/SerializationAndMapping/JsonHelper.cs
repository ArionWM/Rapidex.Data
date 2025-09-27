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
    private static DefaultJsonTypeInfoResolver defaultJsonTypeInfoResolver;
    internal static Dictionary<Type, JsonConverter> JsonConverters { get; private set; } = new();
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
            options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.PropertyNameCaseInsensitive = true;
            options.WriteIndented = true;
            options.AllowTrailingCommas = true;
            options.ReadCommentHandling = JsonCommentHandling.Skip;


            options.Converters.Add(new JsonStringEnumConverter());
            //options.Converters.Add(new AutoStringToBasicConverter());

            foreach (JsonConverter converter in JsonConverters.Values)
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
            options.TypeInfoResolver = defaultJsonTypeInfoResolver;
        }
    }

    public static void Register(JsonConverter converter)
    {
        if (JsonConverters.Values.Any(c => c.GetType() == converter.GetType()))
            return;

        if (converter.Type != null)
            JsonConverters.Set(converter.Type, converter);
        JsonSerializerOptions.Converters.Add(converter);
    }

    public static void AddDerivedTypes(DefaultJsonTypeInfoResolver tiResolver, Type baseType, Type[] types)
    {
        JsonPolymorphismOptions defaultOpt = new();
        defaultOpt.IgnoreUnrecognizedTypeDiscriminators = false;
        defaultOpt.TypeDiscriminatorPropertyName = "$type";
        defaultOpt.UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType;


        foreach (var type in types)
        {
            var derivedTypeAttr = type.GetCustomAttribute<JsonDerivedTypeAttribute>(false);
            if (derivedTypeAttr != null)
                continue; //Has own attribute

            var converterAttr = type.GetCustomAttribute<JsonConverterAttribute>(false);
            if (converterAttr != null)
                continue; //Has own converter

            if (!type.IsAbstract && type.IsAssignableTo(baseType))
                defaultOpt.DerivedTypes.Add(new JsonDerivedType(type));
        }

        tiResolver.Modifiers.Add(typeInfo =>
        {
            if (typeInfo.Type.IsAssignableTo(baseType) && !typeInfo.Type.IsSealed)
            {

            }

            if (typeInfo.Type == baseType && !typeInfo.Type.IsSealed)
            {
                if (typeInfo.PolymorphismOptions == null)
                    typeInfo.PolymorphismOptions = new JsonPolymorphismOptions();

                typeInfo.PolymorphismOptions.IgnoreUnrecognizedTypeDiscriminators = defaultOpt.IgnoreUnrecognizedTypeDiscriminators;
                typeInfo.PolymorphismOptions.TypeDiscriminatorPropertyName = defaultOpt.TypeDiscriminatorPropertyName;
                typeInfo.PolymorphismOptions.UnknownDerivedTypeHandling = defaultOpt.UnknownDerivedTypeHandling;

                foreach (var derivedType in defaultOpt.DerivedTypes.Where(t => !t.DerivedType.IsAbstract && t.DerivedType.IsAssignableTo(typeInfo.Type)))
                    typeInfo.PolymorphismOptions.DerivedTypes.Add(derivedType);
            }
        });
    }

    public static void AddDerivedRootTypes(DefaultJsonTypeInfoResolver tiResolver, Type[] types)
    {
        foreach (var rootType in types)
        {
            Type[] subTypes = Common.Assembly.FindDerivedClassTypes(rootType);
            if (subTypes.IsNOTNullOrEmpty())
            {
                AddDerivedTypes(tiResolver, rootType, subTypes);
            }
        }
    }

    public static void AddDerivedTypes(DefaultJsonTypeInfoResolver tiResolver)
    {
        Type[] types = Common.Assembly.FindTypesHasAttribute<JsonDerivedBaseAttribute>(false);
        AddDerivedRootTypes(tiResolver, types);
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

    public static IEnumerable<Dictionary<string, object>> FromJsonToListOfDictionary(this string json)
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
        
        dynamic d = JsonSerializer.Deserialize<IEnumerable<Dictionary<string, object>>>(json, options);
        return d;
    }


    public static string ToJson<T>(this T obj)
    {
        return JsonSerializer.Serialize<T>(obj, JsonSerializerOptions);
    }

    [Obsolete("", true)]
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
