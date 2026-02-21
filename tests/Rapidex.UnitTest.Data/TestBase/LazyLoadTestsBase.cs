using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MimeTypes;
using Rapidex.Data;
using Rapidex.UnitTest.Data.Fixtures;
using Rapidex.UnitTest.Data.TestContent;

namespace Rapidex.UnitTest.Data.TestBase;

public abstract class LazyLoadTestsBase<T> : DbDependedTestsBase<T> where T : IDbProvider
{
    protected LazyLoadTestsBase(EachTestClassIsolatedFixtureFactory<DbWithProviderFixture<T>> factory) : base(factory)
    {
    }

    [Fact]
    public virtual void LazySave_01()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<ConcreteEntity01>();
        db.Structure.DropEntity<ConcreteEntity01>();
        db.Structure.ApplyEntityStructure<ConcreteEntity01>();


        //using var work1 = db.BeginWork();

        byte[] imageContentOriginal01 = this.Fixture.GetFileContentAsBinary("TestContent\\Image01.png");
        int hash01 = HashHelper.GetStableHashCode(imageContentOriginal01);

        //Let's create entity without scope and work
        ConcreteEntity01 refEnt01 = new ConcreteEntity01();
        refEnt01.Name = "Ent LL 01";
        refEnt01.Phone = "5336722201";
        refEnt01.Address = "Address 01";
        refEnt01.Picture = new Image();
        refEnt01.Picture.Set(imageContentOriginal01, "image.png", MimeTypeMap.GetMimeType("png"));

        using var work1 = db.BeginWork();
        work1.Save(refEnt01);
        //refEnt01.Save();
        work1.CommitChanges();
    }

    [Fact]
    public virtual void LazyThings_02()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<ConcreteEntity01>();
        db.Metadata.AddIfNotExist<ConcreteEntity02>();

        db.Structure.ApplyEntityStructure<ConcreteEntity01>();
        db.Structure.ApplyEntityStructure<ConcreteEntity02>();

        ConcreteEntity01 myUnattachedEntity0 = new ConcreteEntity01();
        Assert.False(myUnattachedEntity0.IsAttached());

        ConcreteEntity02 myUnattachedEntity1 = new ConcreteEntity02();
        Assert.False(myUnattachedEntity1.IsAttached());
        myUnattachedEntity1.MyReference = myUnattachedEntity0;

        ConcreteEntity04 myOne2NParentEntity = new();
        ConcreteEntity03 myOne2NChildEntity = new();
        myOne2NParentEntity.Details01.Add(myOne2NChildEntity);

        ConcreteEntityForN2NTest01 myN2NParentEntity = new();
        ConcreteEntityForN2NTest02 myN2NChildEntity = new();
        myN2NParentEntity.Relation01.Add(myN2NChildEntity);


        using var work1 = db.BeginWork();
        work1.Save(myUnattachedEntity0);
        work1.Save(myUnattachedEntity1);
        work1.Save(myOne2NParentEntity);
        work1.Save(myOne2NChildEntity);
        work1.Save(myN2NParentEntity);
        work1.Save(myN2NChildEntity);
        work1.CommitChanges();

        //Unattached one2n
        //Unattached n2n


    }
}
