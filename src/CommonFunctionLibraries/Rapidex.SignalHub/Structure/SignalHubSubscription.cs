using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.SignalHub;

internal class SignalHubSubscription
{
    public int Id { get; set; }
    public SignalTopic Topic { get; set; }
    internal List<string> TopicSectionsForLocate { get; set; }
    internal int TopicSectionLevelForLocate { get; set; } = 0;
    public SignalHubSubscriptionTreeItem? LocatedItem { get; set; }
    public Func<ISignalArguments, Task<ISignalHandlingResult>> Handler { get; set; }

    public SignalHubSubscription(int handlerId, SignalTopic topic, Func<ISignalArguments, Task<ISignalHandlingResult>> handler)
    {
        topic.Check();

        this.Id = handlerId;
        this.Topic = topic;
        this.TopicSectionsForLocate = new List<string>(topic.Sections);
        this.Handler = handler;
    }
}
