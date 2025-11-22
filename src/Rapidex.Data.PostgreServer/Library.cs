global using Rapidex;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("Rapidex.UnitTest.Data")]
[assembly: InternalsVisibleTo("Rapidex.UnitTest.Data.PostgreServer")]
[assembly: InternalsVisibleTo("Rapidex.UnitTest.Base.Application")]

namespace Rapidex.Data.PostgreServer;

internal class Library : AssemblyDefinitionBase, IRapidexAssemblyDefinition
{
    public override string Name => "Data / Orm Library Postgre extensions";
    public override string TablePrefix => "data";
    public override int Index => 1000;

    public override void SetupServices(IServiceCollection services)
    {
        services.AddSingletonForProd<IExceptionTranslator, PostgreSqlServerExceptionTranslator>();
        services.AddKeyedTransient<IDbProvider, PostgreSqlServerProvider>("PostgreSqlServerProvider");
        services.AddKeyedTransient<IDbProvider, PostgreSqlServerProvider>("Rapidex.Data.PostgreServer.PostgreSqlServerProvider");
        services.AddKeyedTransient<IDbStructureProvider, PostgreSqlStructureProvider>("postgresql");
    }

    public override void Initialize(IServiceProvider serviceProvider)
    {
    }

    public override void Start(IServiceProvider serviceProvider)
    {

    }
}
