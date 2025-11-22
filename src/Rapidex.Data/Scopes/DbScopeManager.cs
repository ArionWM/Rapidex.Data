using Rapidex.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace Rapidex.Data.Scopes;

internal class DbScopeManager : IDbManager
{
    private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
    private readonly IServiceProvider serviceProvider;


    internal bool IsMultiDb { get; set; } = false;
    internal Dictionary<string, IDbScope> DbScopes { get; private set; } = new Dictionary<string, IDbScope>(StringComparer.InvariantCultureIgnoreCase);


    public DbScopeManager(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public IDbScope AddMainDbIfNotExists()
    {
        this.locker.EnterUpgradeableReadLock();
        try
        {
            if (this.DbScopes.ContainsKey(DatabaseConstants.MASTER_DB_ALIAS_NAME))
            {
                return this.DbScopes.Get(DatabaseConstants.MASTER_DB_ALIAS_NAME);
            }

            this.locker.EnterWriteLock();

            try
            {
                DbProviderFactory dbCreator = this.serviceProvider.GetRequiredService<DbProviderFactory>(); // new DbProviderFactory();
                IDbScope dbScope = dbCreator.CheckMasterDb();

                this.DbScopes.Add(DatabaseConstants.MASTER_DB_ALIAS_NAME, dbScope);

                return dbScope;
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }
        finally
        {
            this.locker.ExitUpgradeableReadLock();
        }
    }


    public IDbScope AddDbIfNotExists(long id, string dbNameOrAliasName)
    {
        if (this.DbScopes.ContainsKey(dbNameOrAliasName))
            return this.DbScopes.Get(dbNameOrAliasName);

        DbProviderFactory dbCreator = this.serviceProvider.GetRequiredService<DbProviderFactory>(); 
        IDbScope dbScope = dbCreator.Create(id, dbNameOrAliasName);

        this.DbScopes.Add(dbNameOrAliasName, dbScope);
        return dbScope;
    }

    public IDbScope Db(string dbName = null, bool throwErrorIfNotExists = true)
    {
        if (dbName.IsNullOrEmpty())
        {
            dbName = DatabaseConstants.MASTER_DB_ALIAS_NAME;
        }

        if (!string.Equals(dbName, DatabaseConstants.MASTER_DB_ALIAS_NAME, StringComparison.InvariantCultureIgnoreCase) && !this.IsMultiDb)
        {
            throw new InvalidOperationException("MultiDb enviroment is not enabled. Use EnableMultiDb() method to enable it.");
        }

        dbName.ValidateInvariantName();

        if (throwErrorIfNotExists && !this.DbScopes.ContainsKey(dbName))
        {
            throw new InvalidOperationException($"Database with alias '{dbName}' is not found.");
        }

        return this.DbScopes.Get(dbName).NotNull($"Database scope '{dbName}' not found");
    }

    public void EnableMultiDb()
    {
        DbValidator dbValidator = new DbValidator();
        var dbMaster = Database.Dbs.Db(DatabaseConstants.MASTER_DB_ALIAS_NAME);
        var dbMasterScope = dbMaster.Base;
        var validation = dbValidator.ValidateMultiDb(dbMasterScope.DbProvider);
        if (!validation.Success)
        {
            throw new DataConnectionException(validation.CreateErrorDescription());
        }

        this.IsMultiDb = true;
    }


    internal void ClearCache()
    {
        this.DbScopes.Clear();
        Database.EntityFactory.Clear();
    }
}
