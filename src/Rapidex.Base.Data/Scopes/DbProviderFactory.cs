using Rapidex.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Rapidex.Data.Scopes;

internal class DbProviderFactory
{
    public IDbProvider CreateProvider(string name, string connectionString = null)
    {
        name.NotEmpty();

        Type type = Common.Assembly.FindType(name);

        type.NotNull($"Type '{name}' is not found.");


        IDbProvider provider = (IDbProvider)Activator.CreateInstance(type, connectionString);
        //TypeHelper.CreateInstance<IDbProvider>(type, connectionString);
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
        DbConnectionInfo connectionInfo = Database.Configuration.ConnectionInfo.Get(DatabaseConstants.MASTER_DB_NAME);
        if (connectionInfo == null)
        {
            throw new DataConnectionException($"Master database connection info is not found. See: https://github.com/ArionWM/Rapidex.Base.Data/blob/main/docs/DatabaseConnectionAndRequiredRights.md\"");
        }

        IDbProvider provider = this.CreateProvider(connectionInfo);
        var structureProvider = provider.GetStructureProvider();
        var checkResult = structureProvider.CheckMasterConnection();

        switch (checkResult.status)
        {
            case MasterDbConnectionStatus.Valid:
            case MasterDbConnectionStatus.Created:
                IDbScope dbScope = new DbScope(DatabaseConstants.MASTER_TENANT_ID, DatabaseConstants.MASTER_DB_NAME, provider);
                return dbScope;
            default:
                throw new DataConnectionException($"Master database connection is not valid. \r\n{checkResult.description} \r\nSee: https://github.com/ArionWM/Rapidex.Base.Data/blob/main/docs/DatabaseConnectionAndRequiredRights.md");
        }


    }

    public IDbScope Create(long id, string dbAliasName)
    {
        IDbProvider provider;

        DbConnectionInfo connectionInfo = Database.Configuration.ConnectionInfo.Get(dbAliasName);
        if (connectionInfo == null)
        {
            connectionInfo = Database.Configuration.ConnectionInfo.Get(DatabaseConstants.MASTER_DB_NAME);
        }

        provider = this.CreateProvider(connectionInfo);
        provider.NotNull("Provider is not found for the given name: " + connectionInfo.Provider);

        DbScope scope = new DbScope(id, dbAliasName, provider);
        return scope;
    }


}
