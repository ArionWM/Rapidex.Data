using System.Runtime.CompilerServices;
using Rapidex.Data.Entities;
using Rapidex.Data.Metadata;
using Rapidex.Data.Scopes;
using Rapidex.Data.SerializationAndMapping.JsonConverters;
using Rapidex.Data.SerializationAndMapping.MetadataImplementers;


[assembly: InternalsVisibleTo("Rapidex.UnitTest.Data")]
[assembly: InternalsVisibleTo("Rapidex.UnitTest.Data.SqlServer")]
[assembly: InternalsVisibleTo("Rapidex.UnitTest.Data.PostgreServer")]
[assembly: InternalsVisibleTo("Rapidex.UnitTest.Base.Application")]
[assembly: InternalsVisibleTo("Rapidex.Base.Application.Common")]
[assembly: InternalsVisibleTo("Rapidex.Base.Application.Common")]

namespace Rapidex.Data;

internal class Library : AssemblyDefinitionBase, IRapidexMetadataReleatedAssemblyDefinition
{
    public override string Name => "Data / Orm Library";
    public override string TablePrefix => "data";
    public override int Index => 10;

    public override void SetupServices(IServiceCollection services)
    {
        services.AddSingletonForProd<IDbEntityMetadataFactory, DbEntityMetadataFactory>();
        services.AddTransientForProd<IFieldMetadataFactory, FieldMetadataFactory>();
        services.AddSingletonForProd<DbEntityFactory, DbEntityFactory>();

        services.AddSingletonForProd<IDbManager, DbScopeManager>();

        services.AddTransientForProd<IDbCriteriaParser, FilterTextParser>();
        services.AddTransientForProd<FilterTextParser, FilterTextParser>();

        services.AddTransientForProd<IMetadataImplementHost, DefaultMetadataImplementHost>();

        
        
        //ImplementerJsonDiscriminatorSelectorConverter.Register();
        EntityDataListImplementerJsonConverter.Register();
        EntityDataNestedListImplementerJsonConverter.Register();

        RapidexEntityDataConverterFactory.Register();
        DataTypeDefaultJsonConverter.Register();
        ReferenceJsonConverter<Image>.Register();
        ReferenceJsonConverter<Reference>.Register();
        EntityReferenceJsonConverter.Register();
        RelationN2NJsonConverter.Register();
        RelationOne2NJsonConverter.Register();
        ImageJsonConverter.Register();
        EntityJsonConverter.Register();
        EnumerationJsonConverter.Register();
        TagsJsonConverter.Register();
        DateTimeStartEndJsonConverter.Register();
        OneWayPasswordJsonConverter.Register();
        PasswordJsonConverter.Register();
        TextJsonConverter.Register();
        CurrencyJsonConverter.Register();

        services.AddDefaultJsonOptions();

        FieldMetadataCollection fmc = new();
        fmc.Setup(services);
    }

    public override void Start(IServiceProvider serviceProvider)
    {
        MetadataImplementerContainer.Setup();

        EntitySignalProviderHelper.CreatePredefinedContent(Rapidex.Signal.Hub);
    }

    public void SetupMetadata(IDbScope db)
    {
        db.Metadata.AddIfNotExist<SchemaInfo>()
            .MarkOnlyBaseSchema();

        db.Metadata.AddIfNotExist<BlobRecord>();
        db.Metadata.AddIfNotExist<GenericJunction>();
        db.Metadata.AddIfNotExist<TagRecord>();
    }
}
