using AutoTradeSystem;
using AutoTradeSystem.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using AutoTradeSystem.Interfaces;
using System.Collections.Concurrent;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace AutoTradeSystemTests
{
    public class PricingServiceTests
    {
        private readonly Mock<ILogger<PricingService>> _priceLogger;
        private readonly Mock<IConfiguration> _configuration;
        public PricingServiceTests()
        {
            _priceLogger = new Mock<ILogger<PricingService>>();
            _configuration = new Mock<IConfiguration>();
            _configuration.SetupGet(x => x[It.Is<string>(s => s == "PricingSystemBaseURL")])
                                      .Returns("http://api.example.com/prices");
        }

        private IHttpClientFactory SetupFactory(HttpResponseMessage httpResponseMessage, bool ShouldFail = false, bool ThrowsException = false)
        {
            var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            if (!ShouldFail)
            {
                mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);
            }
            else
            {
                mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Throws(ThrowsException ? new Exception() : new HttpRequestException());
            }


            var controlledHttpClient = new HttpClient(mockHandler.Object);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                                 .Returns(controlledHttpClient);
            return mockHttpClientFactory.Object;
        }
        [Fact]
        public async Task GetPrices_GivenValidTicker_ReturnsPriceSuccessfully()
        {
            // Arrange
            var mockResponseContent = JsonSerializer.Serialize(new GetPriceResponse(true, "", new Dictionary<string, decimal>()));
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(mockResponseContent, System.Text.Encoding.UTF8, "application/json")
            };

            var pricingService = new PricingService(
                _priceLogger.Object,
                _configuration.Object,
                SetupFactory(httpResponse)
            );

            // Act
            Assert.IsType<Dictionary<string, decimal>>( await pricingService.GetPrices());
        }

        [Fact]
        public async Task GetPrices_ApiReturnsErrorStatusCode_ThrowsHttpRequestException()
        {
            // Arrange
            var exceptionType = typeof(HttpRequestException);
            var mockResponseContent = JsonSerializer.Serialize(new GetPriceResponse(true, "", new Dictionary<string, decimal>()));
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(mockResponseContent, System.Text.Encoding.UTF8, "application/json")
            };

            var pricingService = new PricingService(
                _priceLogger.Object,
                _configuration.Object,
                SetupFactory(httpResponse, true)
            );
            // Act and Assert
            var result = await pricingService.GetPrices();
            _priceLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("GetAllPrices Failed due to HTTP request error.")),
                It.IsAny<HttpRequestException>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetPrices_ApiReturnsMalformedJson_ThrowsJsonException(string response)
        {
            // Arrange
            var mockResponseContent = response;
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(mockResponseContent, System.Text.Encoding.UTF8, "application/json")
            };

            var pricingService = new PricingService(
                _priceLogger.Object,
                _configuration.Object,
                SetupFactory(httpResponse)
            );

            var exceptionType = typeof(JsonException);
            // Act and Assert
            var result = await pricingService.GetPrices();
            _priceLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("GetAllPrices Failed JSON deserialization")),
                It.IsAny<JsonException>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
        }

        [Fact]
        public async Task GetPrices_ApiReturnsJsonWithoutPriceValue_ThrowsException()
        {
            // Arrange
            var mockResponseContent = JsonSerializer.Serialize(new GetPriceResponse(true, "", new Dictionary<string, decimal>()));
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(mockResponseContent, System.Text.Encoding.UTF8, "application/json")
            };

            var exceptionType = typeof(Exception);

            var pricingService = new PricingService(
                _priceLogger.Object,
                _configuration.Object,
                SetupFactory(httpResponse, true, true)
            );
            // Act and Assert
            var result = await pricingService.GetPrices();
            _priceLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("An unexpected error occurred while getting all prices")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
        }
        [Fact]
        public async Task GetTickers_GivenValidTicker_ReturnsPriceSuccessfully()
        {
            // Arrange
            var mockResponseContent = JsonSerializer.Serialize(new GetTickersResponse(true, "", new List<string>()));
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(mockResponseContent, System.Text.Encoding.UTF8, "application/json")
            };

            var pricingService = new PricingService(
                _priceLogger.Object,
                _configuration.Object,
                SetupFactory(httpResponse)
            );

            // Act
            Assert.IsType<List<string>>(await pricingService.GetTickers());
        }

        [Fact]
        public async Task GetTickers_ApiReturnsErrorStatusCode_ThrowsHttpRequestException()
        {
            // Arrange
            var exceptionType = typeof(HttpRequestException);
            var mockResponseContent = JsonSerializer.Serialize(new GetTickersResponse(true, "", new List<string>()));
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(mockResponseContent, System.Text.Encoding.UTF8, "application/json")
            };

            var pricingService = new PricingService(
                _priceLogger.Object,
                _configuration.Object,
                SetupFactory(httpResponse, true)
            );
            // Act and Assert
            var result = await pricingService.GetTickers();
            _priceLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("GetTickers Failed due to HTTP request error.")),
                It.IsAny<HttpRequestException>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetTickers_ApiReturnsMalformedJson_ThrowsJsonException(string response)
        {
            // Arrange
            var mockResponseContent = response;
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(mockResponseContent, System.Text.Encoding.UTF8, "application/json")
            };

            var pricingService = new PricingService(
                _priceLogger.Object,
                _configuration.Object,
                SetupFactory(httpResponse)
            );

            var exceptionType = typeof(JsonException);
            // Act and Assert
            var result = await pricingService.GetTickers();
            _priceLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("GetTickers Failed JSON deserialization")),
                It.IsAny<JsonException>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
        }

        [Fact]
        public async Task GetTickers_ApiReturnsJsonWithoutPriceValue_ThrowsException()
        {
            // Arrange
            var mockResponseContent = JsonSerializer.Serialize(new GetTickersResponse(true, "", new List<string>()));
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(mockResponseContent, System.Text.Encoding.UTF8, "application/json")
            };

            var exceptionType = typeof(Exception);

            var pricingService = new PricingService(
                _priceLogger.Object,
                _configuration.Object,
                SetupFactory(httpResponse, true, true)
            );
            // Act and Assert
            var result = await pricingService.GetTickers();
            _priceLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("An unexpected error occurred while getting tickers")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
        }
    }
}