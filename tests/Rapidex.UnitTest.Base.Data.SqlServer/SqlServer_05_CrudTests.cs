
namespace Rapidex.UnitTest.Data.SqlServer;

public class SqlServer_05_CrudTests : CrudTestsBase<DbSqlServerProvider>
{
    public SqlServer_05_CrudTests(SingletonFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
    {
    }
}
