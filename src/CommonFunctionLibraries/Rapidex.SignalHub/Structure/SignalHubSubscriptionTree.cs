using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rapidex.SignalHub;

internal class SignalHubSubscriptionTreeItem
{
    private readonly ReaderWriterLockSlim locker = new();

    public string Section { get; set; }

    public List<SignalHubSubscription> Subscriptions { get; set; } = new List<SignalHubSubscription>(); //TODO: Add locker for manuplations / ConcurrentBag?
    public DictionaryA<SignalHubSubscriptionTreeItem> Items { get; } = new DictionaryA<SignalHubSubscriptionTreeItem>(); //TODO: Add locker for manuplations / ConcurrentDictionary?

    public SignalHubSubscriptionTreeItem(string section)
    {
        this.Section = section;
    }

    protected virtual void AddInternal(SignalHubSubscription subscription)
    {
        //`<tenantShortName>/<workspace>/<module>/<message>/<entityName>/<entityId>/<fieldName>`
        //Level0 / 1 / 2 / 3 / 4 / 5 / 6
        //# wildcard support after level 3

        if (subscription.TopicSectionsForLocate.Any())
        {
            string section = subscription.TopicSectionsForLocate[0].Trim();
            subscription.TopicSectionsForLocate.RemoveAt(0);

            if (subscription.TopicSectionLevelForLocate < 3 && section == "#")
            {
                //Wildcard, but not allowed before level 3
                throw new InvalidOperationException("Wildcard '#' is not allowed before level 3 in topic sections.");
            }

            subscription.TopicSectionLevelForLocate++;

            SignalHubSubscriptionTreeItem item = this.Items.GetOr(section, () =>
            {
                //Yeni bir bölüm
                var item = new SignalHubSubscriptionTreeItem(section);
                return item;
            });

            item.Add(subscription);
        }
        else
        {
            subscription.LocatedItem = this;
            this.Subscriptions.Add(subscription);
        }
    }

    public virtual void Add(SignalHubSubscription subscription)
    {
        //`<tenantShortName>/<workspace>/<module>/<message>/<entityName>/<entityId>/<fieldName>`
        //Level0 / 1 / 2 / 3 / 4 / 5 / 6
        //# wildcard support after level 3

        locker.EnterWriteLock();
        try
        {
            this.AddInternal(subscription);
        }
        finally
        {
            locker.ExitWriteLock();
        }
    }

    public SignalHubSubscription[] GetSubscribers(string[] sections)
    {
        //`<tenantShortName>/<workspace>/<module>/<message>/<entityName>/<entityId>/<fieldName>`
        //Level0 / 1 / 2 / 3 / 4 / 5 / 6

        //if (sections.IsNullOrEmpty())
        //    return this.Subscribers.ToArray();

        locker.EnterReadLock();

        try
        {
            List<string> _sections = new List<string>(sections);

            List<SignalHubSubscription> subscribers = new List<SignalHubSubscription>();

            string section = _sections[0];
            _sections.RemoveAt(0);


            //TODO: yayınlanan topiclerde sections içerisinde wildcard olabilir mi?
            if (SignalConstants.WildcardStrs.Contains(section))
            {
                var subSections = _sections.ToArray();

                if (section == "+")
                {
                    if (subSections.IsNullOrEmpty())
                    {
                        //Tek seviye wildcard, alt section'lara inilmez
                        //Kısa kalanlar (+ yerine geçer)
                        subscribers.AddRange(this.Subscriptions);
                    }
                    else
                    {
                        //Tek seviye wildcard, alt section'lara inilir
                        foreach (var item in this.Items)
                        {
                            subscribers.AddRange(item.Value.GetSubscribers(subSections));
                        }
                    }
                }

                if (section == "#")
                {
                    //Tüm seviye wildcard, alt section'lara inilmez
                    //Kısa kalanlar (# yerine geçer)
                    subscribers.AddRange(this.Subscriptions);
                }

            }
            else
            {
                //Bire bir uyuşanlar
                SignalHubSubscriptionTreeItem subItemDirect = this.Items.Get(section);
                if (subItemDirect != null)
                {
                    //Kısa kalanlar (# yerine geçer)
                    subscribers.AddRange(subItemDirect.Subscriptions);

                    //Alt section'lara abone olanlar
                    if (_sections.Any())
                        subscribers.AddRange(subItemDirect.GetSubscribers(_sections.ToArray()));
                }

                //Wildcard'ı olanlar
                SignalHubSubscriptionTreeItem subItemSectionWildcard = this.Items.Get("+");
                if (subItemSectionWildcard != null)
                {
                    if (!_sections.Any())
                        subscribers.AddRange(subItemSectionWildcard.Subscriptions);
                    else
                        subscribers.AddRange(subItemSectionWildcard.GetSubscribers(_sections.ToArray()));
                }

                SignalHubSubscriptionTreeItem subItemAllWildcard = this.Items.Get("#");
                if (subItemAllWildcard != null)
                {
                    subscribers.AddRange(subItemAllWildcard.Subscriptions);
                }
            }

            return subscribers.ToArray();
        }
        finally
        {
            locker.ExitReadLock();
        }
    }


    public SignalHubSubscription[] GetSubscribers(SignalTopic topic)
    {
        return this.GetSubscribers(topic.Sections);
    }
}

internal class SignalHubSubscriptionTree : SignalHubSubscriptionTreeItem
{
    private readonly ReaderWriterLockSlim treeLocker = new();
    protected Dictionary<int, SignalHubSubscription> subscriptionHandlerIndex = new Dictionary<int, SignalHubSubscription>();

    public SignalHubSubscriptionTree() : base(null)
    {
    }

    public override void Add(SignalHubSubscription subscription)
    {
        treeLocker.EnterWriteLock();
        try
        {
            subscription.TopicSectionsForLocate.NotEmpty("Subscriber's topic sections cannot be empty.");
            this.subscriptionHandlerIndex.Set(subscription.Id, subscription);
            base.Add(subscription);
        }
        finally { treeLocker.ExitWriteLock(); }
    }

    public virtual void Add(int handlerId, SignalTopic topic, Func<ISignalArguments, Task<ISignalHandlingResult>> handler)
    {
        var subscription = new SignalHubSubscription(handlerId, topic, handler);
        this.Add(subscription);
    }

    //public void Remove(SignalHubSubscriber subscriber)
    //{
    //    //throw new NotImplementedException();
    //    this.subscriberHandlerIndex.Remove(subscriber.Id);
    //    base.Remove(subscriber);
    //}

    public void Remove(int subscriptionId)
    {
        treeLocker.EnterWriteLock();
        try
        {
            if (this.subscriptionHandlerIndex.TryGetValue(subscriptionId, out var subscription))
            {
                var item = subscription.LocatedItem.NotNull();
                item.Subscriptions.Remove(subscription);
                this.subscriptionHandlerIndex.Remove(subscriptionId);
            }
        }
        finally
        {
            treeLocker.ExitWriteLock();

        }
    }
}
