using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rapidex.Data.Entities;
using Rapidex.Data.Query;
using Rapidex.UnitTest.Data.Helpers;
using Rapidex.UnitTest.Data.TestContent;

namespace Rapidex.UnitTest.Data.TestBase;


public abstract class QueryTestsBase<T> : DbDependedTestsBase<T> where T : IDbProvider
{
    public QueryTestsBase(EachTestClassIsolatedFixtureFactory<DbWithProviderFixture<T>> factory) : base(factory)
    {
    }

    [Fact]
    public virtual void Load_01_LoadMultiple()
    {
        this.Fixture.ClearCaches();

        var db = Database.Dbs.AddMainDbIfNotExists();

        db.Metadata.AddIfNotExist<ConcreteEntity01>();
        db.Structure.DropEntity<ConcreteEntity01>();

        db.Structure.ApplyEntityStructure<ConcreteEntity01>();

        using var work = db.BeginWork();

        for (int i = 0; i < 10; i++)
        {
            ConcreteEntity01 entity = work.New<ConcreteEntity01>();
            entity.Name = $"Entity Name {i:000}";
            entity.CreditLimit1 = 10000 * i;
            entity.Description = $"Description {i:000}";
            entity.Save();
        }

        work.CommitChanges();

        this.Fixture.ClearCaches();

        db.Metadata.AddIfNotExist<ConcreteEntity01>();

        var result = db.Load<ConcreteEntity01>().Result;
        Assert.Equal(10, result.ItemCount);

        foreach (var item in result)
        {
            Assert.NotNull(item.Name);
            Assert.NotNull(item.Description);

            item.CheckIDataTypeAssignments();
        }
    }


    [Fact]
    public virtual void Load_02_SimpleCriteria_Concrete()
    {
        this.Fixture.ClearCaches();

        var db = Database.Dbs.AddMainDbIfNotExists();

        db.Metadata.AddIfNotExist<ConcreteEntity01>();
        db.Structure.DropEntity<ConcreteEntity01>();

        db.Structure.ApplyEntityStructure<ConcreteEntity01>();

        using var work = db.BeginWork();

        for (int i = 0; i < 10; i++)
        {
            ConcreteEntity01 entity = work.New<ConcreteEntity01>();
            entity.Name = $"Load_01_SimpleCriteria_Concrete {i:000}";
            entity.CreditLimit1 = 10000 * i;

            if (i % 2 == 0)
            {
                entity.Phone = $"Phone {i:000}";
                entity.Description = $"even";
            }

            entity.Save();
        }

        work.CommitChanges();

        var result = db.Load<ConcreteEntity01>(q => q.Eq(nameof(ConcreteEntity01.Name), "Phone 002"));
    }


    protected void Load_03_GenerateEntities(IDbSchemaScope scope, IDbEntityMetadata em)
    {
        var db = Database.Dbs.AddMainDbIfNotExists();

        db.Structure.DropEntity<ConcreteEntity01>();
        db.Structure.DropEntity(em);

        db.Structure.ApplyEntityStructure<ConcreteEntity01>();
        db.Structure.ApplyEntityStructure(em);

        //100 entity
        //"No" birer artıyor (1'den başlayarak)
        //Name: "Entity Name 001" şeklinde
        //CreditLimit1: 10000 * No
        //Description: "Description 001" şeklinde
        //Amount: i % 10
        //IsActive: i % 2 == 0
        //Phone: "53367222(i:00)" şeklinde
        //Email: ilk 50 adedi "email(i:00)@testA.com", diğerleri "email(i-49:00)@testB.com"
        //BirthDate: 01.01.2000 + i gün
        //UniqueId: Guid.NewGuid()
        //Reference01: 10 tane ConcreteEntity01 oluşturuyoruz, index = i % 10 olarak ekliyoruz

        var work1 = scope.BeginWork();

        List<ConcreteEntity01> refEntities = new List<ConcreteEntity01>();

        for (int i = 0; i < 10; i++)
        {
            ConcreteEntity01 entity = work1.New<ConcreteEntity01>();
            entity.Name = $"Entity Name {i:000}";
            entity.CreditLimit1 = 10000 * i;
            entity.Description = $"Description {i:000}";
            entity.Save();

            refEntities.Add(entity);
        }

        work1.CommitChanges();


        var work2 = scope.BeginWork();

        for (int i = 0; i < 100; i++)
        {
            IEntity entity = work2.New(em);
            entity["No"] = i + 1;
            entity["Name"] = $"Entity Name {i + 1:000}";
            entity["Value"] = 10000 * (i + 1);
            entity["Description"] = $"Description {i + 1:000}";
            entity["Amount"] = i % 10;
            entity["IsActive"] = i % 2 == 0;
            entity["Phone"] = $"53367222{i:00}";
            entity["EMail"] = i < 50 ? $"email(i:00)@testA.com" : $"email(i-49:00)@testB.com";
            entity["BirthDate"] = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero).AddDays(i);
            entity["UniqueId"] = Guid.NewGuid();

            int index = i % 10;
            entity["Reference01"] = refEntities[index];

            entity.Save();
        }

