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
    public void TTT1()
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
    public void TTT2()
    {
        var db = Database.Dbs.Db();
        db.ReAddReCreate<ConcreteEntity02>();
        db.ReAddReCreate<ConcreteEntity01>();

        using var work = db.BeginWork();
        ConcreteEntity01 ent1_1 = work.New<ConcreteEntity01>();
        ent1_1.Name = "test 1";
        ent1_1.Save();

        using (var scope = db.BeginWork())
        {
            ConcreteEntity02 ent2_1 = scope.New<ConcreteEntity02>();
            ent2_1.MyReference = ent1_1;
            ent2_1.Save();
        }

        work.CommitChanges();
    }
}
