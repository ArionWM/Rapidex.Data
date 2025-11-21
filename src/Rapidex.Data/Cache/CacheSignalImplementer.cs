using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Rapidex.Data.Cache;

internal class CacheSignalImplementer
{
    public ISignalHandlingResult AfterCommitCallback(IEntityReleatedMessageArguments args)
    {
        var entity = args.Entity;
        Database.Cache.AddEntity(entity);
        return args.CreateHandlingResult(null);
    }

    public void Start()
    {
        SignalTopic topic = SignalTopic.Create(SignalTopic.ANY, SignalTopic.ANY, SignalTopic.ANY, DataReleatedSignalConstants.SIGNAL_AFTERCOMMIT, SignalTopic.ANY, SignalTopic.ANY_ALL_SECTIONS);
        Signal.Hub.SubscribeEntityReleated(topic, this.AfterCommitCallback);
    }
}
