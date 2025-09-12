using Microsoft.AspNetCore.Mvc;
using Rapidex.Data.Sample.App1.Services;

namespace Rapidex.Data.Sample.App1.Controllers;

public class DataController
{
    protected SampleServiceA ServiceA { get; }

    public DataController(SampleServiceA serviceA)
    {
        this.ServiceA = serviceA;
    }

    
    public IActionResult ListOrders(string filter)
    {
        var orders = this.ServiceA.ListOrders(filter);
        return new JsonResult(orders);
    }

}
