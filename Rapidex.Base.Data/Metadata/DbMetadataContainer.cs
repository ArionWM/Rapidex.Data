using Rapidex.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Metadata;
internal class DbMetadataContainer : IDbMetadataContainer
{
    public IDbScope Parent { get; }
    public ComponentDictionary<IDbEntityMetadata> Entities { get; } = new();

    public PredefinedDataCollection Data { get; } 

    public PredefinedDataCollection DemoData { get; } 

    public DbMetadataContainer(IDbScope parent)
    {
        this.Parent = parent;
        this.Data = new PredefinedDataCollection(this);
        this.DemoData = new PredefinedDataCollection(this);
    }

    public void Add(IDbEntityMetadata em)
    {
        if (em.Parent != null && em.Parent != this)
        {
            throw new InvalidOperationException($"Entity metadata '{em.Name}' is already associated with another metadata container.");
        }

        em.Parent = this;

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
        else
        {
            throw new KeyNotFoundException($"Entity with name '{entityName}' not found in metadata container.");
        }
    }
}
