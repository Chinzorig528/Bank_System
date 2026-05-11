using BankApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueueController : ControllerBase
{
    private readonly QueueService _queueService;

    public QueueController(QueueService queueService)
    {
        _queueService = queueService;
    }

    [HttpPost("next")]
    public IActionResult Next()
    {
        var ticket = _queueService.GetNext();

        if (ticket == null)
            return NotFound("Queue empty");

        return Ok(ticket);
    }

    [HttpGet("current")]
    public IActionResult Current()
    {
        return Ok(_queueService.GetCurrent());
    }
}