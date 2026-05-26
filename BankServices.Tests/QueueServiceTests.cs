using BankInfrastructure.Data;
using BankInfrastructure.Repositories;
using BankServices.Services;
using Microsoft.EntityFrameworkCore;
using BankDomain.Entities;

namespace BankServices.Tests
{
    // QueueServiceTests класс нь QueueService-ийн business logic-ийг
    // unit test хийх зориулалттай.
    //
    // QueueService нь банкны үүдэнд байрлах дугаар олгох системийн
    // үндсэн logic-ийг хариуцна.
    //
    // Энэ test class дараах зүйлсийг шалгана:
    // 1. Database хоосон үед A001 дугаар үүсэж байна уу?
    // 2. A001 байгаа үед дараагийн дугаар A002 болж байна уу?
    // 3. Олон удаа дугаар үүсгэхэд A001, A002, A003 гэх мэт дарааллаар явж байна уу?
    // 4. CallNextAsync() хамгийн эхний дуудагдаагүй дугаарыг буцааж байна уу?
    // 5. Дугаар дуудагдсан үед IsCalled = true болж байна уу?
    // 6. A998-ийн дараа A999 үүсэж байна уу?
    //
    // Test бүр бодит database ашиглахгүй.
    // Entity Framework Core-ийн InMemoryDatabase ашиглаж түр database үүсгэнэ.
    [TestClass]
    public class QueueServiceTests
    {
        // Test бүрт зориулж шинэ InMemory database үүсгэдэг helper method.
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
        public async Task CreateQueueAsync_WhenDatabaseIsEmpty_CreatesA001()
        {
            // Arrange
            // Түр InMemory database үүсгэнэ.
            // Энэ database эхэндээ хоосон байна.
            using var db = CreateDbContext();

            // Repository нь database-тэй шууд харьцдаг давхарга.
            var repository = new QueueRepository(db);

            // Service нь queue үүсгэх, дараагийн дугаар дуудах business logic-ийг хариуцна.
            var service = new QueueService(repository);

            // Act
            // Database хоосон үед шинэ queue дугаар үүсгэнэ.
            var result = await service.CreateQueueAsync();

            // Assert
            // Үүссэн queue null биш байх ёстой.
            Assert.IsNotNull(result);

            // Эхний дугаар заавал A001 байх ёстой.
            Assert.AreEqual("A001", result.Number);

            // Шинээр үүссэн queue дуудагдаагүй төлөвтэй байх ёстой.
            Assert.IsFalse(result.IsCalled);

            // Database-д яг 1 queue record нэмэгдсэн байх ёстой.
            Assert.AreEqual(1, db.CustomerQueues.Count());
        }

        [TestMethod]
        public async Task CreateQueueAsync_WhenA001Exists_CreatesA002()
        {
            // Arrange
            // Түр database үүсгэнэ.
            using var db = CreateDbContext();

            // Repository болон service үүсгэнэ.
            var repository = new QueueRepository(db);
            var service = new QueueService(repository);

            // Act
            // Эхний дуудалтаар A001 үүснэ.
            var firstQueue = await service.CreateQueueAsync();

            // Хоёр дахь дуудалтаар A002 үүсэх ёстой.
            var secondQueue = await service.CreateQueueAsync();

            // Assert
            // Хоёр queue хоёулаа null биш байх ёстой.
            Assert.IsNotNull(firstQueue);
            Assert.IsNotNull(secondQueue);

            // Эхний queue-ийн дугаар A001 байна.
            Assert.AreEqual("A001", firstQueue.Number);

            // Хоёр дахь queue-ийн дугаар A002 байна.
            Assert.AreEqual("A002", secondQueue.Number);

            // Шинээр үүссэн хоёр дахь queue дуудагдаагүй байх ёстой.
            Assert.IsFalse(secondQueue.IsCalled);

            // Database-д нийт 2 queue record байх ёстой.
            Assert.AreEqual(2, db.CustomerQueues.Count());
        }

        [TestMethod]
        public async Task CreateQueueAsync_WhenCalledThreeTimes_CreatesSequentialNumbers()
        {
            // Arrange
            // Түр database үүсгэнэ.
            using var db = CreateDbContext();

            // QueueRepository болон QueueService үүсгэнэ.
            var repository = new QueueRepository(db);
            var service = new QueueService(repository);

            // Act
            // CreateQueueAsync()-ийг 3 удаа дараалан дуудна.
            var queue1 = await service.CreateQueueAsync();
            var queue2 = await service.CreateQueueAsync();
            var queue3 = await service.CreateQueueAsync();

            // Assert
            // Дугаарууд дарааллаараа үүссэн эсэхийг шалгана.
            Assert.AreEqual("A001", queue1.Number);
            Assert.AreEqual("A002", queue2.Number);
            Assert.AreEqual("A003", queue3.Number);

            // Database-д нийт 3 queue record хадгалагдсан байх ёстой.
            Assert.AreEqual(3, db.CustomerQueues.Count());
        }

