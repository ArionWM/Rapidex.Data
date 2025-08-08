using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Implementers;
internal class EntityDataItemImplementer : Dictionary<string, object>, IImplementer
{
    public string[] SupportedTags => null;
    public virtual bool Implemented { get; set; }
    public EntityDataItemImplementer() : base(StringComparer.InvariantCultureIgnoreCase)
    {

    }

    public IUpdateResult Implement(IImplementHost host, IImplementer parentImplementer, ref object target)
    {
        ModuleMetadataImplementer mmi = host as ModuleMetadataImplementer;
        string entityName = this.Get("entity").ToString();

        var em = Database.Metadata.Get(entityName).NotNull($"Entity not found with: '{entityName}'");

        UpdateResult ures = new UpdateResult();

        //Dikkat scoped entity oluşturuyor, bunu uygular iken scope'ları kaldırmalı ya da dikkate almamalı !!
        IEntity entity = mmi.DbScope.Mapper.MapToNew(em, this);
        ures.Added(entity);
        target = entity;

        this.Implemented = true;
        return ures;
    }
}

