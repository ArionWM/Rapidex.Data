using Rapidex.UnitTest.Data.TestBase;

namespace Rapidex.UnitTest.Data.SqlServer;

public class SqlServer_10_LinqTests : LinqTestsBase<DbSqlServerProvider>
{
    public SqlServer_10_LinqTests(EachTestClassIsolatedFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
    {
    }
}
