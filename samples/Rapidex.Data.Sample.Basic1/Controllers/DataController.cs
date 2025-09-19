using Microsoft.AspNetCore.Mvc;
using Rapidex.Data.Sample.App1.Services;
using System.ComponentModel.DataAnnotations;

namespace Rapidex.Data.Sample.App1.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Data Management")]
public class DataController : ControllerBase
{
    private readonly IDbSchemaScope db;

    protected SampleServiceA ServiceA { get; }

    public DataController(SampleServiceA serviceA, IDbSchemaScope dbSchemaScope)
    {
        this.ServiceA = serviceA;
        this.db = dbSchemaScope;
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
        var contacts = this.ServiceA.ListContacts(this.db, filter);
        return Ok(contacts);
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
            return NotFound($"Contact with ID {id} not found");
        
        return Ok(contact);
    }

    /// <summary>
    /// Updates an existing contact or creates a new one
    /// </summary>
    /// <param name="request">Contact update request data</param>
    /// <returns>The updated or created contact</returns>
    /// <response code="200">Returns the updated contact</response>
    /// <response code="400">If the request data is invalid</response>
    [HttpPost("contacts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult UpdateContact([FromBody] UpdateContactRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var contact = this.ServiceA.UpdateContact(
            this.db, 
            request.Id, 
            request.FirstName, 
            request.Surname, 
            request.Email, 
            request.PhoneNumber, 
            request.BirthDate);
        
        return Ok(contact);
    }

    /// <summary>
    /// Retrieves a list of orders with optional filtering
    /// </summary>
    /// <param name="filter">Optional filter string to search orders</param>
    /// <returns>List of orders matching the filter criteria</returns>
    /// <response code="200">Returns the list of orders</response>
    [HttpGet("orders")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ListOrders([FromQuery] string? filter)
    {
        var orders = this.ServiceA.ListOrders(this.db, filter);
        return Ok(orders);
    }

    /// <summary>
    /// Retrieves a specific order by ID
    /// </summary>
    /// <param name="id">The unique identifier of the order</param>
    /// <returns>The order details</returns>
    /// <response code="200">Returns the order</response>
    /// <response code="404">If the order is not found</response>
    [HttpGet("orders/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetOrder([FromRoute] long id)
    {
        var order = this.ServiceA.GetOrder(this.db, id);
        if (order == null)
            return NotFound($"Order with ID {id} not found");
            
        return Ok(order);
    }

    /// <summary>
    /// Creates a new order for a contact
    /// </summary>
    /// <param name="request">Order creation request data</param>
    /// <returns>The created order</returns>
    /// <response code="201">Returns the created order</response>
    /// <response code="400">If the request data is invalid</response>
    [HttpPost("orders")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var order = this.ServiceA.CreateOrder(this.db, request.ContactId, request.ItemCodes);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }
}

/// <summary>
/// Request model for updating a contact
/// </summary>
public class UpdateContactRequest
{
    /// <summary>
    /// Contact ID (null for new contact)
    /// </summary>
    public long? Id { get; set; }

    /// <summary>
    /// First name of the contact
    /// </summary>
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Surname of the contact
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Surname { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the contact
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Phone number of the contact
    /// </summary>
    [Phone]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Birth date of the contact
    /// </summary>
    public DateTimeOffset? BirthDate { get; set; }
}

/// <summary>
/// Request model for creating an order
/// </summary>
public class CreateOrderRequest
{
    /// <summary>
    /// ID of the contact placing the order
    /// </summary>
    [Required]
    public long ContactId { get; set; }

    /// <summary>
    /// List of item codes for the order
    /// </summary>
    [Required]
    [MinLength(1)]
    public string[] ItemCodes { get; set; } = Array.Empty<string>();
}
