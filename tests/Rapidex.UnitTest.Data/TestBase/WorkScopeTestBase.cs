using Rapidex.UnitTest.Data.TestContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestBase;
public abstract class WorkScopeTestBase<T> : DbDependedTestsBase<T> where T : IDbProvider
{
    protected WorkScopeTestBase(EachTestClassIsolatedFixtureFactory<DbWithProviderFixture<T>> factory) : base(factory)
    {
    }

    [Fact]
    public virtual void Crud_08_Workscope_Basics()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();

        db.ReAddReCreate<ConcreteEntity01>();
        db.ReAddReCreate<ConcreteEntity02>();

        long count = db.GetQuery<ConcreteEntity01>().Count();
        Assert.Equal(0, count);

        using var work = db.BeginWork();

        ConcreteEntity01 ent1 = work.New<ConcreteEntity01>();
        ent1.Name = "Ent 1";
        ent1.Save();

        work.CommitChanges();
        count = db.GetQuery<ConcreteEntity01>().Count();
        Assert.Equal(1, count);

        var tran1 = db.BeginWork();

        ConcreteEntity01 ent2 = work.New<ConcreteEntity01>();
        ent2.Name = "Ent 2";
        ent2.Save();

        tran1.Rollback();

        count = db.GetQuery<ConcreteEntity01>().Count();
        Assert.Equal(1, count);

        using (var tran2 = db.BeginWork())
        {
            ConcreteEntity01 ent3 = work.New<ConcreteEntity01>();
            ent3.Name = "Ent 3";
            ent3.Save();
        }

        count = db.GetQuery<ConcreteEntity01>().Count();
        Assert.Equal(2, count);
    }

}
