using Microsoft.Data.SqlClient;
using Rapidex.Data.SqlServer;

namespace Rapidex.Data.SqlServer;

public class DbSqlServerProvider : IDbProvider
{
    protected string _connectionString;
    protected string _databaseName;

    protected SqlConnectionStringBuilder Connectionbuilder { get; set; }
    //protected Dictionary<IDbSchemaScope, IDbDataModificationPovider> dmProviders = new();

    internal static IExceptionTranslator SqlServerExceptionTranslator { get; }

    public IDbSchemaScope ParentScope { get; private set; }
    public string ConnectionString { get { return _connectionString; } set { SetConnectionString(value); } }
    public string DatabaseName { get { return _databaseName; } set { SetDatabaseName(value); } }

    

    public IExceptionTranslator ExceptionTranslator => SqlServerExceptionTranslator;

    static DbSqlServerProvider()
    {
        SqlServerExceptionTranslator = new DbSqlServerExceptionTranslator();
    }

    public DbSqlServerProvider(IDbSchemaScope parentScope, string connectionString) : this(connectionString)
    {
        this.ParentScope = parentScope;
    }

    public DbSqlServerProvider(string connectionString)
    {
        this.ConnectionString = connectionString;
    }

    protected void SetConnectionString(string connectionString)
    {
        this._connectionString = connectionString;
        this.Connectionbuilder = new SqlConnectionStringBuilder(connectionString);
        this._databaseName = this.Connectionbuilder.InitialCatalog;
    }

    private void SetDatabaseName(string value)
    {
        this._databaseName = value;
        this.Connectionbuilder.InitialCatalog = value;
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

    public IDbDataModificationPovider GetDataModificationProvider()
    {
        this.ValidateInitialization();
        return new DbSqlServerDataModificationProvider(this.ParentScope, this, ConnectionString);
    }

    public IDbStructureProvider GetStructureProvider()
    {
        this.ValidateInitialization();

        return new DbSqlStructureProvider(this, ConnectionString);

    }

    public IDataUnitTestHelper GetTestHelper()
    {
        return new DbSqlServerTestHelper();
    }
}
