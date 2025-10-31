using MimeTypes;
using Rapidex.Data.Entities;
using Rapidex.UnitTest.Data.TestContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data;
public class OtherTests : DbDependedTestsBase<DbSqlServerProvider>
{
    public OtherTests(SingletonFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
    {
    }

    [Fact]
    public void T01_IndependendEntitiesInDifferentScopes()
    {
        var db = Database.Dbs.Db();
        db.ReAddReCreate<ConcreteEntity03>();

        using var work1 = db.BeginWork();

        ConcreteEntity03 ent1 = work1.New<ConcreteEntity03>();
        ent1.Name = "test 1";
        ent1.Save();

        using (var work1_1 = db.BeginWork())
        {
            ConcreteEntity03 ent2 = work1_1.New<ConcreteEntity03>();
            ent2.Name = "test 2";
            ent2.Save();
        }

        long count = work1.GetQuery<ConcreteEntity03>().Count();
        Assert.Equal(1, count);

        var all = db.Load<ConcreteEntity03>();
        Assert.Single(all);
        Assert.Equal("test 2", all.First().Name);

        work1.CommitChanges();

        count = db.GetQuery<ConcreteEntity03>().Count();
        Assert.Equal(2, count);

        all = db.Load<ConcreteEntity03>();
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public void T02_DependentNewEntitiesInDifferentScopesTurnIntoHell()
    {
        Assert.Throws<EntityNotFoundException>(() =>
        {
            var db = Database.Dbs.Db();
            db.ReAddReCreate<ConcreteEntity01>();
            db.ReAddReCreate<ConcreteEntity02>();

            using var work1 = db.BeginWork();
            ConcreteEntity01 ent1_1 = work1.New<ConcreteEntity01>();
            ent1_1.Name = "test 1";
            ent1_1.Save();

            using (var work1_1 = db.BeginWork())
            {
                ConcreteEntity02 ent2_1 = work1_1.New<ConcreteEntity02>();
                ent2_1.MyReference = ent1_1;
                ent2_1.Save(); // <- BOOM! 
            }

            work1.CommitChanges();
        });
    }

    [Fact]
    public void T03_DependentNewEntitiesInDifferentScopes()
    {
        var db = Database.Dbs.Db();
        db.ReAddReCreate<ConcreteEntity01>();
        db.ReAddReCreate<ConcreteEntity02>();

        using var work1 = db.BeginWork();
        ConcreteEntity01 ent1_1 = work1.New<ConcreteEntity01>();
        ent1_1.Name = "test 1";
        ent1_1.Save();

        work1.CommitChanges();

        using (var work1_1 = db.BeginWork())
        {
            ConcreteEntity02 ent2_1 = work1_1.New<ConcreteEntity02>();
            ent2_1.MyReference = ent1_1;
            ent2_1.Save(); // <- BOOM! 
        }
    }

    [Fact]
    public void T03_ExpectedException_AlreadyFinalizedScope1()
    {
        var db = Database.Dbs.Db();
        db.Metadata.AddIfNotExist<ConcreteEntity01>();
        db.Structure.ApplyEntityStructure<ConcreteEntity01>();

        using var work1 = db.BeginWork();

        ConcreteEntity01 ent1 = work1.New<ConcreteEntity01>();
        ent1.Name = "test 1";
        ent1.Save();

        work1.CommitChanges();

        Assert.Throws<WorkScopeNotAvailableException>(() =>
        {
            ConcreteEntity01 ent2 = work1.New<ConcreteEntity01>(); // <- BOOM!
            ent2.Name = "test 2";
            ent2.Save();
        });
    }

    [Fact]
    public void T03_ExpectedException_AlreadyFinalizedScope2()
    {
        Assert.Throws<WorkScopeNotAvailableException>(() =>
        {
            var db = Database.Dbs.Db();
            db.Metadata.AddIfNotExist<BlobRecord>();
            db.Metadata.AddIfNotExist<ConcreteEntity01>();
            db.Structure.ApplyEntityStructure<BlobRecord>();
            db.Structure.ApplyEntityStructure<ConcreteEntity01>();

            using var work1 = db.BeginWork();

            byte[] imageContentOriginal01 = this.Fixture.GetFileContentAsBinary("TestContent\\Image01.png");
            int hash01 = HashHelper.GetStableHashCode(imageContentOriginal01);

            ConcreteEntity01 entity = work1.New<ConcreteEntity01>();
            entity.Name = "Binary 001";
            entity.Picture.Set(imageContentOriginal01, "image.png", MimeTypeMap.GetMimeType("png"));
            entity.Save();
            work1.CommitChanges();

            //WorkScope is finalized...
            long entityId01 = entity.Id;

            entity = db.Find<ConcreteEntity01>(entityId01);
            entity.Picture.SetEmpty(); // <- BOOM!
            entity.Save();

            work1.CommitChanges();
        });
    }
}
