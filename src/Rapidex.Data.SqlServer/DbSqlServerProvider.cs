using System.Xml.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Rapidex.Data.SqlServer;

namespace Rapidex.Data.SqlServer;

public class DbSqlServerProvider : IDbProvider
{
    protected string _connectionString;
    protected string _databaseName;
    protected bool isInitialized = false;
    protected ILogger logger;
    protected IServiceProvider ServiceProvider { get; }
    protected SqlConnectionStringBuilder Connectionbuilder { get; set; }

    internal static IExceptionTranslator SqlServerExceptionTranslator { get; } = new DbSqlServerExceptionTranslator();

    public IDbSchemaScope ParentScope { get; private set; }
    public string ConnectionString { get { return _connectionString; } set { this.SetConnectionString(value); } }
    public string DatabaseName { get { return _databaseName; } set { this.SetDatabaseName(value); } }

    public string StartDbName { get; protected set; }

    public IExceptionTranslator ExceptionTranslator => SqlServerExceptionTranslator;



    public DbSqlServerProvider(IServiceProvider serviceProvider, ILogger<DbSqlServerProvider> logger)
    {
        this.ServiceProvider = serviceProvider;
        this.logger = logger;
    }

    //public DbSqlServerProvider(IDbSchemaScope parentScope, string connectionString) : this(connectionString)
    //{
    //    this.ParentScope = parentScope;
    //}

    //public DbSqlServerProvider(string connectionString)
    //{
    //    this.ConnectionString = connectionString;
    //}

    public void Initialize(string connectionString)
    {
        //this.ParentScope = parentScope;
        this.ConnectionString = connectionString;
        this.isInitialized = this.ParentScope != null;
    }

    public void SetParentScope(IDbSchemaScope parent)
    {
        parent.NotNull();
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
        this.Connectionbuilder = new SqlConnectionStringBuilder(connectionString);
        this._databaseName = this.Connectionbuilder.InitialCatalog;
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
        this.Connectionbuilder.InitialCatalog = dbName;
        this._connectionString = this.Connectionbuilder.ConnectionString;
    }

    public IValidationResult ValidateConnection()
    {
        this.CheckInitialized();
        ConfigurationValidator validator = new ConfigurationValidator();
        return validator.Validate(this);
    }



    public void SwitchDb(string dbNameOrAlias)
    {

        var strMan = this.GetStructureProvider();
        dbNameOrAlias = this.GetDatabaseName(dbNameOrAlias);
        strMan.SwitchDatabase(dbNameOrAlias);
        this.SetDatabaseName(dbNameOrAlias);
    }




    public IDbDataModificationPovider GetDataModificationProvider()
    {
        this.CheckInitialized();
        return new DbSqlServerDataModificationProvider(this.ParentScope, this, ConnectionString);
    }

    public IDbStructureProvider GetStructureProvider()
    {
        var sp = this.ServiceProvider.GetRequiredKeyedService<IDbStructureProvider>("mssql");
        sp.Initialize(this, this.ConnectionString);
        return sp; 

    }

    public IDataUnitTestHelper GetTestHelper()
    {
        return new DbSqlServerTestHelper();
    }

    public IDbAuthorizationChecker GetAuthorizationChecker()
    {
        //this.CheckInitialized();
        return new DbSqlServerAuthorizationChecker(this.ConnectionString);
    }
}
