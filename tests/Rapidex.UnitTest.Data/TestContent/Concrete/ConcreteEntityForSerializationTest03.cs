using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestContent;
internal class ConcreteEntityForSerializationTest03 : DbConcreteEntityBase
{
    public string Name { get; set; }

    public int NumberField { get; set; }

    public Reference<ConcreteEntityForSerializationTest02> ReferenceTo02 { get; set; }
}
