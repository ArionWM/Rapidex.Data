using Rapidex.Data.Sample.App1.ConcreteEntities;
using Rapidex.Data.Sample.App1.Models;

namespace Rapidex.Data.Sample.App1.Services;
public class SampleServiceA
{
    private readonly IEnumerable<IDbCriteriaParser> parsers;

    public SampleServiceA(IEnumerable<IDbCriteriaParser> parsers)
    {
        this.parsers = parsers;
    }


    internal CheckEntityContentResultModel CheckEntityContent(IDbSchemaScope db, string json, bool validate, bool runCustomLogic)
    {
        var entities = EntityDataJsonConverter.Deserialize(json, db);

        var entity = entities.FirstOrDefault();
        entity.NotNull("No entity found in the request");


        CheckEntityContentResultModel resultModel = new();

        if (validate)
            resultModel.ValidationResult = entity.Validate().Result;

        if (runCustomLogic)
            resultModel.Entity = entity.ExecLogic().Result;

        return resultModel;
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


    

    internal Order GetOrder(IDbSchemaScope db, long id)
    {
        var order = db.Find<Order>(id);
        return order;

    }

    internal IEntityLoadResult<Item> ListItems(IDbSchemaScope db, string? filter)
    {
        var query = db.GetQuery<Item>()
            .OrderBy(OrderDirection.Asc, nameof(Item.Name));

        if (filter.IsNOTNullOrEmpty())
        {
            this.parsers.FindParser(filter)
                .NotNull($"No parser found for filter: {filter}")
                .Parse(query, filter);
        }

        return query.Load();
    }

    internal Item GetItem(IDbSchemaScope db, long id)
    {
        var item = db.Find<Item>(id);
        return item;
    }
}
