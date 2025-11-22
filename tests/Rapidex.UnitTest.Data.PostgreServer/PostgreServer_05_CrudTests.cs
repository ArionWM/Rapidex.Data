
using Rapidex.Data.PostgreServer;

namespace Rapidex.UnitTest.Data.PostgreServer
{
    public class PostgreServer_05_CrudTests : CrudTestsBase<PostgreSqlServerProvider>
    {
        public PostgreServer_05_CrudTests(EachTestClassIsolatedFixtureFactory<DbWithProviderFixture<PostgreSqlServerProvider>> factory) : base(factory)
        {
        }
    }
}
