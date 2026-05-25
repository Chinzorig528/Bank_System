using BankApi.Channels;
using BankServices.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueueController : ControllerBase
{
    private readonly IQueueService _service;

    private readonly QueueChannelService _channel;

    public QueueController(
        IQueueService service,
        QueueChannelService channel)
    {
        _service = service;
        _channel = channel;
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
        var request = new QueueRequest();

        await _channel.Queue.Writer
            .WriteAsync(request);

        var result =
            await request.Completion.Task;

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }
}