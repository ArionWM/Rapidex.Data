using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestContent.Concrete;
internal class ConcreteEntityForImplementationTests02 : DbConcreteEntityBase
{
    public string Name { get; set; }
    public Percent Percent { get; set; }
    public Enumeration<MyEnum01> EnumField { get; set; }
    public DateTimeOffset? DateTimeField { get; set; }
}

internal class ConcreteEntityForImplementationTests02Implementer : IConcreteEntityImplementer<ConcreteEntityForImplementationTests02>
{
    public void SetupMetadata(IDbScope owner, IDbEntityMetadata metadata)
    {
       
    }
}

