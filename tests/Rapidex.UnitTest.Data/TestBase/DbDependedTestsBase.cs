using Rapidex.Data;

using Rapidex.UnitTest.Data.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestBase
{
    public class DbDependedTestsBase<T> : IClassFixture<SingletonFixtureFactory<DbWithProviderFixture<T>>> where T : IDbProvider
    {
        protected DbWithProviderFixture<T> Fixture { get; }
        public DbDependedTestsBase(SingletonFixtureFactory<DbWithProviderFixture<T>> factory)
        {
            Fixture = factory.GetFixture();
        }


    }
}
