using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using BankTicket;

namespace BankTicket.Tests
{
    // Энэ класс нь TicketService-ийн CreateTicketAsync() функцийг test хийх зориулалттай.
    // Unit test гэдэг нь програмын нэг жижиг хэсэг зөв ажиллаж байгаа эсэхийг тусад нь шалгах арга.
    //
    // Энэ тестүүд бодит API сервер асаахгүйгээр ажиллана.
    // Учир нь HttpClient-ийн дотор өөрсдийн fake HttpMessageHandler-уудыг ашиглаж,
    // API-аас ирэх response-ийг хиймлээр үүсгэж өгч байгаа.
    [TestClass]
    public class TicketServiceTests
    {
        [TestMethod]
        public async Task CreateTicketAsync_ReturnsTicket_WhenApiSucceeds()
        {
            // Arrange
            // FakeHttpMessageHandler нь API амжилттай ажиллаж байгаа мэт
            // 200 OK status code болон ticket JSON буцаана.
            var handler = new FakeHttpMessageHandler();

            // HttpClient үүсгэж байна.
            // BaseAddress нь API-ийн үндсэн хаяг.
            // Гэхдээ энэ test дээр бодит localhost руу явахгүй,
            // handler доторх fake response-ийг ашиглана.
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost")
            };

            // TicketService нь HttpClient ашиглан API руу request явуулдаг service.
            var service = new TicketService(client);

            // Act
            // CreateTicketAsync() функцийг дуудаж ticket үүсгэж байна.
            var result = await service.CreateTicketAsync();

            // Assert
            // API амжилттай гэж үзэж байгаа тул result null биш байх ёстой.
            Assert.IsNotNull(result);

            // API-аас ирсэн ticket number A001 байх ёстой.
            Assert.AreEqual("A001", result.Number);

            // API ISO форматтай createdAt буцаасан ч уншигдаж байх ёстой.
            Assert.AreEqual("2026-05-27T12:42:55.4918995+08:00", result.CreatedAtRaw);
            Assert.IsNotNull(result.CreatedAt);

            // Шинээр үүссэн ticket дуудсан төлөвтэй биш байх ёстой.
            Assert.IsFalse(result.IsCalled);
        }

        [TestMethod]
        public async Task CreateTicketAsync_ReturnsNull_WhenApiFails()
        {
            // Arrange
            // FailedHttpMessageHandler нь API алдаа өгч байгаа нөхцөлийг дуурайлгана.
            // Энэ handler 500 InternalServerError буцаана.
            var handler = new FailedHttpMessageHandler();

            // Fake handler-тай HttpClient үүсгэнэ.
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost")
            };

            // Test хийх service-ээ үүсгэнэ.
            var service = new TicketService(client);

            // Act
            // API алдаа өгөх үед CreateTicketAsync() ямар үр дүн буцаахыг шалгана.
            var result = await service.CreateTicketAsync();

            // Assert
            // API амжилтгүй үед result null байх ёстой.
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task CreateTicketAsync_NumberFormat_IsCorrect()
        {
            // Arrange
            // API амжилттай A001 гэсэн number буцаах fake handler.
            var handler = new FakeHttpMessageHandler();

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost")
            };

            var service = new TicketService(client);

            // Act
            // Ticket үүсгэх service method-ийг дуудна.
            var result = await service.CreateTicketAsync();

            // Assert
            // Эхлээд result null биш эсэхийг шалгана.
            Assert.IsNotNull(result);

