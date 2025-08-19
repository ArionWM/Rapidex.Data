using Rapidex.Data.SqlServer;
using Rapidex.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rapidex.UnitTest.Data.Fixtures;
using static System.Formats.Asn1.AsnWriter;
using Rapidex.UnitTest.Data.TestContent;

namespace Rapidex.UnitTest.Data.TestBase
{
    public abstract class DatabaseStructureTestsBase<T> : DbDependedTestsBase<T> where T : IDbProvider
    {
        protected DatabaseStructureTestsBase(SingletonFixtureFactory<DbWithProviderFixture<T>> factory) : base(factory)
        {
        }

        [Fact]
        public virtual void Structure_01_MasterAndBaseSchema()
        {
            this.Fixture.Reinit();

            var db = Database.Scopes.AddMainDbIfNotExists();

            string databaseName = db.DatabaseName; // Database.Configuration.DefaultDatabaseNamePrefix + "_" + dbScope.Name;
            Assert.True(db.Structure.IsDatabaseAvailable(databaseName));
            Assert.True(db.Structure.IsSchemaAvailable("Base"));
        }

        [Fact]
        public virtual async Task Structure_02_PredefinedRecords()
        {
            var db = Database.Scopes.AddMainDbIfNotExists();

            string content07 = this.Fixture.GetFileContentAsString("TestContent\\jsonEntity07.withPredefinedData.json");
            db.Metadata.AddIfNotExist<ConcreteEntity01>();
            db.Metadata.AddJson(content07);

            db.Structure.ApplyAllStructure();

            var em = db.Metadata.Get("myJsonEntity07");
            Assert.NotNull(em);

            var predefinedValues = await db.Load("myJsonEntity07");
            Assert.Equal(2, predefinedValues.ItemCount);

            IEntity ent01 = await db.Find("myJsonEntity07", 1);
            Assert.NotNull(ent01);
            Assert.Equal("Meeting 1", ent01["Subject"]);


            IEntity ent03 = await db.Find("myJsonEntity07", 3);
            Assert.NotNull(ent03);
            Assert.Equal("Meeting 2", ent03["Subject"]);
            Assert.Equal(333, ent03["Price"].As<decimal>());


        }

        //[Fact]
        //public virtual void BaseSchema_02_InOtherDb()
        //{
        //    this.Fixture.Clear();
        //}
    }
}
