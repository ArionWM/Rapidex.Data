using Rapidex.Data.Query;
using Rapidex.Data.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Rapidex.Data.FieldTypes;
using Rapidex.Data.Configuration;
using Rapidex.Data.Parsing;
using Rapidex.SignalHub;


[assembly: InternalsVisibleTo("Rapidex.UnitTest.Base.Data")]
[assembly: InternalsVisibleTo("Rapidex.UnitTest.Base.Data.SqlServer")]
[assembly: InternalsVisibleTo("Rapidex.UnitTest.Base.Data.PostgreServer")]
[assembly: InternalsVisibleTo("Rapidex.UnitTest.Base.Application")]
[assembly: InternalsVisibleTo("Rapidex.Base.Application.Common")]
[assembly: InternalsVisibleTo("Rapidex.Base.Application.Common")]

namespace Rapidex.Data;

internal class Library : AssemblyDefinitionBase, IRapidexAssemblyDefinition
{
    public override string Name => "Data / Orm Library";
    public override string TablePrefix => "data";
    public override int Index => 1000;

    public override void SetupServices(IServiceCollection services)
    {
        services.AddTransientForProd<IDbCriteriaParser, FilterTextParser>();
        services.AddTransientForProd<FilterTextParser, FilterTextParser>();

        services.AddTransientForProd<IEntitySerializationDataCreator, EntitySerializationDataCreator>();
        services.AddTransientForProd<IPredefinedValueProcessor, PredefinedValueProcessor>();
    }

    public override void SetupMetadata(IServiceCollection services)
    {
        FieldTypeJsonConverterBDT.Register();

        Database.Metadata.AddIfNotExist<SchemaInfo>()
            .MarkOnlyBaseSchema();
    }

    public override void Start(IServiceProvider serviceProvider)
    {
        EntitySignalProviderHelper.CreatePredefinedContent(Rapidex.Common.SignalHub);
    }
}
