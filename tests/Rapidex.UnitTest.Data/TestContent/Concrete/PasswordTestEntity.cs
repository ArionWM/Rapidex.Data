using Rapidex.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestContent
{
    internal class PasswordTestEntity : DbConcreteEntityBase
    {
        public string Name { get; set; }

        public Password MyPassword { get; set; }

        public OneWayPassword MyOneWayPassword { get; set; }
    }
}
