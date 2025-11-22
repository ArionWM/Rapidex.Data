using Microsoft.Extensions.Logging;
using Rapidex.SignalHub;
using Rapidex.UnitTest.Base.Common.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rapidex.SignalHub.TopicParser;

namespace Rapidex.UnitTest.SignalHub;
public class SignalHubTests : IClassFixture<SingletonFixtureFactory<DefaultEmptyFixture>>
{
    DefaultEmptyFixture Fixture { get; }
    public ILogger Logger => this.Fixture.Logger;

    public SignalHubTests(EachTestClassIsolatedFixtureFactory<DefaultEmptyFixture> factory)
    {
        this.Fixture = factory.GetFixture(this.GetType());
        this.Logger?.LogInformation("SignalHubTests initialized.");
    }

    [Fact]
    public void T01_Basics_SubscribeAndFind()
    {
        var messageHub = new Rapidex.SignalHub.SignalHub();

        IResult<int> subs1Result = messageHub.Subscribe("/+/base/+/new/myEntity", (args) =>
           {
               return args.CreateResult();
           });
        Assert.True(subs1Result.Success);

        IResult<int> subs2Result = messageHub.Subscribe("/myTenant1/base/+/new/myEntity", (args) =>
        {
            return args.CreateResult();
        });
        Assert.True(subs2Result.Success);

        SignalHubSubscriptionTreeItem? titem1 = messageHub.Subscriptions.Items["+"]?.Items["base"]?.Items["+"]?.Items["new"]?.Items["myEntity"];
        Assert.NotNull(titem1);
        Assert.Equal(1, titem1.Subscribers.Count);
        Assert.Equal(subs1Result.Content, titem1.Subscribers[0].Id);

        SignalHubSubscriptionTreeItem? titem2 = messageHub.Subscriptions.Items["myTenant1"]?.Items["base"]?.Items["+"]?.Items["new"]?.Items["myEntity"];
        Assert.NotNull(titem2);
        Assert.Equal(1, titem2.Subscribers.Count);
        Assert.Equal(subs2Result.Content, titem2.Subscribers[0].Id);

        SignalHubSubscriber subscriber = titem1.Subscribers[0];
        Assert.Equal("+/base/+/new/myEntity", subscriber.Topic);

        TopicParseResult parseResult1 = TopicParser.Parse(null, "myTenant1/base/+/new/myEntity");
        Assert.True(parseResult1.Valid);
        Assert.Equal("myTenant1", parseResult1.Topic.DatabaseOrTenant);

        SignalHubSubscriber[] subscribers = messageHub.Subscriptions.GetSubscribers(parseResult1.Topic);
        Assert.NotNull(subscribers);
        Assert.Equal(2, subscribers.Length);
        Assert.Contains(subs1Result.Content, subscribers.Select(x => x.Id));



        TopicParseResult parseResult2 = TopicParser.Parse(null, "myTenant2/base/+/new/myEntity");
        Assert.True(parseResult2.Valid);
        Assert.Equal("myTenant2", parseResult2.Topic.DatabaseOrTenant);

        subscribers = messageHub.Subscriptions.GetSubscribers(parseResult2.Topic);
        Assert.NotNull(subscribers);
        Assert.Equal(1, subscribers.Length);
        Assert.Equal(subs1Result.Content, subscribers[0].Id); //Son eklediğimizi alamıyor olmalıyız, sadece ilk eklediğimiz geliyor olmalı
    }

    [Fact]
    public async Task T02_Basics_Invoke()
    {
        var messageHub = new Rapidex.SignalHub.SignalHub();

        List<string> subs1Invokes = new List<string>();
        List<string> subs2Invokes = new List<string>();

        IResult<int> subs1Result = messageHub.Subscribe("/+/base/+/new/myEntity", (args) =>
        {
            subs1Invokes.Add(args.Tags);
            return args.CreateResult();
        });
        Assert.True(subs1Result.Success);

        IResult<int> subs2Result = messageHub.Subscribe("/myTenant1/base/+/new/myEntity", (args) =>
        {
            subs2Invokes.Add(args.Tags);
            return args.CreateResult();
        });
        Assert.True(subs2Result.Success);

        SignalArguments args01 = new SignalArguments();
        args01.Tags = "invoke01";

        await messageHub.PublishAsync("/myTenant2/base/+/new/myEntity", args01);
        Assert.Equal(1, subs1Invokes.Count);
        Assert.Equal(0, subs2Invokes.Count);
        Assert.Equal("invoke01", subs1Invokes[0]);
        subs1Invokes.Clear();

        SignalArguments args02 = new SignalArguments();
        args02.Tags = "invoke02";
        await messageHub.PublishAsync("/myTenant1/base/+/new/myEntity", args02);
        Assert.Equal(1, subs1Invokes.Count);
        Assert.Equal(1, subs2Invokes.Count);
        Assert.Equal("invoke02", subs1Invokes[0]);
        Assert.Equal("invoke02", subs2Invokes[0]);

    }
}
