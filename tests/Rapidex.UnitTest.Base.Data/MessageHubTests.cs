using Rapidex.UnitTest.Data.TestContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.MessageHub;
public class MessageHubTests : DbDependedTestsBase<DbSqlServerProvider>
{
    public MessageHubTests(SingletonFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
    {
    }

    [Fact]
    public async Task T01_OnNew()
    {
        //ConcreteEntity01
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<ConcreteEntity01>();

        

        List<string> subs1Invokes = new List<string>();

        IResult<int> subs1Result = await Rapidex.Common.SignalHub.Subscribe("abc", "+/+/+/New/ConcreteEntity01", args =>
        {
            IEntityReleatedMessageArguments eargs = (IEntityReleatedMessageArguments)args;
            ConcreteEntity01 cent = (ConcreteEntity01)eargs.Entity;
            cent.Name = RandomHelper.RandomText(10);
            subs1Invokes.Add(cent.Name);
            return args;
        });

        Assert.True(subs1Result.Success);

        ConcreteEntity01 ent01 = db.New<ConcreteEntity01>();

        Assert.NotEmpty(subs1Invokes);

        string name01 = subs1Invokes.First();
        Assert.Equal(name01, ent01.Name);
    }

}
