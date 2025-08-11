
using Rapidex.Base.Common.Assemblies;
using Rapidex.Data.Entities;
using Rapidex.Data.Enumerations;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;

namespace Rapidex.Data.Metadata;

[Obsolete("Use DbScope.Metadata instead")]
internal class DbEntityMetadataManager : IDbEntityMetadataManager
{
    protected bool setupOk = false;

    protected List<Type> concreteDefinitions = new List<Type>();

    protected Dictionary<string, IDbEntityMetadata> metadataIndex = new Dictionary<string, IDbEntityMetadata>(StringComparer.InvariantCultureIgnoreCase);

    public IFieldMetadataFactory FieldMetadataFactory { get; }
    public IDbEntityMetadataFactory EntityMetadataFactory { get; protected set; }

    public EnumerationDefinitionFactory EnumerationDefinitionFactory { get; }

    public DbEntityMetadataManager()
    {
        this.EntityMetadataFactory = new DbEntityMetadataFactoryBase(); //Şimdilik ..
        this.FieldMetadataFactory = new FieldMetadataFactory(this);
        this.EnumerationDefinitionFactory = new EnumerationDefinitionFactory();
    }

    public DbEntityMetadataManager(IDbEntityMetadataFactory mFactory)
    {
        this.EntityMetadataFactory = mFactory;
        this.FieldMetadataFactory = new FieldMetadataFactory(this);
        this.EnumerationDefinitionFactory = new EnumerationDefinitionFactory();
    }

    public virtual void SetEntityMetadataFactory(IDbEntityMetadataFactory factory)
    {
        this.EntityMetadataFactory = factory;
    }

    protected virtual void ValidateConcreteType(Type type)
    {
        if (type.IsInterface || type.IsAbstract)
            throw new MetadataException($"Can't use interfaces or abstract classes: '{type.Name}'");

        type.Name.ValidateInvariantName();
    }


    protected virtual void AddField(IDbEntityMetadata em, PropertyInfo propertyInfo)
    {
        IDbFieldMetadata fm = this.FieldMetadataFactory.CreateType(em, propertyInfo.PropertyType, propertyInfo.Name, null);
        em.AddField(fm);
    }

    protected virtual IDbEntityMetadata CreateMetadata(Type type)
    {
        IDbEntityMetadata em = this.metadataIndex.Get(type.Name); //Daha önce prematüre eklenmiş olabilir
        if (em == null)
        {
            em = this.EntityMetadataFactory.Create(type.Name);
        }
        else
        {
            if (!em.IsPremature)
                throw new MetadataException($"'{type.Name}' already available");
        }

        em.ConcreteTypeName = type.FullName;

        PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);

        foreach (PropertyInfo inf in propertyInfos)
        {
            if (!inf.IsUseful())
                continue;

            this.AddField(em, inf);
        }

        if (em.IsPremature)
            em.IsPremature = false;

