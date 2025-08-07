using NuGet.Frameworks;

using Rapidex.Data;
using Rapidex.Data.SqlServer;

using Rapidex.UnitTest.Data.Fixtures;
using Rapidex.UnitTest.Data.TestBase;
using Rapidex.UnitTest.Data.TestContent;
using System.Data;
using static Rapidex.Data.Reference;
using static Rapidex.UnitTest.Data.Behaviors.BehaviorTests;

namespace Rapidex.UnitTest.Data
{
    public class MetadataTests : DbDependedTestsBase<DbSqlServerProvider>
    {
        public MetadataTests(SingletonFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
        {
        }


        /*
        Entity definition tests
        Structure create tests (create, update)

        Custom entity definition tests
        */



        [Fact]
        public void MetadataCreation_01_WithConcreteEntity()
        {

            this.Fixture.ClearCaches();
            var database = Database.Scopes.Db();
            var schema = database.AddSchemaIfNotExists("myTestSchema01");

            Database.Metadata.AddIfNotExist<ConcreteEntity01>();

            IDbEntityMetadata em01 = Database.Metadata.Get<ConcreteEntity01>();

            //DbConcreteEntityBase oluþmalý

            //Assert.Equal(entityMetadata.EntityType, typeof(ConcreteEntity01));
            //Assert.Equal("myTestSchema01", em01.SchemaName);
            Assert.Equal("unitTestData", em01.ModuleName); //UnitTest.Data -> UnitTestData
            Assert.Equal("utest", em01.Prefix);
            Assert.Equal("ConcreteEntity01", em01.Name);
            Assert.Equal(em01.Fields["Id"], em01.PrimaryKey);
            Assert.Equal(17, em01.Fields.Count);
            Assert.Equal("Id", em01.Fields["Id"].Name);
            Assert.Equal(typeof(long), em01.Fields["Id"].Type);
            Assert.Equal(typeof(long), em01.Fields["Id"].BaseType);
            Assert.Equal(DbFieldType.Int64, em01.Fields["Id"].DbType);
            Assert.Equal("Name", em01.Caption.Name);

            Assert.Equal("Name", em01.Fields["Name"].Name);
            Assert.Equal(typeof(string), em01.Fields["Name"].Type);
            Assert.Equal(typeof(string), em01.Fields["Name"].BaseType);
            Assert.Equal(DbFieldType.String, em01.Fields["Name"].DbType);

            Assert.NotNull(em01.PrimaryKey);
            Assert.Equal("Id", em01.PrimaryKey.Name);

            //Assert.True(em01.PrimaryKey.Sealed)

            var pictureFm = em01.Fields["Picture"];
            Assert.False(pictureFm.SkipDirectLoad);
            Assert.False(pictureFm.SkipDirectSet);


            Database.Metadata.AddIfNotExist<ConcreteEntity02>();
            IDbEntityMetadata em02 = Database.Metadata.Get<ConcreteEntity02>();

            //Reference ...
        }

        [Fact]
        public void MetadataCreation_02_WithConcreteEntity_AsPremature()
        {
            //ConcreteEntity02, ConcreteEntity01'e referans veriyor.
            //ConcreteEntity01 ise henüz eklenmedi. Bu durumda prematüre bir taným oluþmalý
            try
            {
                var database = Database.Scopes.Db();
                var schema = database.AddSchemaIfNotExists("myTestSchema02");
                
                Database.Metadata.Remove<ConcreteEntity01>();
                Database.Metadata.ReAdd<ConcreteEntity02>();

                var em = Database.Metadata.Get("ConcreteEntity01");
                Assert.NotNull(em);
                Assert.True(em.IsPremature);


                Database.Metadata.AddIfNotExist<ConcreteEntity01>();

                em = Database.Metadata.Get("ConcreteEntity01");
                Assert.NotNull(em);
                Assert.False(em.IsPremature);
            }
            finally
            {
                this.Fixture.ClearCaches();
            }
        }

        [Fact]
        public void MetadataCreation_03_WithJson()
        {
            //EntityMetadata01.json
            //EntityMetadata02.json
            //DbEntity oluþmalý
            this.Fixture.ClearCaches();

            var database = Database.Scopes.Db();
            var schema = database.AddSchemaIfNotExists("myTestSchema03");


            string content = this.Fixture.GetFileContentAsString("TestContent\\jsonEntity01.json");
            Database.Metadata.AddFromJson(content);

            IDbEntityMetadata em01 = Database.Metadata.Get("myJsonEntity01");
            Assert.NotNull(em01);
            //Assert.Equal("myTestSchema03", em01.SchemaName);
            Assert.Equal("myJsonEntity01", em01.Name);
            Assert.Equal(em01.Fields["Id"], em01.PrimaryKey);
            Assert.Equal(18, em01.Fields.Count); //12 alan var ancak ekledikleri ile birlikte 16 oluyor
            Assert.Equal("Id", em01.Fields["Id"].Name);
            Assert.Equal(typeof(long), em01.Fields["Id"].Type);
            Assert.Equal(typeof(long), em01.Fields["Id"].BaseType);
            Assert.Equal(DbFieldType.Int64, em01.Fields["Id"].DbType);

            var fm01 = em01.Fields.Get("Subject");
            Assert.NotNull(fm01);
            Assert.Equal("Subject", fm01.Name);
            Assert.Equal(typeof(string), fm01.Type);
            Assert.Equal(typeof(string), fm01.BaseType);
            Assert.Equal(DbFieldType.String, fm01.DbType);

            Assert.Equal("Subject", em01.Caption.Name);
        }

        [Fact]
        public void MetadataCreation_04_WithJson_IdFieldAddAutomaticaly()
        {
            this.Fixture.ClearCaches();

            var dbScope = Database.Scopes.Db();

            string content = this.Fixture.GetFileContentAsString("TestContent\\jsonEntity02.WithBehavior.json");
            Database.Metadata.AddFromJson(content);

            dbScope.Structure.DropEntity("myJsonEntity02");


            IDbEntityMetadata em01 = Database.Metadata.Get("myJsonEntity02"); // jsonEntity02.json tanýmýnda Id alaný bulunmuyor, otomatik eklenmeli
            Assert.NotNull(em01);
            Assert.NotNull(em01.PrimaryKey);
            Assert.Equal("Id", em01.PrimaryKey.Name);

        }

        [Fact]
        public void MetadataCreation_04_JsonDefinitionsShouldContainTypeInfo()
        {
            //EntityMetadata01.json
            //EntityMetadata02.json
            //DbEntity oluþmalý
            this.Fixture.ClearCaches();

            var database = Database.Scopes.Db();
            var schema = database.AddSchemaIfNotExists("myTestSchema03");



            string content = this.Fixture.GetFileContentAsString("TestContent\\jsonEntity01.json");
            content = content.Replace("\"EntityDefinition\"", "\"\""); //Type bilgisi olmadan hata vermesi gerekiyor
            Assert.ThrowsAny<Exception>(() => Database.Metadata.AddFromJson(content));
        }

        [Fact]
        public void MetadataCreation_04_CombineWithJsonAndConcrete()
        {
            this.Fixture.ClearCaches();

            var database = Database.Scopes.Db();
            var schema = database.AddSchemaIfNotExists("myTestSchema03");

            Database.Metadata.AddIfNotExist<ConcreteEntity01>();
            Database.Metadata.AddIfNotExist<ConcreteEntity02>();

            string content = this.Fixture.GetFileContentAsString("TestContent\\jsonEntity02.WithBehavior.json");
            Database.Metadata.AddFromJson(content);

            //myJsonEntity02

            IDbEntityMetadata em01 = Database.Metadata.Get("myJsonEntity02");
            Assert.NotNull(em01);
            //Assert.Equal("myTestSchema03", em01.SchemaName);
            Assert.Equal("myJsonEntity02", em01.Name);
            Assert.Equal(em01.Fields["Id"], em01.PrimaryKey);

            IDbFieldMetadata ref01Field = em01.Fields["ConcreteReference01"];
            Assert.NotNull(ref01Field);
            Assert.Equal("ConcreteReference01", ref01Field.Name);
            Assert.Equal(typeof(long), ref01Field.BaseType);
            Assert.False(ref01Field.SkipDirectLoad);
            Assert.True(ref01Field.Type.IsSupportTo<ILazy>());
            Assert.True(ref01Field.Type.IsSupportTo<Reference>());

            ReferenceDbFieldMetadata referenceDbFieldMetadata01 = ref01Field as ReferenceDbFieldMetadata;
            Assert.NotNull(referenceDbFieldMetadata01);
            Assert.Equal("ConcreteEntity02", referenceDbFieldMetadata01.ReferencedEntity);

            IDbFieldMetadata ref02Field = em01.Fields["ConcreteReference02"];
            Assert.NotNull(ref02Field);
            Assert.Equal("ConcreteReference02", ref02Field.Name);
            Assert.Equal(typeof(long), ref02Field.BaseType);
            Assert.True(ref02Field.Type.IsSupportTo<ILazy>());
            Assert.True(ref02Field.Type.IsSupportTo<Reference>());

            ReferenceDbFieldMetadata referenceDbFieldMetadata02 = ref02Field as ReferenceDbFieldMetadata;
            Assert.NotNull(referenceDbFieldMetadata02);
            Assert.Equal("ConcreteEntity01", referenceDbFieldMetadata02.ReferencedEntity);
        }

        [Fact]
        public void MetadataCreation_05_ReferenceOne2N_Concrete()
        {
            this.Fixture.ClearCaches();
            var database = Database.Scopes.Db();
            var schema = database.AddSchemaIfNotExists("myTestSchema01");

            Database.Metadata.AddIfNotExist<ConcreteEntity03>();
            Database.Metadata.AddIfNotExist<ConcreteEntity04>();

            IDbEntityMetadata emEnt03 = Database.Metadata.Get<ConcreteEntity03>();

            IDbEntityMetadata emEnt04 = Database.Metadata.Get<ConcreteEntity04>();

            var fm01 = emEnt04.Fields.Get("Details01");
            Assert.NotNull(fm01);
            Assert.Equal("Details01", fm01.Name);
            Assert.False(fm01.IsPersisted);

            var fm02 = emEnt03.Fields.Get("ParentConcreteEntity04");
            Assert.NotNull(fm02);
        }

        [Fact]
        public void MetadataCreation_06_JsonWithBehavior()
        {
            string content = this.Fixture.GetFileContentAsString("TestContent\\jsonEntity02.WithBehavior.json");
            Database.Metadata.AddFromJson(content);

            var em = Database.Metadata.Get("myJsonEntity02");
            Assert.NotNull(em);
            Assert.True(em.Has<HasTags>());
            Assert.True(em.Has<TestBehavior1>());
        }










    }
}