using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Metadata.Implementers;
internal class DefaultMetadataImplementHost : IMetadataImplementHost
{
    public string ModuleName { get; }

    public IDbMetadataContainer Parent { get; protected set; }
    public IDbEntityMetadataFactory EntityMetadataFactory { get; }
    public IFieldMetadataFactory FieldMetadataFactory { get; }

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


    public IUpdateResult AddJson(string json)
    {
        IImplementer imp = json.FromJson<IImplementer>();
        imp.NotNull("Implementer can't be null");

        object target = null;
        return imp.Implement(this, null, ref target);
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
