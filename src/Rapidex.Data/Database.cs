using Microsoft.Extensions.Configuration;
using Rapidex.Data.Cache;

namespace Rapidex.Data;

public static class Database
{
    public static DbEntityFactory EntityFactory { get; private set; } //Internal olmalı ancak DbDataModificationManager ların erişmesi lazım ?
    public static DbConfigurationManager Configuration { get; private set; }
    public static IDbEntityMetadataFactory EntityMetadataFactory { get; private set; }
    public static ICache Cache { get; private set; }

    [Obsolete("Use Database.Dbs instead", true)]
    public static IDbManager Scopes { get => Database.Dbs; set => Database.Dbs = value; }

    /// <summary>
    /// Database manager, contains all db objects
    /// </summary>
    public static IDbManager Dbs { get; private set; }

    static Database()
    {
        //Scopes = new DbScopeManager();
        //Configuration = new DbConfigurationManager();
        //EntityFactory = new DbEntityFactory();
        //ConcreteEntityMapper = new ConcreteEntityMapper(); //KAldırılacak ve tek bir mapper'a geçilecek
        //Metadata = new DbEntityMetadataManager(); //TODO: To services
        //PredefinedValues = new PredefinedValueProcessor(); //TODO: To Metadata.Data ...
    }

    /// <summary>
    /// Load configuration and prepare for work
    /// </summary>
    /// <param name="configuration"></param>
    public static void Setup(IServiceCollection services, IConfiguration configuration = null)
    {
        Database.Configuration = new DbConfigurationManager();
        if (configuration != null)
            Database.Configuration.Configuration = configuration;
        else
            Database.Configuration.Configuration = Common.Configuration;

        Database.Configuration.Setup();
    }

    private static void InitializeCache(IServiceProvider sp)
    {
        CacheFactory cacheFactory = sp.GetRequiredService<CacheFactory>();
        Database.Cache = cacheFactory.Create();
    }

    public static void Start(IServiceProvider sp)
    {
        //TODO: Refactor this, should not required for this.
        sp.GetRapidexService<ISignalHub>()
            .NotNull("Signal hub not found");

        Database.Dbs = sp.GetRapidexService<IDbManager>()
                        .NotNull("Db scope manager not found");

        Database.EntityFactory = sp.GetRapidexService<DbEntityFactory>()
            .NotNull("Db entity factory not found");

        Database.EntityMetadataFactory = sp.GetRapidexService<IDbEntityMetadataFactory>()
            .NotNull("Db entity metadata factory not found");

        Database.InitializeCache(sp);

        //Database.FieldMetadataFactory = sp.GetRapidexService<IFieldMetadataFactory>()
        //    .NotNull();

        Database.Dbs.AddMainDbIfNotExists();

        Database.Configuration.LoadDbScopeDefinitions();
    }

}
