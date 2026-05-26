using Bank.API.Controllers;
using BankDomain.Entities;
using BankInfrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BankServices.Tests
{
    // AccountTransactionControllerTests класс нь account дээрх мөнгөн гүйлгээний
    // үйлдлүүдийг test хийх зориулалттай.
    //
    // Энэ тестүүд дараах үйлдлүүдийг шалгана:
    // 1. Deposit хийхэд balance нэмэгдэж байна уу?
    // 2. Withdraw хийхэд balance хасагдаж байна уу?
    // 3. Үлдэгдэл хүрэлцэхгүй үед withdraw хийхийг хориглож байна уу?
    //
    // Онцлог:
    // Энд зөвхөн controller шууд database өөрчлөхгүй.
    // Controller нь гүйлгээний хүсэлтийг TransactionChannelService рүү оруулна.
    // Харин TransactionWorker тэр хүсэлтийг уншиж database дээр balance өөрчилнө.
    [TestClass]
    public class AccountTransactionControllerTests
    {
        // Test-д хэрэглэх ServiceProvider үүсгэдэг helper method.
        //
        // ServiceProvider гэдэг нь dependency injection container юм.
        // Өөрөөр хэлбэл BankDbContext гэх мэт хэрэгтэй object-уудыг
        // автоматаар үүсгэж өгдөг сав гэж ойлгож болно.
        //
        // databaseName параметр ашиглаж байгаа шалтгаан:
        // Нэг ижил нэртэй InMemory database ашиглавал өгөгдөл хоорондоо холилдож болно.
        // Тиймээс test бүр Guid ашиглаж өөр databaseName үүсгэнэ.
        private ServiceProvider CreateServiceProvider(
            string databaseName)
        {
            // DI container-д бүртгэх service-үүдийг хадгалах collection үүсгэнэ.
            var services =
                new ServiceCollection();

            // BankDbContext-ийг InMemory database ашиглахаар бүртгэнэ.
            // Энэ нь бодит SQLite/MySQL database ашиглахгүй гэсэн үг.
            services.AddDbContext<BankDbContext>(
                options =>
                    options.UseInMemoryDatabase(databaseName));

            // Дээр бүртгэсэн service-үүдээр ServiceProvider үүсгээд буцаана.
            return services.BuildServiceProvider();
        }

        // Test эхлэхээс өмнө database-д account урьдчилж нэмэх helper method.
        //
        // Жишээ:
        // AddAccountAsync(databaseName, "ACC001", 10000m)
        // гэж дуудвал ACC001 account 10000 balance-тэйгээр database-д нэмэгдэнэ.
        private async Task AddAccountAsync(
            string databaseName,
            string accountNumber,
            decimal balance)
        {
            // Тухайн databaseName-тэй InMemory database ашиглах provider үүсгэнэ.
            using var provider =
                CreateServiceProvider(databaseName);

            // Scope үүсгэж байна.
            // Scoped service болох BankDbContext-ийг зөв lifetime-тай авахын тулд scope хэрэгтэй.
            using var scope =
                provider.CreateScope();

            // DI container-оос BankDbContext object авна.
            var db =
                scope.ServiceProvider
                    .GetRequiredService<BankDbContext>();

            // Database-д шинэ BankAccount entity нэмнэ.
            db.BankAccounts.Add(
                new BankAccount
                {
                    AccountNumber = accountNumber,
                    Balance = balance,
                    CreatedAt = DateTime.Now
                });

            // Өөрчлөлтийг InMemory database-д хадгална.
            await db.SaveChangesAsync();
        }

        // Database дотроос тухайн account-ийн balance-ийг уншиж авах helper method.
        //
        // Энэ method нь Act хэсгийн дараа balance үнэхээр өөрчлөгдсөн эсэхийг
        // шалгахад ашиглагдана.
        private async Task<decimal> GetBalanceAsync(
            string databaseName,
            string accountNumber)
        {
            // Өмнө ашигласан databaseName-ээр provider дахин үүсгэнэ.
            // Нэр ижил учраас өмнөх account-ийн data хадгалагдсан хэвээр байна.
            using var provider =
                CreateServiceProvider(databaseName);

            using var scope =
                provider.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<BankDbContext>();

            // AccountNumber таарсан account-ийг database-аас олно.
            // FirstAsync нь account олдохгүй бол exception гаргана.
            // Энэ тестүүд дээр account заавал байх ёстой гэж үзэж байна.
            var account =
                await db.BankAccounts
                    .FirstAsync(x =>
                        x.AccountNumber == accountNumber);

            // Олдсон account-ийн balance-ийг буцаана.
            return account.Balance;
        }

        // TransactionWorker-ийг test дээр ажиллуулах helper method.
        //
        // Яагаад worker хэрэгтэй вэ?
        // Deposit болон Withdraw үйлдэл controller дээр шууд balance өөрчлөхгүй байж болно.
        // Controller нь TransactionChannelService рүү request хийж,
        // worker нь тэр request-ийг уншаад database дээр balance шинэчилдэг.
        private async Task<TransactionWorker> StartWorkerAsync(
            string databaseName,
            TransactionChannelService channel)
        {
            // Worker database-тэй ажиллахын тулд ServiceProvider хэрэгтэй.
            var provider =
                CreateServiceProvider(databaseName);

            // TransactionWorker үүсгэнэ.
            // IServiceScopeFactory нь worker дотор шинэ scope үүсгэж,
            // BankDbContext авахад ашиглагдана.
            var worker =
                new TransactionWorker(
                    provider.GetRequiredService<IServiceScopeFactory>(),
                    channel);

            // Worker-ийг эхлүүлнэ.
            // Ингэснээр channel рүү орсон transaction-уудыг боловсруулж эхэлнэ.
            await worker.StartAsync(
                CancellationToken.None);

            // Дараа нь test дуусахад StopAsync хийхийн тулд worker object-ийг буцаана.
            return worker;
        }

        [TestMethod]
        public async Task Deposit_WhenAccountExists_IncreasesBalance()
        {
            // Arrange
            // Test бүр өөрийн тусдаа database-тэй байх ёстой.
            // Тиймээс random unique databaseName үүсгэнэ.
            var databaseName =
                Guid.NewGuid().ToString();

            // ACC001 account-ийг 10000 төгрөгийн үлдэгдэлтэйгээр урьдчилж нэмнэ.
            await AddAccountAsync(
                databaseName,
                "ACC001",
                10000m);

            // Controller болон worker хоёрын хооронд transaction дамжуулах channel үүсгэнэ.
            var channel =
                new TransactionChannelService();

            // TransactionWorker-ийг эхлүүлнэ.
            // Worker ажиллаж байж deposit request database дээр balance нэмнэ.
            var worker =
                await StartWorkerAsync(
                    databaseName,
                    channel);

            // Controller-д хэрэгтэй provider, scope, dbContext-ийг үүсгэнэ.
            using var provider =
                CreateServiceProvider(databaseName);

            using var scope =
                provider.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<BankDbContext>();

            // Test хийх AccountController object үүсгэнэ.
            var controller =
                new AccountController(db, channel);

            // Deposit хийхэд дамжуулах DTO.
            // ACC001 account руу 5000 төгрөг нэмнэ.
            var dto =
                new DepositDto
                {
                    AccountNumber = "ACC001",
                    Amount = 5000m
                };

            // Act
            // Deposit action method-ийг дуудна.
            // Энэ нь transaction request-ийг channel руу оруулна.
            var result =
                await controller.Deposit(dto);

            // Deposit хийгдсэний дараах balance-ийг database-аас дахин уншина.
            var balance =
                await GetBalanceAsync(
                    databaseName,
                    "ACC001");

            // Assert
            // Deposit амжилттай бол controller OkObjectResult буцаах ёстой.
            Assert.IsInstanceOfType(
                result,
                typeof(OkObjectResult));

            // Анхны balance 10000 байсан.
            // Deposit amount 5000 нэмэгдээд нийт 15000 болох ёстой.
            Assert.AreEqual(
                15000m,
                balance);

            // Test дууссаны дараа worker-ийг зогсооно.
            // Ингэхгүй бол background worker үргэлжлээд үлдэх эрсдэлтэй.
            await worker.StopAsync(
                CancellationToken.None);
        }

        [TestMethod]
        public async Task Withdraw_WhenAccountHasEnoughBalance_DecreasesBalance()
        {
            // Arrange
            // Тусдаа InMemory database нэр үүсгэнэ.
            var databaseName =
                Guid.NewGuid().ToString();

            // ACC001 account-ийг 10000 balance-тэйгээр бэлдэнэ.
            await AddAccountAsync(
                databaseName,
                "ACC001",
                10000m);

            // Transaction дамжуулах channel үүсгэнэ.
            var channel =
                new TransactionChannelService();

            // Worker-ийг эхлүүлнэ.
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

            // Controller үүсгэнэ.
            var controller =
                new AccountController(db, channel);

            // Withdraw хийх DTO.
            // ACC001 account-аас 3000 төгрөг хасна.
            var dto =
                new WithdrawDto
                {
                    AccountNumber = "ACC001",
                    Amount = 3000m
                };

            // Act
            // Withdraw action method-ийг дуудна.
            var result =
                await controller.Withdraw(dto);

            // Withdraw-ийн дараах balance-ийг шалгана.
            var balance =
                await GetBalanceAsync(
                    databaseName,
                    "ACC001");

            // Assert
            // Withdraw амжилттай үед OkResult буцаах ёстой.
            Assert.IsInstanceOfType(
                result,
                typeof(OkResult));

            // Анхны balance 10000 байсан.
            // 3000 хасагдаад 7000 үлдэх ёстой.
            Assert.AreEqual(
                7000m,
                balance);

            // Worker-ийг зогсооно.
            await worker.StopAsync(
                CancellationToken.None);
        }

        [TestMethod]
        public async Task Withdraw_WhenBalanceIsNotEnough_ReturnsBadRequest()
        {
            // Arrange
            // Тусдаа InMemory database нэр үүсгэнэ.
            var databaseName =
                Guid.NewGuid().ToString();

            // ACC001 account-ийг 10000 balance-тэйгээр бэлдэнэ.
            await AddAccountAsync(
                databaseName,
                "ACC001",
                10000m);

            // Transaction дамжуулах channel үүсгэнэ.
            var channel =
                new TransactionChannelService();

            // Worker-ийг эхлүүлнэ.
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

            // Controller үүсгэнэ.
            var controller =
                new AccountController(db, channel);

            // Withdraw хийх DTO.
            // 10000 balance-тэй account-аас 15000 авах гэж оролдож байна.
            // Үлдэгдэл хүрэлцэхгүй тул зөвшөөрөх ёсгүй.
            var dto =
                new WithdrawDto
                {
                    AccountNumber = "ACC001",
                    Amount = 15000m
                };

            // Act
            // Withdraw action method-ийг дуудна.
            var result =
                await controller.Withdraw(dto);

            // Withdraw амжилтгүй болсон тул balance өөрчлөгдөөгүй байх ёстой.
            var balance =
                await GetBalanceAsync(
                    databaseName,
                    "ACC001");

            // Assert
            // Үлдэгдэл хүрэлцэхгүй үед BadRequestObjectResult буцаах ёстой.
            Assert.IsInstanceOfType(
                result,
                typeof(BadRequestObjectResult));

            // Balance өмнөх шигээ 10000 хэвээр байх ёстой.
            Assert.AreEqual(
                10000m,
                balance);

            // Worker-ийг зогсооно.
            await worker.StopAsync(
                CancellationToken.None);
        }
    }
}