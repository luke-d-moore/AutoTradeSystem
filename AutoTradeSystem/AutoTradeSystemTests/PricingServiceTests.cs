using AutoTradeSystem;
using AutoTradeSystem.Services;
using Moq;

namespace AutoTradeSystemTests
{
    public class PricingServiceTests
    {
        private readonly IPricingService _pricingService;

        public PricingServiceTests()
        {
            _pricingService = new PricingService();
        }
        [Fact]
        public async Task GetCurrentPrice_ValidTicker_ReturnsDecimalAsync()
        {
            //Arrange
            var result = await _pricingService.GetCurrentPrice("Test");
            //Act and Assert
            Assert.IsType<decimal>(result);
        }
        [Fact]
        public void GetCurrentPrice_InValidTicker_ThrowsArgumentException()
        {
            // Arrange
            var exceptionType = typeof(ArgumentException);
            // Act and Assert
            Assert.ThrowsAsync(exceptionType, async () => await _pricingService.GetCurrentPrice("wrong"));
        }
        [Fact]
        public void GetCurrentPrice_NullTicker_ThrowsArgumentException()
        {
            // Arrange
            var exceptionType = typeof(ArgumentException);
            // Act and Assert
            Assert.ThrowsAsync(exceptionType, async () => await _pricingService.GetCurrentPrice(null));
        }
        [Fact]
        public void Buy_Valid_ReturnsDecimal()
        {
            //Arrange
            var result = _pricingService.Buy("Test", 10, 100, 90);
            //Act and Assert
            Assert.Equal(100, result);
        }
        [Fact]
        public void Buy_InValidTicker_ThrowsArgumentException()
        {
            // Arrange
            var exceptionType = typeof(ArgumentException);
            // Act and Assert
            Assert.Throws(exceptionType, () => _pricingService.Buy("wrong",10, 100,90));
        }
        [Fact]
        public void Buy_NullTicker_ThrowsArgumentException()
        {
            // Arrange
            var exceptionType = typeof(ArgumentException);
            // Act and Assert
            Assert.Throws(exceptionType, () => _pricingService.Buy(null, 10, 100, 90));
        }
        [Fact]
        public void Buy_InValidQuantity_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var exceptionType = typeof(ArgumentOutOfRangeException);
            // Act and Assert
            Assert.Throws(exceptionType, () => _pricingService.Buy("Test", 0, 100, 90));
        }
        [Fact]
        public void Sell_Valid_ReturnsDecimal()
        {
            //Arrange
            var result = _pricingService.Sell("Test", 10, 100, 110);
            //Act and Assert
            Assert.Equal(100, result);
        }
        [Fact]
        public void Sell_InValidTicker_ThrowsArgumentException()
        {
            // Arrange
            var exceptionType = typeof(ArgumentException);
            // Act and Assert
            Assert.Throws(exceptionType, () => _pricingService.Sell("wrong", 10, 100, 110));
        }
        [Fact]
        public void Sell_NullTicker_ThrowsArgumentException()
        {
            // Arrange
            var exceptionType = typeof(ArgumentException);
            // Act and Assert
            Assert.Throws(exceptionType, () => _pricingService.Sell(null, 10, 100, 110));
        }
        [Fact]
        public void Sell_InValidQuantity_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var exceptionType = typeof(ArgumentOutOfRangeException);
            // Act and Assert
            Assert.Throws(exceptionType, () => _pricingService.Sell("Test", 0, 100, 110));
        }
    }
}