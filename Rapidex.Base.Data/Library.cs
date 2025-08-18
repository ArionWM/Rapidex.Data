using Rapidex.Data.Configuration;
using Rapidex.Data.Entities;
using Rapidex.Data.FieldTypes;
using Rapidex.Data.Metadata;
using Rapidex.Data.Metadata.Implementers;
using Rapidex.Data.Parsing;
using Rapidex.Data.Query;
using Rapidex.SignalHub;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;


[assembly: InternalsVisibleTo("Rapidex.UnitTest.Base.Data")]
[assembly: InternalsVisibleTo("Rapidex.UnitTest.Base.Data.SqlServer")]
[assembly: InternalsVisibleTo("Rapidex.UnitTest.Base.Data.PostgreServer")]
[assembly: InternalsVisibleTo("Rapidex.UnitTest.Base.Application")]
[assembly: InternalsVisibleTo("Rapidex.Base.Application.Common")]
[assembly: InternalsVisibleTo("Rapidex.Base.Application.Common")]

namespace Rapidex.Data;

internal class Library : AssemblyDefinitionBase, IRapidexMetadataReleatedAssemblyDefinition
{
    public override string Name => "Data / Orm Library";
    public override string TablePrefix => "data";
    public override int Index => 1000;

    public override void SetupServices(IServiceCollection services)
    {
        services.AddSingletonForProd<IDbEntityMetadataFactory, DbEntityMetadataFactoryBase>();
        services.AddTransientForProd<IDbCriteriaParser, FilterTextParser>();
        services.AddTransientForProd<FilterTextParser, FilterTextParser>();

        services.AddTransientForProd<IEntitySerializationDataCreator, EntitySerializationDataCreator>();
        services.AddTransientForProd<IPredefinedValueProcessor, PredefinedValueProcessor>();
        services.AddTransientForProd<IMetadataImplementHost, DefaultMetadataImplementHost>();

        FieldTypeJsonConverterBDT.Register();
    }

    public override void Start(IServiceProvider serviceProvider)
    {
        EntitySignalProviderHelper.CreatePredefinedContent(Rapidex.Common.SignalHub);
    }

    public void SetupMetadata(IServiceProvider sp, IDbScope scope)
    {
        scope.Metadata.AddIfNotExist<SchemaInfo>()
            .MarkOnlyBaseSchema();

        scope.Metadata.AddIfNotExist<SchemaInfo>();
        scope.Metadata.AddIfNotExist<BlobRecord>();
        scope.Metadata.AddIfNotExist<GenericJunction>();
        scope.Metadata.AddIfNotExist<TagRecord>();
    }
}
