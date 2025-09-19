using Rapidex.Data.Query;
using Rapidex.Data;
using System;
using Rapidex.Data.Behaviors;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Rapidex.Base.Application.Common")]

namespace Rapidex.Base.Data.Behaviors;

internal class Module:AssemblyDefinitionBase, IRapidexAssemblyDefinition
{
    public override string Name => "Data / Orm Library Behaviors";

    public override string TablePrefix => "data";


    public override int Index => 1000;

    public override void SetupServices(IServiceCollection services)
    {
        services.AddTransient<IEntityBehaviorDefinition, ArchiveEntity>();
        services.AddTransient<IEntityBehaviorDefinition, HasTags>();
        services.AddTransient<IEntityBehaviorDefinition, DefinitionEntity>();


    }

    public override void Start(IServiceProvider serviceProvider)
    {

    }
}
