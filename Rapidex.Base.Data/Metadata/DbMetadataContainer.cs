using Rapidex.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Metadata;
internal class DbMetadataContainer : IDbMetadataContainer
{
    public IDbScope DbScope { get; }
    public ComponentDictionary<IDbEntityMetadata> Entities { get; } = new();

    public PredefinedDataCollection Data { get; }

    public PredefinedDataCollection DemoData { get; }

    public DbMetadataContainer(IDbScope parent)
    {
        this.DbScope = parent;
        this.Data = new PredefinedDataCollection(this);
        this.DemoData = new PredefinedDataCollection(this);
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

    public void Add(IDbEntityMetadata em)
    {
        if (em.Parent != null && em.Parent != this)
        {
            throw new InvalidOperationException($"Entity metadata '{em.Name}' is already associated with another metadata container.");
        }

        em.Parent = this;

        IDbEntityMetadata existingEm = this.Get(em.Name);
        if (existingEm != null && existingEm.IsPremature)
        {
            this.MergeFieldsWithPremature(em, existingEm);
        }

        this.Entities.Set(em.Name, em);
    }

    public IDbEntityMetadata Get(string entityName)
    {
        return this.Entities.Get(entityName);
    }

    public IDbEntityMetadata[] GetAll()
    {
        return this.Entities.Values.ToArray();
    }

    public void Remove(string entityName)
    {
        if (this.Entities.ContainsKey(entityName))
        {
            this.Entities.Remove(entityName);
        }
    }
}
