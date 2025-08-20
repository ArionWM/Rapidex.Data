using Mapster.Utils;
using Rapidex.Data.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace Rapidex.Data;

internal class EntityMetadataBuilderFromEnum : EntityMetadataBuilderBase
{
    public EntityMetadataBuilderFromEnum(IDbMetadataContainer parent, IDbEntityMetadataFactory dbEntityMetadataFactory, IFieldMetadataFactory fieldMetadataFactory) : base(parent, dbEntityMetadataFactory, fieldMetadataFactory)
    {
    }


    protected ObjDictionary GetEnumValueEntity(IDbEntityMetadata em, long value, string name, string color, string icon, bool isActive, bool @sealed, string description)
    {
        ObjDictionary rec = new ObjDictionary();
        rec["Id"] = value;
        rec["Name"] = name;
        rec["Color"] = color;
        rec["IsActive"] = isActive;
        rec["Sealed"] = @sealed;
        rec["Description"] = description;
        return rec;
    }

    protected ObjDictionary[] GetValueEntities(IDbEntityMetadata em, Type enumType, Action<Enum, ObjDictionary> callb)
    {
        var enumValues = Enum.GetValues(enumType);
        List<ObjDictionary> result = new List<ObjDictionary>();
        foreach (Enum eval in enumValues)
        {
            int i = Convert.ToInt32(eval);
            if (i < 1)
                throw new InvalidOperationException($"Enumeration '{em.Name}' invalid. Minimum enum value should bigger than 0. Minimum enumeration value is 1. Sample: enum ABC {{ Start = 1; Next = 2; }}");

            string name = Enum.GetName(enumType, i);

            ObjDictionary entValues = GetEnumValueEntity(em, i, name, null, null, true, true, null);
            callb?.Invoke(eval, entValues);
            result.Add(entValues);
        }

        return result.ToArray();
    }



    public IDbEntityMetadata CreateMetadata(string enumerationName, string module = null, string prefix = null)
    {
        IDbEntityMetadata em = this.EntityMetadataFactory.Create(enumerationName, module, prefix ?? DatabaseConstants.PREFIX_ENUMERATION);
        em.Parent= this.Parent;
        IDbEntityMetadata bem = em.ShouldSupportTo<IDbEntityMetadata>($"Entity metadata '{em.Name}' should be IDbEntityMetadata");

        em.Fields.AddIfNotExist<long>(CommonConstants.FIELD_ID, CommonConstants.FIELD_ID, field => { field.IsSealed = true; });
        em.Fields.AddIfNotExist<string>(CommonConstants.FIELD_EXTERNAL_ID, CommonConstants.FIELD_EXTERNAL_ID, field => { field.IsSealed = true; });
        em.Fields.AddIfNotExist<int>(CommonConstants.FIELD_VERSION, CommonConstants.FIELD_VERSION, field => { field.IsSealed = true; });
        em.Fields.AddIfNotExist<string>("Name", "Name");
        em.Fields.AddIfNotExist<string>("Description", "Description");
        em.Fields.AddIfNotExist<Color>("Color", "Color");
        em.Fields.AddIfNotExist<bool>("IsArchived", "IsArchived"); // aslında ArchiveEntity ile ekleniyor
        em.Fields.AddIfNotExist<bool>("Sealed", "Sealed");
        em.Fields.AddIfNotExist<string>("Icon", "Icon");

        em.PrimaryKey = em.Fields.Get(CommonConstants.FIELD_ID, true);

        bem.AddBehavior("ArchiveEntity", true, false); //Diğer kitaplıkta kaldı?
        bem.AddBehavior("DefinitionEntity", true, false); //Diğer kitaplıkta kaldı?

        return em;
    }

    public IDbEntityMetadata Add<TEnum>(string module = null, string prefix = null) where TEnum : Enum
    {
        Type type = typeof(TEnum);
        IDbEntityMetadata em = CreateMetadata(typeof(TEnum).Name, module, prefix);
        return em;
    }
    public IDbEntityMetadata Add(Enum enumeration, string module = null, string prefix = null)
    {
        return Add(enumeration.GetType(), module, prefix);
    }

    public IDbEntityMetadata CreateMetadataFromJson(string jsonDefinition)
    {
        throw new NotImplementedException();
    }

    public IDbEntityMetadata Add(Type enumType, string module = null, string prefix = null, Action<Enum, ObjDictionary> callb = null)
    {
        IDbEntityMetadata em = this.Parent.Get(enumType.Name);
        if (em == null || em.IsPremature)
        {
            em = CreateMetadata(enumType.Name, module, prefix);
            this.Parent.AddIfNotExist(em);
        }
        else
        {
            return em;
        }

        var enumValEntities = this.GetValueEntities(em, enumType, callb);
        this.Parent.Data.Add(em, true, enumValEntities);
        return em;
    }



    protected ObjDictionary[] GetValueEntities(IDbEntityMetadata em, JsonNode fieldsNode)
    {
        List<ObjDictionary> result = new List<ObjDictionary>();
        foreach (JsonNode fieldNode in fieldsNode.AsArray())
        {
            long i = fieldNode["id"]?.GetValue<long>() ?? 0;
            string name = fieldNode["name"]?.GetValue<string>();
            string color = fieldNode["color"]?.GetValue<string>();
            string icon = fieldNode["icon"]?.GetValue<string>();
            bool isActive = fieldNode["isActive"]?.GetValue<bool>() ?? false;
            bool @sealed = fieldNode["sealed"]?.GetValue<bool>() ?? false;
            string description = fieldNode["description"]?.GetValue<string>();

            result.Add(GetEnumValueEntity(em, i, name, color, icon, isActive, @sealed, description));
        }

        return result.ToArray();
    }

    public IDbEntityMetadata AddFromJson(JsonNode json, string module = null)
    {
        string name = json["name"].GetValue<string>();
        if (module.IsNullOrEmpty())
            module = json["module"]?.GetValue<string>();
        string prefix = json["dbPrefix"]?.GetValue<string>() ?? json["prefix"]?.GetValue<string>();
        IDbEntityMetadata em = this.Parent.Get(name);
        if (em == null || em.IsPremature)
        {
            em = this.CreateMetadata(name, module, prefix);
            this.Parent.AddIfNotExist(em);
        }

        JsonNode fieldsNode = json["values"];
        var enumValEntities = this.GetValueEntities(em, fieldsNode);
        this.Parent.Data.Add(em, true, enumValEntities);
        return em;
    }

    public IDbEntityMetadata AddFromJson(string json)
    {
        JsonDocumentOptions opt = new JsonDocumentOptions()
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        JsonNode jdoc = JsonNode.Parse(json, null, opt);
        string type = jdoc["type"].GetValue<string>();

        if (type.ToLowerInvariant() != "enumdefinition")
            throw new NotSupportedException($"Type '{type}' is not supported");

        int version = jdoc["version"].GetValue<int>();
        if (version > 1)
            throw new NotSupportedException($"Schema version '{version}' is not supported");

        return this.AddFromJson(jdoc);
    }



}
