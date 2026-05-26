using BankDomain.Entities;
using BankInfrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BankServices.Tests
{
    // TransactionWorkerTests класс нь TransactionWorker-ийн ажиллагааг
    // unit test хийх зориулалттай.
    //
    // TransactionWorker гэж юу вэ?
    // TransactionWorker нь TransactionChannelService-ийн queue дотор орсон
    // transaction request-үүдийг уншаад database дээр balance өөрчилдөг background worker.
    //
    // Энэ test class дараах зүйлсийг шалгана:
    // 1. Deposit request ирэхэд account balance нэмэгдэж байна уу?
    // 2. Withdraw request ирэхэд account balance хасагдаж байна уу?
    // 3. Withdraw amount balance-аас их үед false буцааж, balance өөрчлөхгүй байна уу?
    // 4. Account байхгүй үед false буцааж байна уу?
    //
    // Test бүр бодит database ашиглахгүй.
    // Entity Framework Core-ийн InMemoryDatabase ашиглаж түр database үүсгэнэ.
    [TestClass]
    public class TransactionWorkerTests
    {
        // Test-д хэрэглэх ServiceProvider үүсгэдэг helper method.
        //
        // ServiceProvider гэдэг нь dependency injection container юм.
        // Энэ container-оос BankDbContext, IServiceScopeFactory гэх мэт
        // хэрэгтэй object-уудыг авч ашиглана.
        private ServiceProvider CreateServiceProvider(
            string databaseName)
        {
            // ServiceCollection нь DI container-д бүртгэх service-үүдийг хадгална.
            var services =
                new ServiceCollection();

            // BankDbContext-ийг InMemory database ашиглахаар бүртгэнэ.
            //
            // databaseName ижил байвал тухайн нэртэй нэг database-г ашиглана.
            // Test бүр дээр Guid.NewGuid() ашиглаж өөр нэр өгдөг тул
            // test-үүдийн data хоорондоо холилдохгүй.
            services.AddDbContext<BankDbContext>(
                options =>
                    options.UseInMemoryDatabase(databaseName));

            // Бүртгэсэн service-үүдээр ServiceProvider үүсгээд буцаана.
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
            // Өгөгдсөн databaseName-тэй provider үүсгэнэ.
            using var provider =
                CreateServiceProvider(databaseName);

            // BankDbContext нь scoped service тул scope үүсгэж авна.
            using var scope =
                provider.CreateScope();

            // DI container-оос BankDbContext object авна.
            var db =
                scope.ServiceProvider
                    .GetRequiredService<BankDbContext>();

            // BankAccounts table-д шинэ account нэмнэ.
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

        // Database-аас account-ийн одоогийн balance-ийг уншиж авах helper method.
        //
        // Worker request боловсруулсны дараа balance үнэхээр өөрчлөгдсөн эсэхийг
        // шалгахад энэ method-ийг ашиглана.
        private async Task<decimal> GetBalanceAsync(
            string databaseName,
            string accountNumber)
        {
            // Ижил databaseName ашиглаж байгаа тул өмнө нэмсэн account-ийг уншиж чадна.
            using var provider =
                CreateServiceProvider(databaseName);

            using var scope =
                provider.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<BankDbContext>();

            // AccountNumber таарсан account-ийг database-аас олно.
            //
            // FirstAsync ашиглаж байгаа тул account олдохгүй бол exception гарна.
            // Энэ helper-ийг зөвхөн account байгаа test дээр ашиглаж байна.
            var account =
                await db.BankAccounts
                    .FirstAsync(x =>
                        x.AccountNumber == accountNumber);

            // Олдсон account-ийн balance утгыг буцаана.
            return account.Balance;
        }

        [TestMethod]
        public async Task TransactionWorker_WhenDepositRequest_BalanceIncreases()
        {
            // Arrange
            // Test бүр тусдаа InMemory database ашиглахын тулд unique databaseName үүсгэнэ.
            var databaseName =
                Guid.NewGuid().ToString();

            // ACC001 account-ийг 10000 balance-тэйгээр database-д нэмнэ.
            await AddAccountAsync(
                databaseName,
                "ACC001",
                10000m);

            // Worker-т хэрэгтэй ServiceProvider үүсгэнэ.
            var provider =
                CreateServiceProvider(databaseName);

            // Controller/service болон worker хоёрын хооронд transaction request дамжуулах channel.
            var channel =
                new TransactionChannelService();

            // TransactionWorker үүсгэнэ.
            //
            // IServiceScopeFactory:
            // Worker дотор шинэ scope үүсгэж BankDbContext авахад хэрэглэгдэнэ.
            //
            // channel:
            // Worker унших transaction queue-г агуулна.
            var worker =
                new TransactionWorker(
                    provider.GetRequiredService<IServiceScopeFactory>(),
                    channel);

            // CancellationTokenSource нь worker-ийг зогсоох үед token өгөхөд ашиглагдана.
            using var cts =
                new CancellationTokenSource();

            // Worker-ийг эхлүүлнэ.
            // Ингэснээр channel queue рүү орсон transaction request-ийг уншиж боловсруулна.
            await worker.StartAsync(cts.Token);

            // Deposit transaction request бэлдэж байна.
            // ACC001 account руу 5000 нэмэх хүсэлт.
            var request =
                new TransactionRequest
                {
                    AccountNumber = "ACC001",
                    Amount = 5000m,
                    Type = TransactionType.Deposit
                };

            // Act
            // Transaction request-ийг channel-ийн queue рүү бичнэ.
            // Worker энэ request-ийг уншаад balance нэмнэ.
            await channel.Queue.Writer.WriteAsync(request);

            // Worker request-ийг боловсруулж дуусаад CompletionSource-д true/false result тавина.
            // Энд тэр result-ийг хүлээж авна.
            var result =
                await request.CompletionSource.Task;

            // Database-аас шинэ balance-ийг уншина.
            var balance =
                await GetBalanceAsync(
                    databaseName,
                    "ACC001");

            // Assert
            // Deposit амжилттай бол result true байна.
            Assert.IsTrue(result);

            // Анхны balance 10000 байсан.
            // Deposit 5000 нэмэгдээд 15000 болох ёстой.
            Assert.AreEqual(15000m, balance);

            // Test дууссаны дараа worker-ийг зогсооно.
            await worker.StopAsync(cts.Token);
        }

        [TestMethod]
        public async Task TransactionWorker_WhenWithdrawRequest_BalanceDecreases()
        {
            // Arrange
            // Тусдаа databaseName үүсгэнэ.
            var databaseName =
                Guid.NewGuid().ToString();

            // ACC001 account-ийг 10000 balance-тэйгээр бэлдэнэ.
            await AddAccountAsync(
                databaseName,
                "ACC001",
                10000m);

            // Worker-т хэрэгтэй ServiceProvider үүсгэнэ.
            var provider =
                CreateServiceProvider(databaseName);

            // Transaction request дамжуулах channel үүсгэнэ.
            var channel =
                new TransactionChannelService();

            // Worker үүсгэнэ.
            var worker =
                new TransactionWorker(
                    provider.GetRequiredService<IServiceScopeFactory>(),
                    channel);

            // Worker-ийн cancel token бэлдэнэ.
            using var cts =
                new CancellationTokenSource();

            // Worker-ийг эхлүүлнэ.
            await worker.StartAsync(cts.Token);

            // Withdraw transaction request бэлдэнэ.
            // ACC001 account-аас 3000 хасна.
            var request =
                new TransactionRequest
                {
                    AccountNumber = "ACC001",
                    Amount = 3000m,
                    Type = TransactionType.Withdraw
                };

            // Act
            // Request-ийг channel queue рүү бичнэ.
            await channel.Queue.Writer.WriteAsync(request);

            // Worker боловсруулж дуусаад result тавихыг хүлээнэ.
            var result =
                await request.CompletionSource.Task;

            // Withdraw-ийн дараах balance-ийг database-аас уншина.
            var balance =
                await GetBalanceAsync(
                    databaseName,
                    "ACC001");

            // Assert
            // Withdraw амжилттай бол result true байна.
            Assert.IsTrue(result);

            // Анхны balance 10000 байсан.
            // 3000 хасагдаад 7000 үлдэх ёстой.
            Assert.AreEqual(7000m, balance);

            // Worker-ийг зогсооно.
            await worker.StopAsync(cts.Token);
        }

        [TestMethod]
        public async Task TransactionWorker_WhenWithdrawAmountIsGreaterThanBalance_ReturnsFalse()
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

            // Worker-т хэрэгтэй provider үүсгэнэ.
            var provider =
                CreateServiceProvider(databaseName);

            // TransactionChannelService үүсгэнэ.
            var channel =
                new TransactionChannelService();

            // TransactionWorker үүсгэнэ.
            var worker =
                new TransactionWorker(
                    provider.GetRequiredService<IServiceScopeFactory>(),
                    channel);

            using var cts =
                new CancellationTokenSource();

            // Worker-ийг эхлүүлнэ.
            await worker.StartAsync(cts.Token);

            // Balance хүрэлцэхгүй withdraw request бэлдэнэ.
            // Account дээр 10000 байгаа боловч 15000 авах гэж байна.
            var request =
                new TransactionRequest
                {
                    AccountNumber = "ACC001",
                    Amount = 15000m,
                    Type = TransactionType.Withdraw
                };

            // Act
            // Request-ийг channel queue рүү бичнэ.
            await channel.Queue.Writer.WriteAsync(request);

            // Worker боловсруулж дуусаад false result тавих ёстой.
            var result =
                await request.CompletionSource.Task;

            // Balance өөрчлөгдөөгүй эсэхийг шалгахын тулд database-аас дахин уншина.
            var balance =
                await GetBalanceAsync(
                    databaseName,
                    "ACC001");

            // Assert
            // Үлдэгдэл хүрэлцэхгүй тул transaction амжилтгүй буюу false байх ёстой.
            Assert.IsFalse(result);

            // Balance өмнөх шигээ 10000 хэвээр байх ёстой.
            Assert.AreEqual(10000m, balance);

            // Worker-ийг зогсооно.
            await worker.StopAsync(cts.Token);
        }

        [TestMethod]
        public async Task TransactionWorker_WhenAccountDoesNotExist_ReturnsFalse()
        {
            // Arrange
            // Тусдаа InMemory database нэр үүсгэнэ.
            // Энэ database-д ямар ч account нэмэхгүй.
            var databaseName =
                Guid.NewGuid().ToString();

            // Worker-т хэрэгтэй provider үүсгэнэ.
            var provider =
                CreateServiceProvider(databaseName);

            // Transaction channel үүсгэнэ.
            var channel =
                new TransactionChannelService();

            // TransactionWorker үүсгэнэ.
            var worker =
                new TransactionWorker(
                    provider.GetRequiredService<IServiceScopeFactory>(),
                    channel);

            using var cts =
                new CancellationTokenSource();

            // Worker-ийг эхлүүлнэ.
            await worker.StartAsync(cts.Token);

            // Database-д байхгүй UNKNOWN account руу deposit хийх request бэлдэнэ.
            var request =
                new TransactionRequest
                {
                    AccountNumber = "UNKNOWN",
                    Amount = 5000m,
                    Type = TransactionType.Deposit
                };

            // Act
            // Request-ийг channel queue рүү бичнэ.
            await channel.Queue.Writer.WriteAsync(request);

            // Worker account олохгүй учраас false result тавих ёстой.
            var result =
                await request.CompletionSource.Task;

            // Assert
            // Account байхгүй тул transaction амжилтгүй буюу false байх ёстой.
            Assert.IsFalse(result);

            // Worker-ийг зогсооно.
            await worker.StopAsync(cts.Token);
        }
    }
}