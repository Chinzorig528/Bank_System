using Bank.API.Controllers;
using BankDomain.Entities;
using BankInfrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankServices.Tests
{
    [TestClass]
    public class AccountControllerTests
    {
        private BankDbContext CreateDbContext()
        {
            var options =
                new DbContextOptionsBuilder<BankDbContext>()
                    .UseInMemoryDatabase(
                        databaseName: Guid.NewGuid().ToString())
                    .Options;

            return new BankDbContext(options);
        }

        [TestMethod]
        public async Task Create_WhenAccountDoesNotExist_CreatesAccount()
        {
            // Arrange
            using var db = CreateDbContext();

            var channel = new TransactionChannelService();

            var controller =
                new AccountController(db, channel);

            // Act
            var result =
                await controller.Create("ACC001");

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var account =
                await db.BankAccounts
                    .FirstOrDefaultAsync(x =>
                        x.AccountNumber == "ACC001");

            Assert.IsNotNull(account);
            Assert.AreEqual("ACC001", account.AccountNumber);
            Assert.AreEqual(0m, account.Balance);
        }

        [TestMethod]
        public async Task Create_WhenAccountAlreadyExists_ReturnsBadRequest()
        {
            // Arrange
            using var db = CreateDbContext();

            db.BankAccounts.Add(
                new BankAccount
                {
                    AccountNumber = "ACC001",
                    Balance = 0,
                    CreatedAt = DateTime.Now
                });

            await db.SaveChangesAsync();

            var channel = new TransactionChannelService();

            var controller =
                new AccountController(db, channel);

            // Act
            var result =
                await controller.Create("ACC001");

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            Assert.AreEqual(1, db.BankAccounts.Count());
        }

        [TestMethod]
        public async Task Balance_WhenAccountExists_ReturnsBalance()
        {
            // Arrange
            using var db = CreateDbContext();

            db.BankAccounts.Add(
                new BankAccount
                {
                    AccountNumber = "ACC001",
                    Balance = 50000,
                    CreatedAt = DateTime.Now
                });

            await db.SaveChangesAsync();

            var channel = new TransactionChannelService();

            var controller =
                new AccountController(db, channel);

            // Act
            var result =
                await controller.Balance("ACC001");

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var okResult =
                result as OkObjectResult;

            Assert.AreEqual(50000m, okResult!.Value);
        }

        [TestMethod]
        public async Task Balance_WhenAccountDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            using var db = CreateDbContext();

            var channel = new TransactionChannelService();

            var controller =
                new AccountController(db, channel);

            // Act
            var result =
                await controller.Balance("UNKNOWN");

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Delete_WhenAccountExistsAndBalanceIsZero_DeletesAccount()
        {
            // Arrange
            using var db = CreateDbContext();

            db.BankAccounts.Add(
                new BankAccount
                {
                    AccountNumber = "ACC001",
                    Balance = 0,
                    CreatedAt = DateTime.Now
                });

            await db.SaveChangesAsync();

            var channel = new TransactionChannelService();

            var controller =
                new AccountController(db, channel);

            // Act
            var result =
                await controller.Delete("ACC001");

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));

            Assert.AreEqual(0, db.BankAccounts.Count());
        }

        [TestMethod]
        public async Task Delete_WhenAccountHasBalance_ReturnsBadRequest()
        {
            // Arrange
            using var db = CreateDbContext();

            db.BankAccounts.Add(
                new BankAccount
                {
                    AccountNumber = "ACC001",
                    Balance = 10000,
                    CreatedAt = DateTime.Now
                });

            await db.SaveChangesAsync();

            var channel = new TransactionChannelService();

            var controller =
                new AccountController(db, channel);

            // Act
            var result =
                await controller.Delete("ACC001");

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            Assert.AreEqual(1, db.BankAccounts.Count());
        }

        [TestMethod]
        public async Task Delete_WhenAccountDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            using var db = CreateDbContext();

            var channel = new TransactionChannelService();

            var controller =
                new AccountController(db, channel);

            // Act
            var result =
                await controller.Delete("UNKNOWN");

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}