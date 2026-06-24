using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Rapidex.SignalHub;

internal class SignalHub : ISignalHub //TODO: Add Stop / Dispose + channel.Complete
{

    internal class AsyncSignalItem : IComparable<AsyncSignalItem>
    {

        public SignalTopic Topic { get; }
        public ISignalArguments Args { get; }

        public AsyncSignalItem(SignalTopic topic, ISignalArguments args)
        {
            this.Topic = topic;
            this.Args = args;
        }

        public int CompareTo(AsyncSignalItem? other)
        {
            if (other is null) return -1;

            return this.Args.Priority.CompareTo(other.Args.Priority);
        }
    }

    int lastHandlerId = 1000000;
    long lastSignalId = 0;
    const long MAX_SIGNAL_ID = 0x7fffffffffffffffL - 10000;
    SemaphoreSlim lastSignalIdLock = new SemaphoreSlim(1, 1);
    ITimeProvider timeProvider;
    private readonly Channel<AsyncSignalItem> channel;
    private readonly ILogger<SignalHub> logger;


#if DEBUG
    public int DebugId { get; private set; }
#endif

    public ISignalDefinitionCollection Definitions { get; } = new SignalDefinitionCollection();



    internal SignalHubSubscriptionTree Subscriptions { get; } = new SignalHubSubscriptionTree();


    public SignalHub(IServiceProvider serviceProvider)
    {
#if DEBUG
        this.DebugId = RandomHelper.Random(99999999);
#endif
        this.timeProvider = serviceProvider.GetRequiredService<ITimeProvider>();
        this.channel = Channel.CreateUnboundedPrioritized<AsyncSignalItem>(new UnboundedPrioritizedChannelOptions<AsyncSignalItem>()
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });
        this.logger = serviceProvider.GetRequiredService<ILogger<SignalHub>>();
    }

    protected int GetHandlerId()
    {
        Interlocked.Increment(ref this.lastHandlerId);
        return this.lastHandlerId;
    }

    protected async Task<long> GetSignalId()
    {
        await lastSignalIdLock.WaitAsync();
        try
        {
            Interlocked.Increment(ref this.lastSignalId);
            if (this.lastSignalId > MAX_SIGNAL_ID)
                this.lastSignalId = 0;

            return this.lastSignalId;
        }
        finally
        {
            lastSignalIdLock.Release();
        }

    }

    protected async Task<ISignalHandlingResult> Invoke(SignalHubSubscription subscription, ISignalArguments args)
    {
        ISignalHandlingResult resultArgs = await subscription.Handler.Invoke(args);
        return resultArgs;
    }

    protected virtual bool IsSynchronousSignal(SignalTopic topic, ISignalArguments args)
    {
        if (args.IsSynchronous.HasValue)
            return args.IsSynchronous.Value;

        if (topic.SignalDefinition != null)
            return topic.SignalDefinition.IsSynchronous;
        return false;
    }

    protected virtual async Task<ISignalProcessResult> PublishInternalSynchronous(SignalTopic topic, ISignalArguments args)
    {

        SignalHubSubscription[] subscribers = this.Subscriptions.GetSubscribers(topic.Sections.ToArray());
        if (subscribers.IsNullOrEmpty())
            return new SignalProcessResult(SignalProcessStatus.Completed, args);

        ISignalArguments _input = args.Clone<ISignalArguments>();

        List<ISignalHandlingResult> returns = new List<ISignalHandlingResult>();

        foreach (SignalHubSubscription subs in subscribers)
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
                }

                returns.Add(hResult);
            }
            catch (Exception ex)
            {
                ex.Log();
            }



        return new SignalProcessResult(SignalProcessStatus.Completed, args);
    }

    protected virtual ISignalProcessResult PublishInternalAsynchronous(SignalTopic topic, ISignalArguments args)
    {
        _ = this.channel.Writer.WriteAsync(new AsyncSignalItem(topic, args));
        return new SignalProcessResult(SignalProcessStatus.Processing, args);
    }

    protected virtual void ProcessPublishInternalAsynchronous(AsyncSignalItem item)
    {
        SignalTopic topic = item.Topic;
        ISignalArguments args = item.Args;

        try
        {
            SignalHubSubscription[] subscribers = this.Subscriptions.GetSubscribers(topic.Sections.ToArray());
            if (subscribers.IsNullOrEmpty())
                return;

            ISignalArguments _input = args.Clone<ISignalArguments>();
            foreach (SignalHubSubscription subs in subscribers)
                try
                {
                    ISignalArguments argsForInvoke = _input.CloneFor(subs.Id);
#pragma warning disable CS4014
                    this.Invoke(subs, argsForInvoke);
#pragma warning restore CS4014
                }
                catch (Exception ex)
                {
                    ex.Log();
                }
        }
        catch (Exception ex)
        {
            ex.Log();
        }
    }

    protected virtual async Task<ISignalProcessResult> PublishInternal(SignalTopic topic, ISignalArguments args)
    {
        if (topic.SignalDefinition == null)
        {
            topic.SignalDefinition = this.Definitions.Get(topic.Event);
        }

        //TODO: Signal is sync or async?

        topic.Check()
            .Sections.NotEmpty();

        args.Id = await this.GetSignalId();
        args.Topic = topic;
        args.SignalName ??= topic.Event;
        args.Time = this.timeProvider.UtcNow;

        args.NotNull();
        args.Topic.NotNull();

        if (this.IsSynchronousSignal(topic, args))
        {
            return await this.PublishInternalSynchronous(topic, args);
        }
        else
        {
            return this.PublishInternalAsynchronous(topic, args);
        }
    }

    public async Task<ISignalProcessResult> PublishAsync(SignalTopic topic, ISignalArguments args)
    {
        return await this.PublishInternal(topic, args);
    }

    [Obsolete("Use Subscribe with Func<ISignalArguments, Task<ISignalHandlingResult>> handler")]
    public IResult<int> Subscribe(SignalTopic topic, Func<ISignalArguments, ISignalHandlingResult> handler)
    {
        topic.Check()
            .Sections.NotEmpty();

        int handlerId = this.GetHandlerId();
        this.Subscriptions.Add(handlerId, topic,
            (args) => Task.FromResult(handler.Invoke(args)));
        return Result<int>.Ok(handlerId);
    }

    public IResult<int> Subscribe(SignalTopic topic, Func<ISignalArguments, Task<ISignalHandlingResult>> handler)
    {
        topic.Check()
            .Sections.NotEmpty();
        int handlerId = this.GetHandlerId();
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

    public async Task Start(IServiceProvider serviceProvider)
    {
        _ = Task.Run(async () =>
        {
            await foreach (var item in this.channel.Reader.ReadAllAsync(CancellationToken.None))
            {
                try
                {
                    this.ProcessPublishInternalAsynchronous(item);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"{item.Topic}, {item.Args.Id}");
                }
            }
        });
    }
}