        [TestMethod]
        public async Task CallNextAsync_WhenQueuesExist_ReturnsFirstUncalledQueue()
        {
            // Arrange
            // Түр database үүсгэнэ.
            using var db = CreateDbContext();

            // Repository болон service үүсгэнэ.
            var repository = new QueueRepository(db);
            var service = new QueueService(repository);

            // Database-д A001, A002, A003 гэсэн 3 дуудагдаагүй дугаар үүсгэнэ.
            await service.CreateQueueAsync(); // A001
            await service.CreateQueueAsync(); // A002
            await service.CreateQueueAsync(); // A003

            // Act
            // Дараагийн дуудагдаагүй queue-г дуудна.
            var result = await service.CallNextAsync();

            // Assert
            // Дуудагдах queue null биш байх ёстой.
            Assert.IsNotNull(result);

            // Хамгийн эхний дуудагдаагүй queue болох A001 буцах ёстой.
            Assert.AreEqual("A001", result.Number);
        }

        [TestMethod]
        public async Task CallNextAsync_WhenQueueIsCalled_MarksQueueAsCalled()
        {
            // Arrange
            // Түр database үүсгэнэ.
            using var db = CreateDbContext();

            // Repository болон service үүсгэнэ.
            var repository = new QueueRepository(db);
            var service = new QueueService(repository);

            // Database-д A001 гэсэн нэг дуудагдаагүй queue үүсгэнэ.
            await service.CreateQueueAsync(); // A001

            // Act
            // A001 дугаарыг дуудаж байна.
            var result = await service.CallNextAsync();

            // Assert
            // Дуудагдсан queue null биш байна.
            Assert.IsNotNull(result);

            // Дуудагдсан queue-ийн дугаар A001 байна.
            Assert.AreEqual("A001", result.Number);

            // CallNextAsync() дуудагдсаны дараа IsCalled = true болсон байх ёстой.
            Assert.IsTrue(result.IsCalled);
        }

        [TestMethod]
        public async Task CreateQueueAsync_WhenLastNumberIsA999_CreatesA001Again()
        {
            // Arrange
            // Түр database үүсгэнэ.
            using var db = CreateDbContext();

            // Database-д A999 гэсэн хамгийн сүүлийн дугаар урьдчилж нэмнэ.
            //
            // Энэ test-ийн зорилго:
            // Хэрвээ хамгийн сүүлийн дугаар A999 болсон бол дараагийн дугаар
            // дахин A001 болж reset хийх ёстой эсэхийг шалгах.
            //
            db.CustomerQueues.Add(
                new CustomerQueue
                {
                    Number = "A999",
                    IsCalled = false,
                    CreatedAt = DateTime.Now
                });

            // A999 queue-г database-д хадгална.
            await db.SaveChangesAsync();

            // Repository болон service үүсгэнэ.
            var repository = new QueueRepository(db);
            var service = new QueueService(repository);

            // Act
            // A999-ийн дараах дугаар үүсгэхийг оролдоно.
            var result = await service.CreateQueueAsync();

            // Assert
            // Result null биш байх ёстой.
            Assert.IsNotNull(result);

            // Хэрвээ reset logic хэрэгжсэн бол дараагийн дугаар A001 болох ёстой.
            Assert.AreEqual("A001", result.Number);

            // Шинэ дугаар дуудагдаагүй байх ёстой.
            Assert.IsFalse(result.IsCalled);
        }

        [TestMethod]
        public async Task CreateQueueAsync_WhenLastNumberIsA099_CreatesA100()
        {
            using var db = CreateDbContext();

            db.CustomerQueues.Add(
                new CustomerQueue
                {
                    Number = "A099",
                    IsCalled = false,
                    CreatedAt = DateTime.Now
                });

            await db.SaveChangesAsync();

            var repository = new QueueRepository(db);
            var service = new QueueService(repository);

            var result = await service.CreateQueueAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual("A100", result.Number);
            Assert.IsFalse(result.IsCalled);
        }

        [TestMethod]
        public async Task CallNextAsync_WhenNoUncalledQueueExists_ReturnsNull()
        {
            using var db = CreateDbContext();

            db.CustomerQueues.Add(
                new CustomerQueue
                {
                    Number = "A001",
                    IsCalled = true,
                    CreatedAt = DateTime.Now
                });

            await db.SaveChangesAsync();

            var repository = new QueueRepository(db);
            var service = new QueueService(repository);

            var result = await service.CallNextAsync();

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task CreateQueueAsync_WhenLastNumberIsA998_CreatesA999()
        {
            // Arrange
            // Түр database үүсгэнэ.
            using var db = CreateDbContext();

            // Database-д A998 гэсэн дугаар урьдчилж нэмнэ.
            // Энэ нь дараагийн дугаар A999 болж өсөх эсэхийг шалгах нөхцөл.
            db.CustomerQueues.Add(
                new CustomerQueue
                {
                    Number = "A998",
                    IsCalled = false,
                    CreatedAt = DateTime.Now
                });

            // A998 queue-г database-д хадгална.
            await db.SaveChangesAsync();

            // Repository болон service үүсгэнэ.
            var repository = new QueueRepository(db);
            var service = new QueueService(repository);

            // Act
            // A998-ийн дараагийн queue дугаарыг үүсгэнэ.
            var result = await service.CreateQueueAsync();

            // Assert
            // Result null биш байх ёстой.
            Assert.IsNotNull(result);

            // A998-ийн дараа A999 үүсэх ёстой.
            Assert.AreEqual("A999", result.Number);

            // Шинээр үүссэн A999 дуудагдаагүй төлөвтэй байх ёстой.
            Assert.IsFalse(result.IsCalled);
        }
    }
}
