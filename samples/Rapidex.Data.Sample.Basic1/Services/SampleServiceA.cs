using Rapidex.Data.Sample.App1.ConcreteEntities;
using Rapidex.Data.Transform;
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


    internal IEntityLoadResult<Contact> ListContacts(IDbSchemaScope db, string? filter)
    {
        var query = db.GetQuery<Contact>()
            .OrderBy(OrderDirection.Asc, nameof(Contact.FullName))
            .IsNotArchived();

        if (filter.IsNOTNullOrEmpty())
        {
            this.parsers.FindParser(filter)
                .NotNull($"No parser found for filter: {filter}")
                .Parse(query, filter);
        }

        return query.Load();

    }

    internal Contact GetContact(IDbSchemaScope db, long id)
    {
        var contact = db.Find<Contact>(id);
        return contact;
    }

    [Obsolete]
    internal Contact UpdateContact(IDbSchemaScope db, EntityDataDto entityValues)
    {
        using var work = db.BeginWork();

        long? id = entityValues.Values.Get(nameof(Contact.Id), true).As<long?>();

        Contact contact;
        if (id.HasValue)
        {
            contact = work.Find<Contact>(id.Value);
            contact.NotNull($"Contact not found: {id}");
        }
        else
        {
            contact = work.New<Contact>();
        }

        contact.FirstName = entityValues.Values.Get(nameof(Contact.FirstName)).As<string>();
        contact.LastName = entityValues.Values.Get(nameof(Contact.LastName)).As<string>();
        contact.Email = entityValues.Values.Get(nameof(Contact.Email)).As<string>();
        contact.PhoneNumber = entityValues.Values.Get(nameof(Contact.PhoneNumber)).As<string>();

        DateTimeOffset? birthDate = entityValues.Values.Get(nameof(Contact.BirthDate)).As<DateTimeOffset?>();
        if (birthDate.HasValue)
            contact.BirthDate = birthDate;
        contact.Save();
        work.CommitChanges();
        return contact;
    }





    public IEntityLoadResult<Order> ListOrders(IDbSchemaScope schema, string? filter)
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

    internal Order GetOrder(IDbSchemaScope db, long id)
    {
        var order = db.Find<Order>(id);
        return order;

    }
}
