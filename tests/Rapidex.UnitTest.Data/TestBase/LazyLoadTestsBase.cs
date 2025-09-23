using Rapidex.Data;

using Rapidex.UnitTest.Data.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestBase;

public abstract class LazyLoadTestsBase<T> : DbDependedTestsBase<T> where T : IDbProvider
{
    protected LazyLoadTestsBase(SingletonFixtureFactory<DbWithProviderFixture<T>> factory) : base(factory)
    {
    }
}
