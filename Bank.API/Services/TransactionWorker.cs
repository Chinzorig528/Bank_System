using BankInfrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class TransactionWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly TransactionChannelService _channel;

    public TransactionWorker(
        IServiceScopeFactory scopeFactory,
        TransactionChannelService channel)
    {
        _scopeFactory = scopeFactory;
        _channel = channel;
    }

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