using System.Net;
using System.Net.Http;
using BankTicket;

namespace BankTicket.Tests
{
    [TestClass]
    public class TicketServiceTests
    {
        [TestMethod]
        public async Task CreateTicketAsync_ReturnsTicket_WhenApiSucceeds()
        {
            // Arrange

            var handler = new FakeHttpMessageHandler();

            var client = new HttpClient(handler)
            {
                BaseAddress = new System.Uri("http://localhost")
            };

            var service = new TicketService(client);

            // Act

            var result = await service.CreateTicketAsync();

            // Assert

            Assert.IsNotNull(result);

            Assert.AreEqual("A001", result.Number);
        }

        [TestMethod]
        public async Task CreateTicketAsync_ReturnsNull_WhenApiFails()
        {
            // Arrange

            var handler = new FailedHttpMessageHandler();

            var client = new HttpClient(handler)
            {
                BaseAddress = new System.Uri("http://localhost")
            };

            var service = new TicketService(client);

            // Act

            var result = await service.CreateTicketAsync();

            // Assert

            Assert.IsNull(result);
        }
        [TestMethod]
        public async Task CreateTicketAsync_NumberFormat_IsCorrect()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler();

            var client = new HttpClient(handler)
            {
                BaseAddress = new System.Uri("http://localhost")
            };

            var service = new TicketService(client);

            // Act
            var result = await service.CreateTicketAsync();

            // Assert

            Assert.IsNotNull(result);

            StringAssert.Matches(
                result.Number,
                new System.Text.RegularExpressions.Regex(
                    @"^A\d{3}$"));
        }
        public class EmptyTicketHandler
    : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage>
                SendAsync(
                    HttpRequestMessage request,
                    CancellationToken cancellationToken)
            {
                return Task.FromResult(
                    new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,

                        Content =
                        new StringContent(
                                    @"{
                            ""id"":1,
                            ""number"":"""",
                            ""isCalled"":false
                            }")
                    });
            }
        }
        [TestMethod]
        public async Task CreateTicketAsync_TimeoutThrowsException()
        {
            // Arrange

            var handler = new TimeoutHandler();

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost")
            };

            var service = new TicketService(client);

            try
            {
                // Act
                await service.CreateTicketAsync();

                Assert.Fail(
                    "TaskCanceledException гарах ёстой байсан");
            }
            catch (TaskCanceledException)
            {
                // Test pass
                Assert.IsTrue(true);
            }
        }
        [TestMethod]
        public async Task CreateTicketAsync_CallsQueueEndpoint()
        {
            // Arrange

            var handler =
                new EndpointCheckHandler();

            var client =
                new HttpClient(handler)
                {
                    BaseAddress =
                        new Uri("http://localhost")
                };

            var service =
                new TicketService(client);

            // Act

            await service.CreateTicketAsync();

            // Assert

            Assert.AreEqual(
                "api/queue",
                handler.CalledUrl);
        }
        [TestMethod]
        public async Task CreateTicketAsync_ApiUnavailable_ThrowsHttpRequestException()
        {
            // Arrange

            var handler = new ApiOfflineHandler();

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost")
            };

            var service = new TicketService(client);

            try
            {
                // Act
                await service.CreateTicketAsync();

                Assert.Fail(
                    "HttpRequestException гарах ёстой байсан");
            }
            catch (HttpRequestException)
            {
                // Test pass
                Assert.IsTrue(true);
            }
        }
    }

    public class FakeHttpMessageHandler
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage>
            SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,

                    Content = new StringContent(
                            @"{
                                ""id"":1,
                                ""number"":""A001"",
                                ""isCalled"":false
                            }")
                });
        }
    }

    public class FailedHttpMessageHandler
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage>
            SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });
        }
    }
    public class TimeoutHandler
    : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage>
            SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
        {
            await Task.Delay(5000);

            throw new TaskCanceledException();
        }
    }
    public class EndpointCheckHandler
    : HttpMessageHandler
    {
        public string? CalledUrl { get; private set; }

        protected override Task<HttpResponseMessage>
            SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
        {
            CalledUrl =
                request.RequestUri
                .AbsolutePath
                .Trim('/');

            return Task.FromResult(
                new HttpResponseMessage
                {
                    StatusCode =
                        HttpStatusCode.OK,

                    Content =
                        new StringContent(
                                @"{
                            ""id"":1,
                            ""number"":""A001"",
                            ""isCalled"":false
                            }")
                });
        }
    }
    public class ApiOfflineHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage>
        SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            throw new HttpRequestException(
                "API server is unavailable");
        }
    }
}