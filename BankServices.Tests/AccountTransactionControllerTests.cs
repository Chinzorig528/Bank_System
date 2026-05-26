using Bank.API.Controllers;
using BankDomain.Entities;
using BankInfrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BankServices.Tests
{
    [TestClass]
    public class AccountTransactionControllerTests
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

        private async Task<TransactionWorker> StartWorkerAsync(
            string databaseName,
            TransactionChannelService channel)
        {
            var provider =
                CreateServiceProvider(databaseName);

            var worker =
                new TransactionWorker(
                    provider.GetRequiredService<IServiceScopeFactory>(),
                    channel);

            await worker.StartAsync(
                CancellationToken.None);

            return worker;
        }

        [TestMethod]
        public async Task Deposit_WhenAccountExists_IncreasesBalance()
        {
            // Arrange
            var databaseName =
                Guid.NewGuid().ToString();

            await AddAccountAsync(
                databaseName,
                "ACC001",
                10000m);

            var channel =
                new TransactionChannelService();

            var worker =
                await StartWorkerAsync(
                    databaseName,
                    channel);

            using var provider =
                CreateServiceProvider(databaseName);

            using var scope =
                provider.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<BankDbContext>();

            var controller =
                new AccountController(db, channel);

            var dto =
                new DepositDto
                {
                    AccountNumber = "ACC001",
                    Amount = 5000m
                };

            // Act
            var result =
                await controller.Deposit(dto);

            var balance =
                await GetBalanceAsync(
                    databaseName,
                    "ACC001");

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(OkObjectResult));

            Assert.AreEqual(
                15000m,
                balance);

            await worker.StopAsync(
                CancellationToken.None);
        }

        [TestMethod]
        public async Task Withdraw_WhenAccountHasEnoughBalance_DecreasesBalance()
        {
            // Arrange
            var databaseName =
                Guid.NewGuid().ToString();

            await AddAccountAsync(
                databaseName,
                "ACC001",
                10000m);

            var channel =
                new TransactionChannelService();

            var worker =
                await StartWorkerAsync(
                    databaseName,
                    channel);

            using var provider =
                CreateServiceProvider(databaseName);

            using var scope =
                provider.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<BankDbContext>();

            var controller =
                new AccountController(db, channel);

            var dto =
                new WithdrawDto
                {
                    AccountNumber = "ACC001",
                    Amount = 3000m
                };

            // Act
            var result =
                await controller.Withdraw(dto);

            var balance =
                await GetBalanceAsync(
                    databaseName,
                    "ACC001");

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(OkResult));

            Assert.AreEqual(
                7000m,
                balance);

            await worker.StopAsync(
                CancellationToken.None);
        }

        [TestMethod]
        public async Task Withdraw_WhenBalanceIsNotEnough_ReturnsBadRequest()
        {
            // Arrange
            var databaseName =
                Guid.NewGuid().ToString();

            await AddAccountAsync(
                databaseName,
                "ACC001",
                10000m);

            var channel =
                new TransactionChannelService();

            var worker =
                await StartWorkerAsync(
                    databaseName,
                    channel);

            using var provider =
                CreateServiceProvider(databaseName);

            using var scope =
                provider.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<BankDbContext>();

            var controller =
                new AccountController(db, channel);

            var dto =
                new WithdrawDto
                {
                    AccountNumber = "ACC001",
                    Amount = 15000m
                };

            // Act
            var result =
                await controller.Withdraw(dto);

            var balance =
                await GetBalanceAsync(
                    databaseName,
                    "ACC001");

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(BadRequestObjectResult));

            Assert.AreEqual(
                10000m,
                balance);

            await worker.StopAsync(
                CancellationToken.None);
        }
    }
}