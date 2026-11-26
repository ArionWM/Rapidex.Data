using Rapidex.Data.PostgreServer;
using Rapidex.UnitTest.Data.TestBase;

namespace Rapidex.UnitTest.Data.PostgreServer;

public class PostgreServer_10_LinqTests : LinqTestsBase<PostgreSqlServerProvider>
{
    public PostgreServer_10_LinqTests(EachTestClassIsolatedFixtureFactory<DbWithProviderFixture<PostgreSqlServerProvider>> factory) : base(factory)
    {
    }
}
