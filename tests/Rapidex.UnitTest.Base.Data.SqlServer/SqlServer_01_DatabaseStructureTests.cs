namespace Rapidex.UnitTest.Data.SqlServer;

public class SqlServer_01_DatabaseStructureTests : DatabaseStructureTestsBase<DbSqlServerProvider>
{
    public SqlServer_01_DatabaseStructureTests(SingletonFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
    {
    }

    public override void Structure_01_MasterAndBaseSchema()
    {
        base.Structure_01_MasterAndBaseSchema();

        DbConnectionInfo dbc = Database.Configuration.ConnectionInfo.Get(DatabaseConstants.MASTER_DB_NAME);
        SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(dbc.ConnectionString);

        using DbSqlServerConnection connection = new DbSqlServerConnection(sqlConnectionStringBuilder.ConnectionString);
        DataTable table = connection.Execute($"SELECT SCHEMA_ID('Base') AS SchemaId");
        Assert.Equal(1, table.Rows.Count);


    }
}
