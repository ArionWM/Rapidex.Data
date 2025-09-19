

using Rapidex.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestContent
{
    internal class ConcreteEntityForFilterTest : DbConcreteEntityBase
    {
        //public Text CustomerNumber { get; set; } ??
        public string Name { get; set; }
        public string Address { get; set; }
        public Phone Phone { get; set; }

        public int Age { get; set; }

        public DateTimeOffset Date { get; set; }

        public Enumeration<ContactTypeTest> ContactType { get; set; }

    }
}
