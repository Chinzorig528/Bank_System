using BankApi.Channels;
using BankInfrastructure.Interfaces;
using Microsoft.Extensions.Hosting;

namespace BankServices.Services;

/// <summary>
/// Queue дуудах хүсэлтүүдийг channel-оос дарааллаар уншиж боловсруулдаг background worker.
/// </summary>
public class QueueWorker : BackgroundService
{
    private readonly QueueChannelService _channel;

    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Queue worker-д шаардлагатай channel болон scoped service factory-г онооно.
    /// </summary>
    /// <param name="channel">Дараагийн queue дуудах хүсэлт дамжуулах channel.</param>
    /// <param name="scopeFactory">Request бүрт шинэ dependency scope үүсгэх factory.</param>
    public QueueWorker(
        QueueChannelService channel,
        IServiceScopeFactory scopeFactory)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// API ажиллаж байх хугацаанд queue хүсэлтүүдийг тасралтгүй сонсож боловсруулна.
    /// </summary>
    /// <param name="stoppingToken">Апп зогсох үед worker-ийг цуцлах token.</param>
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        await foreach (var request in
            _channel.Queue.Reader.ReadAllAsync(stoppingToken))
        {
            using var scope =
                _scopeFactory.CreateScope();

            var repo =
                scope.ServiceProvider
                    .GetRequiredService<IQueueRepository>();

            var next =
                await repo.GetNextAsync();

            if (next != null)
            {
                next.IsCalled = true;

                await repo.SaveChangesAsync();
            }

            request.Completion
                .SetResult(next);
        }
    }
}
