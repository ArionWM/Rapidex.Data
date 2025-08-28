using Rapidex.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace Rapidex.Data.Scopes;

internal class DbScopeManager : IDbManager
{
    internal bool IsMultiDb { get; set; } = false;
    internal Dictionary<string, IDbScope> _dbScopes = new Dictionary<string, IDbScope>(StringComparer.InvariantCultureIgnoreCase);

    ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    public IDbScope AddMainDbIfNotExists()
    {
        _lock.EnterUpgradeableReadLock();
        try
        {
            if (_dbScopes.ContainsKey(DatabaseConstants.MASTER_DB_NAME))
            {
                return _dbScopes.Get(DatabaseConstants.MASTER_DB_NAME);
            }

            _lock.EnterWriteLock();

            try
            {
                DbProviderFactory dbCreator = new DbProviderFactory();
                IDbScope dbScope = dbCreator.Create(DatabaseConstants.MASTER_TENANT_ID, DatabaseConstants.MASTER_DB_NAME);

                _dbScopes.Add(DatabaseConstants.MASTER_DB_NAME, dbScope);

                return dbScope;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        finally
        {
            _lock.ExitUpgradeableReadLock();
        }
    }


    public IDbScope AddDbIfNotExists(long id, string dbNameOrAliasName)
    {
        if (_dbScopes.ContainsKey(dbNameOrAliasName))
            return _dbScopes.Get(dbNameOrAliasName);

        DbProviderFactory dbCreator = new DbProviderFactory();
        IDbScope dbScope = dbCreator.Create(id, dbNameOrAliasName);

        _dbScopes.Add(dbNameOrAliasName, dbScope);
        return dbScope;
    }

    public IDbScope Db(string dbName = null, bool throwErrorIfNotExists = true)
    {
        if (dbName.IsNullOrEmpty())
        {
            dbName = DatabaseConstants.MASTER_DB_NAME;
        }

        if (!string.Equals(dbName, DatabaseConstants.MASTER_DB_NAME, StringComparison.InvariantCultureIgnoreCase) && !this.IsMultiDb)
        {
            throw new InvalidOperationException("MultiDb enviroment is not enabled. Use EnableMultiDb() method to enable it.");
        }

        dbName.ValidateInvariantName();

        if (throwErrorIfNotExists && !_dbScopes.ContainsKey(dbName))
        {
            throw new InvalidOperationException($"Database '{dbName}' is not found.");
        }

        return _dbScopes.Get(dbName).NotNull($"Database scope '{dbName}' not found");
    }

    public void EnableMultiDb()
    {
        DbValidator dbValidator = new DbValidator();
        var dbMaster = Database.Dbs.Db(DatabaseConstants.MASTER_DB_NAME);
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
        _dbScopes.Clear();
        Database.EntityFactory.Clear();
    }
}
