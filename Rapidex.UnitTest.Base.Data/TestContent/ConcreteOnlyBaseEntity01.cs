using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestContent
{
    internal class ConcreteOnlyBaseEntity01 : DbConcreteEntityBase
    {
        public string Name { get; set; }
    }

    internal class ConcreteOnlyBaseReferencedEntity02 : DbConcreteEntityBase
    {
        public string Name { get; set; }

        public Reference<ConcreteOnlyBaseEntity01> Reference { get; set; }
    }
}
