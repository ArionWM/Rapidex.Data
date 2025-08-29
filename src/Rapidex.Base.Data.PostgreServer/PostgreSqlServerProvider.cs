
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

    protected NpgsqlConnectionStringBuilder Connectionbuilder { get; set; }

    internal static IExceptionTranslator PostgreServerExceptionTranslator { get; }

    public IDbSchemaScope ParentScope { get; private set; }
    public string ConnectionString { get { return _connectionString; } set { SetConnectionString(value); } }
    public string DatabaseName
    {
        get { return _databaseName; }
        set
        {
            SetDatabaseName(value);
        }
    }



    public IExceptionTranslator ExceptionTranslator => PostgreServerExceptionTranslator;

    public string StartDbName { get; protected set; }

    static PostgreSqlServerProvider()
    {
        PostgreServerExceptionTranslator = new PostgreSqlServerExceptionTranslator();
    }

    public PostgreSqlServerProvider(IDbSchemaScope parentScope, string connectionString) : this(connectionString)
    {
        this.ParentScope = parentScope;
    }

    public PostgreSqlServerProvider(string connectionString)
    {
        this.ConnectionString = connectionString;
    }

    protected void SetConnectionString(string connectionString)
    {
        this._connectionString = connectionString;
        this.Connectionbuilder = new NpgsqlConnectionStringBuilder(connectionString);
        this._databaseName = this.Connectionbuilder.Database;
        this.StartDbName = this._databaseName;
    }

    private string GetDatabaseName(string dbName)
    {
        if (this.StartDbName.IsNOTNullOrEmpty() && !dbName.StartsWith(this.StartDbName))
        {
            string databaseName = this.StartDbName + dbName.ToFriendly().Trim();
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

    protected void ValidateInitialization()
    {
        this.ConnectionString.NotEmpty("Connection string cannot be empty or null");
        this.ParentScope.NotNull("Parent scope cannot be null");
    }

    public IValidationResult ValidateConnection()
    {
        ConfigurationValidator validator = new ConfigurationValidator();
        return validator.Validate(this);
    }

    public void SetParentScope(IDbSchemaScope parent)
    {
        if (this.ParentScope != null)
        {
            throw new InvalidOperationException("Parent scope is already set");
        }

        this.ParentScope = parent;
    }

    public void UseDb(string dbName)
    {
        var strMan = this.GetStructureProvider();

        dbName = this.GetDatabaseName(dbName);
        strMan.SwitchDatabase(dbName);

        this.SetDatabaseName(dbName);
    }

    public void Setup(IServiceCollection services)
    {

    }
    public void Start() //Hangisi?
    {

    }

    public void Start(IServiceProvider serviceProvider) //Hangisi?
    {

    }

    public IDbDataModificationPovider GetDataModificationProvider()
    {
        this.ValidateInitialization();

        return new PostgreSqlServerDataModificationProvider(this.ParentScope, this, ConnectionString);
    }

    public IDbStructureProvider GetStructureProvider()
    {
        this.ValidateInitialization();

        return new PostgreSqlStructureProvider(this, ConnectionString);

    }

    public IDataUnitTestHelper GetTestHelper()
    {
        return new PostgreTestHelper();
    }

    public IDbAuthorizationChecker GetAuthorizationChecker()
    {
        this.ValidateInitialization();
        return new PostgreSqlServerAuthorizationChecker(this.ConnectionString);
    }
}
