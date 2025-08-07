
using Rapidex.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestContent;

internal class ConcreteEntity01 : DbConcreteEntityBase
{
    //public Text CustomerNumber { get; set; } ??
    public string Name { get; set; }
    public string Address { get; set; }
    public Phone Phone { get; set; }

    public Currency CreditLimit1 { get; set; }

    public Currency CreditLimit2 { get; set; }

    public Currency Total { get { return this.CreditLimit1 + this.CreditLimit2; } }

    public Text Description { get; set; }

    public Image Picture { get; set; }

    public DateTimeOffset BirthDate { get; set; }

    public Enumeration<ContactType> ContactType { get; set; }

}
