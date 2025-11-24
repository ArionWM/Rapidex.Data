using Rapidex.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Rapidex.Data.Scopes;

internal class DbProviderFactory
{
    private readonly IServiceProvider serviceProvider;

    public DbProviderFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public IDbProvider CreateProvider(string dbProviderTypeName, string connectionString = null)
    {
        dbProviderTypeName.NotEmpty();

        Type type = Common.Assembly.FindType(dbProviderTypeName)
            .NotNull($"Type '{dbProviderTypeName}' is not found.");

        IDbProvider provider = this.serviceProvider.GetKeyedService<IDbProvider>(type.Name);
        provider.NotNull(string.Format("DbProvider of type '{0}' is not registered in the DI container.", type.FullName));
        provider.Initialize(connectionString);
        return provider;
    }

    public IDbProvider CreateProvider(DbConnectionInfo connectionInfo)
    {
        connectionInfo.NotNull();

        IDbProvider provider = this.CreateProvider(connectionInfo.Provider, connectionInfo.ConnectionString);
        return provider;
    }

    public IDbScope CheckMasterDb()
    {
        DbConnectionInfo connectionInfo = Database.Configuration.ConnectionInfo.Get(DatabaseConstants.MASTER_DB_ALIAS_NAME);
        if (connectionInfo == null)
        {
            throw new DataConnectionException($"Master database connection info is not found. See: https://github.com/ArionWM/Rapidex.Data/blob/main/docs/DatabaseConnectionAndRequiredRights.md\"");
        }

        IDbProvider provider = this.CreateProvider(connectionInfo);
        var structureProvider = provider.GetStructureProvider();
        var checkResult = structureProvider.CheckMasterConnection();

        switch (checkResult.status)
        {
            case MasterDbConnectionStatus.Valid:
            case MasterDbConnectionStatus.Created:
                var dbScope = this.serviceProvider.GetKeyedService<IDbScope>("raw").NotNull();
                dbScope.Initialize(DatabaseConstants.MASTER_TENANT_ID, DatabaseConstants.MASTER_DB_ALIAS_NAME, provider);
                return dbScope;
            default:
                throw new DataConnectionException($"Master database connection is not valid. \r\n{checkResult.description} \r\nSee: https://github.com/ArionWM/Rapidex.Data/blob/main/docs/DatabaseConnectionAndRequiredRights.md");
        }


    }

    public IDbScope Create(long id, string dbNameOrAliasName)
    {
        IDbProvider provider;

        bool trySwitchDb = false;
        DbConnectionInfo connectionInfo = Database.Configuration.ConnectionInfo.Get(dbNameOrAliasName);
        if (connectionInfo == null)
        {
            trySwitchDb = !string.Equals(dbNameOrAliasName, DatabaseConstants.MASTER_DB_ALIAS_NAME, StringComparison.InvariantCultureIgnoreCase);
            connectionInfo = Database.Configuration.ConnectionInfo.Get(DatabaseConstants.MASTER_DB_ALIAS_NAME);
        }

        provider = this.CreateProvider(connectionInfo);
        provider.NotNull("Provider is not found for the given name: " + connectionInfo.Provider);

        if (trySwitchDb)
            provider.SwitchDb(dbNameOrAliasName);

        var scope = this.serviceProvider.GetKeyedService<IDbScope>("raw");
        scope.Initialize(id, dbNameOrAliasName, provider);
        return scope;
    }


}
