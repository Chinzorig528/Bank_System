using Bank.API.Controllers;
using Bank.Application.DTOs;
using BankDomain.Entities;
using BankInfrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankServices.Tests
{
    /// <summary>
    /// AccountController-ийн transfer endpoint мөнгө зөв шилжүүлж, алдаатай нөхцөлүүдийг зөв хориглож байгааг шалгах тестүүд.
    /// </summary>
    [TestClass]
    public class AccountTransferControllerTests
    {
        /// <summary>
        /// Test бүрт тусдаа InMemory database context үүсгэнэ.
        /// </summary>
        /// <returns>Шинэ test database context.</returns>
        private BankDbContext CreateDbContext()
        {
            var options =
                new DbContextOptionsBuilder<BankDbContext>()
                    .UseInMemoryDatabase(
                        databaseName: Guid.NewGuid().ToString())
                    .Options;

            return new BankDbContext(options);
        }

        /// <summary>
        /// Transfer тестэд хэрэглэх AccountController үүсгэнэ.
        /// </summary>
        /// <param name="db">Тестийн database context.</param>
        /// <returns>Test хийх controller.</returns>
        private static AccountController CreateController(BankDbContext db)
        {
            return new AccountController(
                db,
                new TransactionChannelService());
        }

        /// <summary>
        /// Тестийн database-д урьдчилсан данс нэмнэ.
        /// </summary>
        /// <param name="db">Тестийн database context.</param>
        /// <param name="accountNumber">Дансны дугаар.</param>
        /// <param name="balance">Эхний үлдэгдэл.</param>
        private static async Task AddAccountAsync(
            BankDbContext db,
            string accountNumber,
            decimal balance)
        {
            db.BankAccounts.Add(
                new BankAccount
                {
                    AccountNumber = accountNumber,
                    Balance = balance,
                    CreatedAt = DateTime.Now
                });

            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Илгээгч болон хүлээн авагч данс байж, үлдэгдэл хүрэлцэх үед мөнгө зөв шилжиж байгааг шалгана.
        /// </summary>
        [TestMethod]
        public async Task Transfer_WhenAccountsExistAndBalanceIsEnough_MovesMoney()
        {
            using var db = CreateDbContext();

            await AddAccountAsync(db, "ACC001", 10000m);
            await AddAccountAsync(db, "ACC002", 2000m);

            var controller = CreateController(db);

            var result =
                await controller.Transfer(
                    new TransferDto
                    {
                        FromAccount = "ACC001",
                        ToAccount = "ACC002",
                        Amount = 3000m
                    });

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var sender =
                await db.BankAccounts.FirstAsync(x => x.AccountNumber == "ACC001");

            var receiver =
                await db.BankAccounts.FirstAsync(x => x.AccountNumber == "ACC002");

            Assert.AreEqual(7000m, sender.Balance);
            Assert.AreEqual(5000m, receiver.Balance);
        }

        /// <summary>
        /// Шилжүүлэх дүн 0 эсвэл сөрөг үед BadRequest буцааж байгааг шалгана.
        /// </summary>
        [TestMethod]
        public async Task Transfer_WhenAmountIsZeroOrNegative_ReturnsBadRequest()
        {
            using var db = CreateDbContext();

            await AddAccountAsync(db, "ACC001", 10000m);
            await AddAccountAsync(db, "ACC002", 2000m);

            var controller = CreateController(db);

            var zeroResult =
                await controller.Transfer(
                    new TransferDto
                    {
                        FromAccount = "ACC001",
                        ToAccount = "ACC002",
                        Amount = 0m
                    });

            var negativeResult =
                await controller.Transfer(
                    new TransferDto
                    {
                        FromAccount = "ACC001",
                        ToAccount = "ACC002",
                        Amount = -100m
                    });

            Assert.IsInstanceOfType(zeroResult, typeof(BadRequestObjectResult));
            Assert.IsInstanceOfType(negativeResult, typeof(BadRequestObjectResult));
        }

        /// <summary>
        /// Нэг данс руу өөрөөс нь шилжүүлэхийг хориглож байгааг шалгана.
        /// </summary>
        [TestMethod]
        public async Task Transfer_WhenFromAndToAccountAreSame_ReturnsBadRequest()
        {
            using var db = CreateDbContext();

            await AddAccountAsync(db, "ACC001", 10000m);

            var controller = CreateController(db);

            var result =
                await controller.Transfer(
                    new TransferDto
                    {
                        FromAccount = "ACC001",
                        ToAccount = "ACC001",
                        Amount = 1000m
                    });

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        /// <summary>
        /// Илгээгч данс байхгүй үед BadRequest буцааж байгааг шалгана.
        /// </summary>
        [TestMethod]
        public async Task Transfer_WhenSenderDoesNotExist_ReturnsBadRequest()
        {
            using var db = CreateDbContext();

            await AddAccountAsync(db, "ACC002", 2000m);

            var controller = CreateController(db);

            var result =
                await controller.Transfer(
                    new TransferDto
                    {
                        FromAccount = "UNKNOWN",
                        ToAccount = "ACC002",
                        Amount = 1000m
                    });

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        /// <summary>
        /// Хүлээн авагч данс байхгүй үед BadRequest буцааж байгааг шалгана.
        /// </summary>
        [TestMethod]
        public async Task Transfer_WhenReceiverDoesNotExist_ReturnsBadRequest()
        {
            using var db = CreateDbContext();

            await AddAccountAsync(db, "ACC001", 10000m);

            var controller = CreateController(db);

            var result =
                await controller.Transfer(
                    new TransferDto
                    {
                        FromAccount = "ACC001",
                        ToAccount = "UNKNOWN",
                        Amount = 1000m
                    });

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        /// <summary>
        /// Илгээгчийн үлдэгдэл хүрэлцэхгүй үед шилжүүлэг хийгдэхгүй, үлдэгдэл хэвээр үлдэж байгааг шалгана.
        /// </summary>
        [TestMethod]
        public async Task Transfer_WhenBalanceIsNotEnough_ReturnsBadRequestAndKeepsBalances()
        {
            using var db = CreateDbContext();

            await AddAccountAsync(db, "ACC001", 1000m);
            await AddAccountAsync(db, "ACC002", 2000m);

            var controller = CreateController(db);

            var result =
                await controller.Transfer(
                    new TransferDto
                    {
                        FromAccount = "ACC001",
                        ToAccount = "ACC002",
                        Amount = 3000m
                    });

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            var sender =
                await db.BankAccounts.FirstAsync(x => x.AccountNumber == "ACC001");

            var receiver =
                await db.BankAccounts.FirstAsync(x => x.AccountNumber == "ACC002");

            Assert.AreEqual(1000m, sender.Balance);
            Assert.AreEqual(2000m, receiver.Balance);
        }
    }
}
