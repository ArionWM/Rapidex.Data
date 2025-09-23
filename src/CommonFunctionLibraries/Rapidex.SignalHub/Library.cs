using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


[assembly: InternalsVisibleTo("Rapidex.UnitTest.SignalHub")]
[assembly: InternalsVisibleTo("Rapidex.UnitTest.Data")]
[assembly: InternalsVisibleTo("Rapidex.UnitTest.Data")]


namespace Rapidex.SignalHub;
internal class Library : AssemblyDefinitionBase, IRapidexAssemblyDefinition
{
    public override string Name => "Signal Hub Library";
    public override string TablePrefix => "sgn";
    public override int Index => 1;

    public override void SetupServices(IServiceCollection services)
    {
        services.AddRapidexSignalHub();
    }

    public override void Start(IServiceProvider serviceProvider)
    {
        serviceProvider.StartRapidexSignalHub();
    }
}
