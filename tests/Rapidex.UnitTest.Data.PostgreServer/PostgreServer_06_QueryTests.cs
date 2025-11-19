using Rapidex.Data.PostgreServer;
using Rapidex.UnitTest.Data.TestBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.PostgreServer
{
    public class PostgreServer_06_QueryTests : QueryTestsBase<PostgreSqlServerProvider>
    {
        public PostgreServer_06_QueryTests( SingletonFixtureFactory<DbWithProviderFixture<PostgreSqlServerProvider>> factory) : base(factory)
        {
        }

    }
}
