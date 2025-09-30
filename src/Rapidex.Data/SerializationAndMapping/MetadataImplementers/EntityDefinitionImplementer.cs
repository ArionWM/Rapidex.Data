using Rapidex.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Rapidex.Data.SerializationAndMapping.MetadataImplementers;

internal class EntityDefinitionImplementer : IImplementer<IDbEntityMetadata>
{
    public virtual string[] SupportedTags => new string[] { "!entity", "entity", "entitydefinition" };
    public virtual bool Implemented { get; set; }

    [JsonIgnore]
    public IDbEntityMetadata EntityMetadata { get; set; } = null!;

    [Required]
    public string Name { get; set; }

    public string? DbPrefix { get; set; }

    public string? Module { get; set; }

    public string? TableName { get; set; }

    public bool? OnlyBaseSchema { get; set; }

    public string? CaptionField { get; set; }

    public string[]? Behaviors { get; set; }

    public string[]? Marks { get; set; }



    public List<EntityFieldDefinitionImplementer> Fields { get; set; } = new List<EntityFieldDefinitionImplementer>();

    public EntityDataNestedListImplementer? Data { get; set; }


    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }


    public EntityDefinitionImplementer()
    {
        
    }

    public virtual IUpdateResult Implement(IMetadataImplementHost host, IImplementer parentImplementer, ref object target)
    {
        var updateRes = new UpdateResult();

        IDbEntityMetadata em = host.Parent.Get(this.Name);
        if (em == null || em.IsPremature)
        {
            //em = Database.EntityMetadataFactory.Create(this.Name, moduleDef.NavigationName, moduleDef.TablePrefix);
            string moduleName = this.Module ?? host.ModuleName;
            em = Database.EntityMetadataFactory.Create(this.Name, moduleName, this.DbPrefix);

            if (this.OnlyBaseSchema.HasValue && this.OnlyBaseSchema.Value)
                em.MarkOnlyBaseSchema();

            host.Parent.Add(em);
            updateRes.Added(em);
        }
        else
        {
            updateRes.Modified(em);
        }

        this.EntityMetadata = em;

        this.MergeTo(host, em);
        em.Check();

        if(this.Behaviors.IsNOTNullOrEmpty())
        {
            foreach(var behavior in this.Behaviors)
            {
                em.AddBehavior(behavior, false, false);
            }
        }

        if (this.Marks.IsNOTNullOrEmpty())
        {
            foreach (string mark in this.Marks)
            {
                em.Tags.Add(mark);
            }
        }

        if (this.Data.IsNOTNullOrEmpty())
        {
            object trg = em;
            this.Data.Implement(host, this, ref trg);
        }


        IUpdateResult ures = em.ApplyBehaviors();

        foreach (object added in ures.AddedItems)
            if (added is IDbEntityMetadata aem)
            {
                host.Parent.Add(em);
            }

        //moduleDef.Entities.Add(em);
        target = em;
        em.Check();

        foreach (IEntityBehaviorDefinition behaviorDef in em.BehaviorDefinitions.List)
        {
            if (behaviorDef.ImplementerConfigKey.IsNullOrEmpty())
                continue;

            object behaviorData = this.ExtensionData.Get(behaviorDef.ImplementerConfigKey);

            if (behaviorData != null)
            {
                if (behaviorDef.ImplementerType == null)
                    throw new InvalidOperationException($"Implementer type not found for behavior '{behaviorDef.Name}' in data for '{behaviorDef.ImplementerConfigKey}' key");

                string jsonData = behaviorData.ToJson();
                IImplementer bImplementer = (IImplementer)jsonData.FromJson(behaviorDef.ImplementerType);

                //IImplementer bImplementer = TypeHelper.CreateInstance<IImplementer>(behaviorDef.ImplementerType);
                object trg = em;
                bImplementer.Implement(host, this, ref trg);
            }
            else
            {
                //See: EntityMetadata.ApplyBehavior / BehaviorDefinition.Setup
            }
        }

        //if (this.Injection.IsNOTNullOrEmpty())
        //{
        //    object trg = this.EntityMetadata;
        //    this.Injection.Implement(host, this, ref trg);
        //}

        //if (this.Automation.IsNOTNullOrEmpty())
        //{
        //    object trg = this.EntityMetadata;
        //    this.Automation.Implement(host, this, ref trg);
        //}

        //if (this.Filters.IsNOTNullOrEmpty())
        //{
        //    object trg = this.EntityMetadata;
        //    this.Filters.Implement(host, this, ref trg);
        //}

        this.Implemented = true;

        return updateRes;
    }

    public void Implement(IImplementTarget module, ref object target)
    {
        throw new NotImplementedException();
    }
}