        work2.CommitChanges();

    }

    protected virtual void Load_03_Criterias(IDbSchemaScope scope, IDbEntityMetadata em)
    {
        this.Load_03_GenerateEntities(scope, em);

        //1. IsActive == true olanları getir
        long count = scope.GetQuery(em).Eq("IsActive", true).Count().Result;
        Assert.Equal(50, count);

        //2. IsActive == false olanları getir
        count = scope.GetQuery(em).Eq("IsActive", false).Count().Result;
        Assert.Equal(50, count);

        //3. Amount == 5 olanları getir
        count = scope.GetQuery(em).Eq("Amount", 5).Count().Result;
        Assert.Equal(10, count);

        //4. Amount > 5 olanları getir
        count = scope.GetQuery(em).Gt("Amount", 5).Count().Result;
        Assert.Equal(40, count);

        //5. Amount < 5 olanları getir
        count = scope.GetQuery(em).Lt("Amount", 5).Count().Result;
        Assert.Equal(50, count);

        //6. Amount >= 5 olanları getir
        count = scope.GetQuery(em).GtEq("Amount", 5).Count().Result;
        Assert.Equal(50, count);

        //7. Amount <= 5 olanları getir
        count = scope.GetQuery(em).LtEq("Amount", 5).Count().Result;
        Assert.Equal(60, count);

        //8. Amount between 3 and 7 olanları getir
        count = scope.GetQuery(em).Between("Amount", 3, 7).Count().Result;
        Assert.Equal(50, count);

        //9. Name like "Entity Name 005" olanları getir
        count = scope.GetQuery(em).Like("Name", "Entity Name 005").Count().Result;
        Assert.Equal(1, count);

        //10. Name like "Entity Name *" olanları getir
        count = scope.GetQuery(em).Like("Name", "Entity Name 00*").Count().Result;
        Assert.Equal(9, count);

        //11. Phone like "5336722205" olanları getir
        count = scope.GetQuery(em).Like("Phone", "5336722205").Count().Result;
        Assert.Equal(1, count);

        //12. Value > 50000 olanları getir
        count = scope.GetQuery(em).Gt("Value", 50000).Count().Result;
        Assert.Equal(95, count);

        //13. BirthDate > 20.01.2000 olanları getir
        count = scope.GetQuery(em).Gt("BirthDate", new DateTimeOffset(2000, 1, 20, 0, 0, 0, TimeSpan.Zero)).Count().Result;
        Assert.Equal(80, count);

        //14. BirthDate < 20.01.2000 olanları getir
        count = scope.GetQuery(em).Lt("BirthDate", new DateTimeOffset(2000, 1, 20, 0, 0, 0, TimeSpan.Zero)).Count().Result;
        Assert.Equal(19, count);

        //15. Reference ilişkisindeki kayıtın adı "Entity Name 001" olanları getir
        count = scope.GetQuery(em).Nested("Reference01", q => q.Eq(nameof(ConcreteEntity01.Name), "Entity Name 001")).Count().Result;
        Assert.Equal(10, count);



    }


    [Fact]
    public virtual void Load_03_Criterias_Concrete()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<ConcreteEntity01>();
        db.Metadata.AddIfNotExist<CriteriaTestEntity01>();

        db.Structure.ApplyEntityStructure<ConcreteEntity01>();
        db.Structure.ApplyEntityStructure<CriteriaTestEntity01>();

        this.Load_03_Criterias(db, db.Metadata.Get<CriteriaTestEntity01>());
    }


    [Fact]
    public virtual void Load_04_Criterias_Nested()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<ConcreteEntity01>();
        db.Metadata.AddIfNotExist<ConcreteEntity02>();

        db.Structure.DropEntity<ConcreteEntity01>();
        db.Structure.DropEntity<ConcreteEntity02>();

        db.Structure.ApplyEntityStructure<ConcreteEntity01>();
        db.Structure.ApplyEntityStructure<ConcreteEntity02>();

        //---------------------------------------------------------------------------------
        using var work = db.BeginWork();

        ConcreteEntity01 refEnt01 = work.New<ConcreteEntity01>();
        refEnt01.Name = "Ent 01";
        refEnt01.Save();

        ConcreteEntity01 refEnt02 = work.New<ConcreteEntity01>();
        refEnt02.Name = "Ent 02";
        refEnt02.Save();

        ConcreteEntity02 tent01 = work.New<ConcreteEntity02>();
        tent01.MyReference = refEnt01;
        tent01.Save();

        ConcreteEntity02 tent02 = work.New<ConcreteEntity02>();
        tent02.MyReference = null;
        tent02.Save();

        work.CommitChanges();

        //---------------------------------------------------------------------------------

        var loadResult01 = db.GetQuery<ConcreteEntity02>()
              .Nested(nameof(ConcreteEntity02.MyReference), q => q.Eq(nameof(ConcreteEntity01.Name), "Ent 01"))
              .Load()
              .Result;

        Assert.Single(loadResult01);




    }


    [Fact]
    public virtual void Load_05_Criterias_NestedWithOnlyBase()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();

        var schemaBase = db.Schema("base");
        var schema02 = db.AddSchemaIfNotExists("schema02");

        db.Metadata.AddIfNotExist<ConcreteOnlyBaseEntity01>().MarkOnlyBaseSchema();
        db.Metadata.AddIfNotExist<ConcreteOnlyBaseReferencedEntity02>();

        schemaBase.Structure.DropEntity<ConcreteOnlyBaseEntity01>();
        schemaBase.Structure.DropEntity<ConcreteOnlyBaseReferencedEntity02>();
        schemaBase.Structure.ApplyEntityStructure<ConcreteOnlyBaseEntity01>();
        schemaBase.Structure.ApplyEntityStructure<ConcreteOnlyBaseReferencedEntity02>();


        schema02.Structure.DropEntity<ConcreteOnlyBaseReferencedEntity02>();
        schema02.Structure.ApplyEntityStructure<ConcreteOnlyBaseReferencedEntity02>();

        //---------------------------------------------------------------------------------

        using var workOnBase = schemaBase.BeginWork();

        ConcreteOnlyBaseEntity01 baseEnt01 = workOnBase.New<ConcreteOnlyBaseEntity01>();
        baseEnt01.Name = "Nest Test 01 CEnt 01";
        baseEnt01.Save();

        ConcreteOnlyBaseEntity01 baseEnt02 = workOnBase.New<ConcreteOnlyBaseEntity01>();
        baseEnt02.Name = "Nest Test 01 CEnt 02";
        baseEnt02.Save();

        workOnBase.CommitChanges();

        long baseId01 = (long)baseEnt01.GetId();
        long baseId02 = (long)baseEnt02.GetId();


        using var workOnSchema02 = schema02.BeginWork();

        ConcreteOnlyBaseReferencedEntity02 refEntity01 = workOnSchema02.New<ConcreteOnlyBaseReferencedEntity02>();
        refEntity01.Name = "Nest Test REnt 01";
        refEntity01.Reference = baseEnt01;
        refEntity01.Save();

        ConcreteOnlyBaseReferencedEntity02 refEntity02 = workOnSchema02.New<ConcreteOnlyBaseReferencedEntity02>();
        refEntity02.Name = "Nest Test REnt 02";
        refEntity02.Reference = baseEnt02;
        refEntity02.Save();

        ConcreteOnlyBaseReferencedEntity02 refEntity03 = workOnSchema02.New<ConcreteOnlyBaseReferencedEntity02>();
        refEntity03.Name = "Nest Test REnt 03";
        refEntity03.Reference = null;
        refEntity03.Save();

        workOnSchema02.CommitChanges();

        long refId01 = (long)refEntity01.GetId();
        long refId02 = (long)refEntity02.GetId();
        long refId03 = (long)refEntity03.GetId();

        var loadResult01 = schema02.GetQuery<ConcreteOnlyBaseReferencedEntity02>()
              .Nested(nameof(ConcreteOnlyBaseReferencedEntity02.Reference), q => q.Eq(nameof(ConcreteOnlyBaseEntity01.Name), "Nest Test 01 CEnt 01"))
              .Load()
              .Result;

        Assert.Single(loadResult01);

        ConcreteOnlyBaseReferencedEntity02 lent01 = loadResult01.First();
        Assert.Equal("Nest Test REnt 01", lent01.Name);

    }


    [Fact]
    public virtual void BulkUpdate_06_Criterias_NestedWithOnlyBase()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<ConcreteEntity01>();
        db.Structure.DropEntity<ConcreteEntity01>();
        db.Structure.ApplyEntityStructure<ConcreteEntity01>();

        using var work1 = db.BeginWork();

        ConcreteEntity01 refEnt01 = work1.New<ConcreteEntity01>();
        refEnt01.Name = "Ent 01";
        refEnt01.Phone = "5336722201";
        refEnt01.Address = "Address 01";
        refEnt01.Save();

        ConcreteEntity01 refEnt02 = work1.New<ConcreteEntity01>();
        refEnt02.Name = "Ent 02";
        refEnt02.Phone = "5336722202";
        refEnt02.Address = "Address 02";
        refEnt02.Save();

        ConcreteEntity01 refEnt03 = work1.New<ConcreteEntity01>();
        refEnt03.Name = "Ent 03";
        refEnt03.Phone = "6336722203";
        refEnt03.Address = "Address 03";
        refEnt03.Save();

        work1.CommitChanges();

        long count01 = db.GetQuery<ConcreteEntity01>()
             .Like("Phone", "533672220*")
             .Count().Result;

        Assert.Equal(2, count01);

        long count02 = db.GetQuery<ConcreteEntity01>()
            .Eq("Address", "Address 03")
            .Count().Result;

        Assert.Equal(1, count02);

        using var work2 = db.BeginWork();
        db.GetQuery<ConcreteEntity01>()
             .EnterUpdateMode()
             .Like("Phone", "533672220*")
             .Update(work2, new ObjDictionary() { { "Address", "Updated Address" } });

        //var data = new ObjDictionary();
        //data.Add("Address", "Updated Address");
        //query.Update(data);

        work2.CommitChanges();

        long count03 = db.GetQuery<ConcreteEntity01>()
            .Eq("Address", "Address 03")
            .Count().Result;

        Assert.Equal(1, count03);

        long count04 = db.GetQuery<ConcreteEntity01>()
            .Eq("Address", "Address 01")
            .Count().Result;

        Assert.Equal(0, count04);

        long count05 = db.GetQuery<ConcreteEntity01>()
            .Eq("Address", "Updated Address")
            .Count().Result;

        Assert.Equal(2, count05);

        //Bulk delete
        using var work3 = db.BeginWork();
        db.GetQuery<ConcreteEntity01>()
            .EnterUpdateMode()
            .Limit(10)
            .Eq("Address", "Address 03")
            .Delete(work3);

        work3.CommitChanges();

        long count06 = db.GetQuery<ConcreteEntity01>()
            .Eq("Address", "Address 03")
            .Count().Result;

        Assert.Equal(0, count06);

        long count07 = db.GetQuery<ConcreteEntity01>()
        .Eq("Address", "Updated Address")
        .Count().Result;

        Assert.Equal(2, count07);

    }

    //Nester

    //Order

    //Aggregation

    [Fact]
    public virtual void Load_06_Criterias_Relation()
    {
        var db = Database.Dbs.Db();

        db.Metadata.AddIfNotExist<ConcreteEntityForN2NTest01>();
        db.Metadata.AddIfNotExist<ConcreteEntityForN2NTest02>();
        db.Metadata.AddIfNotExist<GenericJunction>();

        db.Structure.DropEntity<ConcreteEntityForN2NTest01>();
        db.Structure.DropEntity<ConcreteEntityForN2NTest02>();
        db.Structure.DropEntity<GenericJunction>();

        db.Structure.ApplyEntityStructure<ConcreteEntityForN2NTest01>();
        db.Structure.ApplyEntityStructure<ConcreteEntityForN2NTest02>();
        db.Structure.ApplyEntityStructure<GenericJunction>();

        using var work = db.BeginWork();

        ConcreteEntityForN2NTest01 master01 = work.New<ConcreteEntityForN2NTest01>();
        master01.Name = "master01";
        master01.Save();

        ConcreteEntityForN2NTest02 detail01_01 = work.New<ConcreteEntityForN2NTest02>();
        detail01_01.Name = "detail01-01";
        detail01_01.Save();
        master01.Relation01.Add(detail01_01);

        //ConcreteEntityForN2NTest02 detail01_02 = database.New<ConcreteEntityForN2NTest02>();
        //detail01_02.Name = "detail01-02";
        //detail01_02.Save();
        //master01.Relation01.Add(detail01_02);


        ConcreteEntityForN2NTest01 master02 = work.New<ConcreteEntityForN2NTest01>();
        master02.Name = "master02";
        master02.Save();

        ConcreteEntityForN2NTest02 detail02_01 = work.New<ConcreteEntityForN2NTest02>();
        detail02_01.Name = "detail02-02";
        detail02_01.Save();
        master02.Relation01.Add(detail02_01);

        ConcreteEntityForN2NTest02 detail02_02 = work.New<ConcreteEntityForN2NTest02>();
        detail02_02.Name = "detail02-02";
        detail02_02.Save();
        master02.Relation01.Add(detail02_02);

        work.CommitChanges();

        long count01 = db.GetQuery<ConcreteEntityForN2NTest02>()
              .Related(master01, nameof(ConcreteEntityForN2NTest01.Relation01))
              .Count().Result;

        Assert.Equal(1, count01);

        long count02 = db.GetQuery<ConcreteEntityForN2NTest02>()
              .Related(master02, nameof(ConcreteEntityForN2NTest01.Relation01))
              .Count().Result;

        Assert.Equal(2, count02);


    }

    [Fact]
    public virtual void Load_07_DateTimeWithOffset()
    {
        TestEntityCache cache = Database.Cache.ShouldSupportTo<TestEntityCache>();
        Assert.False(TestEntityCache.MemoryCacheEnabled);

        var db = Database.Dbs.Db();
        db.ReAddReCreate<ConcreteEntity01>();
        db.Structure.ApplyAllStructure();

        using var work1 = db.BeginWork();
        ConcreteEntity01 entity1 = work1.New<ConcreteEntity01>();
        entity1.Name = "DateTimeOffset Test";
        entity1.BirthDate = new DateTimeOffset(2024, 6, 15, 10, 30, 0, TimeSpan.Zero);
        entity1.Save();

        ConcreteEntity01 entity2 = work1.New<ConcreteEntity01>();
        entity2.Name = "DateTimeOffset Test";
        entity2.BirthDate = new DateTimeOffset(2024, 2, 17, 09, 21, 0, TimeSpan.FromHours(2));
        entity2.Save();


        work1.CommitChanges();

        long id1 = entity1.Id;
        long id2 = entity2.Id;

        ConcreteEntity01 loadedEntity1 = db.Find<ConcreteEntity01>(id1).Result;
        Assert.Equal(new DateTimeOffset(2024, 6, 15, 10, 30, 0, TimeSpan.Zero), loadedEntity1.BirthDate);

        ConcreteEntity01 loadedEntity2 = db.Find<ConcreteEntity01>(id2).Result;
        Assert.Equal(new DateTimeOffset(2024, 2, 17, 09, 21, 0, TimeSpan.FromHours(2)), loadedEntity2.BirthDate);
        //Load with native
    }

    [Fact]
    public virtual void Load_08_CriteriaProvider_InjectsFilterOnLoad()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.ReAddReCreate<ConcreteEntity01>();
        db.Structure.ApplyEntityStructure<ConcreteEntity01>();

        using (var work = db.BeginWork())
        {
            ConcreteEntity01 e1 = work.New<ConcreteEntity01>();
            e1.Name = "Allowed-01";
            e1.Address = "A";
            e1.Save();

            ConcreteEntity01 e2 = work.New<ConcreteEntity01>();
            e2.Name = "Allowed-02";
            e2.Address = "B";
            e2.Save();

            ConcreteEntity01 e3 = work.New<ConcreteEntity01>();
            e3.Name = "Denied-01";
            e3.Address = "A";
            e3.Save();

            ConcreteEntity01 e4 = work.New<ConcreteEntity01>();
            e4.Name = "Denied-02";
            e4.Address = "B";
            e4.Save();

            work.CommitChanges();
        }

        Assert.Equal(4, db.GetQuery<ConcreteEntity01>().Count().Result);

        var provider = new TestCriteriaProvider(q => q.Like(nameof(ConcreteEntity01.Name), "Allowed*"));
        db.AddCriteriaProvider(provider);
        try
        {
            var loaded = db.GetQuery<ConcreteEntity01>().Load().Result;
            Assert.Equal(2, loaded.ItemCount);
            Assert.All(loaded, e => Assert.StartsWith("Allowed", ((ConcreteEntity01)e).Name));
            Assert.True(provider.CheckCallCount >= 1);

            var first = db.GetQuery<ConcreteEntity01>()
                .Eq(nameof(ConcreteEntity01.Name), "Denied-01")
                .First().Result;
            Assert.Null(first);
        }
        finally
        {
            db.CriteriaProviders.Remove(provider);
        }
    }

    [Fact]
    public virtual void Load_09_CriteriaProvider_AndOverExisting_CombinesWithUserCriteria()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.ReAddReCreate<ConcreteEntity01>();
        db.Structure.ApplyEntityStructure<ConcreteEntity01>();

        using (var work = db.BeginWork())
        {
            ConcreteEntity01 e1 = work.New<ConcreteEntity01>();
            e1.Name = "Allowed-01";
            e1.Address = "Office";
            e1.Save();

            ConcreteEntity01 e2 = work.New<ConcreteEntity01>();
            e2.Name = "Allowed-02";
            e2.Address = "Home";
            e2.Save();

            ConcreteEntity01 e3 = work.New<ConcreteEntity01>();
            e3.Name = "Denied-01";
            e3.Address = "Office";
            e3.Save();

            ConcreteEntity01 e4 = work.New<ConcreteEntity01>();
            e4.Name = "Denied-02";
            e4.Address = "Home";
            e4.Save();

            work.CommitChanges();
        }

        var provider = new TestCriteriaProvider(q => q.Like(nameof(ConcreteEntity01.Name), "Allowed*"));
        db.AddCriteriaProvider(provider);
        try
        {
            var loaded = db.GetQuery<ConcreteEntity01>()
                .Eq(nameof(ConcreteEntity01.Address), "Office")
                .Load().Result;

            Assert.Single(loaded);
            Assert.Equal("Allowed-01", ((ConcreteEntity01)loaded.First()).Name);
        }
        finally
        {
            db.CriteriaProviders.Remove(provider);
        }
    }

    [Fact]
    public virtual void Load_10_CriteriaProvider_AndOverExisting_PreservesOrGroup()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.ReAddReCreate<ConcreteEntity01>();
        db.Structure.ApplyEntityStructure<ConcreteEntity01>();

        using (var work = db.BeginWork())
        {
            ConcreteEntity01 e1 = work.New<ConcreteEntity01>();
            e1.Name = "Allowed-Office";
            e1.Address = "Office";
            e1.Save();

            ConcreteEntity01 e2 = work.New<ConcreteEntity01>();
            e2.Name = "Allowed-Home";
            e2.Address = "Home";
            e2.Save();

            ConcreteEntity01 e3 = work.New<ConcreteEntity01>();
            e3.Name = "Allowed-Other";
            e3.Address = "Other";
            e3.Save();

            ConcreteEntity01 e4 = work.New<ConcreteEntity01>();
            e4.Name = "Denied-Office";
            e4.Address = "Office";
            e4.Save();

            ConcreteEntity01 e5 = work.New<ConcreteEntity01>();
            e5.Name = "Denied-Home";
            e5.Address = "Home";
            e5.Save();

            work.CommitChanges();
        }

        var provider = new TestCriteriaProvider(q => q.Like(nameof(ConcreteEntity01.Name), "Allowed*"));
        db.AddCriteriaProvider(provider);
        try
        {
            // (Address = Office OR Address = Home) AND (Name like Allowed*)
            // AndOverExisting sayesinde OR grubu tek dalda kalır; yetki koşulu dış AND olur.
            var loaded = db.GetQuery<ConcreteEntity01>()
                .Or(
                    q => q.Eq(nameof(ConcreteEntity01.Address), "Office"),
                    q => q.Eq(nameof(ConcreteEntity01.Address), "Home"))
                .Load().Result;

            Assert.Equal(2, loaded.ItemCount);
            var names = loaded.Select(e => ((ConcreteEntity01)e).Name).OrderBy(n => n).ToArray();
            Assert.Equal(new[] { "Allowed-Home", "Allowed-Office" }, names);
        }
        finally
        {
            db.CriteriaProviders.Remove(provider);
        }
    }

    [Fact]
    public virtual void Load_11_CheckFilters_AppliesProvidersExplicitly()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.ReAddReCreate<ConcreteEntity01>();
        db.Structure.ApplyEntityStructure<ConcreteEntity01>();

        using (var work = db.BeginWork())
        {
            ConcreteEntity01 e1 = work.New<ConcreteEntity01>();
            e1.Name = "Allowed-01";
            e1.Save();

            ConcreteEntity01 e2 = work.New<ConcreteEntity01>();
            e2.Name = "Denied-01";
            e2.Save();

            work.CommitChanges();
        }

        var provider = new TestCriteriaProvider(q => q.Like(nameof(ConcreteEntity01.Name), "Allowed*"));
        db.AddCriteriaProvider(provider);
        try
        {
            var query = db.GetQuery<ConcreteEntity01>();
            int before = provider.CheckCallCount;
            query.CheckFilters();
            Assert.Equal(before + 1, provider.CheckCallCount);

            // İkinci çağrı idempotent; provider tekrar çalışmaz
            query.CheckFilters();
            Assert.Equal(before + 1, provider.CheckCallCount);

            long count = query.Count().Result;
            Assert.Equal(1, count);
            Assert.Equal(before + 1, provider.CheckCallCount);
        }
        finally
        {
            db.CriteriaProviders.Remove(provider);
        }
    }

    [Fact]
    public virtual void Load_12_CriteriaProvider_AppliesOnAggregatePaths()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.ReAddReCreate<ConcreteEntity01>();
        db.Structure.ApplyEntityStructure<ConcreteEntity01>();

        using (var work = db.BeginWork())
        {
            ConcreteEntity01 e1 = work.New<ConcreteEntity01>();
            e1.Name = "Allowed-01";
            e1.Number = 10;
            e1.Save();

            ConcreteEntity01 e2 = work.New<ConcreteEntity01>();
            e2.Name = "Allowed-02";
            e2.Number = 20;
            e2.Save();

            ConcreteEntity01 e3 = work.New<ConcreteEntity01>();
            e3.Name = "Denied-01";
            e3.Number = 100;
            e3.Save();

            work.CommitChanges();
        }

        var provider = new TestCriteriaProvider(q => q.Like(nameof(ConcreteEntity01.Name), "Allowed*"));
        db.AddCriteriaProvider(provider);
        try
        {
            long count = db.GetQuery<ConcreteEntity01>().Count().Result;
            Assert.Equal(2, count);
            Assert.True(provider.CheckCallCount >= 1);

            bool existsAllowed = db.GetQuery<ConcreteEntity01>()
                .Eq(nameof(ConcreteEntity01.Name), "Allowed-01")
                .Exist().Result;
            Assert.True(existsAllowed);

            bool existsDenied = db.GetQuery<ConcreteEntity01>()
                .Eq(nameof(ConcreteEntity01.Name), "Denied-01")
                .Exist().Result;
            Assert.False(existsDenied);

            long sum = db.GetQuery<ConcreteEntity01>()
                .Sum<long>(nameof(ConcreteEntity01.Number)).Result;
            Assert.Equal(30, sum);

            long min = db.GetQuery<ConcreteEntity01>()
                .Min<long>(nameof(ConcreteEntity01.Number)).Result;
            Assert.Equal(10, min);

            long max = db.GetQuery<ConcreteEntity01>()
                .Max<long>(nameof(ConcreteEntity01.Number)).Result;
            Assert.Equal(20, max);
        }
        finally
        {
            db.CriteriaProviders.Remove(provider);
        }
    }
}






