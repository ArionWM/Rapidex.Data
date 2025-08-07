global using Rapidex;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("Rapidex.UnitTest.Base.Data")]
[assembly: InternalsVisibleTo("Rapidex.UnitTest.Base.Data.PostgreServer")]
[assembly: InternalsVisibleTo("Rapidex.UnitTest.Base.Application")]

namespace Rapidex.Data.PostgreServer;

internal class Module : AssemblyDefinitionBase, IRapidexAssemblyDefinition
{
    public override string Name => "Data / Orm Library Postgre extensions";
    public override string TablePrefix => "data";
    public override int Index => 1000;

    public override void SetupServices(IServiceCollection services)
    {
        services.AddSingletonForProd<IExceptionTranslator, PostgreSqlServerExceptionTranslator>();
    }

    public override void SetupMetadata(IServiceCollection services)
    {
    }

    public override void Start(IServiceProvider serviceProvider)
    {

    }
}