        return em;
    }


    public static string[] CaptionFieldNames = new string[] { "Caption", "Title", "Subject", "FullName", "Name" };
    protected virtual void CheckCaptionField(IDbEntityMetadata em)
    {
        foreach (string captionFieldName in CaptionFieldNames)
        {
            if (em.Fields.ContainsKey(captionFieldName))
            {
                em.Caption = em.Fields[captionFieldName];
                break;
            }
        }
    }

    protected virtual void CheckEnumerations(IDbEntityMetadata em)
    {
        foreach (var field in em.Fields.Values)
        {
            if (field.Type.IsEnum)
            {
                throw new MetadataException($"{em.Name} / {field.Name} is system.Enum. Should convert to Enumeration<Enum>.");
            }

            if (field.Type.IsSupportTo<Rapidex.Data.Enumeration>())
            {
                if (field.Type.IsGenericType)
                {
                    Type enumType = field.Type.GetGenericArguments()[0];
                    //Log.Debug(string.Format("Enumeration field: {0} / {1} / {}", em.Name, field.Name, enumType.Name));

                    this.EnumerationDefinitionFactory.Apply(enumType);
                }
            }
        }
    }

    protected virtual void MergeFieldsWithPremature(IDbEntityMetadata em, IDbEntityMetadata premature)
    {
        foreach (IDbFieldMetadata field in premature.Fields.Values)
        {
            if (!em.Fields.ContainsKey(field.Name))
            {
                em.AddField(field);
            }
        }
    }

    public virtual void Check(IDbEntityMetadata em)
    {
        em.Fields.AddfNotExist<long>(CommonConstants.FIELD_ID, CommonConstants.FIELD_ID, field => { field.IsSealed = true; });
        em.Fields.AddfNotExist<string>(CommonConstants.FIELD_EXTERNAL_ID, CommonConstants.FIELD_EXTERNAL_ID, field => { field.IsSealed = true; });
        em.Fields.AddfNotExist<int>(CommonConstants.FIELD_VERSION, CommonConstants.FIELD_VERSION, field => { field.IsSealed = true; });
        em.PrimaryKey = em.Fields.Get(CommonConstants.FIELD_ID, true);

        em.TableName = em.Prefix.IsNullOrEmpty() ? em.Name : $"{em.Prefix}_{em.Name}";

        this.CheckCaptionField(em);
        this.CheckEnumerations(em);
    }

    internal virtual void ScanLibrariesForConcreteDefinitions()
    {
        this.AddIfNotExist<SchemaInfo>();
        this.AddIfNotExist<BlobRecord>();
        //this.AddIfNotExist<References>();
        this.AddIfNotExist<GenericJunction>();
        this.AddIfNotExist<TagRecord>();

        Log.Debug("Database", "Metadata; Scanning libraries for concrete definitions");
        var types = Common.Assembly.FindDerivedClassTypesWithAssemblyInfo<IConcreteEntity>();
        foreach (var tinfo in types)
        {
            var availEm = this.Get(tinfo.type.Name);
            if (availEm != null && !availEm.IsPremature)
            {
                availEm.Prefix = tinfo.assembly.DatabaseEntityPrefix;
                continue;
            }

            this.Add(tinfo.type, tinfo.assembly.NavigationName, tinfo.assembly.DatabaseEntityPrefix);
        }
    }

    public virtual IDbEntityMetadata AddPremature(string entityName)
    {
        if (this.metadataIndex.ContainsKey(entityName))
            throw new MetadataException($"'{entityName}' already available");

        IDbEntityMetadata prematureEm = this.EntityMetadataFactory.Create(entityName);// new DbEntityMetadata(entityName);
        prematureEm.IsPremature = true;

        this.metadataIndex.Set(prematureEm.Name, prematureEm);

        return prematureEm;
    }

    public virtual IDbEntityMetadata Add(Type type, string module = null, string prefix = null)
    {
        Log.Debug("Database", $"Metadata; Add: {type.FullName}");

        try
        {
            this.ValidateConcreteType(type);

            IDbEntityMetadata em = this.CreateMetadata(type);
            em.ModuleName = module;
            em.Prefix = prefix;

            if (em.ModuleName.IsNullOrEmpty())
            {
                //Modülünü bulacağız
                var aInfo = Common.Assembly.FindAssemblyInfo(type.Assembly);
                em.ModuleName = aInfo.NavigationName;

                if (em.Prefix.IsNullOrEmpty())
                    em.Prefix = aInfo.DatabaseEntityPrefix;
            }

            if (em.Prefix.IsNullOrEmpty())
            {
                em.Prefix = DatabaseConstants.PREFIX_DEFAULT;
            }

            this.Add(em);
            this.concreteDefinitions.Add(type);

            return em;
        }
        catch (Exception ex)
        {
            ex.Log();
            throw ex.Translate();
        }
    }

    public virtual IDbEntityMetadata Add<TDeclaration>(string module = null, string prefix = null) where TDeclaration : IDbDefinition
    {
        return this.Add(typeof(TDeclaration), module, prefix);
    }

    public virtual void Add(IDbEntityMetadata em)
    {
        this.Check(em);

        IDbEntityMetadata existingEm = this.metadataIndex.Get(em.Name);
        if (existingEm != null && existingEm.IsPremature)
        {
            this.MergeFieldsWithPremature(em, existingEm);
        }

        this.metadataIndex.Set(em.Name, em);
    }

    public virtual IDbEntityMetadata AddFromEnum<TEnum>(string module = null, string prefix = null, Action<Enum, ObjDictionary> callb = null) where TEnum : System.Enum
    {
        return this.EnumerationDefinitionFactory.Apply(typeof(TEnum), module, prefix, callb);
    }

    protected virtual IDbFieldMetadata AddXmlField(IDbEntityMetadata em, XmlElement fmElement)
    {
        string fieldName = fmElement["name"].Value;
        string fieldType = fmElement["type"].Value?.ToLowerInvariant();

        if (fieldName.IsNullOrEmpty())
            throw new MetadataException("Field name is required");

        if (fieldType.IsNullOrEmpty())
            throw new MetadataException("Field type is required");

        ObjDictionary values = new ObjDictionary();
        foreach (XmlNode valueNode in fmElement.ChildNodes)
        {
            values.Set(valueNode.Name, valueNode.Value);
            //values.Set(valueNode["name"].Value, valueNode["value"].Value);
        }


        IDbFieldMetadata fm = this.FieldMetadataFactory.CreateType(em, fieldType, fieldName, values);
        return fm;
    }
    protected virtual IDbFieldMetadata AddJsonField(IDbEntityMetadata em, string fieldName, string fieldType, ObjDictionary values)
    {
        IDbFieldMetadata fm = this.FieldMetadataFactory.CreateType(em, fieldType, fieldName, values);
        return fm;
    }

    protected virtual IDbFieldMetadata AddJsonField(IDbEntityMetadata em, JsonNode jsonNode)
    {
        string fieldName = jsonNode["name"].GetValue<string>();
        string fieldType = jsonNode["type"].GetValue<string>()?.ToLowerInvariant();

        if (fieldName.IsNullOrEmpty())
            throw new MetadataException("Field name is required");

        if (fieldType.IsNullOrEmpty())
            throw new MetadataException("Field type is required");

        //How to get all property names?

        ObjDictionary values = new ObjDictionary();
        foreach (var kv in jsonNode.AsObject().AsEnumerable())
        {
            var value = kv.Value.GetValueAsOriginalType();
            values.Set(kv.Key, value);
        }

        IDbFieldMetadata fm = this.AddJsonField(em, fieldName, fieldType, values);
        return fm;
    }

    protected virtual IDbEntityMetadata AddEntityFromJson(JsonNode jdoc, string module = null)
    {
        string name = jdoc["name"].NotEmpty().GetValue<string>();
        string primaryKeyFieldName = jdoc["primaryKey"]?.GetValue<string>() ?? CommonConstants.FIELD_ID;
        if (module.IsNullOrEmpty())
            module = jdoc["module"]?.GetValue<string>();
        string prefix = jdoc["dbPrefix"]?.GetValue<string>() ?? jdoc["prefix"]?.GetValue<string>();

        IDbEntityMetadata em = this.metadataIndex.Get(name); //Daha önce prematüre eklenmiş olabilir
        if (em == null || em.IsPremature)
            em = this.EntityMetadataFactory.Create(name, module, prefix);

        bool? onlyBaseSchema = jdoc["onlyBase"]?.GetValue<bool>() ?? jdoc["onlyBaseSchema"]?.GetValue<bool>();
        if (onlyBaseSchema.HasValue)
            em.OnlyBaseSchema = onlyBaseSchema.Value;

        JsonNode fieldsNode = jdoc["fields"].NotNull();
        foreach (JsonNode fieldNode in fieldsNode.AsArray())
        {
            IDbFieldMetadata fm = this.AddJsonField(em, fieldNode);
            em.AddFieldIfNotExist(fm);
        }

        if (em.Fields.Get(primaryKeyFieldName) == null)
        {
            em.Fields.AddfNotExist<long>(primaryKeyFieldName, CommonConstants.FIELD_ID);
        }

        this.Add(em);

        JsonNode behaviorsNode = jdoc["behaviors"];
        if (behaviorsNode != null)
        {
            foreach (JsonNode bnode in behaviorsNode.AsArray())
            {
                string behaviorName = bnode.GetValue<string>();
                em.AddBehavior(behaviorName, true, false);
            }
        }

        if (jdoc["data"] != null)
        {
            Database.PredefinedValues.Register(em, jdoc);
        }



        return em;
    }

    protected virtual IDbEntityMetadata AddEnumFromJson(JsonNode jnode, string module = null)
    {
        return this.EnumerationDefinitionFactory.Apply(jnode, module);
    }

    public virtual IDbEntityMetadata AddFromJson(string json, string module = null)
    {
        JsonDocumentOptions opt = new JsonDocumentOptions()
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        JsonNode jdoc = JsonObject.Parse(json, null, opt);


        int version = jdoc["version"]
            .NotEmpty("'version' is required")
            .GetValue<int>();

        if (version > 1)
            throw new NotSupportedException($"Schema version '{version}' is not supported");

        string type = jdoc["type"]
            .NotEmpty("'type' is required")
            .GetValue<string>()
            .NotEmpty("'type' is required");

        switch (type.ToLowerInvariant())
        {
            case "entitydefinition":
                return this.AddEntityFromJson(jdoc, module);
            case "enumdefinition":
                return this.AddEnumFromJson(jdoc, module);
            default:
                throw new NotSupportedException($"Schema type '{type}' is not supported");
        }

    }


    public virtual IDbEntityMetadata Get(string entityName)
    {
        IDbEntityMetadata em = this.metadataIndex.Get(entityName, false);
        return em;
    }

    public virtual IDbEntityMetadata[] GetAll()
    {
        return this.metadataIndex.Values.ToArray();
    }


    //Load extras ... (Filters < behaviors vs?)

    public virtual void LoadFromDb()
    {
        throw new NotImplementedException();
    }

    public virtual void Reload()
    {
        throw new NotImplementedException();
    }

    public virtual void Setup(IServiceCollection services)
    {
        if (setupOk)
            return;

        this.FieldMetadataFactory.Setup(services);
        this.ScanLibrariesForConcreteDefinitions();

        setupOk = true;
    }

    public virtual void Start(IServiceProvider serviceProvider)
    {
        this.FieldMetadataFactory.Start(serviceProvider);
    }

    void IDbEntityMetadataManager.Clear()
    {
        this.concreteDefinitions.Clear();
        this.metadataIndex.Clear();
        setupOk = false;
    }

    void IDbEntityMetadataManager.Remove(IDbEntityMetadata em)
    {
        if (metadataIndex.ContainsKey(em.Name))
            metadataIndex.Remove(em.Name);
    }
}
