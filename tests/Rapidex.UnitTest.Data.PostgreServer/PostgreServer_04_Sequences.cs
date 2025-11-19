

using Rapidex.Data.PostgreServer;

namespace Rapidex.UnitTest.Data.PostgreServer
{
    public class PostgreServer_04_Sequences : SequenceTestsBase<PostgreSqlServerProvider>
    {
        public PostgreServer_04_Sequences(ILogger logger, SingletonFixtureFactory<DbWithProviderFixture<PostgreSqlServerProvider>> factory) : base(factory)
        {
        }
    }

}
