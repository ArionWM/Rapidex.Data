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
        public string RapidexSectionName { get; set; } = "Rapidex";
        public string DatabaseSectionName { get; set; } = "Databases";
        public string SoftDefinitionsBaseFolder { get; set; } = "App_Content";
        public IConfiguration Configuration { get; set; }
        public IConfigurationSection? Root { get;protected set; }
        public IDictionary<string, DbConnectionInfo> ConnectionInfo { get; set; } = new Dictionary<string, DbConnectionInfo>();


        public IDictionary<string, DbConnectionInfo> ReadConnectionInfo()
        {
            var dbConfigRoot = this.Root;
            if (this.DatabaseSectionParentName.IsNOTNullOrEmpty())
            {
                dbConfigRoot = dbConfigRoot.GetSection(this.DatabaseSectionParentName);
            }

            IConfigurationSection databaseSection = dbConfigRoot.GetSection(this.DatabaseSectionName);
            if (databaseSection == null)
            {
                return null;
                //throw new InvalidOperationException("'Databases' section not found in configuration file. See: abcd");
            }

            IDictionary<string, DbConnectionInfo> connections = databaseSection.Get<Dictionary<string, DbConnectionInfo>>();

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
            this.Root = Database.Configuration.Configuration.GetSection(this.RapidexSectionName);
            this.Root.NotNull($"'{this.RapidexSectionName}' section not found in configuration file. See: appsettings.json");

            this.SoftDefinitionsBaseFolder = this.Root.GetValue<string>("SoftDefinitionsBaseFolder", this.SoftDefinitionsBaseFolder);
            this.ReadConnectionInfo();
        }

        public void LoadDbScopeDefinition(string dbName)
        {
            DbConnectionInfo cinfo = this.ConnectionInfo.Get(dbName, true);
            cinfo.NotNull($"Connection info found in configuration for '{dbName}'. see appsettings.json");

            cinfo.Provider.NotNull($"Provider not found in configuration for '{dbName}'. see appsettings.json");
            cinfo.ConnectionString.NotEmpty($"ConnectionString found in configuration for '{dbName}'. see appsettings.json");
            cinfo.Name.NotEmpty($"Name found in configuration for '{dbName}'. see appsettings.json");

            IDbScope db;
            if (cinfo.Name == DatabaseConstants.MASTER_DB_ALIAS_NAME)
            {
                db = Database.Dbs.AddMainDbIfNotExists();
            }
            else
            {
                throw new NotSupportedException("Removed, but additional connection definitions is not required for now");
                //Database.Databases.EnableMultiDb();
                //Database.Databases.AddDbIfNotExists(dbName);
            }
        }

        public void LoadDbScopeDefinitions()
        {
            DbConnectionInfo masterConnInfo = this.ConnectionInfo.Get(DatabaseConstants.MASTER_DB_ALIAS_NAME);
            masterConnInfo.NotNull("Any `default` db connection configuration not found (Name should be: 'Master'). See: appsettings.json");

            this.LoadDbScopeDefinition(DatabaseConstants.MASTER_DB_ALIAS_NAME);

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
