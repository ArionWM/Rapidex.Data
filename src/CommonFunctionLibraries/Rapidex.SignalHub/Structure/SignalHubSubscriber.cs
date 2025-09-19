using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.SignalHub;
internal class SignalHubSubscriber
{
    public int Id { get; set; }
    public SignalTopic Topic { get; set; }
    internal List<string> TopicSectionsForLocate { get; set; }
    internal int TopicSectionLevelForLocate { get; set; } = 0;
    public Func<ISignalArguments, ISignalHandlingResult> Handler { get; set; }

    public SignalHubSubscriber(int handlerId, SignalTopic topic, Func<ISignalArguments, ISignalHandlingResult> handler)
    {
        topic.Check();

        this.Id = handlerId;
        this.Topic = topic;
        this.TopicSectionsForLocate = new List<string>(topic.Sections);
        this.Handler = handler;
    }
}
