using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Sample.Library.ConcreteEntities;
public class Address : DbConcreteEntityBase
{
    public string AddressName { get; set; }
    public string Street { get; set; }
    public string HouseNumber { get; set; }
    public Reference<City> City { get; set; }
    public Reference<State> State { get; set; }
    public Reference<Country> Country { get; set; }
    public string ZipCode { get; set; }
    public string Line1 { get; set; }
    public string Line2 { get; set; }
}
