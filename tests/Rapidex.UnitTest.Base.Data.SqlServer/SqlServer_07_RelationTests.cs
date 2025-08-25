using Rapidex.UnitTest.Data.TestBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.SqlServer;

public class SqlServer_07_RelationTests : RelationsTestBase<DbSqlServerProvider>
{
    public SqlServer_07_RelationTests(SingletonFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
    {
    }

}
