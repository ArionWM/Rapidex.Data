using Microsoft.Extensions.Configuration;
using Rapidex.Data.Scopes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data
{
    public class DbConfigurationManager
    {
        [Obsolete("", true)]
        public string DefaultDatabaseNamePrefix { get; set; } = "prox";

        /// <summary>
        /// For test purposes, different master database configuration for different provider
        /// </summary>
        internal string DatabaseSectionParentName { get; set; } = null;
        public string DatabaseSectionName { get; set; } = "Databases";
        public IConfiguration Configuration { get; set; }
        public IDictionary<string, DbConnectionInfo> ConnectionInfo { get; set; } = new Dictionary<string, DbConnectionInfo>();


        public IDictionary<string, DbConnectionInfo> ReadConnectionInfo()
        {
            var dbConfigRoot = Database.Configuration.Configuration;
            if (this.DatabaseSectionParentName.IsNOTNullOrEmpty())
            {
                dbConfigRoot = dbConfigRoot.GetSection(this.DatabaseSectionParentName);
            }

            IConfigurationSection connectionDatabaseSection = dbConfigRoot.GetSection("Databases");
            if (connectionDatabaseSection == null)
            {
                return null;
                //throw new InvalidOperationException("'Databases' section not found in configuration file. See: abcd");
            }

            IDictionary<string, DbConnectionInfo> connections = connectionDatabaseSection.Get<Dictionary<string, DbConnectionInfo>>();

            connections.NotNull("Connection definitions not found");

            if (connections.Count == 0)
            {
                return null;
                //throw new InvalidOperationException("Any connection definition not found");
            }

            foreach (string key in connections.Keys)
            {
                connections[key].Name = key;
            }

            this.ConnectionInfo = connections;
            return connections;
        }

        public void Setup()
        {
            this.ReadConnectionInfo();
        }

        public void LoadDbScopeDefinition(IServiceProvider sp, string dbName)
        {
            DbConnectionInfo cinfo = this.ConnectionInfo.Get(dbName, true);
            cinfo.NotNull($"Connection info found in configuration for '{dbName}'. see appsettings.json");

            cinfo.Provider.NotNull($"Provider not found in configuration for '{dbName}'. see appsettings.json");
            cinfo.ConnectionString.NotEmpty($"ConnectionString found in configuration for '{dbName}'. see appsettings.json");
            cinfo.Name.NotEmpty($"Name found in configuration for '{dbName}'. see appsettings.json");

            IDbScope db;
            if (cinfo.Name == DatabaseConstants.MASTER_DB_NAME)
            {
                db = Database.Scopes.AddMainDbIfNotExists();
            }
            else
            {
                throw new NotSupportedException("Removed, but additional connection definitions is not required for now");
                //Database.Scopes.EnableMultiDb();
                //Database.Scopes.AddDbIfNotExists(dbName);
            }

            

        }

        public void LoadDbScopeDefinitions(IServiceProvider sp)
        {
            DbConnectionInfo masterConnInfo = this.ConnectionInfo.Get(DatabaseConstants.MASTER_DB_NAME);
            masterConnInfo.NotNull("Any `default` db connection configuration not found (Name should be: 'Master'). See: appsettings.json");

            this.LoadDbScopeDefinition(sp, DatabaseConstants.MASTER_DB_NAME);

            //Removed, but additional connection definitions is not required for now
            //foreach (var dbName in this.ConnectionInfo.Keys)
            //{
            //    if (string.Equals(DatabaseConstants.MASTER_DB_NAME, dbName, StringComparison.InvariantCultureIgnoreCase))
            //        continue;

            //    this.LoadDbScopeDefinition(dbName);
            //}

        }

    }
}
