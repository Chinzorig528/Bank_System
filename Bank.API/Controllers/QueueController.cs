using BankServices.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueueController : ControllerBase
{
    private readonly IQueueService _service;

    public QueueController(IQueueService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create()
    {
        var queue = await _service.CreateQueueAsync();

        return Ok(queue);
    }

    [HttpPost("next")]
    public async Task<IActionResult> Next()
    {
        var next = await _service.CallNextAsync();

        if (next == null)
            return NotFound();

        return Ok(next);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }
}