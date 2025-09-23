using Rapidex.Data.PostgreServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.PostgreServer
{
    public class PostgreServer_08_AggregationTests : AggregationTestsBase<PostgreSqlServerProvider>
    {
        public PostgreServer_08_AggregationTests(SingletonFixtureFactory<DbWithProviderFixture<PostgreSqlServerProvider>> factory) : base(factory)
        {
        }
    }
}
