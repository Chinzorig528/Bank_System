using BankApi.Channels;
using BankServices.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankApi.Controllers;

/// <summary>
/// Queue дугаар үүсгэх, дараагийн үйлчлүүлэгчийг дуудах API endpoint-ууд.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class QueueController : ControllerBase
{
    private readonly IQueueService _service;

    private readonly QueueChannelService _channel;

    /// <summary>
    /// Queue controller-д service болон worker-той холбогдох channel-ийг онооно.
    /// </summary>
    /// <param name="service">Queue дугаарын бизнес логик.</param>
    /// <param name="channel">Дараагийн queue дуудах хүсэлт дамжуулах channel.</param>
    public QueueController(
        IQueueService service,
        QueueChannelService channel)
    {
        _service = service;
        _channel = channel;
    }

    /// <summary>
    /// Шинэ queue дугаар үүсгээд ticket app-д буцаана.
    /// </summary>
    /// <returns>Үүсгэсэн queue дугаарын мэдээлэл.</returns>
    [HttpPost]
    public async Task<IActionResult> Create()
    {
        var queue = await _service.CreateQueueAsync();

        return Ok(queue);
    }

    /// <summary>
    /// Teller app-аас дараагийн үйлчлүүлэгчийг дуудах хүсэлтийг worker руу илгээнэ.
    /// </summary>
    /// <returns>Дуудагдсан queue мэдээлэл, байхгүй бол 404.</returns>
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

    /// <summary>
    /// Одоогоор хадгалагдсан бүх queue бичлэгийг авна.
    /// </summary>
    /// <returns>Queue жагсаалт.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }
}
