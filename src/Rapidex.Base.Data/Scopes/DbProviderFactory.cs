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

        IDbProvider provider = TypeHelper.CreateInstance<IDbProvider>(type, connectionString);
        return provider;
    }

    public IDbProvider CreateProvider(DbConnectionInfo connectionInfo)
    {
        connectionInfo.NotNull();

        IDbProvider provider = CreateProvider(connectionInfo.Provider, connectionInfo.ConnectionString);
        return provider;
    }

    public IDbScope Create(long id, string dbAliasName)
    {
        IDbProvider provider;

        DbConnectionInfo connectionInfo = Database.Configuration.ConnectionInfo.Get(dbAliasName);
        if (connectionInfo == null)
        {
            connectionInfo = Database.Configuration.ConnectionInfo.Get(DatabaseConstants.MASTER_DB_NAME);
        }

        provider = CreateProvider(connectionInfo);
        provider.NotNull("Provider is not found for the given name: " + connectionInfo.Provider);

        DbScope scope = new DbScope(id, dbAliasName, provider);
        return scope;
    }


}
