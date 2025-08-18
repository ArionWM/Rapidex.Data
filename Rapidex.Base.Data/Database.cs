using Microsoft.Extensions.Configuration;
using Rapidex.Data.Configuration;
using Rapidex.Data.Entities;
using Rapidex.Data.Metadata;
using Rapidex.Data.Scopes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Rapidex.Data;

public static class Database
{

    
    public static DbEntityFactory EntityFactory { get; private set; } //Internal olmalı ancak DbDataModificationManager ların erişmesi lazım ?
    public static DbConfigurationManager Configuration { get; private set; }

    public static IFieldMetadataFactory FieldMetadataFactory { get; private set; }
    public static IDbEntityMetadataFactory EntityMetadataFactory { get; private set; }


    public static IDbScopeManager Scopes { get; private set; }
    public static IDbThreadScopeManager Current { get; private set; }



    [Obsolete("Use DbScope.Metadata instead", true)]
    public static IDbEntityMetadataManager Metadata { get; private set; }

    [Obsolete("Use DbScope.Metadata.Data instead", true)]
    public static IPredefinedValueProcessor PredefinedValues { get; private set; }

    static Database()
    {
        //Scopes = new DbScopeManager();
        //Configuration = new DbConfigurationManager();
        //EntityFactory = new DbEntityFactory();
        //ConcreteEntityMapper = new ConcreteEntityMapper(); //KAldırılacak ve tek bir mapper'a geçilecek
        //Metadata = new DbEntityMetadataManager(); //TODO: To services
        PredefinedValues = new PredefinedValueProcessor(); //TODO: To Metadata.Data ...
    }

    //public static void SetMetadataFactory<T>() where T : IDbEntityMetadataFactory
    //{
    //    Type metadataFactoryType = typeof(T);

    //    IDbEntityMetadataFactory dbEntityMetadataFactory = TypeHelper.CreateInstance<IDbEntityMetadataFactory>(metadataFactoryType);
    //    Metadata.SetEntityMetadataFactory(dbEntityMetadataFactory);
    //}

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

        //Database.Metadata.Setup(services);

    }

    public static void Start(IServiceProvider sp)
    {
        //TODO: Refactor this, should not required for this.
        sp.GetRapidexService<ISignalHub>()
            .NotNull("Signal hub not found");

        Database.Scopes = sp.GetRapidexService<IDbScopeManager>()
                        .NotNull("Db scope manager not found");

        Database.EntityFactory = sp.GetRapidexService<DbEntityFactory>()
            .NotNull("Db entity factory not found");

        Database.EntityMetadataFactory = sp.GetRapidexService<IDbEntityMetadataFactory>()
            .NotNull("Db entity metadata factory not found");

        Database.FieldMetadataFactory = sp.GetRapidexService<IFieldMetadataFactory>()
            .NotNull();


        Database.Configuration.LoadDbScopeDefinitions(sp);
    }

}
