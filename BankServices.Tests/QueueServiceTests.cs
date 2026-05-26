using BankInfrastructure.Data;
using BankInfrastructure.Repositories;
using BankServices.Services;
using Microsoft.EntityFrameworkCore;
using BankDomain.Entities;

namespace BankServices.Tests
{
    [TestClass]
    public class QueueServiceTests
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
        public async Task CreateQueueAsync_WhenDatabaseIsEmpty_CreatesA001()
        {
            // Arrange
            using var db = CreateDbContext();

            var repository = new QueueRepository(db);
            var service = new QueueService(repository);

            // Act
            var result = await service.CreateQueueAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("A001", result.Number);
            Assert.IsFalse(result.IsCalled);
            Assert.AreEqual(1, db.CustomerQueues.Count());
        }

        [TestMethod]
        public async Task CreateQueueAsync_WhenA001Exists_CreatesA002()
        {
            // Arrange
            using var db = CreateDbContext();

            var repository = new QueueRepository(db);
            var service = new QueueService(repository);

            // Act
            var firstQueue = await service.CreateQueueAsync();
            var secondQueue = await service.CreateQueueAsync();

            // Assert
            Assert.IsNotNull(firstQueue);
            Assert.IsNotNull(secondQueue);

            Assert.AreEqual("A001", firstQueue.Number);
            Assert.AreEqual("A002", secondQueue.Number);

            Assert.IsFalse(secondQueue.IsCalled);
            Assert.AreEqual(2, db.CustomerQueues.Count());
        }

        [TestMethod]
        public async Task CreateQueueAsync_WhenCalledThreeTimes_CreatesSequentialNumbers()
        {
            // Arrange
            using var db = CreateDbContext();

            var repository = new QueueRepository(db);
            var service = new QueueService(repository);

            // Act
            var queue1 = await service.CreateQueueAsync();
            var queue2 = await service.CreateQueueAsync();
            var queue3 = await service.CreateQueueAsync();

            // Assert
            Assert.AreEqual("A001", queue1.Number);
            Assert.AreEqual("A002", queue2.Number);
            Assert.AreEqual("A003", queue3.Number);

            Assert.AreEqual(3, db.CustomerQueues.Count());
        }

        [TestMethod]
        public async Task CallNextAsync_WhenQueuesExist_ReturnsFirstUncalledQueue()
        {
            // Arrange
            using var db = CreateDbContext();

            var repository = new QueueRepository(db);
            var service = new QueueService(repository);

            await service.CreateQueueAsync(); // A001
            await service.CreateQueueAsync(); // A002
            await service.CreateQueueAsync(); // A003

            // Act
            var result = await service.CallNextAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("A001", result.Number);
        }

        [TestMethod]
        public async Task CallNextAsync_WhenQueueIsCalled_MarksQueueAsCalled()
        {
            // Arrange
            using var db = CreateDbContext();

            var repository = new QueueRepository(db);
            var service = new QueueService(repository);

            await service.CreateQueueAsync(); // A001

            // Act
            var result = await service.CallNextAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("A001", result.Number);
            Assert.IsTrue(result.IsCalled);
        }

        [TestMethod]
        [Ignore]
        public async Task CreateQueueAsync_WhenLastNumberIsA999_CreatesA001Again()
        {
            // Arrange
            using var db = CreateDbContext();

            db.CustomerQueues.Add(
                new CustomerQueue
                {
                    Number = "A999",
                    IsCalled = false,
                    CreatedAt = DateTime.Now
                });

            await db.SaveChangesAsync();

            var repository = new QueueRepository(db);
            var service = new QueueService(repository);

            // Act
            var result = await service.CreateQueueAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("A001", result.Number);
            Assert.IsFalse(result.IsCalled);
        }

        [TestMethod]
        public async Task CreateQueueAsync_WhenLastNumberIsA998_CreatesA999()
        {
            // Arrange
            using var db = CreateDbContext();

            db.CustomerQueues.Add(
                new CustomerQueue
                {
                    Number = "A998",
                    IsCalled = false,
                    CreatedAt = DateTime.Now
                });

            await db.SaveChangesAsync();

            var repository = new QueueRepository(db);
            var service = new QueueService(repository);

            // Act
            var result = await service.CreateQueueAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("A999", result.Number);
            Assert.IsFalse(result.IsCalled);
        }
    }
}