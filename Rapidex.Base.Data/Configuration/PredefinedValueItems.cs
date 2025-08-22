using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;

public class PredefinedValueItems
{
    public IDbEntityMetadata EntityMetadata { get; set; }
    public Dictionary<long, ObjDictionary> Entities { get; set; } = new();

    //public bool Override { get; set; }

    public PredefinedValueItems(IDbEntityMetadata entityMetadata)
    {
        EntityMetadata = entityMetadata;
    }

    public void Add(ObjDictionary item)
    {
        this.Entities.Set(item["Id"].As<long>(), item);
    }
}
