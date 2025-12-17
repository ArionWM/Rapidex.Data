using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rapidex.SignalHub;

internal class SignalHub : ISignalHub
{
    public ISignalDefinitionCollection Definitions { get; } = new SignalDefinitionCollection();

    int lastId = 1000000;

    internal SignalHubSubscriptionTree Subscriptions { get; } = new SignalHubSubscriptionTree();


    protected int GetId()
    {
        Interlocked.Increment(ref this.lastId);
        return this.lastId;
    }

    protected async Task<ISignalHandlingResult> Invoke(SignalHubSubscriber subscriber, ISignalArguments args)
    {
        ISignalHandlingResult resultArgs = await subscriber.Handler.Invoke(args);
        return resultArgs;
    }

    protected virtual async Task<ISignalProcessResult> PublishInternal(SignalTopic topic, ISignalArguments args)
    {
        if (topic.SignalDefinition != null)
        {
            topic.SignalDefinition = this.Definitions.Get(topic.Event);
        }

        topic.Check()
            .Sections.NotEmpty();

        args.Id = Guid.NewGuid();
        args.Topic = topic;
        args.SignalName ??= topic.Event;
        args.Time = DateTimeOffset.UtcNow;

        args.NotNull();
        args.Topic.NotNull();

        SignalHubSubscriber[] subscribers = this.Subscriptions.GetSubscribers(topic.Sections.ToArray());
        if (subscribers.IsNullOrEmpty())
            return new SignalProcessResult(SignalProcessStatus.Completed, args);

        ISignalArguments _input = args.Clone<ISignalArguments>();

        List<ISignalHandlingResult> returns = new List<ISignalHandlingResult>();

        foreach (SignalHubSubscriber subs in subscribers)
            try
            {
                ISignalArguments argsForInvoke = _input.CloneFor(subs.Id);

                ISignalHandlingResult hResult = await this.Invoke(subs, argsForInvoke);
                if (hResult == null)
                {
                    var nhResult = new SignalHandlingResult(subs.Id);
                    hResult = nhResult;
                }
                else
                {
                    if (!hResult.Success)
                    {
                        SignalProcessResult failResult = new SignalProcessResult(SignalProcessStatus.Failed, argsForInvoke, null, null, hResult);
                        return failResult;
                    }

                    //Eğer publish olan değişiklik yaptı ise diğerine veriyoruz.
                    //_input = hResult;
                }

                returns.Add(hResult);
            }
            catch (Exception ex)
            {
                ex.Log();
            }



        return new SignalProcessResult(SignalProcessStatus.Completed, args);

    }

    public Task<ISignalProcessResult> PublishAsync(SignalTopic topic, ISignalArguments args)
    {
        return Task<ISignalProcessResult>.Run(async () =>
        {
            return await this.PublishInternal(topic, args);
        });
    }


    [Obsolete("Use Subscribe with Func<ISignalArguments, Task<ISignalHandlingResult>> handler")]
    public IResult<int> Subscribe(SignalTopic topic, Func<ISignalArguments, ISignalHandlingResult> handler)
    {
        topic.Check()
            .Sections.NotEmpty();

        int handlerId = this.GetId();
        this.Subscriptions.Add(handlerId, topic,
            (args) => Task.FromResult(handler.Invoke(args)));
        return Result<int>.Ok(handlerId);
    }

    public IResult<int> Subscribe(SignalTopic topic, Func<ISignalArguments, Task<ISignalHandlingResult>> handler)
    {
        topic.Check()
            .Sections.NotEmpty();
        int handlerId = this.GetId();
        this.Subscriptions.Add(handlerId, topic, handler);
        return Result<int>.Ok(handlerId);
    }

    public IResult Unsubscribe(int handlerId)
    {
        this.Subscriptions.Remove(handlerId);
        return Result.Ok();
    }

    public void RegisterSignalDefinition(ISignalDefinition signalDefinition)
    {
        this.Definitions.Set(signalDefinition.SignalName, signalDefinition);
    }

    public void Start(IServiceProvider serviceProvider)
    {

    }
}
