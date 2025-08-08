using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Rapidex.Data.Implementers;

internal class EntityFieldDefinitionImplementer : Dictionary<string, object>, IImplementer<IDbFieldMetadata>
{
    public string[] SupportedTags => null;
    public virtual bool Implemented { get; set; }
    public EntityFieldDefinitionImplementer() : base(StringComparer.InvariantCultureIgnoreCase)
    {

    }

    [Required]
    public string Name
    {
        get => this.Get("name").As<string>();
        set => this.Set("name", value);
    }

    [Required]
    public string? Type
    {
        get => this.Get("type").As<string>();
        set => this.Set("type", value);
    }

    [Required]
    public string? Caption
    {
        get => this.Get("caption").As<string>();
        set => this.Set("caption", value);
    }


    [Required]
    public bool? IsSealed
    {
        get => this.Get("isSealed").As<bool>();
        set => this.Set("isSealed", value);
    }

    [Required]
    public string? Reference
    {
        get => this.Get("reference").As<string>();
        set => this.Set("reference", value);
    }


    public override string ToString()
    {
        return this.Name;
    }

    public IUpdateResult Implement(IImplementHost host, IImplementer parentImplementer, ref object target)
    {
        var ures = new UpdateResult();

        EntityDefinitionImplementer entDefImplementer = (EntityDefinitionImplementer)parentImplementer;

        IDbEntityMetadata em = (IDbEntityMetadata)Database.Metadata.Get(entDefImplementer.Name);
        em.NotNull($"Entity metadata not found with '{entDefImplementer.Name}'");
        IDbFieldMetadata fm = em.Fields.Get(this.Name);
        if (fm == null)
        {
            ObjDictionary dict = new ObjDictionary();
            dict.Set(this);
            fm = em.AddFieldIfNotExist(this.Name, this.Type, dict);

            ures.Added(fm);
        }
        else
        {
            ures.Modified(fm);
        }

        this.MergeTo(host, fm);

        this.Implemented = true;

        return ures;
    }

}
