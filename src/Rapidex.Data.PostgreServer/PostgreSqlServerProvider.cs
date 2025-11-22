
using Npgsql;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;

namespace Rapidex.Data.PostgreServer;


public class PostgreSqlServerProvider : IDbProvider
{
    protected string _connectionString;
    protected string _databaseName;
    protected bool isInitialized = false;
    protected ILogger logger;
    protected IServiceProvider ServiceProvider { get; }
    protected NpgsqlConnectionStringBuilder Connectionbuilder { get; set; }

    internal static IExceptionTranslator PostgreServerExceptionTranslator { get; } = new PostgreSqlServerExceptionTranslator();

    public IDbSchemaScope ParentScope { get; private set; }
    public string ConnectionString { get { return _connectionString; } set { this.SetConnectionString(value); } }
    public string DatabaseName
    {
        get { return _databaseName; }
        set
        {
            this.SetDatabaseName(value);
        }
    }



    public IExceptionTranslator ExceptionTranslator => PostgreServerExceptionTranslator;

    public string StartDbName { get; protected set; }


    public PostgreSqlServerProvider(IServiceProvider serviceProvider, ILogger<PostgreSqlServerProvider> logger)
    {
        this.ServiceProvider = serviceProvider;
        this.logger = logger;
    }

    public void Initialize(string connectionString)
    {
        //this.ParentScope = parentScope;
        this.ConnectionString = connectionString;
        this.isInitialized = this.ParentScope != null;
    }

    public void SetParentScope(IDbSchemaScope parent)
    {
        if (this.ParentScope != null)
        {
            throw new InvalidOperationException("Parent scope is already set");
        }

        this.ParentScope = parent;
        this.isInitialized = this.ConnectionString.IsNOTNullOrEmpty();
    }

    protected void CheckInitialized()
    {
        if (!this.isInitialized)
        {
            throw new InvalidOperationException("Provider is not initialized. Call Initialize method first.");
        }

        this.ConnectionString.NotEmpty("Connection string cannot be empty or null");
    }

    protected void SetConnectionString(string connectionString)
    {
        this._connectionString = connectionString;
        this.Connectionbuilder = new NpgsqlConnectionStringBuilder(connectionString);
        //this.Connectionbuilder.Database = PostgreHelper.CheckObjectName(this.Connectionbuilder.Database);

        string databaseName = this.Connectionbuilder.Database;
        PostgreHelper.ValidateObjectName(databaseName);

        this._databaseName = databaseName;
        this.StartDbName = this._databaseName;
    }



    private string GetDatabaseName(string dbName)
    {
        if (this.StartDbName.IsNOTNullOrEmpty() && !dbName.StartsWith(this.StartDbName))
        {
            string databaseName = this.StartDbName + '_' + PostgreHelper.CheckObjectName(dbName.Trim());
            dbName = databaseName;
        }
        return dbName;
    }

    private void SetDatabaseName(string dbName)
    {
        this._databaseName = this.GetDatabaseName(dbName);
        this.Connectionbuilder.Database = dbName;
        this._connectionString = this.Connectionbuilder.ConnectionString;
    }

    public IValidationResult ValidateConnection()
    {
        this.CheckInitialized();
        ConfigurationValidator validator = new ConfigurationValidator();
        return validator.Validate(this);
    }



    public void SwitchDb(string dbName)
    {
        dbName.NotEmpty();
        PostgreHelper.ValidateObjectName(dbName);

        var strMan = this.GetStructureProvider();

        dbName = this.GetDatabaseName(dbName);
        strMan.SwitchDatabase(dbName);

        this.SetDatabaseName(dbName);
    }




    public IDbDataModificationPovider GetDataModificationProvider()
    {
        this.CheckInitialized();
        return new PostgreSqlServerDataModificationProvider(this.ParentScope, this, this.ConnectionString);
    }

    public IDbStructureProvider GetStructureProvider()
    {
        var sp = this.ServiceProvider.GetRequiredKeyedService<IDbStructureProvider>("postgresql");
        sp.Initialize(this, this.ConnectionString);

        return sp;
    }

    public IDataUnitTestHelper GetTestHelper()
    {
        return new PostgreTestHelper();
    }

    public IDbAuthorizationChecker GetAuthorizationChecker()
    {
        this.CheckInitialized();
        return new PostgreSqlServerAuthorizationChecker(this.ConnectionString);
    }
}
