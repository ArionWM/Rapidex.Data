using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rapidex.UnitTest.Data.TestContent;

namespace Rapidex.UnitTest.Data.PostgreServer;

public class PostgreSpecificTests : DbDependedTestsBase<PostgreSqlServerProvider>
{
    public PostgreSpecificTests(EachTestClassIsolatedFixtureFactory<DbWithProviderFixture<PostgreSqlServerProvider>> factory) : base(factory)
    {
    }

    [Fact]
    public async Task TryExhaust()
    {
        await Parallel.ForEachAsync(Enumerable.Range(0, 100), async (i, _) =>
        {
            try
            {
                var dbScope = Database.Dbs.Db();
                dbScope.Metadata.AddIfNotExist<ConcreteEntity01>();
                dbScope.Structure.ApplyEntityStructure<ConcreteEntity01>();

                PostgreSqlServerConnection conn = new PostgreSqlServerConnection(dbScope.DbProvider.ConnectionString);
                NpgsqlConnectionStringBuilder npgsqlConnectionStringBuilder = new NpgsqlConnectionStringBuilder(dbScope.DbProvider.ConnectionString);
                Assert.True(npgsqlConnectionStringBuilder.MaxPoolSize < 20, "MaxPoolSize should be less than 20");

                for (int j = 0; j < 10; j++)
                {
                    await conn.Execute("SELECT pg_sleep(8)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        });
    }
}
