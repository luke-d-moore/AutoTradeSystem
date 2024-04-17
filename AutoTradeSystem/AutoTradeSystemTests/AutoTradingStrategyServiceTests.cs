using AutoTradeSystem;
namespace AutoTradeSystemTests
{
    public class AutoTradingStrategyServiceTests
    {
        //Fill in the tests to cover all test cases
        [Fact]
        public void GetStrategies_NoStrategies_ReturnsZeroStrategies()
        {
            Assert.True(true);
        }
        [Fact]
        public void GetStrategies_OneStrategy_ReturnsOneStrategy()
        {
            Assert.True(true);
        }
        [Fact]
        public void AddStrategy_ValidStrategy_ReturnsTrue()
        {
            Assert.True(true);
        }
        [Fact]
        public void AddStrategy_NullStrategy_ReturnsFalse()
        {
            Assert.False(false);
        }
        [Fact]
        public void AddStrategy_InValidQuantity_ReturnsFalse()
        {
            Assert.False(false);
        }
        [Fact]
        public void AddStrategy_InValidTicker_ReturnsFalse()
        {
            Assert.False(false);
        }
        [Fact]
        public void AddStrategy_InValidPriceChange_ReturnsFalse()
        {
            Assert.False(false);
        }
        [Fact]
        public void RemoveStrategy_ValidID_ReturnsTrue()
        {
            Assert.True(true);
        }
        [Fact]
        public void RemoveStrategy_InValidID_ReturnsFalse()
        {
            Assert.False(false);
        }
    }
}