using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Sample.Data.Basics.ConcreteEntitites;
internal class OrderLine : DbConcreteEntityBase
{
    /// <summary>
    /// If you want add parent reference as concrete field with name "ParentOrder" 
    /// If you don't add this field, the parent reference add automatically as virtual field.
    /// </summary>
    public Reference<Order> ParentOrder { get; set; }


    public Reference<Item> Item { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

}
