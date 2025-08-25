﻿using Rapidex.Data.Entities;
using Rapidex.Data.Query;
using Rapidex.UnitTest.Data.TestContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestBase;
public abstract class QueryTestsBase<T> : DbDependedTestsBase<T> where T : IDbProvider
{
    public QueryTestsBase(SingletonFixtureFactory<DbWithProviderFixture<T>> factory) : base(factory)
    {
    }

    [Fact]
    public virtual async Task Load_01_LoadMultiple()
    {
        this.Fixture.ClearCaches();

        var db = Database.Scopes.AddMainDbIfNotExists();

        db.Metadata.AddIfNotExist<ConcreteEntity01>();
        db.Structure.DropEntity<ConcreteEntity01>();

        db.Structure.ApplyEntityStructure<ConcreteEntity01>();


        for (int i = 0; i < 10; i++)
        {
            ConcreteEntity01 entity = db.New<ConcreteEntity01>();
            entity.Name = $"Entity Name {i:000}";
            entity.CreditLimit1 = 10000 * i;
            entity.Description = $"Description {i:000}";
            entity.Save();
        }

        await db.ApplyChanges();

        this.Fixture.ClearCaches();

        db.Metadata.AddIfNotExist<ConcreteEntity01>();

        var result = await db.Load<ConcreteEntity01>();
        Assert.Equal(10, result.ItemCount);

        foreach (var item in result)
        {
            Assert.NotNull(item.Name);
            Assert.NotNull(item.Description);

            item.TestIDataTypeAssignments();
        }
    }


    [Fact]
    public virtual async Task Load_02_SimpleCriteria_Concrete()
    {
        this.Fixture.ClearCaches();

        var db = Database.Scopes.AddMainDbIfNotExists();

        db.Metadata.AddIfNotExist<ConcreteEntity01>();
        db.Structure.DropEntity<ConcreteEntity01>();

        db.Structure.ApplyEntityStructure<ConcreteEntity01>();


        for (int i = 0; i < 10; i++)
        {
            ConcreteEntity01 entity = db.New<ConcreteEntity01>();
            entity.Name = $"Load_01_SimpleCriteria_Concrete {i:000}";
            entity.CreditLimit1 = 10000 * i;

            if (i % 2 == 0)
            {
                entity.Phone = $"Phone {i:000}";
                entity.Description = $"even";
            }

            entity.Save();
        }

        await db.ApplyChanges();

        var result = db.Load<ConcreteEntity01>(q => q.Eq(nameof(ConcreteEntity01.Name), "Phone 002"));
    }


