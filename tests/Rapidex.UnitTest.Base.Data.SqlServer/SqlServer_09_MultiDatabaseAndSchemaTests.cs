using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.SqlServer;
public class SqlServer_09_MultiDatabaseAndSchemaTests : MultiDatabaseAndSchemaTestsBase<DbSqlServerProvider>
{
    public SqlServer_09_MultiDatabaseAndSchemaTests(SingletonFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
    {
    }
}
