using Microsoft.AspNetCore.Mvc;
using Rapidex.Data.Sample.App1.Services;
using System.Xml.Linq;

namespace Rapidex.Data.Sample.App1.Controllers;

public class DataController
{
    private readonly IDbSchemaScope db;

    protected SampleServiceA ServiceA { get; }

    public DataController(SampleServiceA serviceA, IDbSchemaScope dbSchemaScope)
    {
        this.ServiceA = serviceA;
        this.db = dbSchemaScope;
    }

    public IActionResult ListContacts(string filter)
    {
        var contacts = this.ServiceA.ListContacts(this.db, filter);
        return new JsonResult(contacts);
    }

    public IActionResult GetContact(long id)
    {
        var contact = this.ServiceA.GetContact(this.db, id);
        return new JsonResult(contact);
    }


    [HttpPost]
    public IActionResult UpdateContact(long? id, string firstName, string surname, string email, string phoneNumber, DateTimeOffset? birthDate)
    {
        var contact = this.ServiceA.UpdateContact(this.db, id, firstName, surname, email, phoneNumber, birthDate);
        return new JsonResult(contact);
    }



    public IActionResult ListOrders(string filter)
    {
        var orders = this.ServiceA.ListOrders(this.db, filter);
        return new JsonResult(orders);
    }

    public IActionResult GetOrder(long id)
    {
        var order = this.ServiceA.GetOrder(this.db, id);
        return new JsonResult(order);
    }

    public IActionResult CreateOrder(long contactId, params string[] itemCodes)
    {
        var order = this.ServiceA.CreateOrder(this.db, contactId, itemCodes);
        return new JsonResult(order);
    }


}
