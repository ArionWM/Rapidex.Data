



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestContent
{
    public enum SeverityLevel
    {
        Low = 1,
        Normal = 2,
        High = 3,
    }

    public class ConcreteEntityForUpdateTests01 : DbConcreteEntityBase
    {
        public string Title { get; set; }

        public Reference<Contact> AssignedTo { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public DateTimeStartEnd PlannedDate { get; set; }

        public bool IsImportant { get; set; }

        public Text Description { get; set; }

        public int Status { get; set; }


        public Reference<Contact> Reporter { get; set; }

        public Enumeration<SeverityLevel> Severity { get; set; } = SeverityLevel.Normal;

    }
}
