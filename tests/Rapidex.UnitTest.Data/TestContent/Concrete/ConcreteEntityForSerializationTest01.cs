using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestContent;
internal class ConcreteEntityForSerializationTest01 : DbConcreteEntityBase
{
    public string Name { get; set; }
    public RelationN2N<ConcreteEntityForSerializationTest03> Relation01 { get; set; }
}
