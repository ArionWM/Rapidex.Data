using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Sample.App2.ConcreteEntitites;
internal class Order : DbConcreteEntityBase
{
    public DateTimeOffset OrderDate { get; set; }
    public Reference<Contact> Customer { get; set; }    

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
        if(order.OrderDate == default)
            order.OrderDate = DateTimeOffset.Now;

        return args.CreateResult();
    }

    public void SetupMetadata(IDbScope owner, IDbEntityMetadata metadata)
    {
        metadata
            .AddBehavior<ArchiveEntity>(true, false)
            .AddBehavior<HasTags>(true, false);

        //See: 
        Signal.Hub.SubscribeEntityReleated(DataReleatedSignalConstants.Signal_BeforeSave, OrderImplementer.BeforeSave);
        //See: SignalHub.md
        Signal.Hub.SubscribeEntityReleated(
            DataReleatedSignalConstants.Signal_BeforeSave,
            SignalTopic.ANY,
            SignalTopic.ANY,
            SignalTopic.ANY,
            nameof(Order),
            OrderImplementer.BeforeSave);
    }
}
