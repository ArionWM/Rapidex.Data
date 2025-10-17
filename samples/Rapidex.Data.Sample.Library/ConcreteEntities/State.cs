using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Sample.Library.ConcreteEntities;
public class State : DbConcreteEntityBase
{
    public string Name { get; set; }
    public string Code { get; set; } // ISO 3166-2 code
    
    public Reference<Country> ParentCountry { get; set; }
}

public class StateImplementer : IConcreteEntityImplementer<State>
{
    public void SetupMetadata(IDbScope owner, IDbEntityMetadata metadata)
    {
        var stateEm = metadata;
        stateEm.MarkOnlyBaseSchema();
    }
}
