using Microsoft.AspNetCore.Mvc;
using Rapidex.Data.Sample.App1.Services;

namespace Rapidex.Data.Sample.App1.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Data Management")]
public class DataController : ControllerBase
{
    private readonly IDbSchemaScope db;
    //private readonly IEntitySerializationDataCreator serializationCreator;
    private readonly EntitySerializationOptions serializationOptions;
    protected SampleServiceA ServiceA { get; }

    public DataController(SampleServiceA serviceA, IDbSchemaScope dbSchemaScope)
    {
        this.ServiceA = serviceA;
        this.db = dbSchemaScope;
        //this.serializationCreator = serializationCreator;

        this.serializationOptions = new EntitySerializationOptions
        {
            IncludeBaseFields = true,
            IncludeNestedEntities = true,
            IncludeTypeName = true
        };
    }

    /// <summary>
    /// Retrieves a list of contacts with optional filtering
    /// </summary>
    /// <param name="filter">Optional filter string to search contacts</param>
    /// <returns>List of contacts matching the filter criteria</returns>
    /// <response code="200">Returns the list of contacts</response>
    [HttpGet("contacts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ListContacts([FromQuery] string? filter)
    {
        throw new NotImplementedException();
        var contacts = this.ServiceA.ListContacts(this.db, filter);

        //var listResult = serializationCreator.ConvertToListData(contacts, this.serializationOptions, null, null);
        //return this.Ok(listResult);
    }

    /// <summary>
    /// Retrieves a specific contact by ID
    /// </summary>
    /// <param name="id">The unique identifier of the contact</param>
    /// <returns>The contact details</returns>
    /// <response code="200">Returns the contact</response>
    /// <response code="404">If the contact is not found</response>
    [HttpGet("contacts/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetContact([FromRoute] long id)
    {
        var contact = this.ServiceA.GetContact(this.db, id);
        if (contact == null)
            return this.NotFound($"Contact with ID {id} not found");

        throw new NotImplementedException();

        //var entityResult = serializationCreator.ConvertToEntityData(contact, this.serializationOptions, null, null);
        //return this.Ok(entityResult);
    }

    /// <summary>
    /// Updates an existing contact or creates a new one
    /// </summary>
    /// <param name="request">Contact update request data</param>
    /// <returns>The updated or created contact</returns>
    /// <response code="200">Returns the updated contact</response>
    /// <response code="400">If the request data is invalid</response>
    [Obsolete("")]
    [HttpPost("contacts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult UpdateContact()
    {
        if (!this.ModelState.IsValid)
            return this.BadRequest(this.ModelState);

        throw new NotImplementedException();

        //var contact = this.ServiceA.UpdateContact(this.db, request);

        //return this.Ok(contact);
    }

    /// <summary>
    /// Retrieves a list of orders with optional filtering
    /// </summary>
    /// <param name="filter">Optional filter string to search orders</param>
    /// <returns>List of orders matching the filter criteria</returns>
    /// <response code="200">Returns the list of orders</response>
    [Obsolete("")]
    [HttpGet("orders")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ListOrders([FromQuery] string? filter)
    {
        throw new NotImplementedException();
        //var orders = this.ServiceA.ListOrders(this.db, filter);
        //var listResult = serializationCreator.ConvertToListData(orders, this.serializationOptions, null, null);
        //return this.Ok(listResult);
    }

    /// <summary>
    /// Retrieves a specific order by ID
    /// </summary>
    /// <param name="id">The unique identifier of the order</param>
    /// <returns>The order details</returns>
    /// <response code="200">Returns the order</response>
    /// <response code="404">If the order is not found</response>

    [Obsolete("")]
    [HttpGet("orders/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetOrder([FromRoute] long id)
    {
        var order = this.ServiceA.GetOrder(this.db, id);
        if (order == null)
            return this.NotFound($"Order with ID {id} not found");

        throw new NotImplementedException();
        //var entityResult = serializationCreator.ConvertToEntityData(order, this.serializationOptions, null, null);
        //return this.Ok(entityResult);
    }

    ///// <summary>
    ///// Creates a new order for a contact
    ///// </summary>
    ///// <param name="request">Order creation request data</param>
    ///// <returns>The created order</returns>
    ///// <response code="201">Returns the created order</response>
    ///// <response code="400">If the request data is invalid</response>
    //[HttpPost("orders")]
    //[ProducesResponseType(StatusCodes.Status201Created)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
    //{
    //    if (!this.ModelState.IsValid)
    //        return this.BadRequest(this.ModelState);

    //    var order = this.ServiceA.CreateOrder(this.db, request.ContactId, request.ItemCodes);
    //    return this.CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    //}
}
