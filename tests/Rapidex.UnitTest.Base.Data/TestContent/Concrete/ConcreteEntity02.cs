using Rapidex.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestContent
{
    internal class ConcreteEntity02 : DbConcreteEntityBase
    {
        public Reference<ConcreteEntity01> Parent { get; set; } 


    }
}
