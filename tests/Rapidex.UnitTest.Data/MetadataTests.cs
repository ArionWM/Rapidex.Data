
using Rapidex.Data;
using Rapidex.Data.SqlServer;

using Rapidex.UnitTest.Data.Fixtures;
using Rapidex.UnitTest.Data.TestBase;
using Rapidex.UnitTest.Data.TestContent;
using Rapidex.UnitTest.Data.TestContent.Concrete;
using System.Data;
using static Rapidex.Data.Reference;
using static Rapidex.UnitTest.Data.Behaviors.BehaviorTests;

namespace Rapidex.UnitTest.Data;

public class MetadataTests : DbDependedTestsBase<DbSqlServerProvider>
{
    public MetadataTests( SingletonFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
    {
    }


    /*
    Entity definition tests
    Structure create tests (create, update)

    Custom entity definition tests
    */



    [Fact]
    public void MetadataCreation_WithConcrete_01()
    {

        this.Fixture.ClearCaches();
        var db = Database.Dbs.Db();
        var schema = db.AddSchemaIfNotExists("myTestSchema01");

        db.Metadata.AddIfNotExist<ConcreteEntity01>();

        IDbEntityMetadata em01 = db.Metadata.Get<ConcreteEntity01>();

        //DbConcreteEntityBase oluþmalý

        //Assert.Equal(entityMetadata.EntityType, typeof(ConcreteEntity01));
        //Assert.Equal("myTestSchema01", em01.SchemaName);
        Assert.Equal("unitTestData", em01.ModuleName); //UnitTest.Data -> UnitTestData
        Assert.Equal("utest", em01.Prefix);
        Assert.Equal("ConcreteEntity01", em01.Name);
        Assert.Equal(em01.Fields["Id"], em01.PrimaryKey);
        Assert.Equal(18, em01.Fields.Count);
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


        db.Metadata.AddIfNotExist<ConcreteEntity02>();
        IDbEntityMetadata em02 = db.Metadata.Get<ConcreteEntity02>();

        //Reference ...
    }

    [Fact]
    public void MetadataCreation_WithConcrete_02_AsPremature()
    {
        //ConcreteEntity02, ConcreteEntity01'e referans veriyor.
        //ConcreteEntity01 ise henüz eklenmedi. Bu durumda prematüre bir taným oluþmalý
        try
        {
            this.Fixture.ClearCaches();

            var db = Database.Dbs.Db();
            var schema = db.AddSchemaIfNotExists("myTestSchema02");

            db.Metadata.Remove<ConcreteEntity01>();
            db.Metadata.ReAdd<ConcreteEntity02>();

            var em = db.Metadata.Get("ConcreteEntity01");
            Assert.NotNull(em);
            Assert.True(em.IsPremature);


            db.Metadata.AddIfNotExist<ConcreteEntity01>();

            em = db.Metadata.Get("ConcreteEntity01");
            Assert.NotNull(em);
            Assert.False(em.IsPremature);
        }
        finally
        {
            this.Fixture.ClearCaches();
        }
    }

    [Fact]
    public void MetadataCreation_WithConcrete_03_WithConcreteImplementer()
    {
        this.Fixture.ClearCaches();

        var db = Database.Dbs.Db();
        var em = db.ReAddReCreate<ConcreteEntityForImplementationTests01>();

        Assert.True(em.Fields.ContainsKey("Name"));

        //added by ConcreteEntityForImplementationTests01Implementer
        Assert.True(em.Fields.ContainsKey("newField"));
        Assert.True(em.Has<HasTags>());
    }

    [Fact]
    public void MetadataCreation_WithConcrete_04_WithYamlPredefinedData()
    {
        var db = Database.Dbs.Db();
        var em = db.ReAddReCreate<ConcreteEntityForImplementationTests02>();

        string content = this.Fixture.GetFileContentAsString("TestContent\\Yaml\\EntityData01.yml");
        db.Metadata.AddYaml(content);

        PredefinedValueItems items = db.Metadata.Data.Repository.Get(em);
        Assert.NotNull(items);
        Assert.Equal(2, items.Entities.Count);

        var itm1 = items.Entities.Get(100);
        Assert.NotNull(itm1);

        Assert.Equal("Predefined 1", itm1["Name"].As<string>());
        Assert.Equal(DateTimeOffset.Parse("2023-01-01T10:00:00Z"), itm1["DateTimeField"].As<DateTimeOffset>());
        Assert.Equal(MyEnum01.Value2, itm1["EnumField"].As<MyEnum01>());


    }


    [Fact]
    public void MetadataCreation_WithJson_01()
    {
        //EntityMetadata01.json
        //EntityMetadata02.json
        //DbEntity oluþmalý
        this.Fixture.ClearCaches();

        var db = Database.Dbs.Db();
        var schema = db.AddSchemaIfNotExists("myTestSchema03");


        string content = this.Fixture.GetFileContentAsString("TestContent\\json\\jsonEntity01.json");
        db.Metadata.AddJson(content);

        IDbEntityMetadata em01 = db.Metadata.Get("myJsonEntity01");
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
    public void MetadataCreation_WithCombine_01_JsonAndConcrete()
    {
        this.Fixture.ClearCaches();

        var db = Database.Dbs.Db();
        var schema = db.AddSchemaIfNotExists("myTestSchema03");

        db.Metadata.AddIfNotExist<ConcreteEntity01>();
        db.Metadata.AddIfNotExist<ConcreteEntity02>();

        string content = this.Fixture.GetFileContentAsString("TestContent\\json\\jsonEntity02.WithBehavior.json");
        db.Metadata.AddJson(content);

        //myJsonEntity02

        IDbEntityMetadata em01 = db.Metadata.Get("myJsonEntity02");
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
    public void MetadataCreation_WithJson_02_JsonDefinitionsShouldContainTypeInfo()
    {
        //EntityMetadata01.json
        //EntityMetadata02.json
        //DbEntity oluþmalý
        this.Fixture.ClearCaches();

        var db = Database.Dbs.Db();
        var schema = db.AddSchemaIfNotExists("myTestSchema03");



        string content = this.Fixture.GetFileContentAsString("TestContent\\json\\jsonEntity01.json");
        content = content.Replace("\"EntityDefinition\"", "\"\""); //Type bilgisi olmadan hata vermesi gerekiyor
        Assert.ThrowsAny<Exception>(() => db.Metadata.AddJson(content));
    }

    [Fact]
    public void MetadataCreation_WithJson_03_IdFieldAddAutomaticaly()
    {
        this.Fixture.ClearCaches();

        var db = Database.Dbs.Db();

        string content = this.Fixture.GetFileContentAsString("TestContent\\json\\jsonEntity02.WithBehavior.json");
        db.Metadata.AddJson(content);

        db.Structure.DropEntity("myJsonEntity02");


        IDbEntityMetadata em01 = db.Metadata.Get("myJsonEntity02"); // jsonEntity02.json tanýmýnda Id alaný bulunmuyor, otomatik eklenmeli
        Assert.NotNull(em01);
        Assert.NotNull(em01.PrimaryKey);
        Assert.Equal("Id", em01.PrimaryKey.Name);

    }

    [Fact]
    public void MetadataCreation_WithJson_04_Behavior()
    {
        var db = Database.Dbs.Db();

        string content = this.Fixture.GetFileContentAsString("TestContent\\json\\jsonEntity02.WithBehavior.json");
        db.Metadata.AddJson(content);

        var em = db.Metadata.Get("myJsonEntity02");
        Assert.NotNull(em);
        Assert.True(em.Has<HasTags>());
        Assert.True(em.Has<TestBehavior1>());
    }

    [Fact]
    public void MetadataCreation_WithConcrete_03_ReferenceOne2N()
    {
        this.Fixture.ClearCaches();
        var db = Database.Dbs.Db();
        var schema = db.AddSchemaIfNotExists("myTestSchema01");

        db.Metadata.AddIfNotExist<ConcreteEntity03>();
        db.Metadata.AddIfNotExist<ConcreteEntity04>();

        IDbEntityMetadata emEnt03 = db.Metadata.Get<ConcreteEntity03>();

        IDbEntityMetadata emEnt04 = db.Metadata.Get<ConcreteEntity04>();

        var fm01 = emEnt04.Fields.Get("Details01");
        Assert.NotNull(fm01);
        Assert.Equal("Details01", fm01.Name);
        Assert.False(fm01.IsPersisted);

        var fm02 = emEnt03.Fields.Get("ParentConcreteEntity04");
        Assert.NotNull(fm02);
    }


    [Fact]
    public void MetadataCreation_WithYaml_01()
    {
        this.Fixture.ClearCaches();

        var db = Database.Dbs.Db();
        var schema = db.AddSchemaIfNotExists("myTestSchema03");

        string content = this.Fixture.GetFileContentAsString("TestContent\\yaml\\Entity.01Simple.yml");
        db.Metadata.AddYaml(content);

        IDbEntityMetadata em01 = db.Metadata.Get("myEntity1");
        Assert.NotNull(em01);
        //Assert.Equal("myTestSchema03", em01.SchemaName);
        Assert.Equal("myEntity1", em01.Name);
        Assert.Equal(em01.Fields["Id"], em01.PrimaryKey);
        Assert.Equal(6, em01.Fields.Count); //12 alan var ancak ekledikleri ile birlikte 16 oluyor
        Assert.Equal("Id", em01.Fields["Id"].Name);
        Assert.Equal(typeof(long), em01.Fields["Id"].Type);
        Assert.Equal(typeof(long), em01.Fields["Id"].BaseType);
        Assert.Equal(DbFieldType.Int64, em01.Fields["Id"].DbType);

        var fm01 = em01.Fields.Get("Title");
        Assert.NotNull(fm01);
        Assert.Equal("Title", fm01.Name);
        Assert.Equal(typeof(string), fm01.Type);
        Assert.Equal(typeof(string), fm01.BaseType);
        Assert.Equal(DbFieldType.String, fm01.DbType);

    }

    [Fact]
    public void MetadataCreation_WithYaml_02_PredefinedData()
    {
        this.Fixture.ClearCaches();

        var db = Database.Dbs.Db();
        var schema = db.AddSchemaIfNotExists("myTestSchema03");

        string content = this.Fixture.GetFileContentAsString("TestContent\\yaml\\Entity.02SimpleWithData.yml");
        db.Metadata.AddYaml(content);

        IDbEntityMetadata em01 = db.Metadata.Get("myEntity2");
        Assert.NotNull(em01);
        //Assert.Equal("myTestSchema03", em01.SchemaName);
        Assert.Equal("myEntity2", em01.Name);

        PredefinedValueItems items = db.Metadata.Data.Repository.Get(em01);
        Assert.NotNull(items);
        Assert.Equal(2, items.Entities.Count);

        var item1 = items.Entities.Get(100);
        Assert.NotNull(item1);
        Assert.Equal("My Entity2 1", item1["Title"].As<string>());

        var item2 = items.Entities.Get(101);
        Assert.NotNull(item2);
        Assert.Equal("My Entity2 2", item2["Title"].As<string>());




    }




}