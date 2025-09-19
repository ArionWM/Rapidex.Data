using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex;

public abstract class AssemblyDefinitionBase : IRapidexAssemblyDefinition
{
    private string _navigationName;

    public abstract string Name { get; }
    public abstract string TablePrefix { get; }
    public abstract int Index { get; }

    public virtual string NavigationName
    {
        get
        {
            return _navigationName ?? (_navigationName = this.Name.ToNavigationName());
        }
        protected set
        {
            _navigationName = value;
        }
    }

    public abstract void SetupServices(IServiceCollection services);
    
    public abstract void Start(IServiceProvider serviceProvider);
}
