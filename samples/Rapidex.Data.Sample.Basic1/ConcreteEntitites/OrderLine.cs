using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Sample.App1.ConcreteEntities;
public class OrderLine : DbConcreteEntityBase
{
    /// <summary>
    /// If you want add parent reference as concrete field with name "ParentOrder" 
    /// If you don't add this field, the parent reference add automatically as virtual field.
    /// </summary>
    public Reference<Order> ParentOrder { get; set; }


    public Reference<Item> ItemRef { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}


internal class OrderLineImplementer : IConcreteEntityImplementer<OrderLine>
{
    protected static void CalculateOrderLineValues(OrderLine orderLine)
    {
        if (orderLine.Quantity == 0)
            orderLine.Quantity = 1;

        if (orderLine.UnitPrice.IsNullOrEmpty() && orderLine.ItemRef.IsNOTNullOrEmpty())
        {
            Item item = orderLine.ItemRef;
            orderLine.UnitPrice = item.Price;
        }

        orderLine.TotalPrice = orderLine.UnitPrice * orderLine.Quantity;

    }

    protected static ISignalHandlingResult ExecLogic(IEntityReleatedMessageArguments args)
    {
        OrderLine entity = (OrderLine)args.Entity.EnsureForActualEntity();

        CalculateOrderLineValues(entity);
        return args.CreateHandlingResult(entity);
    }

    protected static ISignalHandlingResult BeforeSave(IEntityReleatedMessageArguments args)
    {
        OrderLine orderLine = (OrderLine)args.Entity.EnsureForActualEntity();
        CalculateOrderLineValues(orderLine);
        return args.CreateHandlingResult();
    }

    protected static ISignalHandlingResult Validate(IEntityReleatedMessageArguments args)
    {
        OrderLine orderLine = (OrderLine)args.Entity.EnsureForActualEntity();
        IValidationResult validationResult = new ValidationResult();

        if (orderLine.UnitPrice < 0)
            validationResult.Error("UnitPrice", "Unit price cannot be negative.");

        if (orderLine.Quantity <= 0)
            validationResult.Error("Quantity", "Quantity must be greater than zero.");


        return args.CreateHandlingValidationResult(orderLine, validationResult);
    }

    public void SetupMetadata(IDbScope owner, IDbEntityMetadata metadata)
    {

        //See: SignalHub.md
        //Register to signals
        Signal.Hub.SubscribeEntityReleated(
            DataReleatedSignalConstants.SIGNAL_BEFORESAVE,
            SignalTopic.ANY,
            SignalTopic.ANY,
            SignalTopic.ANY,
            nameof(OrderLine),
            OrderLineImplementer.BeforeSave);

        Signal.Hub.SubscribeEntityReleated(
            DataReleatedSignalConstants.SIGNAL_VALIDATE,
            SignalTopic.ANY,
            SignalTopic.ANY,
            SignalTopic.ANY,
            nameof(OrderLine),
            OrderLineImplementer.Validate);

        Signal.Hub.SubscribeEntityReleated(
            DataReleatedSignalConstants.SIGNAL_EXEC_LOGIC,
            SignalTopic.ANY,
            SignalTopic.ANY,
            SignalTopic.ANY,
            nameof(OrderLine),
            OrderLineImplementer.ExecLogic);
    }
}
