using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Sample.Library.ConcreteEntities;
public class City : DbConcreteEntityBase
{
    public string Name { get; set; }
    public string ZipCode { get; set; }
    public Reference<State> ParentState { get; set; }
    public Reference<Country> ParentCountry { get; set; }
}

public class CityImplementer : IConcreteEntityImplementer<City>
{
    public void SetupMetadata(IDbScope owner, IDbEntityMetadata metadata)
    {
        var cityEm = metadata;
        cityEm.MarkOnlyBaseSchema();
    }
}