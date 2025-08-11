using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Metadata.Implementers;
internal class DefaultMetadataImplementHost : IImplementHost
{
    public string ModuleName { get; }

    public IDbMetadataContainer Parent { get; protected set; }

    public DefaultMetadataImplementHost(IDbMetadataContainer parent)
    {
        parent.NotNull("Parent can't be null. Use SetParent() method to set the parent before using this implementer.");
        this.Parent = parent;
    }

    public void SetParent(IDbMetadataContainer parent)
    {
        parent.NotNull();
        this.Parent = parent;
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
}
