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

    protected ISignalHandlingResult Invoke(SignalHubSubscriber subscriber, ISignalArguments args)
    {
        ISignalHandlingResult resultArgs = subscriber.Handler.Invoke(args);
        return resultArgs;
    }

    protected ISignalProcessResult PublishInternalSync(SignalTopic topic, ISignalArguments args)
    {
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

                ISignalHandlingResult hResult = this.Invoke(subs, argsForInvoke);
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

    protected virtual ISignalProcessResult PublishInternal(SignalTopic topic, ISignalArguments args)
    {
        if (topic.SignalDefinition != null)
        {
            topic.SignalDefinition = this.Definitions.Get(topic.Event);
        }

        topic.Check()
            .Sections.NotEmpty();

        args.Topic = topic;

        SignalHubSubscriber[] subscribers = this.Subscriptions.GetSubscribers(topic.Sections.ToArray());
        if (subscribers.IsNullOrEmpty())
            return new SignalProcessResult(SignalProcessStatus.Completed, args);

        args.Id = Guid.NewGuid();

        return this.PublishInternalSync(topic, args);

        //Job_PublishInternalAsync
    }

    public Task<ISignalProcessResult> PublishAsync(SignalTopic topic, ISignalArguments args)
    {
        return Task<ISignalProcessResult>.Run(() =>
        {
            return this.PublishInternal(topic, args);
        });
    }

    //public ISignalProcessResult PublishSync(SignalTopic topic, ISignalArguments args)
    //{
    //    return this.PublishInternal(topic, args);
    //}

    public IResult<int> Subscribe(SignalTopic topic, Func<ISignalArguments, ISignalHandlingResult> handler)
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
