using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data.Entities
{
    //TODO:  Sadece base schema@da ... 
    public class TagRecord : DbConcreteEntityBase
    {
        public string Entity { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }

        public string Description { get; set; }
    }
}
