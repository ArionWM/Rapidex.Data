
using Rapidex.Data.PostgreServer;

namespace Rapidex.UnitTest.Data.PostgreServer
{
    public class PostgreServer_03_LazyLoadTests : LazyLoadTestsBase<PostgreSqlServerProvider>
    {
        public PostgreServer_03_LazyLoadTests(SingletonFixtureFactory<DbWithProviderFixture<PostgreSqlServerProvider>> factory) : base(factory)
        {
        }

        //[Fact]
        //public virtual void LazyLoad_01_Reference()
        //{
        //}
    }
}
