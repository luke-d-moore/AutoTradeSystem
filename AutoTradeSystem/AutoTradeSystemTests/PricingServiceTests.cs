//using AutoTradeSystem.Services;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Moq;
//using Moq.Protected;
//using System.Collections.Concurrent;
//using System.Net;
//using System.Text.Json;

//namespace AutoTradeSystemTests
//{
//    public class PricingServiceTests
//    {
//        private readonly Mock<ILogger<PricingService>> _priceLogger;
//        private readonly Mock<IConfiguration> _configuration;
//        public PricingServiceTests()
//        {
//            _priceLogger = new Mock<ILogger<PricingService>>();
//            _configuration = new Mock<IConfiguration>();
//            _configuration.SetupGet(x => x[It.Is<string>(s => s == "PricingSystemBaseURL")])
//                                      .Returns("http://api.example.com/prices");
//        }

//        private IHttpClientFactory SetupFactory(HttpResponseMessage httpResponseMessage, bool ShouldFail = false, bool ThrowsException = false)
//        {
//            var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

//            if (!ShouldFail)
//            {
//                mockHandler
//                .Protected()
//                .Setup<Task<HttpResponseMessage>>(
//                    "SendAsync",
//                    ItExpr.IsAny<HttpRequestMessage>(),
//                    ItExpr.IsAny<CancellationToken>()
//                )
//                .ReturnsAsync(httpResponseMessage);
//            }
//            else
//            {
//                mockHandler
//                .Protected()
//                .Setup<Task<HttpResponseMessage>>(
//                    "SendAsync",
//                    ItExpr.IsAny<HttpRequestMessage>(),
//                    ItExpr.IsAny<CancellationToken>()
//                )
//                .Throws(ThrowsException ? new Exception() : new HttpRequestException());
//            }


//            var controlledHttpClient = new HttpClient(mockHandler.Object);

//            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

//            mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>()))
//                                 .Returns(controlledHttpClient);
//            return mockHttpClientFactory.Object;
//        }
//        [Fact]
//        public async Task GetPrices_ReturnsPriceSuccessfully()
//        {
//            // Arrange
//            var mockResponseContent = JsonSerializer.Serialize(new GetPriceResponse(true, "", new Dictionary<string, decimal>()));
//            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
//            {
//                Content = new StringContent(mockResponseContent, System.Text.Encoding.UTF8, "application/json")
//            };

//            var pricingService = new PricingService(
//                _priceLogger.Object,
//                _configuration.Object,
//                SetupFactory(httpResponse)
//            );

//            var result = pricingService.GetLatestPrices();

//            // Act
//            Assert.IsAssignableFrom<IDictionary<string, decimal>>(result);
//        }

//        [Fact]
//        public void GetPrices_ApiReturnsErrorStatusCode_ThrowsHttpRequestException()
//        {
//            // Arrange
//            var mockResponseContent = JsonSerializer.Serialize(new GetPriceResponse(true, "", new Dictionary<string, decimal>()));
//            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
//            {
//                Content = new StringContent(mockResponseContent, System.Text.Encoding.UTF8, "application/json")
//            };

//            var pricingService = new PricingService(
//                _priceLogger.Object,
//                _configuration.Object,
//                SetupFactory(httpResponse, true)
//            );
//            // Act and Assert
//            var result = pricingService.GetLatestPrices();
//            _priceLogger.Verify(
//            x => x.Log(
//                LogLevel.Error,
//                It.IsAny<EventId>(),
//                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("GetAllPrices Failed due to HTTP request error.")),
//                It.IsAny<HttpRequestException>(),
//                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//            Times.Once);
//        }

//        [Theory]
//        [InlineData("")]
//        [InlineData(" ")]
//        public void GetPrices_ApiReturnsMalformedJson_ThrowsJsonException(string response)
//        {
//            // Arrange
//            var mockResponseContent = response;
//            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
//            {
//                Content = new StringContent(mockResponseContent, System.Text.Encoding.UTF8, "application/json")
//            };

//            var pricingService = new PricingService(
//                _priceLogger.Object,
//                _configuration.Object,
//                SetupFactory(httpResponse)
//            );

//            // Act and Assert
//            var result = pricingService.GetLatestPrices();
//            _priceLogger.Verify(
//            x => x.Log(
//                LogLevel.Error,
//                It.IsAny<EventId>(),
//                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("GetAllPrices Failed JSON deserialization")),
//                It.IsAny<JsonException>(),
//                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//            Times.Once);
//        }

//        [Fact]
//        public void GetPrices_ApiReturnsJsonWithoutPriceValue_ThrowsException()
//        {
//            // Arrange
//            var mockResponseContent = JsonSerializer.Serialize(new GetPriceResponse(true, "", new Dictionary<string, decimal>()));
//            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
//            {
//                Content = new StringContent(mockResponseContent, System.Text.Encoding.UTF8, "application/json")
//            };

//            var pricingService = new PricingService(
//                _priceLogger.Object,
//                _configuration.Object,
//                SetupFactory(httpResponse, true, true)
//            );
//            // Act and Assert
//            var result = pricingService.GetLatestPrices();
//            _priceLogger.Verify(
//            x => x.Log(
//                LogLevel.Error,
//                It.IsAny<EventId>(),
//                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("An unexpected error occurred while getting all prices")),
//                It.IsAny<Exception>(),
//                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//            Times.Once);
//        }
//        [Fact]
//        public void GetTickers_ReturnsTickersSuccessfully()
//        {
//            // Arrange
//            var mockResponseContent = JsonSerializer.Serialize(new GetTickersResponse(true, "", new List<string>()));
//            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
//            {
//                Content = new StringContent(mockResponseContent, System.Text.Encoding.UTF8, "application/json")
//            };

//            var pricingService = new PricingService(
//                _priceLogger.Object,
//                _configuration.Object,
//                SetupFactory(httpResponse)
//            );

//            // Act
//            Assert.IsType<List<string>>(pricingService.GetLatestTickers());
//        }

//        [Fact]
//        public void GetPriceFromTicker_GivenValidTicker_ReturnsPriceSuccessfully()
//        {
//            // Arrange
//            var expectedPrice = 123.45m;
//            var ticker = "TSLA";

//            var mockResponseContent = JsonSerializer.Serialize(new GetPriceResponse(true, "", new Dictionary<string, decimal>() { { ticker, expectedPrice } }));
//            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
//            {
//                Content = new StringContent(mockResponseContent, System.Text.Encoding.UTF8, "application/json")
//            };

//            var pricingService = new PricingService(
//                _priceLogger.Object,
//                _configuration.Object,
//                SetupFactory(httpResponse)
//            );

//            pricingService.Prices.TryAdd(ticker, expectedPrice);

//            // Act
//            var result = pricingService.GetLatestPriceFromTicker(ticker);

//            // Assert
//            result.Equals(expectedPrice);
//        }
//    }
//}