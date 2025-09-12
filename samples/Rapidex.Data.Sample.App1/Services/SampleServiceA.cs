using Rapidex.Data.Sample.App1.ConcreteEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Sample.App1.Services;
public class SampleServiceA
{
    private readonly IEnumerable<IDbCriteriaParser> parsers;

    public SampleServiceA(IEnumerable<IDbCriteriaParser> parsers)
    {
        this.parsers = parsers;
    }

    public IEntityLoadResult<Order> ListOrders(IDbSchemaScope schema, string filter)
    {
        var query = schema.GetQuery<Order>()
            .OrderBy(OrderDirection.Desc, nameof(Order.OrderDate))
            .IsNotArchived();

        if (filter.IsNOTNullOrEmpty())
        {
            this.parsers.FindParser(filter)
                .NotNull($"No parser found for filter: {filter}")
                .Parse(query, filter);
        }

        return query.Load();
    }


    public Order CreateOrder(IDbSchemaScope schema, Contact contact, params Item[] items)
    {
        using var work = schema.BeginWork();

        Order order = work.New<Order>();
        order.Customer = contact;
        order.Save();

        foreach (var item in items)
        {
            OrderLine line = work.New<OrderLine>();
            line.Item = item;
            line.Quantity = 1;
            line.UnitPrice = item.Price;

            order.Lines.Add(line);
        }

        work.CommitChanges();

        return order;
    }

    public Order CreateOrder(IDbSchemaScope schema, long contactId, params string[] itemCodes)
    {
        Contact contact = schema.Find<Contact>(contactId);
        List<Item> items = new List<Item>();

        contact.NotNull($"Contact not found: {contactId}");

        foreach (var itemCode in itemCodes)
        {
            Item item = schema.GetQuery<Item>()
                .Eq(nameof(Item.Code), itemCode)
                .First();

            item.NotNull($"Item not found with: {itemCode}");
            items.Add(item);
        }

        return CreateOrder(schema, contact, items.ToArray());

    }

}
