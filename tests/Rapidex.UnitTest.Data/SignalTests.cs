using Rapidex.UnitTest.Data.TestContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.MessageHub;
public class SignalTests : DbDependedTestsBase<DbSqlServerProvider>
{
    public SignalTests(SingletonFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
    {
    }

    [Fact]
    public void T01_OnNew()
    {
        //ConcreteEntity01
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<ConcreteEntity01>();



        List<string> subs1Invokes = new List<string>();

        IResult<int> subs1Result = Rapidex.Signal.Hub.Subscribe("+/+/+/New/ConcreteEntity01", args =>
        {
            IEntityReleatedMessageArguments eargs = (IEntityReleatedMessageArguments)args;
            ConcreteEntity01 cent = (ConcreteEntity01)eargs.Entity;
            cent.Name = RandomHelper.RandomText(10);
            subs1Invokes.Add(cent.Name);
            return args.CreateResult();
        });

        Assert.True(subs1Result.Success);

        using var work = db.BeginWork();
        ConcreteEntity01 ent01 = work.New<ConcreteEntity01>();

        Assert.NotEmpty(subs1Invokes);

        string name01 = subs1Invokes.First();
        Assert.Equal(name01, ent01.Name);
    }

}
