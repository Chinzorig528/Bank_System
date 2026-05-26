using Bank.API.Controllers;
using BankDomain.Entities;
using BankInfrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankServices.Tests
{
    // AccountControllerTests класс нь AccountController-ийн үндсэн үйлдлүүдийг
    // unit test хийх зориулалттай.
    //
    // Энэ тестүүд бодит SQLite/MySQL database ашиглахгүй.
    // Харин Entity Framework Core-ийн InMemoryDatabase ашиглаж,
    // test бүр дээр түр database үүсгэн ажиллуулна.
    //
    // Шалгаж байгаа үндсэн үйлдлүүд:
    // 1. Account шинээр үүсэх эсэх
    // 2. Давхардсан account үүсгэх үед BadRequest буцаах эсэх
    // 3. Account balance зөв буцаах эсэх
    // 4. Байхгүй account хайхад NotFound буцаах эсэх
    // 5. Balance 0 үед account устах эсэх
    // 6. Balance-тэй account устгахыг хориглож байгаа эсэх
    [TestClass]
    public class AccountControllerTests
    {
        // Test бүрт ашиглах түр InMemory database үүсгэдэг helper method.
        //
        // Guid.NewGuid().ToString() ашиглаж байгаа шалтгаан:
        // Test бүр өөр өөр database нэртэй болно.
        // Ингэснээр нэг test-ийн өгөгдөл нөгөө test-д нөлөөлөхгүй.
        private BankDbContext CreateDbContext()
        {
            var options =
                new DbContextOptionsBuilder<BankDbContext>()
                    .UseInMemoryDatabase(
                        databaseName: Guid.NewGuid().ToString())
                    .Options;

            // Дээр үүсгэсэн options-оор BankDbContext object буцаана.
            return new BankDbContext(options);
        }

        [TestMethod]
        public async Task Create_WhenAccountDoesNotExist_CreatesAccount()
        {
            // Arrange
            // Түр InMemory database үүсгэж байна.
            using var db = CreateDbContext();

            // TransactionChannelService нь AccountController-ийн constructor-т хэрэгтэй dependency.
            // Энэ test дээр гол шалгах зүйл нь account үүсэх эсэх.
            var channel = new TransactionChannelService();

            // Test хийх AccountController object үүсгэж байна.
            var controller =
                new AccountController(db, channel);

            // Act
            // ACC001 дугаартай account үүсгэх action method-ийг дуудна.
            var result =
                await controller.Create("ACC001");

            // Assert
            // Account амжилттай үүссэн бол controller OkObjectResult буцаах ёстой.
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            // Database дотроос ACC001 account үнэхээр нэмэгдсэн эсэхийг хайж байна.
            var account =
                await db.BankAccounts
                    .FirstOrDefaultAsync(x =>
                        x.AccountNumber == "ACC001");

            // Account null биш байвал database-д амжилттай хадгалагдсан гэсэн үг.
            Assert.IsNotNull(account);

            // Account number зөв хадгалагдсан эсэхийг шалгана.
            Assert.AreEqual("ACC001", account.AccountNumber);

            // Шинээр үүссэн account-ийн balance 0 байх ёстой.
            Assert.AreEqual(0m, account.Balance);
        }

        [TestMethod]
        public async Task Create_WhenAccountAlreadyExists_ReturnsBadRequest()
        {
            // Arrange
            // Түр database үүсгэнэ.
            using var db = CreateDbContext();

            // Database-д ACC001 account-ийг урьдчилж нэмнэ.
            // Энэ нь "account аль хэдийн байна" гэсэн нөхцөлийг бэлдэж байгаа.
            db.BankAccounts.Add(
                new BankAccount
                {
                    AccountNumber = "ACC001",
                    Balance = 0,
                    CreatedAt = DateTime.Now
                });

            // Өөрчлөлтийг InMemory database-д хадгална.
            await db.SaveChangesAsync();

            var channel = new TransactionChannelService();

            var controller =
                new AccountController(db, channel);

            // Act
            // Аль хэдийн байгаа ACC001 account-ийг дахин үүсгэх гэж оролдоно.
            var result =
                await controller.Create("ACC001");

            // Assert
            // Давхардсан account үүсгэх гэж оролдсон тул BadRequestObjectResult буцаах ёстой.
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            // Database-д account давхар нэмэгдээгүй эсэхийг шалгана.
            // Count 1 хэвээрээ байх ёстой.
            Assert.AreEqual(1, db.BankAccounts.Count());
        }

        [TestMethod]
        public async Task Balance_WhenAccountExists_ReturnsBalance()
        {
            // Arrange
            // Түр database үүсгэнэ.
            using var db = CreateDbContext();

            // Balance шалгахын тулд ACC001 account-ийг 50000 төгрөгийн үлдэгдэлтэйгээр нэмнэ.
            db.BankAccounts.Add(
                new BankAccount
                {
                    AccountNumber = "ACC001",
                    Balance = 50000,
                    CreatedAt = DateTime.Now
                });

            // Database-д account-оо хадгална.
            await db.SaveChangesAsync();

            var channel = new TransactionChannelService();

            var controller =
                new AccountController(db, channel);

            // Act
            // ACC001 account-ийн balance авах method-ийг дуудна.
            var result =
                await controller.Balance("ACC001");

            // Assert
            // Account байгаа тул OkObjectResult буцаах ёстой.
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            // IActionResult-ийг OkObjectResult болгон cast хийж,
            // доторх Value буюу balance утгыг шалгана.
            var okResult =
                result as OkObjectResult;

            // Balance нь 50000 байх ёстой.
            Assert.AreEqual(50000m, okResult!.Value);
        }

        [TestMethod]
        public async Task Balance_WhenAccountDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            // Түр database үүсгэж байна.
            // Энэ database-д ямар ч account нэмээгүй.
            using var db = CreateDbContext();

            var channel = new TransactionChannelService();

            var controller =
                new AccountController(db, channel);

            // Act
            // Database-д байхгүй UNKNOWN account-ийн balance авах гэж оролдоно.
            var result =
                await controller.Balance("UNKNOWN");

            // Assert
            // Account байхгүй тул NotFoundResult буцаах ёстой.
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Delete_WhenAccountExistsAndBalanceIsZero_DeletesAccount()
        {
            // Arrange
            // Түр database үүсгэнэ.
            using var db = CreateDbContext();

            // Устгах боломжтой account бэлдэж байна.
            // Balance 0 учраас энэ account-ийг устгаж болно.
            db.BankAccounts.Add(
                new BankAccount
                {
                    AccountNumber = "ACC001",
                    Balance = 0,
                    CreatedAt = DateTime.Now
                });

            // Account-ийг database-д хадгална.
            await db.SaveChangesAsync();

            var channel = new TransactionChannelService();

            var controller =
                new AccountController(db, channel);

            // Act
            // ACC001 account-ийг устгах method-ийг дуудна.
            var result =
                await controller.Delete("ACC001");

            // Assert
            // Balance 0 тул устгал амжилттай болж OkResult буцаах ёстой.
            Assert.IsInstanceOfType(result, typeof(OkResult));

            // Database-д account үлдээгүй байх ёстой.
            Assert.AreEqual(0, db.BankAccounts.Count());
        }

        [TestMethod]
        public async Task Delete_WhenAccountHasBalance_ReturnsBadRequest()
        {
            // Arrange
            // Түр database үүсгэнэ.
            using var db = CreateDbContext();

            // Balance-тэй account нэмнэ.
            // Ийм account-ийг шууд устгах ёсгүй.
            db.BankAccounts.Add(
                new BankAccount
                {
                    AccountNumber = "ACC001",
                    Balance = 10000,
                    CreatedAt = DateTime.Now
                });

            // Account-ийг database-д хадгална.
            await db.SaveChangesAsync();

            var channel = new TransactionChannelService();

            var controller =
                new AccountController(db, channel);

            // Act
            // Balance-тэй account устгах гэж оролдоно.
            var result =
                await controller.Delete("ACC001");

            // Assert
            // Balance 10000 байгаа тул BadRequestObjectResult буцаах ёстой.
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            // Account устгагдаагүй байх ёстой.
            // Тиймээс database-д 1 account хэвээр байна.
            Assert.AreEqual(1, db.BankAccounts.Count());
        }

        [TestMethod]
        public async Task Delete_WhenAccountDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            // Түр database үүсгэнэ.
            // Энэ database-д account байхгүй.
            using var db = CreateDbContext();

            var channel = new TransactionChannelService();

            var controller =
                new AccountController(db, channel);

            // Act
            // Байхгүй UNKNOWN account устгах гэж оролдоно.
            var result =
                await controller.Delete("UNKNOWN");

            // Assert
            // Account олдохгүй тул NotFoundResult буцаах ёстой.
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}