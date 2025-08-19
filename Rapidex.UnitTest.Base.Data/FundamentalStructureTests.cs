using Rapidex.UnitTest.Data.TestContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data
{
    public class FundamentalStructureTests : DbDependedTestsBase<DbSqlServerProvider>
    {
        public FundamentalStructureTests(SingletonFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
        {
        }

        [Fact]
        public void T01_Initialization()
        {
            var db = Database.Scopes.AddMainDbIfNotExists();
            Assert.NotNull(db.Metadata);
            Assert.NotNull(Database.Scopes);

            //Assert.NotNull(Database.ConcreteEntityMapper);
            Assert.NotNull(Database.EntityFactory);
            Assert.NotNull(Database.Configuration);

            //Assert.NotNull(Database.Current);
        }

        [Fact]
        public void T02_ParentAssignments()
        {
            var db = Database.Scopes.AddMainDbIfNotExists();
            this.Fixture.ClearCaches();

            db.Metadata.AddIfNotExist<ConcreteEntity01>();
            db.Metadata.AddIfNotExist<ConcreteEntity02>();

            var database = Database.Scopes.AddMainDbIfNotExists();
            var schema01 = database.AddSchemaIfNotExists("myTestSchema01");
            var schema02 = database.AddSchemaIfNotExists("myTestSchema02");

            Assert.Equal("myTestSchema01", schema01.Data.ParentScope.SchemaName);
            Assert.Equal("myTestSchema01", schema01.Structure.ParentScope.SchemaName);

            Assert.Equal("myTestSchema02", schema02.Data.ParentScope.SchemaName);
        }
    }
}
