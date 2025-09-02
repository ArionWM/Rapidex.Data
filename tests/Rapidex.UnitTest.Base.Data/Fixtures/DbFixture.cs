
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

    protected override void Setup(IServiceCollection services)
    {
        //base.Setup(services);

        services.AddRapidexDataLevel();

        Rapidex.Common.Assembly.Add(typeof(DbFixture).Assembly);

        //Rapidex.SignalHub.Library mhLib = new Rapidex.SignalHub.Library();
        //mhLib.SetupServices(services);

        //Rapidex.Data.Library dataModule = new Rapidex.Data.Library();
        //dataModule.SetupMetadata(services);

        //dataModule.SetupServices(services);

        //Database.Setup(services);

    }


    public virtual void DropAllSchemasInDatabase(IDbProvider provider, bool clearCaches)
    {
        var testHelper = provider.GetTestHelper();
        testHelper.DropEverythingInDatabase(provider.ConnectionString);
        if (clearCaches)
        {
            this.ClearCaches();
        }
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

        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        this.Setup(builder.Services);

        //??
        DbConnectionInfo connectionInfo = Database.Configuration.ConnectionInfo.Get(DatabaseConstants.MASTER_DB_NAME);
        DbProviderFactory DbProviderFactory = new DbProviderFactory();
        IDbProvider provider = DbProviderFactory.CreateProvider(connectionInfo);
        this.DropAllTablesInDatabase(provider, false);

        IHost host = builder.Build();
        this.ServiceProvider = host.Services;

        this.ServiceProvider.StartRapidexDataLevel();


        var dbScope = Database.Dbs.AddMainDbIfNotExists();
        dbScope.Structure.ApplyAllStructure();


        initialized = true;
    }


    internal virtual DbSqlServerConnection CreateSqlServerConnection()
    {
        DbConnectionInfo dbc = Database.Configuration.ConnectionInfo.Get(DatabaseConstants.MASTER_DB_NAME);
        SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(dbc.ConnectionString);
        DbSqlServerConnection connection = new DbSqlServerConnection(sqlConnectionStringBuilder.ConnectionString);

        connection.Execute($"USE [{Database.Dbs.Db().DatabaseName}]");
        return connection;
    }



    public override void ClearCaches()
    {
        //InMemCache clear
        //Common caches clear
        ((Rapidex.Data.Scopes.DbScopeManager)Database.Dbs).ClearCache();
        //((IDbEntityMetadataManager)Database.Metadata).Clear();
        //Database.PredefinedValues.Clear();
        //Database.Metadata.Setup(null);

        var db = Database.Dbs.AddMainDbIfNotExists();


    }

    
}
