using Rapidex.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestContent
{
    public class ConcreteEntity04 : DbConcreteEntityBase
    {
        public int Number { get; set; }
        public RelationOne2N<ConcreteEntity03> Details01 { get; set; }

        public Text Description { get; set; }

    }
}
