using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Sample.App1.ConcreteEntities;
public class Order : DbConcreteEntityBase
{
    public DateTimeOffset OrderDate { get; set; }
    public Reference<Contact> Customer { get; set; }
    public decimal PriceSum { get; set; }
    public Text Description { get; set; }

    /// <summary>
    /// Add a one-to-many relation to OrderLine entity.
    /// And add field "ParentOrder" to OrderLine entity as a reference to this entity.
    /// </summary>
    public RelationOne2N<OrderLine> Lines { get; set; }
}

internal class OrderImplementer : IConcreteEntityImplementer<Order>
{
    protected static ISignalHandlingResult BeforeSave(IEntityReleatedMessageArguments args)
    {
        Order order = args.Entity.As<Order>();

        if (order.OrderDate.IsNullOrEmpty())
            order.OrderDate = DateTimeOffset.Now;

        return args.CreateHandlingResult();
    }

    public void SetupMetadata(IDbScope owner, IDbEntityMetadata metadata)
    {
        metadata
            .AddBehavior<ArchiveEntity>(true, false)
            .AddBehavior<HasTags>(true, false);

        //See: SignalHub.md
        Signal.Hub.SubscribeEntityReleated(
            DataReleatedSignalConstants.SIGNAL_BEFORESAVE,
            SignalTopic.ANY,
            SignalTopic.ANY,
            SignalTopic.ANY,
            nameof(Order),
            OrderImplementer.BeforeSave);

        //OR
        Signal.Hub.Subscribe("+/+/+/BeforeSave/Order/#", args =>
        {
            IEntityReleatedMessageArguments eArgs = (IEntityReleatedMessageArguments)args;
            return OrderImplementer.BeforeSave(eArgs);
        });

    }
}
