using Rapidex.Data.PostgreServer;
using Rapidex.UnitTest.Data.TestBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.PostgreServer;
public class PostgreServer_09_MultiDatabaseAndSchemaTests : MultiDatabaseAndSchemaTestsBase<PostgreSqlServerProvider>
{
    public PostgreServer_09_MultiDatabaseAndSchemaTests(SingletonFixtureFactory<DbWithProviderFixture<PostgreSqlServerProvider>> factory) : base(factory)
    {
    }

    protected override string GetRandomDbName()
    {
        return "testdb" + RandomHelper.RandomText(5).ToLowerInvariant();
    }
}
