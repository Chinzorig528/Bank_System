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
    // CurrencyControllerTests класс нь CurrencyController-ийн үйлдлүүдийг
    // unit test хийх зориулалттай.
    //
    // Энэ тестүүд бодит database ашиглахгүй.
    // Харин Entity Framework Core-ийн InMemoryDatabase ашиглаж,
    // test бүр дээр түр database үүсгэн ажиллуулна.
    //
    // Мөн CurrencyController нь SignalR-ийн CurrencyHub ашигладаг.
    // SignalR нь бодит үед frontend дэлгэц рүү ханш өөрчлөгдсөнийг шууд дамжуулахад хэрэглэгдэнэ.
    // Харин unit test дээр бодит hub ажиллуулах шаардлагагүй тул Moq ашиглаж fake hub үүсгэсэн.
    //
    // Энэ test class дараах зүйлсийг шалгана:
    // 1. Currency table хоосон үед default ханшууд үүсэж байна уу?
    // 2. Currency аль хэдийн байвал дахин duplicate үүсгэхгүй байна уу?
    // 3. GetAll хийхэд ханшууд Code-оор эрэмбэлэгдэж ирж байна уу?
    // 4. Нэг currency rate update хийхэд database дээр зөв өөрчлөгдөж байна уу?
    // 5. Байхгүй currency update хийхэд NotFound буцааж байна уу?
    // 6. Хоосон list update хийхэд BadRequest буцааж байна уу?
    // 7. Олон currency rate зэрэг update хийхэд бүгд зөв хадгалагдаж байна уу?
    [TestClass]
    public class CurrencyControllerTests
    {
        // Test бүрт зориулж шинэ InMemory database үүсгэдэг helper method.
        //
        // Guid.NewGuid().ToString() ашиглаж байгаа шалтгаан:
        // Test бүр тусдаа database нэртэй болно.
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

        // CurrencyController үүсгэдэг helper method.
        //
        // CurrencyController нь constructor дээр:
        // 1. BankDbContext
        // 2. IHubContext<CurrencyHub>
        // авдаг.
        //
        // BankDbContext-ийг test дээр бодитоор InMemory database-аас өгнө.
        // Харин IHubContext<CurrencyHub>-ийг Moq ашиглаж fake болгож өгнө.
        private CurrencyController CreateController(
            BankDbContext db)
        {
            return CreateControllerWithClientProxy(db).Controller;
        }

        private (
            CurrencyController Controller,
            Mock<IClientProxy> ClientProxyMock) CreateControllerWithClientProxy(
                BankDbContext db)
        {
            // CurrencyHub-ийн fake mock object үүсгэнэ.
            // Энэ нь бодит SignalR hub биш, test-д зориулсан дуураймал object.
            var hubMock =
                new Mock<IHubContext<CurrencyHub>>();

            // SignalR-ийн Clients хэсгийг fake болгоно.
            // Clients гэдэг нь connected байгаа бүх frontend/client-үүд рүү message явуулахад ашиглагдана.
            var clientsMock =
                new Mock<IHubClients>();

            // ClientProxy нь SignalR-ээр message илгээх object.
            // Жишээ нь Clients.All.SendAsync(...) дуудагдах үед энэ proxy ашиглагдана.
            var clientProxyMock =
                new Mock<IClientProxy>();

            // SendCoreAsync дуудагдах үед ямар ч алдаа гаргахгүй,
            // шууд Task.CompletedTask буцаахаар тохируулж байна.
            //
            // Энэ нь "SignalR message амжилттай илгээгдсэн" мэт нөхцөл үүсгэнэ.
            clientProxyMock
                .Setup(x =>
                    x.SendCoreAsync(
                        It.IsAny<string>(),
                        It.IsAny<object?[]>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // clientsMock.All дуудагдахад clientProxyMock.Object буцаана.
            // Өөрөөр хэлбэл Clients.All.SendAsync(...) ажиллах боломжтой болно.
            clientsMock
                .Setup(x => x.All)
                .Returns(clientProxyMock.Object);

            // hubMock.Clients дуудагдахад clientsMock.Object буцаана.
            hubMock
                .Setup(x => x.Clients)
                .Returns(clientsMock.Object);

            // Бэлдсэн InMemory database болон fake hub-тай controller үүсгээд буцаана.
            return (
                new CurrencyController(
                    db,
                    hubMock.Object),
                clientProxyMock);
        }

        [TestMethod]
        public async Task Seed_WhenCurrencyTableIsEmpty_CreatesDefaultRates()
        {
            // Arrange
            // Түр InMemory database үүсгэнэ.
            // Энэ database эхэндээ хоосон байна.
            using var db =
                CreateDbContext();

            // Fake SignalR hub-тэй CurrencyController үүсгэнэ.
            var controller =
                CreateController(db);

            // Act
            // Seed method-ийг дуудна.
            // CurrencyRates table хоосон бол default ханшууд нэмэгдэх ёстой.
            var result =
                await controller.Seed();

            // Assert
            // Seed амжилттай бол OkObjectResult буцаах ёстой.
            Assert.IsInstanceOfType(
                result,
                typeof(OkObjectResult));

            // Database-аас бүх валютын ханшийг Code-оор эрэмбэлж авна.
            var rates =
                await db.CurrencyRates
                    .OrderBy(x => x.Code)
                    .ToListAsync();

            // Default байдлаар 5 валют нэмэгдсэн байх ёстой.
            Assert.AreEqual(
                5,
                rates.Count);

            // USD валют нэмэгдсэн эсэхийг шалгана.
            Assert.IsTrue(
                rates.Any(x => x.Code == "USD"));

            // EUR валют нэмэгдсэн эсэхийг шалгана.
            Assert.IsTrue(
                rates.Any(x => x.Code == "EUR"));

            // CNY валют нэмэгдсэн эсэхийг шалгана.
            Assert.IsTrue(
                rates.Any(x => x.Code == "CNY"));

            // JPY валют нэмэгдсэн эсэхийг шалгана.
            Assert.IsTrue(
                rates.Any(x => x.Code == "JPY"));

            // KRW валют нэмэгдсэн эсэхийг шалгана.
            Assert.IsTrue(
                rates.Any(x => x.Code == "KRW"));
        }

        [TestMethod]
        public async Task Seed_WhenCurrencyAlreadyExists_DoesNotCreateDuplicates()
        {
            // Arrange
            // Түр database үүсгэнэ.
            using var db =
                CreateDbContext();

            // Database-д USD ханшийг урьдчилж нэмнэ.
            // Энэ нь "currency table хоосон биш" гэсэн нөхцөл үүсгэж байна.
            db.CurrencyRates.Add(
                new CurrencyRate
                {
                    Code = "USD",
                    Name = "Dollar",
                    BuyRate = 3450,
                    SellRate = 3470,
                    UpdatedAt = DateTime.Now
                });

            // Урьдчилж нэмсэн USD-г database-д хадгална.
            await db.SaveChangesAsync();

            // Controller үүсгэнэ.
            var controller =
                CreateController(db);

            // Act
            // Seed method-ийг дахин дуудна.
            var result =
                await controller.Seed();

            // Assert
            // Method амжилттай дууссан тул OkObjectResult буцаах ёстой.
            Assert.IsInstanceOfType(
                result,
                typeof(OkObjectResult));

            // Currency table аль хэдийн өгөгдөлтэй байсан тул
            // default валютуудыг дахин нэмэх ёсгүй.
            // Тиймээс count 1 хэвээр байх ёстой.
            Assert.AreEqual(
                1,
                db.CurrencyRates.Count());
        }

        [TestMethod]
        public async Task GetAll_WhenRatesExist_ReturnsRatesOrderedByCode()
        {
            // Arrange
            // Түр database үүсгэнэ.
            using var db =
                CreateDbContext();

            // Database-д USD болон CNY гэсэн 2 ханш нэмнэ.
            // Санаатайгаар USD-г түрүүлж нэмсэн.
            // Харин GetAll нь Code-оор эрэмбэлж буцаах ёстой.
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

            // Хоёр ханшийг database-д хадгална.
            await db.SaveChangesAsync();

            // Controller үүсгэнэ.
            var controller =
                CreateController(db);

            // Act
            // Бүх валютын ханшийг авах method-ийг дуудна.
            var result =
                await controller.GetAll();

            // Assert
            // Амжилттай үед OkObjectResult буцаах ёстой.
            Assert.IsInstanceOfType(
                result,
                typeof(OkObjectResult));

            // IActionResult-ийг OkObjectResult болгон cast хийж байна.
            var okResult =
                result as OkObjectResult;

            // OkObjectResult-ийн Value дотор List<CurrencyRate> байх ёстой.
            var rates =
                okResult!.Value as List<CurrencyRate>;

            // rates null биш эсэхийг шалгана.
            Assert.IsNotNull(rates);

            // Нийт 2 ханш буцсан байх ёстой.
            Assert.AreEqual(
                2,
                rates.Count);

            // Code-оор эрэмбэлэхэд CNY нь USD-ээс өмнө орно.
            Assert.AreEqual(
                "CNY",
                rates[0].Code);

            // USD хоёр дахь index дээр байх ёстой.
            Assert.AreEqual(
                "USD",
                rates[1].Code);
        }

        [TestMethod]
        public async Task Update_WhenRateExists_UpdatesCurrencyRate()
        {
            // Arrange
            // Түр database үүсгэнэ.
            using var db =
                CreateDbContext();

            // Update хийхийн тулд эхлээд USD ханшийг database-д нэмнэ.
            var rate =
                new CurrencyRate
                {
                    Code = "USD",
                    Name = "Dollar",
                    BuyRate = 3450,
                    SellRate = 3470,
                    UpdatedAt = DateTime.Now
                };

            // USD ханшийг database-д нэмнэ.
            db.CurrencyRates.Add(rate);

            // SaveChangesAsync хийсний дараа rate.Id үүснэ.
            await db.SaveChangesAsync();

            // Controller үүсгэнэ.
            var controller =
                CreateController(db);

            // Шинэчилсэн мэдээлэл.
            // Энэ data-аар дээрх USD ханшийг update хийх ёстой.
            var updated =
                new CurrencyRate
                {
                    Code = "USD",
                    Name = "Америк доллар",
                    BuyRate = 3500,
                    SellRate = 3520
                };

            // Act
            // rate.Id ашиглан тухайн ханшийг update хийнэ.
            var result =
                await controller.Update(
                    rate.Id,
                    updated);

            // Assert
            // Update амжилттай бол OkObjectResult буцаах ёстой.
            Assert.IsInstanceOfType(
                result,
                typeof(OkObjectResult));

            // Database-аас хадгалагдсан USD ханшийг дахин уншина.
            var saved =
                await db.CurrencyRates
                    .FirstAsync(x =>
                        x.Id == rate.Id);

            // Code зөв хэвээр хадгалагдсан эсэхийг шалгана.
            Assert.AreEqual(
                "USD",
                saved.Code);

            // Name шинэчлэгдсэн байх ёстой.
            Assert.AreEqual(
                "Америк доллар",
                saved.Name);

            // BuyRate 3500 болж шинэчлэгдсэн байх ёстой.
            Assert.AreEqual(
                3500m,
                saved.BuyRate);

            // SellRate 3520 болж шинэчлэгдсэн байх ёстой.
            Assert.AreEqual(
                3520m,
                saved.SellRate);
        }

        [TestMethod]
        public async Task Update_WhenRateDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            // Түр хоосон database үүсгэнэ.
            using var db =
                CreateDbContext();

            // Controller үүсгэнэ.
            var controller =
                CreateController(db);

            // Update хийх гэж байгаа шинэ ханшийн мэдээлэл.
            var updated =
                new CurrencyRate
                {
                    Code = "USD",
                    Name = "Dollar",
                    BuyRate = 3500,
                    SellRate = 3520
                };

            // Act
            // 999 гэсэн Id-тай ханш database-д байхгүй.
            // Тиймээс update хийх боломжгүй.
            var result =
                await controller.Update(
                    999,
                    updated);

            // Assert
            // Байхгүй ханш update хийхэд NotFoundObjectResult буцаах ёстой.
            Assert.IsInstanceOfType(
                result,
                typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task UpdateAll_WhenListIsEmpty_ReturnsBadRequest()
        {
            // Arrange
            // Түр database үүсгэнэ.
            using var db =
                CreateDbContext();

            // Controller үүсгэнэ.
            var controller =
                CreateController(db);

            // Act
            // Хоосон list дамжуулж байна.
            // Шинэчлэх ханш байхгүй тул энэ нь буруу request гэж үзнэ.
            var result =
                await controller.UpdateAll(
                    new List<CurrencyRate>());

            // Assert
            // Хоосон list үед BadRequestObjectResult буцаах ёстой.
            Assert.IsInstanceOfType(
                result,
                typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task UpdateAll_WhenRatesExist_UpdatesAllRates()
        {
            // Arrange
            // Түр database үүсгэнэ.
            using var db =
                CreateDbContext();

            // Update хийх эхний ханш: USD.
            var usd =
                new CurrencyRate
                {
                    Code = "USD",
                    Name = "Dollar",
                    BuyRate = 3450,
                    SellRate = 3470,
                    UpdatedAt = DateTime.Now
                };

            // Update хийх хоёр дахь ханш: EUR.
            var eur =
                new CurrencyRate
                {
                    Code = "EUR",
                    Name = "Euro",
                    BuyRate = 3700,
                    SellRate = 3740,
                    UpdatedAt = DateTime.Now
                };

            // USD болон EUR ханшуудыг database-д нэмнэ.
            db.CurrencyRates.AddRange(
                usd,
                eur);

            // SaveChangesAsync хийсний дараа usd.Id болон eur.Id үүснэ.
            await db.SaveChangesAsync();

            // Controller үүсгэнэ.
            var controller =
                CreateController(db);

            // Олон ханшийг зэрэг шинэчлэх list бэлдэнэ.
            //
            // Анхаарах зүйл:
            // Id нь database-д байгаа entity-ийн Id-тай таарч байх ёстой.
            // Ингэж байж controller аль ханшийг update хийхээ мэднэ.
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
            // USD болон EUR ханшийг зэрэг update хийнэ.
            var result =
                await controller.UpdateAll(updatedRates);

            // Assert
            // UpdateAll амжилттай бол OkObjectResult буцаах ёстой.
            Assert.IsInstanceOfType(
                result,
                typeof(OkObjectResult));

            // Database-аас шинэчлэгдсэн USD ханшийг уншина.
            var savedUsd =
                await db.CurrencyRates
                    .FirstAsync(x =>
                        x.Id == usd.Id);

            // Database-аас шинэчлэгдсэн EUR ханшийг уншина.
            var savedEur =
                await db.CurrencyRates
                    .FirstAsync(x =>
                        x.Id == eur.Id);

            // USD-ийн BuyRate 3500 болж шинэчлэгдсэн байх ёстой.
            Assert.AreEqual(
                3500m,
                savedUsd.BuyRate);

            // USD-ийн SellRate 3520 болж шинэчлэгдсэн байх ёстой.
            Assert.AreEqual(
                3520m,
                savedUsd.SellRate);

            // EUR-ийн BuyRate 3800 болж шинэчлэгдсэн байх ёстой.
            Assert.AreEqual(
                3800m,
                savedEur.BuyRate);

            // EUR-ийн SellRate 3840 болж шинэчлэгдсэн байх ёстой.
            Assert.AreEqual(
                3840m,
                savedEur.SellRate);
        }

        [TestMethod]
        public async Task Seed_WhenCurrencyTableIsEmpty_BroadcastsCurrencyRates()
        {
            using var db =
                CreateDbContext();

            var setup =
                CreateControllerWithClientProxy(db);

            await setup.Controller.Seed();

            setup.ClientProxyMock.Verify(
                x => x.SendCoreAsync(
                    "ReceiveCurrencyRates",
                    It.Is<object?[]>(args =>
                        HasRatesPayload(args, 5)),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [TestMethod]
        public async Task Update_WhenRateExists_BroadcastsUpdatedCurrencyRates()
        {
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

            var setup =
                CreateControllerWithClientProxy(db);

            await setup.Controller.Update(
                rate.Id,
                new CurrencyRate
                {
                    Code = "USD",
                    Name = "Америк доллар",
                    BuyRate = 3500,
                    SellRate = 3520
                });

            setup.ClientProxyMock.Verify(
                x => x.SendCoreAsync(
                    "ReceiveCurrencyRates",
                    It.Is<object?[]>(args =>
                        HasSingleUpdatedUsdPayload(args)),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [TestMethod]
        public async Task UpdateAll_WhenRatesExist_BroadcastsUpdatedCurrencyRates()
        {
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

            db.CurrencyRates.AddRange(usd, eur);
            await db.SaveChangesAsync();

            var setup =
                CreateControllerWithClientProxy(db);

            await setup.Controller.UpdateAll(
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
                });

            setup.ClientProxyMock.Verify(
                x => x.SendCoreAsync(
                    "ReceiveCurrencyRates",
                    It.Is<object?[]>(args =>
                        HasUpdatedUsdAndEurPayload(args)),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        private static bool HasRatesPayload(
            object?[] args,
            int expectedCount)
        {
            var rates =
                GetRatesPayload(args);

            return rates != null
                && rates.Count == expectedCount;
        }

        private static bool HasSingleUpdatedUsdPayload(object?[] args)
        {
            var rates =
                GetRatesPayload(args);

            return rates != null
                && rates.Count == 1
                && rates[0].Code == "USD"
                && rates[0].BuyRate == 3500m
                && rates[0].SellRate == 3520m;
        }

        private static bool HasUpdatedUsdAndEurPayload(object?[] args)
        {
            var rates =
                GetRatesPayload(args);

            return rates != null
                && rates.Count == 2
                && rates.Any(rate =>
                    rate.Code == "USD"
                    && rate.BuyRate == 3500m)
                && rates.Any(rate =>
                    rate.Code == "EUR"
                    && rate.BuyRate == 3800m);
        }

        private static List<CurrencyRate>? GetRatesPayload(object?[] args)
        {
            if (args.Length != 1)
                return null;

            return args[0] as List<CurrencyRate>;
        }
    }
}