            // Ticket number зөв форматтай эсэхийг Regex ашиглан шалгана.
            //
            // @"^A\d{3}$" гэсэн regex-ийн тайлбар:
            // ^      -> string-ийн эхлэл
            // A      -> заавал A үсгээр эхэлнэ
            // \d{3}  -> дараа нь яг 3 ширхэг цифр байна
            // $      -> string-ийн төгсгөл
            //
            // Жишээ зөв format:
            // A001, A002, A123, A999
            StringAssert.Matches(
                result.Number,
                new Regex(@"^A\d{3}$"));
        }

        [TestMethod]
        public async Task CreateTicketAsync_TimeoutThrowsException()
        {
            // Arrange
            // TimeoutHandler нь API хариу өгөхгүй timeout болсон нөхцөлийг дуурайлгана.
            var handler = new TimeoutHandler();

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost")
            };

            var service = new TicketService(client);

            await AssertThrowsAsync<TaskCanceledException>(
                () => service.CreateTicketAsync());
        }

        [TestMethod]
        public async Task CreateTicketAsync_CallsQueueEndpoint()
        {
            // Arrange
            // EndpointCheckHandler нь service яг ямар URL рүү request явуулсныг хадгалж авна.
            var handler = new EndpointCheckHandler();

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost")
            };

            var service = new TicketService(client);

            // Act
            // CreateTicketAsync() дуудагдахад API endpoint рүү request явна.
            await service.CreateTicketAsync();

            // Assert
            // Service-ийн дуудсан endpoint нь api/queue мөн эсэхийг шалгана.
            // Ticket үүсгэхдээ queue endpoint ашиглаж байгаа гэсэн үг.
            Assert.AreEqual("api/queue", handler.CalledUrl);
        }

        [TestMethod]
        public async Task CreateTicketAsync_UsesPostMethod()
        {
            // Arrange
            // EndpointCheckHandler нь request-ийн HTTP method-ийг хадгалж авна.
            var handler = new EndpointCheckHandler();

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost")
            };

            var service = new TicketService(client);

            // Act
            // Ticket үүсгэх request явуулна.
            await service.CreateTicketAsync();

            // Assert
            // Шинэ ticket үүсгэж байгаа учраас HTTP method нь POST байх ёстой.
            // GET бол зөвхөн мэдээлэл авахад илүү тохиромжтой.
            Assert.AreEqual(HttpMethod.Post, handler.Method);
        }

        [TestMethod]
        public async Task CreateTicketAsync_ApiUnavailable_ThrowsHttpRequestException()
        {
            // Arrange
            // ApiOfflineHandler нь API сервер ажиллахгүй,
            // холбогдох боломжгүй байгаа нөхцөлийг дуурайлгана.
            var handler = new ApiOfflineHandler();

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost")
            };

            var service = new TicketService(client);

            await AssertThrowsAsync<HttpRequestException>(
                () => service.CreateTicketAsync());
        }

        [TestMethod]
        public async Task CreateTicketAsync_WhenNumberIsEmpty_ReturnsTicketWithEmptyNumber()
        {
            // Arrange
            // EmptyTicketHandler нь number талбар хоосон string байгаа
            // ticket JSON буцаана.
            var handler = new EmptyTicketHandler();

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost")
            };

            var service = new TicketService(client);

            // Act
            // Хоосон number-той ticket response ирэх нөхцөлийг шалгана.
            var result = await service.CreateTicketAsync();

            // Assert
            // Ticket object өөрөө null биш байна.
            Assert.IsNotNull(result);

            // API-аас number хоосон ирсэн тул result.Number мөн хоосон байх ёстой.
            Assert.AreEqual("", result.Number);

            // Ticket дуудаагүй төлөвтэй байна.
            Assert.IsFalse(result.IsCalled);
        }

        private static async Task AssertThrowsAsync<TException>(
            Func<Task> action)
            where TException : Exception
        {
            try
            {
                await action();
            }
            catch (TException)
            {
                return;
            }

            Assert.Fail(
                typeof(TException).Name
                + " гарах ёстой байсан");
        }
    }

    // FakeHttpMessageHandler нь API амжилттай ажиллаж байгаа нөхцөлийг дуурайлгана.
    // Энэ class нь бодит server рүү request явуулахгүй,
    // харин test-д зориулсан fake JSON response буцаана.
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        // HttpClient request явуулах үед SendAsync автоматаар дуудагдана.
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Task.FromResult нь async method-д шууд бэлэн response буцаахад ашиглагдана.
            return Task.FromResult(
                new HttpResponseMessage
                {
                    // 200 OK гэдэг нь API request амжилттай гэсэн утгатай.
                    StatusCode = HttpStatusCode.OK,

                    // API-аас ирж байгаа мэт JSON data үүсгэж өгч байна.
                    Content = new StringContent(
                        @"{
                            ""id"": 1,
                            ""number"": ""A001"",
                            ""isCalled"": false,
                            ""createdAt"": ""2026-05-27T12:42:55.4918995+08:00""
                        }")
                });
        }
    }

    // FailedHttpMessageHandler нь API server дээр алдаа гарсан нөхцөлийг дуурайлгана.
    // Жишээ нь database error, backend exception гэх мэт.
    public class FailedHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // 500 InternalServerError гэдэг нь server талд алдаа гарсан гэсэн утгатай.
            return Task.FromResult(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });
        }
    }

    // TimeoutHandler нь API request timeout болсон нөхцөлийг дуурайлгана.
    // Timeout гэдэг нь server хугацаандаа response өгөхгүй байхыг хэлнэ.
    public class TimeoutHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // HttpClient timeout болох үед ихэвчлэн TaskCanceledException гардаг.
            throw new TaskCanceledException();
        }
    }

    // EndpointCheckHandler нь service ямар endpoint рүү,
    // ямар HTTP method ашиглан request явуулж байгааг шалгах зориулалттай.
    public class EndpointCheckHandler : HttpMessageHandler
    {
        // Service-ийн дуудсан URL path-ийг хадгална.
        // Жишээ: api/queue
        public string? CalledUrl { get; private set; }

        // Service-ийн ашигласан HTTP method-ийг хадгална.
        // Жишээ: POST
        public HttpMethod? Method { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // RequestUri.AbsolutePath нь "/api/queue" хэлбэртэй ирнэ.
            // Trim('/') хийснээр "api/queue" болгож хадгална.
            CalledUrl =
                request.RequestUri!
                    .AbsolutePath
                    .Trim('/');

            // Request-ийн method-ийг хадгална.
            Method =
                request.Method;

            // Endpoint болон method шалгах тестүүд амжилттай үргэлжлэхийн тулд
            // fake 200 OK response буцааж байна.
            return Task.FromResult(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        @"{
                            ""id"": 1,
                            ""number"": ""A001"",
                            ""isCalled"": false,
                            ""createdAt"": ""2026-05-27T12:42:55.4918995+08:00""
                        }")
                });
        }
    }

    // ApiOfflineHandler нь API server огт ажиллахгүй,
    // эсвэл холбогдох боломжгүй нөхцөлийг дуурайлгана.
    public class ApiOfflineHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Server unavailable үед HttpRequestException шидэж байна.
            throw new HttpRequestException(
                "API server is unavailable");
        }
    }

    // EmptyTicketHandler нь API number талбар хоосон буцаасан нөхцөлийг дуурайлгана.
    // Энэ нь edge case буюу ховор боловч шалгах хэрэгтэй нөхцөл.
    public class EmptyTicketHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // number нь хоосон string байна.
            return Task.FromResult(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        @"{
                            ""id"": 1,
                            ""number"": """",
                            ""isCalled"": false
                        }")
                });
        }
    }
}
