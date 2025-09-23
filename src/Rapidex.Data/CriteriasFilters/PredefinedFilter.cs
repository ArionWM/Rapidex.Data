using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;
public class PredefinedFilter : IPredefinedFilter
{
    public string Filter { get; set; }
    public string Hint { get; set; }
    public string Description { get; set; }
    public bool IsDefault { get; set; }

    public string Name { get; set; }

    public string Caption { get; set; }

    public string NavigationName => this.Name.ToNavigationName();
}
