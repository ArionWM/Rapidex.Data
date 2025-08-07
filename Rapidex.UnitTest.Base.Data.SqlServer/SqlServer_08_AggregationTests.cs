using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.SqlServer;

public class SqlServer_08_AggregationTests : AggregationTestsBase<DbSqlServerProvider>
{
    public SqlServer_08_AggregationTests(SingletonFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
    {
    }
}
