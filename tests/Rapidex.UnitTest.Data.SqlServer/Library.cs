

global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using System.Threading.Tasks;

global using Microsoft.Data.SqlClient;
global using System.Data;

global using Rapidex.Data;
global using Rapidex.Data.SqlServer;
global using Rapidex;
global using Rapidex.UnitTests;
global using Rapidex.UnitTest.Data.Fixtures;
global using Rapidex.UnitTest.Data.TestBase;
global using Microsoft.Extensions.Logging;

global using Microsoft.Extensions.DependencyInjection;
global using System.Runtime.CompilerServices;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

[assembly: InternalsVisibleTo("Rapidex.UnitTest.Data")]
[assembly: InternalsVisibleTo("Rapidex.UnitTest.Base.Application")]
[assembly: InternalsVisibleTo("Rapidex.Base.Application.Common")]
[assembly: InternalsVisibleTo("Rapidex.Base.Application.Common")]

namespace Rapidex.UnitTest.Data.SqlServer;

internal class Library : AssemblyDefinitionBase, IRapidexAssemblyDefinition
{
    public override string Name => "UnitTest.Data.SqlServer";
    public override string TablePrefix => "utest";
    public override int Index => 1000;

    public override void SetupServices(IServiceCollection services)
    {
        Rapidex.Common.EnviromentCode = CommonConstants.ENV_UNITTEST;
    }

    public override void Initialize(IServiceProvider serviceProvider)
    {
    }

    public override void Start(IServiceProvider serviceProvider)
    {

    }


}
