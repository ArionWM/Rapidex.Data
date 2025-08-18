
using FluentAssertions.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rapidex.Data;
using Rapidex.Data.Entities;
using Rapidex.Data.Metadata;
using Rapidex.Data.Scopes;
using Rapidex.UnitTest.Base.Common.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.Fixtures;

public class DbFixture : DefaultEmptyFixture, ICoreTestFixture
{
    public DbFixture()
    {
        Database.Configuration.DatabaseSectionParentName = null;

        this.Init();
    }

    protected override void SetupInternal(IServiceCollection services)
    {
        base.SetupInternal(services);

        Rapidex.SignalHub.Library mhLib = new Rapidex.SignalHub.Library();
        mhLib.SetupServices(services);

        Rapidex.Data.Library module = new Rapidex.Data.Library();
        module.SetupMetadata(services);

        module.SetupServices(services);

        Database.Setup(services);

    }

    public virtual void DropAllTablesInDatabase(IDbProvider provider, bool clearCaches)
    {
        var testHelper = provider.GetTestHelper();
        testHelper.DropAllTablesInDatabase(provider.ConnectionString);

        if (clearCaches)
        {
            this.ClearCaches();
        }
    }


    public override void Init()
    {
        if (initialized)
            return;


        //Database.SetMetadataFactory<AppLevelEntityMetadataFactory>();

        base.Init();


        Rapidex.SignalHub.Library mhLib = new Rapidex.SignalHub.Library();
        mhLib.Start(this.ServiceProvider);

        Rapidex.Data.Library module = new Rapidex.Data.Library();
        module.Start(this.ServiceProvider);

        DbConnectionInfo connectionInfo = Database.Configuration.ConnectionInfo.Get(DatabaseConstants.MASTER_DB_NAME);
        DbProviderFactory dbManagersFactory = new DbProviderFactory();
        IDbProvider provider = dbManagersFactory.CreateProvider(connectionInfo);
        this.DropAllTablesInDatabase(provider, false);

        var dbScope = Database.Scopes.AddMainDbIfNotExists();

        Database.Metadata.AddIfNotExist<SchemaInfo>()
            .MarkOnlyBaseSchema();
        Database.Metadata.AddIfNotExist<BlobRecord>();
        Database.Metadata.AddIfNotExist<TagRecord>();
        Database.Metadata.AddIfNotExist<GenericJunction>();
        //Database.Metadata.AddIfNotExist<Contact>();
        //Database.Metadata.AddIfNotExist<Department>();
        //Database.Metadata.AddIfNotExist<TenantRecord>();

        dbScope.Structure.ApplyAllStructure();


        initialized = true;
    }


    internal virtual DbSqlServerConnection CreateSqlServerConnection()
    {
        DbConnectionInfo dbc = Database.Configuration.ConnectionInfo.Get(DatabaseConstants.MASTER_DB_NAME);
        SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(dbc.ConnectionString);
        DbSqlServerConnection connection = new DbSqlServerConnection(sqlConnectionStringBuilder.ConnectionString);

        connection.Execute($"USE [{Database.Scopes.Db().DatabaseName}]");
        return connection;
    }



    public override void ClearCaches()
    {
        //InMemCache clear
        //Common caches clear
        ((Rapidex.Data.Scopes.DbScopeManager)Database.Scopes).ClearCache();
        ((IDbEntityMetadataManager)Database.Metadata).Clear();
        Database.PredefinedValues.Clear();
        Database.Metadata.Setup(null);

        var db = Database.Scopes.AddMainDbIfNotExists();


    }

    //public virtual void DropDatabase(string databaseName)
    //{
    //    DbConnectionInfo dbc = Database.Configuration.ConnectionInfo.Get(DatabaseConstants.MASTER_DB_NAME);
    //    SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(dbc.ConnectionString);

    //    DbSqlServerConnection connection = new DbSqlServerConnection(sqlConnectionStringBuilder.ConnectionString);
    //    //ALTER DATABASE [MyDB] SET AUTO_CLOSE OFF 
    //    //ALTER DATABASE Sales SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    //    connection.Execute($"IF EXISTS(select * FROM master..sysdatabases where [name] ='{databaseName}') ALTER DATABASE {databaseName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE");
    //    connection.Execute($"IF EXISTS(select * FROM master..sysdatabases where [name] ='{databaseName}') ALTER DATABASE {databaseName} SET AUTO_CLOSE OFF");
    //    connection.Execute($"DROP DATABASE IF EXISTS {databaseName} "); //WITH ROLLBACK IMMEDIATE;
    //}

}
