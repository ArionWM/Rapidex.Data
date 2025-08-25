using Rapidex.Data;

using Rapidex.UnitTest.Data.Fixtures;
using Rapidex.UnitTest.Data.TestContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestBase
{
    public abstract class TableStructureTestsBase<T> : DbDependedTestsBase<T> where T : IDbProvider
    {
        protected TableStructureTestsBase(SingletonFixtureFactory<DbWithProviderFixture<T>> factory) : base(factory)
        {
        }

        protected abstract string[] GetTableNamesInSchema(IDbSchemaScope schemaScope);

        [Fact]
        public virtual void Structure_01_CreateTable_FromJson()
        {
            this.Fixture.ClearCaches();

            var dbScope = Database.Scopes.Db();


            string content = this.Fixture.GetFileContentAsString("TestContent\\json\\jsonEntity03.base.json");
            dbScope.Metadata.AddJson(content); //dbScope.Metadata using base schema...
            dbScope.Metadata.Add<ConcreteEntity01>();
            dbScope.Metadata.Add<ConcreteEntity02>();


            dbScope.Structure.DropEntity("myJsonEntity03");

            var em = dbScope.Metadata.Get("myJsonEntity03");
            dbScope.Structure.ApplyEntityStructure(em);

            //... Provider specific overrides ...
        }

        [Fact]
        public virtual void Structure_02_CreateTable_FromConcrete()
        {
            this.Fixture.ClearCaches();


            var dbScope = Database.Scopes.Db();

            dbScope.ReAddReCreate<ConcreteEntity01>();


            //... Provider specific overrides ...
        }

        [Fact]
        public virtual void Structure_03_AddColumnInRuntime()
        {
            this.Fixture.ClearCaches();

            var db = Database.Scopes.Db();

            string content = this.Fixture.GetFileContentAsString("TestContent\\json\\jsonEntity04.base.json");
            db.Metadata.AddJson(content); //dbScope.Metadata using base schema...

            db.Structure.DropEntity("myJsonEntity04");

            var em = db.Metadata.Get("myJsonEntity04");
            db.Structure.ApplyEntityStructure(em);

        }


        [Fact]
        public virtual void Structure_04_PrematureTablesRaiseException()
        {
            this.Fixture.ClearCaches();

            try
            {
                string tableName = RandomHelper.RandomText(10);

                //((Rapidex.Data.Scopes.DbScopeManager)Database.Scopes).ClearCache();

                var dbScope = Database.Scopes.Db();
                dbScope.Metadata.AddPremature(tableName);

                Assert.Throws<InvalidOperationException>(() => dbScope.Structure.ApplyAllStructure());
            }
            finally
            {
                this.Fixture.ClearCaches();
            }
        }


        [Fact]
        public virtual void Structure_05_RuntimeModify()
        {
            this.Fixture.ClearCaches();

            var db = Database.Scopes.Db();

            db.Metadata.Remove("myJsonEntity04");

            string content = this.Fixture.GetFileContentAsString("TestContent\\json\\jsonEntity04.base.json");
            db.Metadata.AddJson(content); //dbScope.Metadata using base schema...
            db.Structure.DropEntity("myJsonEntity04");

            var em = db.Metadata.Get("myJsonEntity04");
            Assert.Equal("utest", em.Prefix);

            Assert.NotNull(em);
            Assert.True(em.Fields.ContainsKey("Id"));


            db.Structure.ApplyEntityStructure(em);

        }

        [Fact]
        public virtual void Structure_06_OnlyBaseTables_Concrete()
        {
            this.Fixture.ClearCaches();

            var db = Database.Scopes.Db();

            var em = db.Metadata.AddIfNotExist<ConcreteOnlyBaseEntity01>().MarkOnlyBaseSchema();

            var baseSchema = db.Schema("base");
            baseSchema.Structure.DropEntity<ConcreteOnlyBaseEntity01>();
            baseSchema.Structure.ApplyAllStructure();

            var schema02 = db.AddSchemaIfNotExists("schema02");
            schema02.Structure.DropEntity<ConcreteOnlyBaseEntity01>();
            schema02.Structure.ApplyAllStructure();

            var tableNames = this.GetTableNamesInSchema(baseSchema);
            Assert.Contains(schema02.Structure.CheckObjectName(em.TableName), tableNames);

            var tableNames02 = this.GetTableNamesInSchema(schema02);
            Assert.DoesNotContain(schema02.Structure.CheckObjectName(em.TableName), tableNames02);
        }

        [Fact]
        public virtual void Structure_07_OnlyBaseTables_Json()
        {
            this.Fixture.ClearCaches();

            var db = Database.Scopes.Db();
            string content = this.Fixture.GetFileContentAsString("TestContent\\json\\jsonEntity09.OnlyBase.json");
            var ems = db.Metadata.AddJson(content);
            Assert.NotNull(ems);
            Assert.Single(ems);

            var baseSchema = db.Schema("base");
            baseSchema.Structure.DropEntity("myJsonEntity09");
            baseSchema.Structure.ApplyAllStructure();

            var schema02 = db.AddSchemaIfNotExists("schema02");
            schema02.Structure.DropEntity("myJsonEntity09");
            schema02.Structure.ApplyAllStructure();

            var tableNames = this.GetTableNamesInSchema(baseSchema);
            Assert.Contains(schema02.Structure.CheckObjectName(ems.First().TableName), tableNames);

            var tableNames02 = this.GetTableNamesInSchema(schema02);
            Assert.DoesNotContain(schema02.Structure.CheckObjectName(ems.First().TableName), tableNames02);
        }

    }
}
