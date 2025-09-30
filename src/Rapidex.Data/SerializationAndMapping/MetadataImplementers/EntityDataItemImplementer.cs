using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rapidex.Data.SerializationAndMapping.MetadataImplementers;
internal class EntityDataItemImplementer : Dictionary<string, object>, IImplementer
{
    public string[] SupportedTags => null;
    public virtual bool Implemented { get; set; }
    public EntityDataItemImplementer() : base(StringComparer.InvariantCultureIgnoreCase)
    {

    }

    public IUpdateResult Implement(IMetadataImplementHost host, IImplementer parentImplementer, ref object target)
    {
        string entityName = this.Get("entity")?.ToString();
        if (entityName.IsNullOrEmpty())
            throw new ArgumentNullException("'entity' key/value can't be null or empty. Use 'entity' property to set entity name.");

        var em = host.Parent.Get(entityName).NotNull($"Entity not found with: '{entityName}'");

        UpdateResult ures = new UpdateResult();

        //Dikkat scoped entity oluşturuyor, bunu uygular iken scope'ları kaldırmalı ya da dikkate almamalı !!
        IEntity entity = Database.EntityFactory.Create(em, host.Parent.DbScope, false);
        foreach (var field in this)
        {
            var fm = em.Fields.Get(field.Key);
            if (fm == null)
                continue;

            object value = field.Value;
            if (value is JsonElement je)
                value = je.GetValueAsOriginalType();

            entity.SetValue(fm.Name, value);
        }
        ures.Added(entity);
        target = entity;

        this.Implemented = true;
        return ures;
    }
}

