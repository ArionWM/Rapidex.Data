using Rapidex.Data.Scopes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data
{
    public static class DbScopeExtentions
    {
        //public static IDbScope AddMainDbIfNotExists(this IDbScopeManager sman)
        //{
        //    var cInfo = Database.Configuration.ConnectionInfo.Get(DatabaseConstants.DEFAULT_DB_NAME);
        //    cInfo.NotNull("Master db connection configuration not found. See: appsettings.json");

        //    return sman.AddMainDbIfNotExists(cInfo);
        //}

        //public static IDbScope AddMainDbIfNotExists<TProvider>(this IDbScopeManager sman, string connectionStringOrConnectionNameInConfig = null) where TProvider : IDbProvider
        //{
        //    var cInfo = Database.Configuration.ConnectionInfo.Get(DatabaseConstants.DEFAULT_DB_NAME);
        //    cInfo.NotNull("Master db connection configuration not found. See: appsettings.json");

        //    DbManagersFactory dbFactory = new DbManagersFactory();
        //    IDbProvider provider = dbFactory.CreateProvider(typeof(TProvider).FullName, cInfo.ConnectionString);

        //    return sman.AddMainDbIfNotExists(cInfo);

        //}

        private static DbConnectionInfo CheckConnectionInfo(this IDbScopeManager sman, string dbName, string connectionStringOrConnectionNameInConfig = null)
        {
            var cInfo = Database.Configuration.ConnectionInfo.Get(dbName);
            if (cInfo == null && connectionStringOrConnectionNameInConfig.IsNOTNullOrEmpty())
            {
                cInfo = Database.Configuration.ConnectionInfo.Get(dbName);
            }

            if (cInfo == null && connectionStringOrConnectionNameInConfig.IsNOTNullOrEmpty())
            {
                cInfo = new DbConnectionInfo();
                cInfo.Name = dbName.ToFriendly();
                cInfo.ConnectionString = connectionStringOrConnectionNameInConfig;

                IDbScope masterScope = Database.Scopes.Db();
                if (masterScope != null)
                {
                    cInfo.Provider = masterScope.DbProvider.GetType().FullName;
                }
            }

            if (cInfo == null)
            {
                throw new Exception($"Connection info for {dbName} not found");
            }

            return cInfo;
        }


        //public static IDbScope AddDbIfNotExists<TProvider>(this IDbScopeManager sman, string dbName, string connectionStringOrConnectionNameInConfig = null) where TProvider : IDbProvider
        //{

        //    DbConnectionInfo cInfo = CheckConnectionInfo(sman, dbName, connectionStringOrConnectionNameInConfig);
        //    cInfo.Provider = typeof(TProvider).FullName;

        //    DbManagersFactory dbFactory = new DbManagersFactory();
        //    IDbProvider provider = dbFactory.CreateProvider(typeof(TProvider).FullName, cInfo.ConnectionString);

        //    return sman.AddDbIfNotExists(provider, dbName);
        //}

        //public static IDbScope AddDbIfNotExists(this IDbScopeManager sman, string dbName)
        //{
        //    IDbScope masterDb = sman.Db();

        //    DbConnectionInfo cInfo = CheckConnectionInfo(sman, dbName, masterDb.ConnectionString);

        //    DbManagersFactory dbFactory = new DbManagersFactory();
        //    IDbProvider provider = dbFactory.CreateProvider(cInfo.Provider, masterDb.DbProvider.ConnectionString);

        //    IDbScope res = sman.AddDbIfNotExists(provider, dbName);
        //    return res;
        //}

    }
}
