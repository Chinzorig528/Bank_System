using BankDomain.Entities;
using BankInfrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BankServices.Tests
{
    [TestClass]
    public class TransactionWorkerTests
    {
        private ServiceProvider CreateServiceProvider(
            string databaseName)
        {
            var services =
                new ServiceCollection();

            services.AddDbContext<BankDbContext>(
                options =>
                    options.UseInMemoryDatabase(databaseName));

            return services.BuildServiceProvider();
        }

        private async Task AddAccountAsync(
            string databaseName,
            string accountNumber,
            decimal balance)
        {
            using var provider =
                CreateServiceProvider(databaseName);

            using var scope =
                provider.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<BankDbContext>();

            db.BankAccounts.Add(
                new BankAccount
                {
                    AccountNumber = accountNumber,
                    Balance = balance,
                    CreatedAt = DateTime.Now
                });

            await db.SaveChangesAsync();
        }

        private async Task<decimal> GetBalanceAsync(
            string databaseName,
            string accountNumber)
        {
            using var provider =
                CreateServiceProvider(databaseName);

            using var scope =
                provider.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<BankDbContext>();

            var account =
                await db.BankAccounts
                    .FirstAsync(x =>
                        x.AccountNumber == accountNumber);

            return account.Balance;
        }

        [TestMethod]
        public async Task TransactionWorker_WhenDepositRequest_BalanceIncreases()
        {
            // Arrange
            var databaseName =
                Guid.NewGuid().ToString();

            await AddAccountAsync(
                databaseName,
                "ACC001",
                10000m);

            var provider =
                CreateServiceProvider(databaseName);

            var channel =
                new TransactionChannelService();

            var worker =
                new TransactionWorker(
                    provider.GetRequiredService<IServiceScopeFactory>(),
                    channel);

            using var cts =
                new CancellationTokenSource();

            await worker.StartAsync(cts.Token);

            var request =
                new TransactionRequest
                {
                    AccountNumber = "ACC001",
                    Amount = 5000m,
                    Type = TransactionType.Deposit
                };

            // Act
            await channel.Queue.Writer.WriteAsync(request);

            var result =
                await request.CompletionSource.Task;

            var balance =
                await GetBalanceAsync(
                    databaseName,
                    "ACC001");

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(15000m, balance);

            await worker.StopAsync(cts.Token);
        }

        [TestMethod]
        public async Task TransactionWorker_WhenWithdrawRequest_BalanceDecreases()
        {
            // Arrange
            var databaseName =
                Guid.NewGuid().ToString();

            await AddAccountAsync(
                databaseName,
                "ACC001",
                10000m);

            var provider =
                CreateServiceProvider(databaseName);

            var channel =
                new TransactionChannelService();

            var worker =
                new TransactionWorker(
                    provider.GetRequiredService<IServiceScopeFactory>(),
                    channel);

            using var cts =
                new CancellationTokenSource();

            await worker.StartAsync(cts.Token);

            var request =
                new TransactionRequest
                {
                    AccountNumber = "ACC001",
                    Amount = 3000m,
                    Type = TransactionType.Withdraw
                };

            // Act
            await channel.Queue.Writer.WriteAsync(request);

            var result =
                await request.CompletionSource.Task;

            var balance =
                await GetBalanceAsync(
                    databaseName,
                    "ACC001");

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(7000m, balance);

            await worker.StopAsync(cts.Token);
        }

        [TestMethod]
        public async Task TransactionWorker_WhenWithdrawAmountIsGreaterThanBalance_ReturnsFalse()
        {
            // Arrange
            var databaseName =
                Guid.NewGuid().ToString();

            await AddAccountAsync(
                databaseName,
                "ACC001",
                10000m);

            var provider =
                CreateServiceProvider(databaseName);

            var channel =
                new TransactionChannelService();

            var worker =
                new TransactionWorker(
                    provider.GetRequiredService<IServiceScopeFactory>(),
                    channel);

            using var cts =
                new CancellationTokenSource();

            await worker.StartAsync(cts.Token);

            var request =
                new TransactionRequest
                {
                    AccountNumber = "ACC001",
                    Amount = 15000m,
                    Type = TransactionType.Withdraw
                };

            // Act
            await channel.Queue.Writer.WriteAsync(request);

            var result =
                await request.CompletionSource.Task;

            var balance =
                await GetBalanceAsync(
                    databaseName,
                    "ACC001");

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(10000m, balance);

            await worker.StopAsync(cts.Token);
        }

        [TestMethod]
        public async Task TransactionWorker_WhenAccountDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var databaseName =
                Guid.NewGuid().ToString();

            var provider =
                CreateServiceProvider(databaseName);

            var channel =
                new TransactionChannelService();

            var worker =
                new TransactionWorker(
                    provider.GetRequiredService<IServiceScopeFactory>(),
                    channel);

            using var cts =
                new CancellationTokenSource();

            await worker.StartAsync(cts.Token);

            var request =
                new TransactionRequest
                {
                    AccountNumber = "UNKNOWN",
                    Amount = 5000m,
                    Type = TransactionType.Deposit
                };

            // Act
            await channel.Queue.Writer.WriteAsync(request);

            var result =
                await request.CompletionSource.Task;

            // Assert
            Assert.IsFalse(result);

            await worker.StopAsync(cts.Token);
        }
    }
}