    protected async Task Load_03_GenerateEntities(IDbSchemaScope scope, IDbEntityMetadata em)
    {
        var db = Database.Scopes.AddMainDbIfNotExists();

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

        List<ConcreteEntity01> refEntities = new List<ConcreteEntity01>();

        for (int i = 0; i < 10; i++)
        {
            ConcreteEntity01 entity = db.New<ConcreteEntity01>();
            entity.Name = $"Entity Name {i:000}";
            entity.CreditLimit1 = 10000 * i;
            entity.Description = $"Description {i:000}";
            entity.Save();

            refEntities.Add(entity);
        }

        await db.ApplyChanges();




        for (int i = 0; i < 100; i++)
        {
            IEntity entity = scope.New(em);
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

        await scope.ApplyChanges();

    }

    protected virtual async Task Load_03_Criterias(IDbSchemaScope scope, IDbEntityMetadata em)
    {
        await this.Load_03_GenerateEntities(scope, em);

        //1. IsActive == true olanları getir
        long count = await scope.GetQuery(em).Eq("IsActive", true).Count();
        Assert.Equal(50, count);

        //2. IsActive == false olanları getir
        count = await scope.GetQuery(em).Eq("IsActive", false).Count();
        Assert.Equal(50, count);

        //3. Amount == 5 olanları getir
        count = await scope.GetQuery(em).Eq("Amount", 5).Count();
        Assert.Equal(10, count);

        //4. Amount > 5 olanları getir
        count = await scope.GetQuery(em).Gt("Amount", 5).Count();
        Assert.Equal(40, count);

        //5. Amount < 5 olanları getir
        count = await scope.GetQuery(em).Lt("Amount", 5).Count();
        Assert.Equal(50, count);

        //6. Amount >= 5 olanları getir
        count = await scope.GetQuery(em).GtEq("Amount", 5).Count();
        Assert.Equal(50, count);

        //7. Amount <= 5 olanları getir
        count = await scope.GetQuery(em).LtEq("Amount", 5).Count();
        Assert.Equal(60, count);

        //8. Amount between 3 and 7 olanları getir
        count = await scope.GetQuery(em).Between("Amount", 3, 7).Count();
        Assert.Equal(50, count);

        //9. Name like "Entity Name 005" olanları getir
        count = await scope.GetQuery(em).Like("Name", "Entity Name 005").Count();
        Assert.Equal(1, count);

        //10. Name like "Entity Name *" olanları getir
        count = await scope.GetQuery(em).Like("Name", "Entity Name 00*").Count();
        Assert.Equal(9, count);

        //11. Phone like "5336722205" olanları getir
        count = await scope.GetQuery(em).Like("Phone", "5336722205").Count();
        Assert.Equal(1, count);

        //12. Value > 50000 olanları getir
        count = await scope.GetQuery(em).Gt("Value", 50000).Count();
        Assert.Equal(95, count);

        //13. BirthDate > 20.01.2000 olanları getir
        count = await scope.GetQuery(em).Gt("BirthDate", new DateTimeOffset(2000, 1, 20, 0, 0, 0, TimeSpan.Zero)).Count();
        Assert.Equal(80, count);

        //14. BirthDate < 20.01.2000 olanları getir
        count = await scope.GetQuery(em).Lt("BirthDate", new DateTimeOffset(2000, 1, 20, 0, 0, 0, TimeSpan.Zero)).Count();
        Assert.Equal(19, count);

        //15. Reference ilişkisindeki kayıtın adı "Entity Name 001" olanları getir
        count = await scope.GetQuery(em).Nested("Reference01", q => q.Eq(nameof(ConcreteEntity01.Name), "Entity Name 001")).Count();
        Assert.Equal(10, count);



    }


    [Fact]
    public virtual async Task Load_03_Criterias_Concrete()
    {
        var db = Database.Scopes.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<ConcreteEntity01>();
        db.Metadata.AddIfNotExist<CriteriaTestEntity01>();

        db.Structure.ApplyEntityStructure<ConcreteEntity01>();
        db.Structure.ApplyEntityStructure<CriteriaTestEntity01>();

        await this.Load_03_Criterias(db, db.Metadata.Get<CriteriaTestEntity01>());
    }


    [Fact]
    public virtual async Task Load_04_Criterias_Nested()
    {
        var db = Database.Scopes.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<ConcreteEntity01>();
        db.Metadata.AddIfNotExist<ConcreteEntity02>();

        db.Structure.DropEntity<ConcreteEntity01>();
        db.Structure.DropEntity<ConcreteEntity02>();

        db.Structure.ApplyEntityStructure<ConcreteEntity01>();
        db.Structure.ApplyEntityStructure<ConcreteEntity02>();

        //---------------------------------------------------------------------------------

        ConcreteEntity01 refEnt01 = db.New<ConcreteEntity01>();
        refEnt01.Name = "Ent 01";
        refEnt01.Save();

        ConcreteEntity01 refEnt02 = db.New<ConcreteEntity01>();
        refEnt02.Name = "Ent 02";
        refEnt02.Save();

        ConcreteEntity02 tent01 = db.New<ConcreteEntity02>();
        tent01.Parent = refEnt01;
        tent01.Save();

        ConcreteEntity02 tent02 = db.New<ConcreteEntity02>();
        tent02.Parent = null;
        tent02.Save();

        await db.Data.ApplyChanges();

        //---------------------------------------------------------------------------------

        var loadResult01 = await db.GetQuery<ConcreteEntity02>()
              .Nested(nameof(ConcreteEntity02.Parent), q => q.Eq(nameof(ConcreteEntity01.Name), "Ent 01"))
              .Load();

        Assert.Single(loadResult01);




    }


    [Fact]
    public virtual async Task Load_05_Criterias_NestedWithOnlyBase()
    {
        var db = Database.Scopes.AddMainDbIfNotExists();

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

        ConcreteOnlyBaseEntity01 baseEnt01 = schemaBase.New<ConcreteOnlyBaseEntity01>();
        baseEnt01.Name = "Nest Test 01 CEnt 01";
        baseEnt01.Save();

        ConcreteOnlyBaseEntity01 baseEnt02 = schemaBase.New<ConcreteOnlyBaseEntity01>();
        baseEnt02.Name = "Nest Test 01 CEnt 02";
        baseEnt02.Save();

        await schemaBase.ApplyChanges();

        long baseId01 = (long)baseEnt01.GetId();
        long baseId02 = (long)baseEnt02.GetId();

        ConcreteOnlyBaseReferencedEntity02 refEntity01 = schema02.New<ConcreteOnlyBaseReferencedEntity02>();
        refEntity01.Name = "Nest Test REnt 01";
        refEntity01.Reference = baseEnt01;
        refEntity01.Save();

        ConcreteOnlyBaseReferencedEntity02 refEntity02 = schema02.New<ConcreteOnlyBaseReferencedEntity02>();
        refEntity02.Name = "Nest Test REnt 02";
        refEntity02.Reference = baseEnt02;
        refEntity02.Save();

        ConcreteOnlyBaseReferencedEntity02 refEntity03 = schema02.New<ConcreteOnlyBaseReferencedEntity02>();
        refEntity03.Name = "Nest Test REnt 03";
        refEntity03.Reference = null;
        refEntity03.Save();

        await schema02.ApplyChanges();

        long refId01 = (long)refEntity01.GetId();
        long refId02 = (long)refEntity02.GetId();
        long refId03 = (long)refEntity03.GetId();

        var loadResult01 = await schema02.GetQuery<ConcreteOnlyBaseReferencedEntity02>()
              .Nested(nameof(ConcreteOnlyBaseReferencedEntity02.Reference), q => q.Eq(nameof(ConcreteOnlyBaseEntity01.Name), "Nest Test 01 CEnt 01"))
              .Load();

        Assert.Single(loadResult01);

        ConcreteOnlyBaseReferencedEntity02 lent01 = loadResult01.First();
        Assert.Equal("Nest Test REnt 01", lent01.Name);

    }


    [Fact]
    public virtual async Task BulkUpdate_06_Criterias_NestedWithOnlyBase()
    {
        var db = Database.Scopes.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<ConcreteEntity01>();
        db.Structure.DropEntity<ConcreteEntity01>();
        db.Structure.ApplyEntityStructure<ConcreteEntity01>();


        ConcreteEntity01 refEnt01 = db.New<ConcreteEntity01>();
        refEnt01.Name = "Ent 01";
        refEnt01.Phone = "5336722201";
        refEnt01.Address = "Address 01";

        refEnt01.Save();

        ConcreteEntity01 refEnt02 = db.New<ConcreteEntity01>();
        refEnt02.Name = "Ent 02";
        refEnt02.Phone = "5336722202";
        refEnt02.Address = "Address 02";
        refEnt02.Save();

        ConcreteEntity01 refEnt03 = db.New<ConcreteEntity01>();
        refEnt03.Name = "Ent 03";
        refEnt03.Phone = "6336722203";
        refEnt03.Address = "Address 03";
        refEnt03.Save();

        await db.ApplyChanges();

        long count01 = await db.GetQuery<ConcreteEntity01>()
             .Like("Phone", "533672220*")
             .Count();

        Assert.Equal(2, count01);

        long count02 = await db.GetQuery<ConcreteEntity01>()
            .Eq("Address", "Address 03")
            .Count();

        Assert.Equal(1, count02);


        db.GetQuery<ConcreteEntity01>()
             .EnterUpdateMode()
             .Like("Phone", "533672220*")
             .Update(new ObjDictionary() { { "Address", "Updated Address" } });

        //var data = new ObjDictionary();
        //data.Add("Address", "Updated Address");
        //query.Update(data);

        await db.ApplyChanges();

        long count03 = await db.GetQuery<ConcreteEntity01>()
            .Eq("Address", "Address 03")
            .Count();

        Assert.Equal(1, count03);

        long count04 = await db.GetQuery<ConcreteEntity01>()
            .Eq("Address", "Address 01")
            .Count();

        Assert.Equal(0, count04);

        long count05 = await db.GetQuery<ConcreteEntity01>()
            .Eq("Address", "Updated Address")
            .Count();

        Assert.Equal(2, count05);
    }

    //Nester

    //Order

    //Aggregation

    [Fact]
    public virtual async Task Load_06_Criterias_Relation()
    {
        var db = Database.Scopes.Db();

        db.Metadata.AddIfNotExist<ConcreteEntityForN2NTest01>();
        db.Metadata.AddIfNotExist<ConcreteEntityForN2NTest02>();
        db.Metadata.AddIfNotExist<GenericJunction>();

        db.Structure.DropEntity<ConcreteEntityForN2NTest01>();
        db.Structure.DropEntity<ConcreteEntityForN2NTest02>();
        db.Structure.ApplyEntityStructure<ConcreteEntityForN2NTest01>();
        db.Structure.ApplyEntityStructure<ConcreteEntityForN2NTest02>();

        ConcreteEntityForN2NTest01 master01 = db.New<ConcreteEntityForN2NTest01>();
        master01.Name = "master01";
        master01.Save();

        ConcreteEntityForN2NTest02 detail01_01 = db.New<ConcreteEntityForN2NTest02>();
        detail01_01.Name = "detail01-01";
        detail01_01.Save();
        master01.Relation01.Add(detail01_01);

        //ConcreteEntityForN2NTest02 detail01_02 = database.New<ConcreteEntityForN2NTest02>();
        //detail01_02.Name = "detail01-02";
        //detail01_02.Save();
        //master01.Relation01.Add(detail01_02);


        ConcreteEntityForN2NTest01 master02 = db.New<ConcreteEntityForN2NTest01>();
        master02.Name = "master02";
        master02.Save();

        ConcreteEntityForN2NTest02 detail02_01 = db.New<ConcreteEntityForN2NTest02>();
        detail02_01.Name = "detail02-02";
        detail02_01.Save();
        master02.Relation01.Add(detail02_01);

        ConcreteEntityForN2NTest02 detail02_02 = db.New<ConcreteEntityForN2NTest02>();
        detail02_02.Name = "detail02-02";
        detail02_02.Save();
        master02.Relation01.Add(detail02_02);

        await db.ApplyChanges();

        long count01 = await db.GetQuery<ConcreteEntityForN2NTest02>()
              .Related(master01, nameof(ConcreteEntityForN2NTest01.Relation01))
              .Count();

        Assert.Equal(1, count01);

        long count02 = await db.GetQuery<ConcreteEntityForN2NTest02>()
              .Related(master02, nameof(ConcreteEntityForN2NTest01.Relation01))
              .Count();

        Assert.Equal(2, count02);


    }


}



