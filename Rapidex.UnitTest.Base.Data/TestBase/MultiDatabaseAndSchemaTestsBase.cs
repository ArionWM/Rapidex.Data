using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestBase;
public class MultiDatabaseAndSchemaTestsBase<T> : DbDependedTestsBase<T> where T : IDbProvider
{
    public MultiDatabaseAndSchemaTestsBase(SingletonFixtureFactory<DbWithProviderFixture<T>> factory) : base(factory)
    {
    }
}
