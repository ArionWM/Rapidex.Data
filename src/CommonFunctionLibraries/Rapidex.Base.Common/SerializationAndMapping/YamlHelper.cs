using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace Rapidex;

public static class YamlHelper
{
    private static YamlDotNet.Serialization.ISerializer defaultSerializer; // = new YamlDotNet.Serialization.Serializer();
    private static YamlDotNet.Serialization.IDeserializer defaultDeserializer;
    private static YamlDotNet.Serialization.IDeserializer expandoDeserializer;

    private static object CheckStringDict(object dict)
    {
        if (dict is IDictionary<object, object> objDict)
        {
            IDictionary<string, object> strDict = objDict.ToStringKeys(true);
            return strDict;
        }
        else
        {
            return dict;
        }
    }

    static YamlHelper()
    {
        YamlHelper.defaultSerializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        YamlHelper.defaultDeserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
            .Build();

        YamlHelper.expandoDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }


    public static void Setup()
    {
        
    }


    public static string ToYaml<T>(this T obj)
    {
        return defaultSerializer.Serialize(obj);
    }

    public static T FromYaml<T>(this string yaml)
    {
        return defaultDeserializer.Deserialize<T>(yaml);

    }

    //https://github.com/aaubry/YamlDotNet/issues/12

    public static IEnumerable<object> DeserializeMany(this IDeserializer deserializer, TextReader input)
    {
        deserializer.NotNull();

        var reader = new Parser(input);
        reader.Consume<StreamStart>();

        DocumentStart dummyStart;
        DocumentEnd dummyEnd;
        while (reader.TryConsume<DocumentStart>(out dummyStart))
        {
            var item = deserializer.Deserialize(reader);
            yield return item;
            reader.TryConsume<DocumentEnd>(out dummyEnd);
        }
    }

    public static IEnumerable<object> DeserializeMany(this IDeserializer deserializer, string input)
    {
        using (var reader = new StringReader(input))
        {
            return deserializer.DeserializeMany(reader).ToArray();
        }
    }

    public static IEnumerable<object> FromYamlMany(this string input)
    {
        using (var reader = new StringReader(input))
        {
            return defaultDeserializer.DeserializeMany(reader).ToArray();
        }
    }

    public static IEnumerable<TItem> DeserializeMany<TItem>(this IDeserializer deserializer, TextReader input)
    {
        var reader = new Parser(input);
        reader.Consume<StreamStart>();

        DocumentStart dummyStart;
        DocumentEnd dummyEnd;
        while (reader.TryConsume<DocumentStart>(out dummyStart))
        {
            var item = deserializer.Deserialize<TItem>(reader);
            yield return item;
            reader.TryConsume<DocumentEnd>(out dummyEnd);
        }
    }

    public static IEnumerable<TItem> FromYamlMany<TItem>(this string input)
    {
        using (var reader = new StringReader(input))
        {
            return defaultDeserializer.DeserializeMany<TItem>(reader).ToArray();
        }
    }



    public static IEnumerable<object> FromYamlManyToExpando(string yaml)
    {
        string _yaml = yaml.Replace("\t", " ").Trim();
        object result = expandoDeserializer.DeserializeMany(_yaml);
        IList<object> objects = (result as IEnumerable<object>)?.ToList();
        if (objects is null)
            objects = new List<object> { result };

        for (int i = 0; i < objects.Count; i++)
        {
            object item = objects[i];
            objects[i] = CheckStringDict(item);
        }

        return objects;
    }

    public static IEnumerable<string> FromYamlManyToJson(string yaml)
    {
        string _yaml = yaml.Replace("\t", " ").Trim();
        object result = expandoDeserializer.DeserializeMany(_yaml);

        //Each object is IDictionary<string, object>
        IEnumerable<object> objects = result as IEnumerable<object>;
        if (objects is null)
            objects = new List<object> { result };

        List<string> jsons = new List<string>();
        foreach (var obj in objects)
        {
            string json = obj.ToJson();
            jsons.Add(json);
        }

        return jsons;
    }
}