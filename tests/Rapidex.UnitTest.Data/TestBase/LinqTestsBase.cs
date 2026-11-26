using Rapidex.Data;
using Rapidex.Data.Entities;
using Rapidex.Data.Linq;
using Rapidex.UnitTest.Data.TestContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestBase;

public abstract class LinqTestsBase<T> : DbDependedTestsBase<T> where T : IDbProvider
{
    protected LinqTestsBase(EachTestClassIsolatedFixtureFactory<DbWithProviderFixture<T>> factory) : base(factory)
    {
    }

    [Fact]
    public virtual async Task Linq_01_Where_Order_Skip_Take_Scalars()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<ConcreteEntity01>();
        db.Structure.DropEntity<ConcreteEntity01>();
        db.Structure.ApplyEntityStructure<ConcreteEntity01>();

        using (var work = db.BeginWork())
        {
            for (int i = 1; i <= 5; i++)
            {
                var e = work.New<ConcreteEntity01>();
                e.Name = $"Linq Ent {i:00}";
                e.Number = i * 10;
                e.Address = i % 2 == 0 ? "Even" : "Odd";
                e.Save();
            }
            work.CommitChanges();
        }

        var list = await db.GetQuery<ConcreteEntity01>()
            .Where(e => e.Number >= 20 && e.Address == "Even")
            .OrderBy(e => e.Name)
            .ToListAsync();

        Assert.Equal(2, list.Count);
        Assert.Equal("Linq Ent 02", list[0].Name);
        Assert.Equal("Linq Ent 04", list[1].Name);

        var page = await db.GetQuery<ConcreteEntity01>()
            .Where(e => e.Number >= 10)
            .OrderBy(e => e.Number)
            .Skip(1)
            .Take(2)
            .ToListAsync();

        Assert.Equal(2, page.Count);
        Assert.Equal(20, page[0].Number);
        Assert.Equal(30, page[1].Number);

        long count = await db.GetQuery<ConcreteEntity01>()
            .Where(e => e.Name.StartsWith("Linq Ent"))
            .CountAsync();
        Assert.Equal(5, count);

