using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestContent
{
    public class ConcreteEntity03 : DbConcreteEntityBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Reference<ConcreteEntity01> Ref1 { get; set; }   
    }
}
