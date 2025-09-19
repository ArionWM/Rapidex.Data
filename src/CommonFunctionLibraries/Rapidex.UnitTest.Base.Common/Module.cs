global using Rapidex;
global using Rapidex.UnitTests;

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Rapidex.UnitTest.Base.Common;

internal class Module : AssemblyDefinitionBase, IRapidexAssemblyDefinition
{
    public override string Name => "UnitTest.Common";
    public override string TablePrefix => "utest";
    public override int Index => 1000;

    public override void SetupServices(IServiceCollection services)
    {
        Rapidex.Common.EnviromentCode = CommonConstants.ENV_UNITTEST;
    }


    public override void Start(IServiceProvider serviceProvider)
    {

    }
}
