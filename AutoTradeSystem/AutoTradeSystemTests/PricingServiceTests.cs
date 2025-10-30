using AutoTradeSystem;
using AutoTradeSystem.Dtos;
using AutoTradeSystem.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AutoTradeSystemTests
{
    public class PricingServiceTests
    {
        private readonly IPricingService _pricingService;
        private readonly ILogger<PricingService> _priceLogger;
        private readonly IConfiguration _configuration;

        public PricingServiceTests()
        {
            _priceLogger = new Mock<ILogger<PricingService>>().Object;
            _configuration = new Mock<IConfiguration>().Object;
            _pricingService = new PricingService(_priceLogger, _configuration);
        }
        public static IEnumerable<object[]> TickerData =>
    new List<object[]>
    {
                new object[] { null },
                new object[] { "wrong"},
                new object[] { "" }
    };

        [Fact]
        public async Task GetCurrentPrice_ValidTicker_ReturnsDecimalAsync()
        {
            //Arrange
            var result = await _pricingService.GetCurrentPrice("IBM");
            //Act and Assert
            Assert.IsType<decimal>(result);
        }
        [Theory, MemberData(nameof(TickerData))]
        public void GetCurrentPrice_InValidTicker_ThrowsArgumentException(string ticker)
        {
            // Arrange
            var exceptionType = typeof(ArgumentException);
            // Act and Assert
            Assert.ThrowsAsync(exceptionType, async () => await _pricingService.GetCurrentPrice(ticker));
        }
        [Fact]
        public void Buy_Valid_ReturnsDecimal()
        {
            //Arrange
            var result = _pricingService.Buy("IBM", 10, 100, 90);
            //Act and Assert
            Assert.Equal(100, result);
        }
        [Theory, MemberData(nameof(TickerData))]
        public void Buy_InValidTicker_ThrowsArgumentException(string ticker)
        {
            // Arrange
            var exceptionType = typeof(ArgumentException);
            // Act and Assert
            Assert.Throws(exceptionType, () => _pricingService.Buy(ticker,10, 100,90));
        }
        [Fact]
        public void Buy_InValidQuantity_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var exceptionType = typeof(ArgumentOutOfRangeException);
            // Act and Assert
            Assert.Throws(exceptionType, () => _pricingService.Buy("IBM", 0, 100, 90));
        }
        [Fact]
        public void Sell_Valid_ReturnsDecimal()
        {
            //Arrange
            var result = _pricingService.Sell("IBM", 10, 100, 110);
            //Act and Assert
            Assert.Equal(100, result);
        }
        [Theory, MemberData(nameof(TickerData))]
        public void Sell_InValidTicker_ThrowsArgumentException(string ticker)
        {
            // Arrange
            var exceptionType = typeof(ArgumentException);
            // Act and Assert
            Assert.Throws(exceptionType, () => _pricingService.Sell(ticker, 10, 100, 110));
        }
        [Fact]
        public void Sell_InValidQuantity_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var exceptionType = typeof(ArgumentOutOfRangeException);
            // Act and Assert
            Assert.Throws(exceptionType, () => _pricingService.Sell("IBM", 0, 100, 110));
        }
    }
}