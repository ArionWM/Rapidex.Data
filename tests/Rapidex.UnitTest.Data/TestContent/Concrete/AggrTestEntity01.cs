using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestContent
{
    internal class AggrTestEntity01 : DbConcreteEntityBase
    {
        public string Name { get; set; }

        public int No { get; set; }
        public int Age { get; set; }

        public DateTimeOffset BirthDate { get; set; }


        public decimal Amount { get; set; }

        public double Weight { get; set; }

        public float Height { get; set; }


        public Currency Value { get; set; }


    }
}
