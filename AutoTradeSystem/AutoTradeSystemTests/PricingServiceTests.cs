using AutoTradeSystem;
namespace AutoTradeSystemTests
{
    public class PricingServiceTests
    {
        //Fill in the tests to cover all test cases
        [Fact]
        public void GetCurrentPrice_ValidTicker_ReturnsDecimal()
        {
            Assert.True(true);
        }
        [Fact]
        public void GetCurrentPrice_InValidTicker_ThrowsArgumentException()
        {
            Assert.False(false);
        }
        [Fact]
        public void Buy_Valid_ReturnsDecimal()
        {
            Assert.True(true);
        }
        [Fact]
        public void Buy_InValidTicker_ThrowsArgumentException()
        {
            Assert.False(false);
        }
        [Fact]
        public void Buy_InValidQuantity_ThrowsArgumentOutOfRangeException()
        {
            Assert.False(false);
        }
        [Fact]
        public void Sell_Valid_ReturnsDecimal()
        {
            Assert.True(true);
        }
        [Fact]
        public void Sell_InValidTicker_ThrowsArgumentException()
        {
            Assert.False(false);
        }
        [Fact]
        public void Sell_InValidQuantity_ThrowsArgumentOutOfRangeException()
        {
            Assert.False(false);
        }
    }
}