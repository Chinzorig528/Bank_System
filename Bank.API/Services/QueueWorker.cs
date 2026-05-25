using BankApi.Channels;
using BankInfrastructure.Interfaces;
using Microsoft.Extensions.Hosting;

namespace BankServices.Services;

public class QueueWorker : BackgroundService
{
    private readonly QueueChannelService _channel;

    private readonly IServiceScopeFactory _scopeFactory;

    public QueueWorker(
        QueueChannelService channel,
        IServiceScopeFactory scopeFactory)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
    }

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