        bool any = await db.GetQuery<ConcreteEntity01>()
            .Where(e => e.Name.Contains("Ent 03"))
            .AnyAsync();
        Assert.True(any);
    }

    [Fact]
    public virtual async Task Linq_02_IsNull_And_In()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<ConcreteEntity01>();
        db.Metadata.AddIfNotExist<ConcreteEntity02>();
        db.Structure.DropEntity<ConcreteEntity02>();
        db.Structure.DropEntity<ConcreteEntity01>();
        db.Structure.ApplyEntityStructure<ConcreteEntity01>();
        db.Structure.ApplyEntityStructure<ConcreteEntity02>();

        using (var work = db.BeginWork())
        {
            var withAddress = work.New<ConcreteEntity01>();
            withAddress.Name = "Has Address";
            withAddress.Address = "Street 1";
            withAddress.Number = 1;
            withAddress.Save();

            var withoutAddress = work.New<ConcreteEntity01>();
            withoutAddress.Name = "No Address";
            withoutAddress.Address = null;
            withoutAddress.Number = 2;
            withoutAddress.Save();

            var ref1 = work.New<ConcreteEntity01>();
            ref1.Name = "Ref A";
            ref1.Address = "Ref Street";
            ref1.Save();

            var e1 = work.New<ConcreteEntity02>();
            e1.MyReference = ref1;
            e1.Save();

            var e2 = work.New<ConcreteEntity02>();
            e2.MyReference = null;
            e2.Save();

            work.CommitChanges();
        }

        // IsNull/IsNotNull target SQL NULL (string). Empty Reference stores DEFAULT_EMPTY_ID, not NULL.
        long nullAddressCount = await db.GetQuery<ConcreteEntity01>().IsNull(nameof(ConcreteEntity01.Address)).CountAsync();
        Assert.Equal(1, nullAddressCount);

        long notNullAddressCount = await db.GetQuery<ConcreteEntity01>().IsNotNull(nameof(ConcreteEntity01.Address)).CountAsync();
        Assert.True(notNullAddressCount >= 1);

        long totalRefEntities = await db.GetQuery<ConcreteEntity02>().CountAsync();
        Assert.Equal(2, totalRefEntities);

        var numbers = new List<int> { 10, 30 };
        db.Structure.DropEntity<ConcreteEntity01>();
        db.Structure.ApplyEntityStructure<ConcreteEntity01>();
        using (var work = db.BeginWork())
        {
            foreach (int n in new[] { 10, 20, 30 })
            {
                var e = work.New<ConcreteEntity01>();
                e.Name = "N" + n;
                e.Number = n;
                e.Save();
            }
            work.CommitChanges();
        }

        var inList = await db.GetQuery<ConcreteEntity01>()
            .Where(e => numbers.Contains(e.Number))
            .OrderBy(e => e.Number)
            .ToListAsync();
        Assert.Equal(2, inList.Count);
        Assert.Equal(10, inList[0].Number);
        Assert.Equal(30, inList[1].Number);
    }

    [Fact]
    public virtual async Task Linq_03_Reference_Exists_And_Enumeration()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<ConcreteEntity01>();
        db.Metadata.AddIfNotExist<ConcreteEntity02>();
        db.Structure.DropEntity<ConcreteEntity02>();
        db.Structure.DropEntity<ConcreteEntity01>();
        db.Structure.ApplyEntityStructure<ConcreteEntity01>();
        db.Structure.ApplyEntityStructure<ConcreteEntity02>();

        ConcreteEntity01 refEnt01;
        ConcreteEntity01 refEnt02;
        using (var work = db.BeginWork())
        {
            refEnt01 = work.New<ConcreteEntity01>();
            refEnt01.Name = "Ent 01";
            refEnt01.ContactType = ContactType.Employee;
            refEnt01.Save();

            refEnt02 = work.New<ConcreteEntity01>();
            refEnt02.Name = "Ent 02";
            refEnt02.ContactType = ContactType.Personal;
            refEnt02.Save();

            var tent01 = work.New<ConcreteEntity02>();
            tent01.MyReference = refEnt01;
            tent01.Save();

            var tent02 = work.New<ConcreteEntity02>();
            tent02.MyReference = refEnt02;
            tent02.Save();

            var tent03 = work.New<ConcreteEntity02>();
            tent03.MyReference = null;
            tent03.Save();

            work.CommitChanges();
        }

        var byNestedName = await db.GetQuery<ConcreteEntity02>()
            .Where(e => ((ConcreteEntity01)e.MyReference).Name == "Ent 01")
            .ToListAsync();
        Assert.Single(byNestedName);

        var byExistsHelper = await db.GetQuery<ConcreteEntity02>()
            .ExistsReference(nameof(ConcreteEntity02.MyReference), q => q.Eq(nameof(ConcreteEntity01.Name), "Ent 02"))
            .Load();
        Assert.Single(byExistsHelper);

        long refId = refEnt01.Id;
        var byRefId = await db.GetQuery<ConcreteEntity02>()
            .Where(e => e.MyReference.TargetId == refId)
            .ToListAsync();
        Assert.Single(byRefId);

        var byEnum = await db.GetQuery<ConcreteEntity01>()
            .Where(e => e.ContactType == ContactType.Employee)
            .ToListAsync();
        Assert.Single(byEnum);
        Assert.Equal("Ent 01", byEnum[0].Name);
    }

    [Fact]
    public virtual async Task Linq_04_GetContent_Rejected()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<ConcreteEntity01>();
        db.Metadata.AddIfNotExist<ConcreteEntity02>();
        db.Structure.ApplyEntityStructure<ConcreteEntity01>();
        db.Structure.ApplyEntityStructure<ConcreteEntity02>();

        await Assert.ThrowsAsync<NotSupportedException>(async () =>
        {
            await db.GetQuery<ConcreteEntity02>()
                .Where(e => e.MyReference.GetContent().Name == "X")
                .ToListAsync();
        });
    }

    [Fact]
    public virtual async Task Linq_05_One2N_Any()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<ConcreteEntity03>();
        db.Metadata.AddIfNotExist<ConcreteEntity04>();
        db.Structure.DropEntity<ConcreteEntity03>();
        db.Structure.DropEntity<ConcreteEntity04>();
        db.Structure.ApplyEntityStructure<ConcreteEntity03>();
        db.Structure.ApplyEntityStructure<ConcreteEntity04>();

        ConcreteEntity04 masterWithMatch;
        ConcreteEntity04 masterWithout;
        using (var work = db.BeginWork())
        {
            masterWithMatch = work.New<ConcreteEntity04>();
            masterWithMatch.Number = 1;
            masterWithMatch.Save();

            var d1 = work.New<ConcreteEntity03>();
            d1.Name = "Match Detail";
            masterWithMatch.Details01.Add(d1);

            var d2 = work.New<ConcreteEntity03>();
            d2.Name = "Other Detail";
            masterWithMatch.Details01.Add(d2);

            masterWithout = work.New<ConcreteEntity04>();
            masterWithout.Number = 2;
            masterWithout.Save();

            var d3 = work.New<ConcreteEntity03>();
            d3.Name = "Other Detail";
            masterWithout.Details01.Add(d3);

            work.CommitChanges();
        }

        long anyCount = await db.GetQuery<ConcreteEntity04>()
            .Where(m => m.Details01.Any())
            .CountAsync();
        Assert.Equal(2, anyCount);

        var filtered = await db.GetQuery<ConcreteEntity04>()
            .Where(m => m.Details01.Any(d => d.Name == "Match Detail"))
            .ToListAsync();
        Assert.Single(filtered);
        Assert.Equal(masterWithMatch.Id, filtered[0].Id);

        long existsHelperCount = await db.GetQuery<ConcreteEntity04>()
            .ExistsOne2N(nameof(ConcreteEntity04.Details01), q => q.Eq(nameof(ConcreteEntity03.Name), "Match Detail"))
            .CountAsync();
        Assert.Equal(1, existsHelperCount);
    }

    [Fact]
    public virtual async Task Linq_06_N2N_Any()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<GenericJunction>();
        db.Metadata.AddIfNotExist<ConcreteEntityForN2NTest02>();
        db.Metadata.AddIfNotExist<ConcreteEntityForN2NTest01>();

        db.Structure.DropEntity<ConcreteEntityForN2NTest02>();
        db.Structure.DropEntity<ConcreteEntityForN2NTest01>();
        db.Structure.DropEntity<GenericJunction>();

        db.Structure.ApplyEntityStructure<ConcreteEntityForN2NTest02>();
        db.Structure.ApplyEntityStructure<ConcreteEntityForN2NTest01>();
        db.Structure.ApplyEntityStructure<GenericJunction>();

        ConcreteEntityForN2NTest01 entity01;
        ConcreteEntityForN2NTest01 entity02;
        using (var work = db.BeginWork())
        {
            var related01 = work.New<ConcreteEntityForN2NTest02>();
            related01.Name = "Related Entity 01";
            related01.Save();

            var related02 = work.New<ConcreteEntityForN2NTest02>();
            related02.Name = "Related Entity 02";
            related02.Save();

            entity01 = work.New<ConcreteEntityForN2NTest01>();
            entity01.Name = "Entity 01";
            entity01.Save();
            entity01.Relation01.Add(related01);
            entity01.Relation01.Add(related02);
            entity01.Save();

            entity02 = work.New<ConcreteEntityForN2NTest01>();
            entity02.Name = "Entity 02";
            entity02.Save();
            entity02.Relation01.Add(related02);
            entity02.Save();

            work.CommitChanges();
        }

        long anyCount = await db.GetQuery<ConcreteEntityForN2NTest01>()
            .Where(e => e.Relation01.Any())
            .CountAsync();
        Assert.Equal(2, anyCount);

        var filtered = await db.GetQuery<ConcreteEntityForN2NTest01>()
            .Where(e => e.Relation01.Any(r => r.Name == "Related Entity 01"))
            .ToListAsync();
        Assert.Single(filtered);
        Assert.Equal(entity01.Id, filtered[0].Id);
    }
}
