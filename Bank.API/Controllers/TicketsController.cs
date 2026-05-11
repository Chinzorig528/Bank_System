using BankApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly QueueService _queueService;

    public TicketsController(QueueService queueService)
    {
        _queueService = queueService;
    }

    [HttpPost("create")]
    public IActionResult CreateTicket([FromQuery] string serviceType)
    {
        var ticket = _queueService.CreateTicket(serviceType);

        return Ok(ticket);
    }
}