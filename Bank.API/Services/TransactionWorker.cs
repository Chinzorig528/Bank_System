using BankInfrastructure.Data;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Дансны орлого, зарлагын хүсэлтүүдийг channel-оор дарааллуулан боловсруулдаг background worker.
/// </summary>
public class TransactionWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly TransactionChannelService _channel;

    /// <summary>
    /// Transaction worker-д шаардлагатай dependency scope factory болон channel-ийг онооно.
    /// </summary>
    /// <param name="scopeFactory">Хүсэлт бүрт шинэ DbContext авахад ашиглах scope factory.</param>
    /// <param name="channel">Орлого, зарлагын хүсэлтүүд дамжих channel service.</param>
    public TransactionWorker(
        IServiceScopeFactory scopeFactory,
        TransactionChannelService channel)
    {
        _scopeFactory = scopeFactory;
        _channel = channel;
    }

    /// <summary>
    /// Channel-д орж ирсэн transaction хүсэлтүүдийг уншиж дансны үлдэгдлийг шинэчилнэ.
    /// </summary>
    /// <param name="stoppingToken">Апп хаагдах үед worker-ийг зогсоох token.</param>
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        await foreach (var request in
            _channel.Queue.Reader.ReadAllAsync(
                stoppingToken))
        {
            using var scope =
                _scopeFactory.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<BankDbContext>();

            var account =
                await db.BankAccounts
                    .FirstOrDefaultAsync(x =>
                        x.AccountNumber ==
                        request.AccountNumber);

            if (account == null)
            {
                request.CompletionSource
                    .SetResult(false);

                continue;
            }

            if (request.Type ==
                TransactionType.Deposit)
            {
                account.Balance += request.Amount;
            }
            else
            {
                if (account.Balance <
                    request.Amount)
                {
                    request.CompletionSource
                        .SetResult(false);

                    continue;
                }

                account.Balance -=
                    request.Amount;
            }

            await db.SaveChangesAsync();

            request.CompletionSource
                .SetResult(true);
        }
    }
}
