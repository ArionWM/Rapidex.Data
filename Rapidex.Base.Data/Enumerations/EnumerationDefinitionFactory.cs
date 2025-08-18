using Mapster.Utils;
using Rapidex.Data;
using Rapidex.Data.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace Rapidex.Data.Enumerations;

internal class EnumerationDefinitionFactory
{
    //public IDbEntityMetadataFactory EntityMetadataFactory { get; } //??

    internal IDbMetadataContainer Parent { get; }
    internal IDbEntityMetadataFactory EntityMetadataFactory { get; }
    internal IFieldMetadataFactory FieldMetadataFactory { get; }

    public EnumerationDefinitionFactory(IDbMetadataContainer parent, IDbEntityMetadataFactory emf, IFieldMetadataFactory fmf)
    {
        this.Parent = parent.NotNull();
        this.EntityMetadataFactory = emf.NotNull();
        this.FieldMetadataFactory = fmf.NotNull();
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

        IDbEntityMetadata bem = em.ShouldSupportTo<IDbEntityMetadata>($"Entity metadata '{em.Name}' should be IDbEntityMetadata");

        em.Fields.AddfNotExist<long>(this.FieldMetadataFactory, CommonConstants.FIELD_ID, CommonConstants.FIELD_ID, field => { field.IsSealed = true; });
        em.Fields.AddfNotExist<string>(this.FieldMetadataFactory, CommonConstants.FIELD_EXTERNAL_ID, CommonConstants.FIELD_EXTERNAL_ID, field => { field.IsSealed = true; });
        em.Fields.AddfNotExist<int>(this.FieldMetadataFactory, CommonConstants.FIELD_VERSION, CommonConstants.FIELD_VERSION, field => { field.IsSealed = true; });
        em.Fields.AddfNotExist<string>(this.FieldMetadataFactory, "Name", "Name");
        em.Fields.AddfNotExist<string>(this.FieldMetadataFactory, "Description", "Description");
        em.Fields.AddfNotExist<Color>(this.FieldMetadataFactory, "Color", "Color");
        em.Fields.AddfNotExist<bool>(this.FieldMetadataFactory, "IsArchived", "IsArchived"); // aslında ArchiveEntity ile ekleniyor
        em.Fields.AddfNotExist<bool>(this.FieldMetadataFactory, "Sealed", "Sealed");
        em.Fields.AddfNotExist<string>(this.FieldMetadataFactory, "Icon", "Icon");

        bem.AddBehavior("ArchiveEntity", true, false); //Diğer kitaplıkta kaldı?
        bem.AddBehavior("DefinitionEntity", true, false); //Diğer kitaplıkta kaldı?

        return em;
    }

    public IDbEntityMetadata CreateMetadata<TEnum>(string module = null, string prefix = null) where TEnum : Enum
    {
        Type type = typeof(TEnum);
        IDbEntityMetadata em = CreateMetadata(typeof(TEnum).Name, module, prefix);
        //em.ConcreteTypeName = type.FullName;
        return em;
    }

    public IDbEntityMetadata CreateMetadata(Type enumType, string module = null, string prefix = null)
    {
        IDbEntityMetadata em = CreateMetadata(enumType.Name, module, prefix);
        //em.ConcreteTypeName = enumType.FullName;
        return em;
    }

    public IDbEntityMetadata CreateMetadata(Enum enumeration, string module = null, string prefix = null)
    {
        return CreateMetadata(enumeration.GetType(), module, prefix);
    }

    public IDbEntityMetadata CreateMetadataFromJson(string jsonDefinition)
    {
        throw new NotImplementedException();
    }

    public IDbEntityMetadata Apply(Type enumType, string module = null, string prefix = null, Action<Enum, ObjDictionary> callb = null)
    {
        IDbEntityMetadata em = this.Parent.Get(enumType.Name);
        if (em == null || em.IsPremature)
        {
            em = this.CreateMetadata(enumType, module, prefix);
            this.Parent.AddIfNotExist(em);
        }
        else
        {
            return em;
        }

        var enumValEntities = this.GetValueEntities(em, enumType, callb);
        Database.PredefinedValues.Register(em, true, enumValEntities);
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

    public IDbEntityMetadata Apply(JsonNode json, string module = null)
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
        Database.PredefinedValues.Register(em, true, enumValEntities);
        return em;
    }

    public IDbEntityMetadata Apply(string json)
    {
        JsonDocumentOptions opt = new JsonDocumentOptions()
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        JsonNode jdoc = JsonObject.Parse(json, null, opt);
        string type = jdoc["type"].GetValue<string>();

        if (type.ToLowerInvariant() != "enumdefinition")
            throw new NotSupportedException($"Type '{type}' is not supported");

        int version = jdoc["version"].GetValue<int>();
        if (version > 1)
            throw new NotSupportedException($"Schema version '{version}' is not supported");

        return this.Apply(jdoc);
    }



}
