using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;

public class PredefinedValueItem
{
    public IDbEntityMetadata EntityMetadata { get; set; }
    public Dictionary<long, ObjDictionary> Entities { get; set; } = new();

    public bool Override { get; set; }

    public PredefinedValueItem(IDbEntityMetadata entityMetadata, bool @override)
    {
        EntityMetadata = entityMetadata;
        Override = @override;
    }
}
