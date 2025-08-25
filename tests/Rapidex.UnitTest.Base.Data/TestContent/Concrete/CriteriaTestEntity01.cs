using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestContent
{
    internal class CriteriaTestEntity01 : DbConcreteEntityBase
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public int No { get; set; }
        public int Age { get; set; }

        public DateTimeOffset BirthDate { get; set; }

        public bool IsActive { get; set; }

        public decimal Amount { get; set; }

        public double Weight { get; set; }

        public float Height { get; set; }

        public Guid UniqueId { get; set; }

        public Currency Value { get; set; }

        public Text Notes { get; set; }

        public Reference<ConcreteEntity01> Reference01 { get; set; }

        public Phone Phone { get; set; }

        public EMail EMail { get; set; }




    }
}
