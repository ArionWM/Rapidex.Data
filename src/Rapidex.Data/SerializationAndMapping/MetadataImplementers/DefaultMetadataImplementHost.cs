using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Rapidex.Data.Metadata;

namespace Rapidex.Data.SerializationAndMapping.MetadataImplementers;
internal class DefaultMetadataImplementHost : IMetadataImplementHost
{
    internal static JsonSerializerOptions JsonSerializerOptionsWithExcludes { get; private set; }

    public string ModuleName { get; }

    public IDbMetadataContainer Parent { get; protected set; }
    public IDbEntityMetadataFactory EntityMetadataFactory { get; }
    public IFieldMetadataFactory FieldMetadataFactory { get; }

    static DefaultMetadataImplementHost()
    {
        JsonSerializerOptionsWithExcludes = new();
        JsonHelper.SetDefaultOptions(JsonSerializerOptionsWithExcludes); //typeof(ImplementerJsonDiscriminatorSelectorConverter)
    }

    public DefaultMetadataImplementHost(IDbEntityMetadataFactory entityMetadataFactory, IFieldMetadataFactory fieldMetadataFactory)
    {
        this.FieldMetadataFactory = fieldMetadataFactory.NotNull("FieldMetadataFactory can't be null.");
        if (this.Parent != null)
            this.FieldMetadataFactory.SetParent(this.Parent);
        this.EntityMetadataFactory = entityMetadataFactory;
    }

    public void SetParent(IDbMetadataContainer parent)
    {
        parent.NotNull();
        this.Parent = parent;
        this.FieldMetadataFactory.SetParent(this.Parent);
    }


    protected IUpdateResult AddJsonObject(JsonNode node)
    {
        Type impType = MetadataImplementerContainer.FindType(node);
        IImplementer implementer = node.Deserialize(impType, DefaultMetadataImplementHost.JsonSerializerOptionsWithExcludes)
                .NotNull()
                .ShouldSupportTo<IImplementer>();

        object target = null;
        return implementer.Implement(this, null, ref target);
    }


    public IUpdateResult AddJson(string json)
    {
        json = json.Trim();
        if (json.IsNullOrEmpty())
            return new UpdateResult();

        JsonNode node = JsonNode.Parse(json, JsonHelper.DefaultJsonNodeOptions, JsonHelper.DefaultJsonDocumentOptions);

        if (node is JsonObject obj)
            return this.AddJsonObject(obj);

        //TODO: Support array of implementers
        throw new NotSupportedException("Invalid JSON, need single object");

    }

    public IUpdateResult AddYaml(string yaml)
    {
        UpdateResult ures = new();

        IEnumerable<string> jsons = YamlHelper.FromYamlManyToJson(yaml);
        foreach (string json in jsons)
        {
            ures.MergeWith(this.AddJson(json));
        }

        return ures;
    }

    public IUpdateResult AddConcrete(Type type)
    {
        UpdateResult ures = new();
        EntityMetadataBuilderFromConcrete cmi = new(this.Parent, this.EntityMetadataFactory, this.FieldMetadataFactory);
        var em = cmi.Add(type);
        ures.Added(em);
        return ures;
    }


}
