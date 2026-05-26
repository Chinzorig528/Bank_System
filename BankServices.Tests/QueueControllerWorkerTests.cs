using BankApi.Channels;
using BankApi.Controllers;
using BankDomain.Entities;
using BankInfrastructure.Data;
using BankInfrastructure.Interfaces;
using BankInfrastructure.Repositories;
using BankServices.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BankServices.Tests
{
    // QueueControllerWorkerTests класс нь QueueController болон QueueWorker-ийн
    // хамтын ажиллагааг test хийх зориулалттай.
    //
    // Энэ test class дараах зүйлсийг шалгана:
    // 1. Дараалалд дуудагдаагүй дугаарууд байвал хамгийн эхнийхийг дуудаж байна уу?
    // 2. Эхний дугаар аль хэдийн дуудагдсан бол дараагийн дуудагдаагүй дугаарыг сонгож байна уу?
    // 3. Бүх дугаар дуудагдсан үед NotFound буцааж байна уу?
    // 4. Дугаар дуудагдсаны дараа database дээр IsCalled = true болж хадгалагдаж байна уу?
    //
    // Онцлог:
    // QueueController нь өөрөө шууд queue-г өөрчлөхөөс гадна QueueChannelService ашиглаж,
    // QueueWorker-той хамтарч ажилладаг.
    //
    // Test дээр бодит database ашиглахгүй.
    // Entity Framework Core-ийн InMemoryDatabase ашиглаж түр database үүсгэнэ.
    [TestClass]
    public class QueueControllerWorkerTests
    {
        // Test-д хэрэглэх ServiceProvider үүсгэдэг helper method.
        //
        // ServiceProvider гэдэг нь dependency injection container юм.
        // Өөрөөр хэлбэл хэрэгтэй service/class-уудыг бүртгээд,
        // дараа нь автоматаар үүсгэж авах боломж олгодог.
        private ServiceProvider CreateServiceProvider(string databaseName)
        {
            // DI container-д бүртгэх service-үүдийг хадгалах collection.
            var services = new ServiceCollection();

            // BankDbContext-ийг InMemory database ашиглахаар бүртгэнэ.
            // databaseName ижил байвал нэг InMemory database-г хуваалцана.
            services.AddDbContext<BankDbContext>(
                options =>
                    options.UseInMemoryDatabase(databaseName));

            // QueueWorker дотор IQueueRepository хэрэгтэй учраас заавал register хийнэ.
            // Interface болох IQueueRepository дуудагдах үед QueueRepository object үүснэ.
            services.AddScoped<IQueueRepository, QueueRepository>();

            // Бүртгэсэн service-үүдээр ServiceProvider үүсгэж буцаана.
            return services.BuildServiceProvider();
        }

        // Test эхлэхээс өмнө database-д queue дугаар урьдчилж нэмэх helper method.
        //
        // Жишээ:
        // AddQueueAsync(databaseName, "A001", false)
        // гэж дуудвал A001 дугаар дуудагдаагүй төлөвтэйгээр database-д нэмэгдэнэ.
        private async Task AddQueueAsync(
            string databaseName,
            string number,
            bool isCalled)
        {
            // Өгөгдсөн databaseName-тэй provider үүсгэнэ.
            using var provider = CreateServiceProvider(databaseName);

            // Scoped service болох BankDbContext авахын тулд scope үүсгэнэ.
            using var scope = provider.CreateScope();

            // DI container-оос BankDbContext object авна.
            var db =
                scope.ServiceProvider
                    .GetRequiredService<BankDbContext>();

            // CustomerQueues table-д шинэ queue record нэмнэ.
            db.CustomerQueues.Add(
                new CustomerQueue
                {
                    Number = number,
                    IsCalled = isCalled,
                    CreatedAt = DateTime.Now
                });

            // Өөрчлөлтийг InMemory database-д хадгална.
            await db.SaveChangesAsync();
        }

        // Database-аас тодорхой дугаартай queue-г хайж буцаах helper method.
        //
        // Энэ method-ийг дугаар дуудагдсаны дараа IsCalled database дээр
        // үнэхээр true болсон эсэхийг шалгахад ашиглана.
        private async Task<CustomerQueue?> GetQueueAsync(
            string databaseName,
            string number)
        {
            // Ижил databaseName ашиглаж байгаа тул өмнөх test data-г уншина.
            using var provider = CreateServiceProvider(databaseName);
            using var scope = provider.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<BankDbContext>();

            // CustomerQueues table дотроос number таарсан queue-г хайна.
            return await db.CustomerQueues
                .FirstOrDefaultAsync(x => x.Number == number);
        }

        // QueueWorker-ийг test дээр эхлүүлэх helper method.
        //
        // Яагаад QueueWorker хэрэгтэй вэ?
        // Controller queue call хийх request-ийг QueueChannelService рүү оруулдаг.
        // Worker тэр channel-аас request уншиж дарааллын дугаарыг боловсруулдаг.
        private async Task<QueueWorker> StartWorkerAsync(
            string databaseName,
            QueueChannelService channel)
        {
            // Worker database-тэй ажиллахын тулд ServiceProvider хэрэгтэй.
            var provider = CreateServiceProvider(databaseName);

            // QueueWorker object үүсгэнэ.
            //
            // channel:
            // Controller болон worker хоёрын хооронд мэдээлэл дамжуулах суваг.
            //
            // IServiceScopeFactory:
            // Worker дотор scope үүсгэж BankDbContext, repository авахад ашиглагдана.
            var worker =
                new QueueWorker(
                    channel,
                    provider.GetRequiredService<IServiceScopeFactory>());

            // Worker-ийг эхлүүлнэ.
            // Ингэснээр channel рүү орсон queue request-үүдийг боловсруулах боломжтой болно.
            await worker.StartAsync(CancellationToken.None);

            // Test дуусахад StopAsync хийхийн тулд worker object-ийг буцаана.
            return worker;
        }

        // QueueController үүсгэдэг helper method.
        //
        // QueueController нь QueueService болон QueueChannelService авдаг.
        // QueueService нь QueueRepository ашиглаж database-тэй ажилладаг.
        private QueueController CreateController(
            BankDbContext db,
            QueueChannelService channel)
        {
            // Repository нь database-тэй шууд ажиллах давхарга.
            var repository =
                new QueueRepository(db);

            // Service нь business logic буюу queue-ийн үндсэн ажиллагааг хариуцна.
            var service =
                new QueueService(repository);

            // Controller нь API endpoint-ийн үүрэгтэй.
            return new QueueController(service, channel);
        }

        // Controller.Next() method гацах, хэт удах эрсдэлээс хамгаалах helper method.
        //
        // WaitAsync(TimeSpan.FromSeconds(3)) гэдэг нь:
        // task 3 секундийн дотор дуусахгүй бол timeout болгож exception гаргана.
        //
        // Ингэснээр worker/channel-ийн алдаанаас болж test хязгааргүй удаан гацахаас сэргийлнэ.
        private static async Task<IActionResult> RunWithTimeout(
            Task<IActionResult> task)
        {
            return await task.WaitAsync(
                TimeSpan.FromSeconds(3));
        }

        [TestMethod]
        public async Task Next_WhenQueuesExist_ReturnsFirstUncalledQueue()
        {
            // Arrange
            // Test бүр тусдаа InMemory database ашиглахын тулд unique databaseName үүсгэнэ.
            var databaseName = Guid.NewGuid().ToString();

            // Дараалалд 3 дуудагдаагүй дугаар нэмнэ.
            // Бүгдийн IsCalled = false.
            await AddQueueAsync(databaseName, "A001", false);
            await AddQueueAsync(databaseName, "A002", false);
            await AddQueueAsync(databaseName, "A003", false);

            // Controller болон worker хоёрын хооронд queue request дамжуулах channel.
            var channel = new QueueChannelService();

            // QueueWorker-ийг эхлүүлнэ.
            // Worker ажиллаж байж channel-аар ирсэн request-ийг боловсруулна.
            var worker =
                await StartWorkerAsync(databaseName, channel);

            // Controller-д хэрэгтэй db context үүсгэнэ.
            using var provider = CreateServiceProvider(databaseName);
            using var scope = provider.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<BankDbContext>();

            // Test хийх QueueController object үүсгэнэ.
            var controller =
                CreateController(db, channel);

            // Act
            // Next() method дараагийн дуудагдаагүй дугаарыг дуудах ёстой.
            // RunWithTimeout ашиглаж test гацахаас хамгаална.
            var result =
                await RunWithTimeout(controller.Next());

            // Assert
            // Дараагийн queue олдсон тул OkObjectResult буцаах ёстой.
            Assert.IsInstanceOfType(
                result,
                typeof(OkObjectResult));

            // OkObjectResult дотор CustomerQueue object ирэх ёстой.
            var okResult =
                result as OkObjectResult;

            var queue =
                okResult!.Value as CustomerQueue;

            // Queue null биш эсэхийг шалгана.
            Assert.IsNotNull(queue);

            // Бүх queue дуудагдаагүй байсан тул хамгийн эхний A001 сонгогдох ёстой.
            Assert.AreEqual("A001", queue.Number);

            // Дуудагдсан учраас IsCalled true болсон байх ёстой.
            Assert.IsTrue(queue.IsCalled);

            // Test дууссаны дараа worker-ийг зогсооно.
            await worker.StopAsync(CancellationToken.None);
        }

        [TestMethod]
        public async Task Next_WhenFirstQueueAlreadyCalled_ReturnsNextUncalledQueue()
        {
            // Arrange
            // Test-д зориулсан unique databaseName үүсгэнэ.
            var databaseName = Guid.NewGuid().ToString();

            // A001 аль хэдийн дуудагдсан.
            // A002 болон A003 дуудагдаагүй байна.
            await AddQueueAsync(databaseName, "A001", true);
            await AddQueueAsync(databaseName, "A002", false);
            await AddQueueAsync(databaseName, "A003", false);

            // Queue request дамжуулах channel үүсгэнэ.
            var channel = new QueueChannelService();

            // Worker-ийг эхлүүлнэ.
            var worker =
                await StartWorkerAsync(databaseName, channel);

            using var provider = CreateServiceProvider(databaseName);
            using var scope = provider.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<BankDbContext>();

            // QueueController үүсгэнэ.
            var controller =
                CreateController(db, channel);

            // Act
            // Дараагийн дуудагдаагүй queue-г авах ёстой.
            var result =
                await RunWithTimeout(controller.Next());

            // Assert
            // Queue олдсон тул OkObjectResult буцаах ёстой.
            Assert.IsInstanceOfType(
                result,
                typeof(OkObjectResult));

            var okResult =
                result as OkObjectResult;

            var queue =
                okResult!.Value as CustomerQueue;

            // Queue null биш байна.
            Assert.IsNotNull(queue);

            // A001 дуудагдсан тул дараагийн дуудагдаагүй A002 сонгогдох ёстой.
            Assert.AreEqual("A002", queue.Number);

            // A002 одоо дуудагдсан төлөвтэй болсон байх ёстой.
            Assert.IsTrue(queue.IsCalled);

            // Worker-ийг зогсооно.
            await worker.StopAsync(CancellationToken.None);
        }

        [TestMethod]
        public async Task Next_WhenNoUncalledQueueExists_ReturnsNotFound()
        {
            // Arrange
            // Unique databaseName үүсгэнэ.
            var databaseName = Guid.NewGuid().ToString();

            // Бүх queue аль хэдийн дуудагдсан төлөвтэй байна.
            await AddQueueAsync(databaseName, "A001", true);
            await AddQueueAsync(databaseName, "A002", true);

            // Queue channel үүсгэнэ.
            var channel = new QueueChannelService();

            // Worker-ийг эхлүүлнэ.
            var worker =
                await StartWorkerAsync(databaseName, channel);

            using var provider = CreateServiceProvider(databaseName);
            using var scope = provider.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<BankDbContext>();

            // Controller үүсгэнэ.
            var controller =
                CreateController(db, channel);

            // Act
            // Дуудагдаагүй queue байхгүй тул Next() нь NotFound буцаах ёстой.
            var result =
                await RunWithTimeout(controller.Next());

            // Assert
            // Дараагийн дугаар байхгүй үед NotFoundResult буцаах ёстой.
            Assert.IsInstanceOfType(
                result,
                typeof(NotFoundResult));

            // Worker-ийг зогсооно.
            await worker.StopAsync(CancellationToken.None);
        }

        [TestMethod]
        public async Task Next_WhenQueueIsCalled_MarksQueueAsCalledInDatabase()
        {
            // Arrange
            // Unique databaseName үүсгэнэ.
            var databaseName = Guid.NewGuid().ToString();

            // A001 дугаарыг дуудагдаагүй төлөвтэйгээр нэмнэ.
            await AddQueueAsync(databaseName, "A001", false);

            // Queue channel үүсгэнэ.
            var channel = new QueueChannelService();

            // Worker-ийг эхлүүлнэ.
            var worker =
                await StartWorkerAsync(databaseName, channel);

            using var provider = CreateServiceProvider(databaseName);
            using var scope = provider.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<BankDbContext>();

            // Controller үүсгэнэ.
            var controller =
                CreateController(db, channel);

            // Act
            // Next() дуудахад A001 дугаар дуудагдах ёстой.
            var result =
                await RunWithTimeout(controller.Next());

            // Database-аас A001 queue-г дахин уншиж,
            // IsCalled үнэхээр true болсон эсэхийг шалгана.
            var savedQueue =
                await GetQueueAsync(databaseName, "A001");

            // Assert
            // Дугаар амжилттай дуудагдсан тул OkObjectResult буцаах ёстой.
            Assert.IsInstanceOfType(
                result,
                typeof(OkObjectResult));

            // Database-аас queue олдсон байх ёстой.
            Assert.IsNotNull(savedQueue);

            // Queue database дээр IsCalled = true болж хадгалагдсан байх ёстой.
            Assert.IsTrue(savedQueue.IsCalled);

            // Worker-ийг зогсооно.
            await worker.StopAsync(CancellationToken.None);
        }
    }
}