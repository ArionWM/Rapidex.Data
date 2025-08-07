
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
    protected Dictionary<IDbSchemaScope, IDbDataModificationPovider> dmProviders = new();

    internal static IExceptionTranslator PostgreServerExceptionTranslator { get; }

    public IDbSchemaScope ParentScope { get; private set; }
    public string ConnectionString { get { return _connectionString; } set { SetConnectionString(value); } }
    public string DatabaseName { get { return _databaseName; } set { SetDatabaseName(value); } }



    public IExceptionTranslator ExceptionTranslator => PostgreServerExceptionTranslator;

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
    }

    private void SetDatabaseName(string value)
    {
        this._databaseName = value;
        this.Connectionbuilder.Database = value;
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
        this.SetDatabaseName(dbName);

        var strMan = this.GetStructureProvider();
        strMan.SwitchDatabase(dbName);
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

    public IDbDataModificationPovider GetDataModificationPovider()
    {
        this.ValidateInitialization();

        var provider = dmProviders.GetOr(this.ParentScope, () =>
        {
            return new PostgreSqlServerDataModificationProvider(this.ParentScope, this, ConnectionString);
        });

        return provider;
    }

    public IDbStructureProvider GetStructureProvider()
    {
        this.ValidateInitialization();

        return new PostgreSqlStructureProvider(this, ConnectionString);

    }

    public IDataTestHelper GetTestHelper()
    {
        return new PostgreTestHelper();
    }
}
