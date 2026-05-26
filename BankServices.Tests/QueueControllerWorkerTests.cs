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
    [TestClass]
    public class QueueControllerWorkerTests
    {
        private ServiceProvider CreateServiceProvider(string databaseName)
        {
            var services = new ServiceCollection();

            services.AddDbContext<BankDbContext>(
                options =>
                    options.UseInMemoryDatabase(databaseName));

            // QueueWorker дотор IQueueRepository хэрэгтэй учраас заавал register хийнэ.
            services.AddScoped<IQueueRepository, QueueRepository>();

            return services.BuildServiceProvider();
        }

        private async Task AddQueueAsync(
            string databaseName,
            string number,
            bool isCalled)
        {
            using var provider = CreateServiceProvider(databaseName);
            using var scope = provider.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<BankDbContext>();

            db.CustomerQueues.Add(
                new CustomerQueue
                {
                    Number = number,
                    IsCalled = isCalled,
                    CreatedAt = DateTime.Now
                });

            await db.SaveChangesAsync();
        }

        private async Task<CustomerQueue?> GetQueueAsync(
            string databaseName,
            string number)
        {
            using var provider = CreateServiceProvider(databaseName);
            using var scope = provider.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<BankDbContext>();

            return await db.CustomerQueues
                .FirstOrDefaultAsync(x => x.Number == number);
        }

        private async Task<QueueWorker> StartWorkerAsync(
            string databaseName,
            QueueChannelService channel)
        {
            var provider = CreateServiceProvider(databaseName);

            var worker =
                new QueueWorker(
                    channel,
                    provider.GetRequiredService<IServiceScopeFactory>());

            await worker.StartAsync(CancellationToken.None);

            return worker;
        }

        private QueueController CreateController(
            BankDbContext db,
            QueueChannelService channel)
        {
            var repository =
                new QueueRepository(db);

            var service =
                new QueueService(repository);

            return new QueueController(service, channel);
        }

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
            var databaseName = Guid.NewGuid().ToString();

            await AddQueueAsync(databaseName, "A001", false);
            await AddQueueAsync(databaseName, "A002", false);
            await AddQueueAsync(databaseName, "A003", false);

            var channel = new QueueChannelService();

            var worker =
                await StartWorkerAsync(databaseName, channel);

            using var provider = CreateServiceProvider(databaseName);
            using var scope = provider.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<BankDbContext>();

            var controller =
                CreateController(db, channel);

            // Act
            var result =
                await RunWithTimeout(controller.Next());

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(OkObjectResult));

            var okResult =
                result as OkObjectResult;

            var queue =
                okResult!.Value as CustomerQueue;

            Assert.IsNotNull(queue);
            Assert.AreEqual("A001", queue.Number);
            Assert.IsTrue(queue.IsCalled);

            await worker.StopAsync(CancellationToken.None);
        }

        [TestMethod]
        public async Task Next_WhenFirstQueueAlreadyCalled_ReturnsNextUncalledQueue()
        {
            // Arrange
            var databaseName = Guid.NewGuid().ToString();

            await AddQueueAsync(databaseName, "A001", true);
            await AddQueueAsync(databaseName, "A002", false);
            await AddQueueAsync(databaseName, "A003", false);

            var channel = new QueueChannelService();

            var worker =
                await StartWorkerAsync(databaseName, channel);

            using var provider = CreateServiceProvider(databaseName);
            using var scope = provider.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<BankDbContext>();

            var controller =
                CreateController(db, channel);

            // Act
            var result =
                await RunWithTimeout(controller.Next());

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(OkObjectResult));

            var okResult =
                result as OkObjectResult;

            var queue =
                okResult!.Value as CustomerQueue;

            Assert.IsNotNull(queue);
            Assert.AreEqual("A002", queue.Number);
            Assert.IsTrue(queue.IsCalled);

            await worker.StopAsync(CancellationToken.None);
        }

        [TestMethod]
        public async Task Next_WhenNoUncalledQueueExists_ReturnsNotFound()
        {
            // Arrange
            var databaseName = Guid.NewGuid().ToString();

            await AddQueueAsync(databaseName, "A001", true);
            await AddQueueAsync(databaseName, "A002", true);

            var channel = new QueueChannelService();

            var worker =
                await StartWorkerAsync(databaseName, channel);

            using var provider = CreateServiceProvider(databaseName);
            using var scope = provider.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<BankDbContext>();

            var controller =
                CreateController(db, channel);

            // Act
            var result =
                await RunWithTimeout(controller.Next());

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(NotFoundResult));

            await worker.StopAsync(CancellationToken.None);
        }

        [TestMethod]
        public async Task Next_WhenQueueIsCalled_MarksQueueAsCalledInDatabase()
        {
            // Arrange
            var databaseName = Guid.NewGuid().ToString();

            await AddQueueAsync(databaseName, "A001", false);

            var channel = new QueueChannelService();

            var worker =
                await StartWorkerAsync(databaseName, channel);

            using var provider = CreateServiceProvider(databaseName);
            using var scope = provider.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<BankDbContext>();

            var controller =
                CreateController(db, channel);

            // Act
            var result =
                await RunWithTimeout(controller.Next());

            var savedQueue =
                await GetQueueAsync(databaseName, "A001");

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(OkObjectResult));

            Assert.IsNotNull(savedQueue);
            Assert.IsTrue(savedQueue.IsCalled);

            await worker.StopAsync(CancellationToken.None);
        }
    }
}