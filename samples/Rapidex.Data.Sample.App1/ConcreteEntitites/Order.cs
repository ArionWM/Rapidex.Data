using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Sample.App1.ConcreteEntities;
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
    protected static IEntityReleatedMessageArguments BeforeSave(IEntityReleatedMessageArguments args)
    {
        Order order = args.Entity.As<Order>();
        if(order.OrderDate == default)
            order.OrderDate = DateTimeOffset.Now;

        return args;
    }

    public void SetupMetadata(IDbScope owner, IDbEntityMetadata metadata)
    {
        metadata
            .AddBehavior<ArchiveEntity>(true, false)
            .AddBehavior<HasTags>(true, false);

        //See: 
        Common.SignalHub.SubscribeOnBeforeSave("/", OrderImplementer.BeforeSave);

    }
}
