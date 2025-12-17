using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.SignalHub;

internal class SignalHubSubscriptionTreeItem
{
    public string Section { get; set; }
    public List<SignalHubSubscriber> Subscribers { get; set; } = new List<SignalHubSubscriber>();
    public DictionaryA<SignalHubSubscriptionTreeItem> Items { get; } = new DictionaryA<SignalHubSubscriptionTreeItem>();

    public SignalHubSubscriptionTreeItem(string section)
    {
        Section = section;
    }
    public virtual void Add(SignalHubSubscriber subscriber)
    {
        //`<tenantShortName>/<workspace>/<module>/<message>/<entityName>/<entityId>/<fieldName>`
        //Level0 / 1 / 2 / 3 / 4 / 5 / 6
        //# wildcard support after level 3

        if (subscriber.TopicSectionsForLocate.Any())
        {
            string section = subscriber.TopicSectionsForLocate[0].Trim();
            subscriber.TopicSectionsForLocate.RemoveAt(0);

            if (subscriber.TopicSectionLevelForLocate < 4 && section == "#")
            {
                //Wildcard, but not allowed before level 4
                throw new InvalidOperationException("Wildcard '#' is not allowed before level 4 in topic sections.");
            }

            subscriber.TopicSectionLevelForLocate++;

            SignalHubSubscriptionTreeItem item = this.Items.GetOr(section, () =>
            {
                //Yeni bir bölüm
                var item = new SignalHubSubscriptionTreeItem(section);
                return item;
            });

            item.Add(subscriber);
        }
        else
        {
            this.Subscribers.Add(subscriber);
        }
    }

    public SignalHubSubscriber[] GetSubscribers(string[] sections)
    {
        //`<tenantShortName>/<workspace>/<module>/<message>/<entityName>/<entityId>/<fieldName>`
        //Level0 / 1 / 2 / 3 / 4 / 5 / 6

        //if (sections.IsNullOrEmpty())
        //    return this.Subscribers.ToArray();

        List<string> _sections = new List<string>(sections);

        List<SignalHubSubscriber> subscribers = new List<SignalHubSubscriber>();

        string section = _sections[0];
        _sections.RemoveAt(0);


        //TODO: yayınlanan topiclerde sections içerisinde wildcard olabilir mi?
        if (SignalConstants.WildcardStrs.Contains(section))
        {
            var subSections = _sections.ToArray();
            //deeper for all
            foreach (var item in this.Items)
            {
                subscribers.AddRange(item.Value.GetSubscribers(subSections));
            }
        }
        else
        {
            //Bire bir uyuşanlar
            SignalHubSubscriptionTreeItem subItemDirect = this.Items.Get(section);
            if (subItemDirect != null)
            {
                //Kısa kalanlar (# yerine geçer)
                subscribers.AddRange(subItemDirect.Subscribers);

                //Alt section'lara abone olanlar
                if (_sections.Any())
                    subscribers.AddRange(subItemDirect.GetSubscribers(_sections.ToArray()));
            }

            //Wildcard'ı olanlar
            SignalHubSubscriptionTreeItem subItemSectionWildcard = this.Items.Get("+");
            if (subItemSectionWildcard != null)
            {
                if (!_sections.Any())
                    subscribers.AddRange(subItemSectionWildcard.Subscribers);
                else
                    subscribers.AddRange(subItemSectionWildcard.GetSubscribers(_sections.ToArray()));
            }

            SignalHubSubscriptionTreeItem subItemAllWildcard = this.Items.Get("#");
            if (subItemAllWildcard != null)
            {
                subscribers.AddRange(subItemAllWildcard.Subscribers);
            }
        }

        return subscribers.ToArray();
    }


    public SignalHubSubscriber[] GetSubscribers(SignalTopic topic)
    {
        return this.GetSubscribers(topic.Sections);
    }
}

internal class SignalHubSubscriptionTree : SignalHubSubscriptionTreeItem
{
    protected Dictionary<int, SignalHubSubscriber> subscriberHandlerIndex = new Dictionary<int, SignalHubSubscriber>();

    public SignalHubSubscriptionTree() : base(null)
    {
    }

    public override void Add(SignalHubSubscriber subscriber)
    {
        subscriber.TopicSectionsForLocate.NotEmpty("Subscriber's topic sections cannot be empty.");
        this.subscriberHandlerIndex.Set(subscriber.Id, subscriber);
        base.Add(subscriber);
    }

    public virtual void Add(int handlerId, SignalTopic topic, Func<ISignalArguments, Task<ISignalHandlingResult>> handler)
    {
        var subscriber = new SignalHubSubscriber(handlerId, topic, handler);
        this.Add(subscriber);
    }

    //public void Remove(SignalHubSubscriber subscriber)
    //{
    //    //throw new NotImplementedException();
    //    this.subscriberHandlerIndex.Remove(subscriber.Id);
    //    base.Remove(subscriber);
    //}

    public void Remove(int subscriberId)
    {
        if (this.subscriberHandlerIndex.TryGetValue(subscriberId, out var subscriber))
        {
            this.subscriberHandlerIndex.Remove(subscriberId);
        }

    }
}
