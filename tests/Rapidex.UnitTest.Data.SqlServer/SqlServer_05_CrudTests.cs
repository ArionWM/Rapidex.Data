
namespace Rapidex.UnitTest.Data.SqlServer;

public class SqlServer_05_CrudTests : CrudTestsBase<DbSqlServerProvider>
{
    public SqlServer_05_CrudTests(EachTestClassIsolatedFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
    {
    }

    public override void Crud_06_Update_Concrete()
    {
        base.Crud_06_Update_Concrete();

        DbConnectionInfo dbc = Database.Configuration.ConnectionInfo.Get(DatabaseConstants.MASTER_DB_ALIAS_NAME);
        SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(dbc.ConnectionString);

        using DbSqlServerConnection connection = new DbSqlServerConnection(sqlConnectionStringBuilder.ConnectionString);
        DataTable table = connection.Execute($"SELECT * from [Base].[utest_ConcreteEntity01]").Result;
        object bDateVal = table.Rows[0]["BirthDate"];
        Assert.Equal(DBNull.Value, bDateVal);
    }
}
