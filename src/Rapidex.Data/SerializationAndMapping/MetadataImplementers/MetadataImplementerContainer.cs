using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Rapidex.Data.SerializationAndMapping.MetadataImplementers;
internal class MetadataImplementerContainer
{
    public static Dictionary<string, Type> ImplementerTagMapping = new Dictionary<string, Type>();

    public static void Register(string tag, Type type)
    {
        string lowerTag = tag.ToLowerInvariant();
        ImplementerTagMapping[lowerTag] = type;
    }

    public static void Register(string[] tags, Type type)
    {
        foreach (string tag in tags)
        {
            string lowerTag = tag.ToLowerInvariant();
            ImplementerTagMapping[lowerTag] = type;
        }
    }


    public static void Register<IImplementer>(params string[] tags)
    {
        foreach (string tag in tags)
        {
            string lowerTag = tag.ToLowerInvariant();
            ImplementerTagMapping[lowerTag] = typeof(IImplementer);
        }
    }

    public static void Setup()
    {
        Type[] implementerTypes = Rapidex.Common.Assembly.FindDerivedClassTypes<IImplementer>();
        foreach (Type type in implementerTypes)
        {
            IImplementer implementer = (IImplementer)TypeHelper.CreateInstanceWithDI(type);
            if (implementer.SupportedTags.IsNOTNullOrEmpty())
                Register(implementer.SupportedTags, type);
        }

    }

    public static Type FindType(object obj)
    {
        if (obj.IsNullOrEmpty())
            throw new BaseValidationException("Object is null or empty");

        string tag = null;
        object objDict = obj as IDictionary<string, object>;
        if (objDict == null)
            objDict = obj as JsonObject;

        objDict.NotNull();

        switch (objDict)
        {
            case IDictionary<string, object> dict:
                tag = dict.Get("_tag").As<string>() ?? dict.FirstOrDefault().Key.As<string>().NotNull();
                break;
            case JsonObject node:
                tag = (node["_tag"] ?? node["type"] ?? node["Type"])?.GetValue<string>();
                break;
            default:
                throw new BaseValidationException("Object is not a dictionary");
        }

        tag.NotNull($"'tag' property not found. Can't find implementer");

        string lowerTag = tag.ToLowerInvariant();
        Type type = MetadataImplementerContainer.ImplementerTagMapping.Get(lowerTag);
        if (type == null)
        {
            throw new BaseValidationException($"Tag '{tag}' not supported");
        }

        return type;
    }
}
