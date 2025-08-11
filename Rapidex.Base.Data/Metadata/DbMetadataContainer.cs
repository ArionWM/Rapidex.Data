using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Metadata;
internal class DbMetadataContainer : IDbMetadataContainer
{
    public ComponentDictionary<IDbEntityMetadata> Entities { get; } = new();

    public List<IEntity> Data { get; } = new();

    public List<IEntity> DemoData { get; } = new();

    public void Add(IDbEntityMetadata em)
    {
        this.Entities.Set(em.Name, em); 
    }

    public IDbEntityMetadata Get(string entityName)
    {
        return this.Entities.Get(entityName);
    }
}
