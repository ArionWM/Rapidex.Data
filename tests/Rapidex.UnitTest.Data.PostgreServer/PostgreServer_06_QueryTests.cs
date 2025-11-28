using Rapidex.Data.PostgreServer;
using Rapidex.UnitTest.Data.TestBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.PostgreServer
{
    public class PostgreServer_06_QueryTests : QueryTestsBase<PostgreSqlServerProvider>
    {
        public PostgreServer_06_QueryTests(EachTestClassIsolatedFixtureFactory<DbWithProviderFixture<PostgreSqlServerProvider>> factory) : base(factory)
        {
        }

        public override void Load_07_DateTimeWithOffset()
        {
            base.Load_07_DateTimeWithOffset();

            using PostgreSqlServerConnection connection = Library.CreatePostgreServerConnection();

            //Ent 1, starttime with offset +00:00, new DateTimeOffset(2024, 6, 15, 10, 30, 0, TimeSpan.Zero)
            //Ent 2, starttime with offset +02:00, new DateTimeOffset(2024, 2, 17, 09, 21, 0, TimeSpan.FromHours(2)

            //var sql = $"SELECT starttime FROM base.utest_myjsonentity03 WHERE id = {this.TestEntityId1};";

        }
    }
}
