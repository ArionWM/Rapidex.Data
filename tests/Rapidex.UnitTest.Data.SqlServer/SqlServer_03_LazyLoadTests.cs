
namespace Rapidex.UnitTest.Data.SqlServer;

public class SqlServer_03_LazyLoadTests : LazyLoadTestsBase<DbSqlServerProvider>
{
    public SqlServer_03_LazyLoadTests(SingletonFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
    {
    }

    //[Fact]
    //public virtual void LazyLoad_01_Reference()
    //{
    //}
}
