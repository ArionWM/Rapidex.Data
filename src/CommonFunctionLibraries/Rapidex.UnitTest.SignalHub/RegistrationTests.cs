using Microsoft.Extensions.Logging;
using Rapidex.SignalHub;
using Rapidex.UnitTest.Base.Common.Fixtures;
using Rapidex.UnitTests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.SignalHub;
public class RegistrationTests : IClassFixture<EachTestClassIsolatedFixtureFactory<DefaultEmptyFixture>>
{
    DefaultEmptyFixture Fixture { get; }
    public ILogger Logger => this.Fixture.Logger;

    public RegistrationTests(EachTestClassIsolatedFixtureFactory<DefaultEmptyFixture> factory)
    {
        this.Fixture = factory.GetFixture(this.GetType());
        this.Logger?.LogInformation("RegistrationTests initialized.");
    }

    //See: SignalHub.md

    [Fact]
    public void Test01()
    {
        SignalHubSubscriptionTree tree = new SignalHubSubscriptionTree();

        tree.Add(1, "tenant1/workspace1/module111/signal111/target1", null);
        tree.Add(2, "tenant1/workspace1/module111/signal111/+", null);

        tree.Add(3, "tenant1/workspace1/module111/signal222", null); //Short registration, equal to '/#'
        tree.Add(4, "tenant1/workspace1/module111/signal222/+", null);
        tree.Add(5, "tenant1/workspace1/module111/signal222/#", null);

        tree.Add(6, "tenant1/workspace1/module222/signal333/target2", null);
        tree.Add(7, "tenant1/workspace1/module222/signal111/target2", null);
        tree.Add(8, "tenant1/workspace1/module222/signal222/target1", null);

        //`<tenantShortName>/<workspace>/<module>/<message>/<entityName>/<entityId>/<fieldName>`
        //Level0 / 1 / 2 / 3 / 4 / 5 / 6

        SignalHubSubscriber[] subscribers0 = tree.GetSubscribers("tenant1/workspace1/module111/signal111/target1");
        Assert.Equal(2, subscribers0.Length);
        Assert.Contains(subscribers0, subs => subs.Id == 1);
        Assert.Contains(subscribers0, subs => subs.Id == 2);

        SignalHubSubscriber[] subscribers1 = tree.GetSubscribers("tenant1/workspace1/module111/signal222/target1");
        Assert.Equal(3, subscribers1.Length);
        Assert.Contains(subscribers1, subs => subs.Id == 3);
        Assert.Contains(subscribers1, subs => subs.Id == 4);
        Assert.Contains(subscribers1, subs => subs.Id == 5);


        SignalHubSubscriber[] subscribers2 = tree.GetSubscribers("tenant1/workspace1/module111/signal222/target1/123");
        Assert.Equal(2, subscribers2.Length);
        Assert.Contains(subscribers1, subs => subs.Id == 3);
        Assert.Contains(subscribers1, subs => subs.Id == 5);


        Assert.Throws<ValidationException>(() =>
        {
            //No signal info, can't process
            tree.GetSubscribers("tenant1/workspace1/module1");
        });

        Assert.Throws<ValidationException>(() =>
        {
            //No signal info, can't process
            tree.GetSubscribers("tenant1/workspace1/module111/+");
        });

        Assert.Throws<ValidationException>(() =>
        {
            tree.Add(99, "tenant1/workspace1/module111/+/target1", null);
        });

    }


    [Fact]
    public void Test02()
    {
        SignalHubSubscriptionTree tree = new SignalHubSubscriptionTree();

        SignalTopic topic1 = new SignalTopic
        {
            DatabaseOrTenant = SignalTopic.ANY,
            Workspace = SignalTopic.ANY,
            Module = SignalTopic.ANY,
            Event = SignalConstants.SIGNAL_EDITING,
            Entity = SignalTopic.ANY,
            EntityId = SignalTopic.ANY,
        };

        SignalTopic topic2 = new SignalTopic
        {
            DatabaseOrTenant = SignalTopic.ANY,
            Workspace = SignalTopic.ANY,
            Module = SignalTopic.ANY,
            Event = SignalConstants.SIGNAL_IMPORTING,
            Entity = SignalTopic.ANY,
            EntityId = SignalTopic.ANY,
        };

        tree.Add(1, topic1, null);
        tree.Add(2, topic2, null);

        SignalHubSubscriber[] subscribers0 = tree.GetSubscribers("tenant1/workspace1/module111/Editing/target1/id1");
        Assert.Equal(1, subscribers0.Length);
        Assert.Contains(subscribers0, subs => subs.Id == 1);

        SignalHubSubscriber[] subscribers1 = tree.GetSubscribers("tenant1/workspace1/module111/Importing/target1/id1");
        Assert.Equal(1, subscribers1.Length);
        Assert.Contains(subscribers1, subs => subs.Id == 2);

    }

}
