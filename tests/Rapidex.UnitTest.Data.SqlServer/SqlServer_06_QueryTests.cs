using Rapidex.UnitTest.Data.TestBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.SqlServer;

public class SqlServer_06_QueryTests : QueryTestsBase<DbSqlServerProvider>
{
    public SqlServer_06_QueryTests( SingletonFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
    {
    }

}
