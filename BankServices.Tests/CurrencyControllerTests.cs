using BankApi.Controllers;
using BankApi.Hubs;
using BankDomain.Entities;
using BankInfrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BankServices.Tests
{
    [TestClass]
    public class CurrencyControllerTests
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

        private CurrencyController CreateController(
            BankDbContext db)
        {
            var hubMock =
                new Mock<IHubContext<CurrencyHub>>();

            var clientsMock =
                new Mock<IHubClients>();

            var clientProxyMock =
                new Mock<IClientProxy>();

            clientProxyMock
                .Setup(x =>
                    x.SendCoreAsync(
                        It.IsAny<string>(),
                        It.IsAny<object?[]>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            clientsMock
                .Setup(x => x.All)
                .Returns(clientProxyMock.Object);

            hubMock
                .Setup(x => x.Clients)
                .Returns(clientsMock.Object);

            return new CurrencyController(
                db,
                hubMock.Object);
        }

        [TestMethod]
        public async Task Seed_WhenCurrencyTableIsEmpty_CreatesDefaultRates()
        {
            // Arrange
            using var db =
                CreateDbContext();

            var controller =
                CreateController(db);

            // Act
            var result =
                await controller.Seed();

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(OkObjectResult));

            var rates =
                await db.CurrencyRates
                    .OrderBy(x => x.Code)
                    .ToListAsync();

            Assert.AreEqual(
                5,
                rates.Count);

            Assert.IsTrue(
                rates.Any(x => x.Code == "USD"));

            Assert.IsTrue(
                rates.Any(x => x.Code == "EUR"));

            Assert.IsTrue(
                rates.Any(x => x.Code == "CNY"));

            Assert.IsTrue(
                rates.Any(x => x.Code == "JPY"));

            Assert.IsTrue(
                rates.Any(x => x.Code == "KRW"));
        }

        [TestMethod]
        public async Task Seed_WhenCurrencyAlreadyExists_DoesNotCreateDuplicates()
        {
            // Arrange
            using var db =
                CreateDbContext();

            db.CurrencyRates.Add(
                new CurrencyRate
                {
                    Code = "USD",
                    Name = "Dollar",
                    BuyRate = 3450,
                    SellRate = 3470,
                    UpdatedAt = DateTime.Now
                });

            await db.SaveChangesAsync();

            var controller =
                CreateController(db);

            // Act
            var result =
                await controller.Seed();

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(OkObjectResult));

            Assert.AreEqual(
                1,
                db.CurrencyRates.Count());
        }

        [TestMethod]
        public async Task GetAll_WhenRatesExist_ReturnsRatesOrderedByCode()
        {
            // Arrange
            using var db =
                CreateDbContext();

            db.CurrencyRates.AddRange(
                new CurrencyRate
                {
                    Code = "USD",
                    Name = "Dollar",
                    BuyRate = 3450,
                    SellRate = 3470,
                    UpdatedAt = DateTime.Now
                },
                new CurrencyRate
                {
                    Code = "CNY",
                    Name = "Yuan",
                    BuyRate = 475,
                    SellRate = 482,
                    UpdatedAt = DateTime.Now
                });

            await db.SaveChangesAsync();

            var controller =
                CreateController(db);

            // Act
            var result =
                await controller.GetAll();

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(OkObjectResult));

            var okResult =
                result as OkObjectResult;

            var rates =
                okResult!.Value as List<CurrencyRate>;

            Assert.IsNotNull(rates);

            Assert.AreEqual(
                2,
                rates.Count);

            Assert.AreEqual(
                "CNY",
                rates[0].Code);

            Assert.AreEqual(
                "USD",
                rates[1].Code);
        }

        [TestMethod]
        public async Task Update_WhenRateExists_UpdatesCurrencyRate()
        {
            // Arrange
            using var db =
                CreateDbContext();

            var rate =
                new CurrencyRate
                {
                    Code = "USD",
                    Name = "Dollar",
                    BuyRate = 3450,
                    SellRate = 3470,
                    UpdatedAt = DateTime.Now
                };

            db.CurrencyRates.Add(rate);

            await db.SaveChangesAsync();

            var controller =
                CreateController(db);

            var updated =
                new CurrencyRate
                {
                    Code = "USD",
                    Name = "Америк доллар",
                    BuyRate = 3500,
                    SellRate = 3520
                };

            // Act
            var result =
                await controller.Update(
                    rate.Id,
                    updated);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(OkObjectResult));

            var saved =
                await db.CurrencyRates
                    .FirstAsync(x =>
                        x.Id == rate.Id);

            Assert.AreEqual(
                "USD",
                saved.Code);

            Assert.AreEqual(
                "Америк доллар",
                saved.Name);

            Assert.AreEqual(
                3500m,
                saved.BuyRate);

            Assert.AreEqual(
                3520m,
                saved.SellRate);
        }

        [TestMethod]
        public async Task Update_WhenRateDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            using var db =
                CreateDbContext();

            var controller =
                CreateController(db);

            var updated =
                new CurrencyRate
                {
                    Code = "USD",
                    Name = "Dollar",
                    BuyRate = 3500,
                    SellRate = 3520
                };

            // Act
            var result =
                await controller.Update(
                    999,
                    updated);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task UpdateAll_WhenListIsEmpty_ReturnsBadRequest()
        {
            // Arrange
            using var db =
                CreateDbContext();

            var controller =
                CreateController(db);

            // Act
            var result =
                await controller.UpdateAll(
                    new List<CurrencyRate>());

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task UpdateAll_WhenRatesExist_UpdatesAllRates()
        {
            // Arrange
            using var db =
                CreateDbContext();

            var usd =
                new CurrencyRate
                {
                    Code = "USD",
                    Name = "Dollar",
                    BuyRate = 3450,
                    SellRate = 3470,
                    UpdatedAt = DateTime.Now
                };

            var eur =
                new CurrencyRate
                {
                    Code = "EUR",
                    Name = "Euro",
                    BuyRate = 3700,
                    SellRate = 3740,
                    UpdatedAt = DateTime.Now
                };

            db.CurrencyRates.AddRange(
                usd,
                eur);

            await db.SaveChangesAsync();

            var controller =
                CreateController(db);

            var updatedRates =
                new List<CurrencyRate>
                {
                    new CurrencyRate
                    {
                        Id = usd.Id,
                        Code = "USD",
                        Name = "Америк доллар",
                        BuyRate = 3500,
                        SellRate = 3520
                    },
                    new CurrencyRate
                    {
                        Id = eur.Id,
                        Code = "EUR",
                        Name = "Евро",
                        BuyRate = 3800,
                        SellRate = 3840
                    }
                };

            // Act
            var result =
                await controller.UpdateAll(updatedRates);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(OkObjectResult));

            var savedUsd =
                await db.CurrencyRates
                    .FirstAsync(x =>
                        x.Id == usd.Id);

            var savedEur =
                await db.CurrencyRates
                    .FirstAsync(x =>
                        x.Id == eur.Id);

            Assert.AreEqual(
                3500m,
                savedUsd.BuyRate);

            Assert.AreEqual(
                3520m,
                savedUsd.SellRate);

            Assert.AreEqual(
                3800m,
                savedEur.BuyRate);

            Assert.AreEqual(
                3840m,
                savedEur.SellRate);
        }
    }
}