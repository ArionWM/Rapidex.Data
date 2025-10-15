using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rapidex.Data.Sample.App1.Models;
using Rapidex.Data.Sample.App1.Services;

namespace Rapidex.Data.Sample.App1.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Data Management")]
public class DataController : ControllerBase
{
    private readonly IDbSchemaScope db;

    protected SampleServiceA SampleService { get; }

    public DataController(SampleServiceA serviceA, IDbSchemaScope dbSchemaScope)
    {
        this.SampleService = serviceA;
        this.db = dbSchemaScope;
    }


    [HttpPost("update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update()
    {
        using var reader = new StreamReader(this.Request.Body);
        string json = await reader.ReadToEndAsync();

        using var work = db.BeginWork();
        var entities = EntityDataJsonConverter.Deserialize(json, this.db);
        entities.Save();
        var result = work.CommitChanges();
        return this.Ok(result);
    }

    /// <summary>
    /// This endpoint receive updated (planned etc) single entity content and checks the entity content;
    /// -- Validate the content
    /// -- Run custom logic (See: ContactImplementer.abc)
    /// Requires the entity content without saving changes
    /// </summary>
    [HttpPost("check-entity-content-with-update-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckEntityContentWithUpdateData(bool? validate, bool? runCustomLogic)
    {
        if (validate == null)
            validate = true;

        if (runCustomLogic == null)
            runCustomLogic = true;

        if (!validate.Value && !runCustomLogic.Value)
            return this.BadRequest("At least one of 'validate' or 'runCustomLogic' must be true.");

        using var reader = new StreamReader(this.Request.Body);
        string json = await reader.ReadToEndAsync();

        var resultModel = this.SampleService.CheckEntityContent(this.db, json, validate.Value, runCustomLogic.Value);

        string resultJson = resultModel.ToJson();
        return this.Content(resultJson, "application/json");

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
        var contacts = this.SampleService.ListContacts(this.db, filter);
        string json = EntityDataJsonConverter.Serialize(contacts);
        return this.Content(json, "application/json");
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
        var contact = this.SampleService.GetContact(this.db, id);
        if (contact == null)
            return this.NotFound($"Contact with ID {id} not found");


        string json = EntityDataJsonConverter.Serialize(contact);
        return this.Content(json, "application/json");
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
        var orders = this.SampleService.ListOrders(this.db, filter);
        string json = EntityDataJsonConverter.Serialize(orders);
        return this.Content(json, "application/json");
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
        var order = this.SampleService.GetOrder(this.db, id);
        if (order == null)
            return this.NotFound($"Order with ID {id} not found");

        string json = EntityDataJsonConverter.Serialize(order);
        return this.Content(json, "application/json");
    }


}
