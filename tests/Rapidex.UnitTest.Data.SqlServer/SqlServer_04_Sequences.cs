

namespace Rapidex.UnitTest.Data.SqlServer;

public class SqlServer_04_Sequences : SequenceTestsBase<DbSqlServerProvider>
{
    public SqlServer_04_Sequences(EachTestClassIsolatedFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
    {
    }
}
