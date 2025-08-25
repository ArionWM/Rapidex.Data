using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestContent
{
    internal class ConcreteEntityForN2NTest01 : DbConcreteEntityBase
    {
        public string Name { get; set; }
        public RelationN2N<ConcreteEntityForN2NTest02> Relation01 { get; set; }
    }
}
