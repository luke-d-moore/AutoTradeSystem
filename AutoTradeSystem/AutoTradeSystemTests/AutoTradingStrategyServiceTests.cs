using AutoTradeSystem;
using AutoTradeSystem.Dtos;
using AutoTradeSystem.Services;
using Microsoft.Extensions.Logging;
using Moq;
namespace AutoTradeSystemTests
{
    public class AutoTradingStrategyServiceTests
    {
        private readonly IPricingService _pricingService;
        private readonly ILogger<AutoTradingStrategyService> _logger;
        private readonly IAutoTradingStrategyService _autoTradingStrategyService;

        public AutoTradingStrategyServiceTests()
        {
            _pricingService = new PricingService();
            _logger = new Mock<ILogger<AutoTradingStrategyService>>().Object;
            _autoTradingStrategyService = new AutoTradingStrategyService(_logger, _pricingService);
        }
        private TradingStrategyDto GetTradingStrategy()
        {
            return new TradingStrategyDto() { Ticker = "TEST", PriceChange = 10, Quantity = 10, TradeAction = TradeAction.Buy };
        }
        [Fact]
        public void GetStrategies_NoStrategies_ReturnsZeroStrategies()
        {
            //Arrange
            var result = _autoTradingStrategyService.GetStrategies();
            //Act and Assert
            Assert.True(result.Count == 0);
        }
        [Fact]
        public async Task GetStrategies_OneStrategy_ReturnsOneStrategy()
        {
            //Arrange
            var tradingStrategy = GetTradingStrategy();
            await _autoTradingStrategyService.AddStrategy(tradingStrategy);
            var result = _autoTradingStrategyService.GetStrategies();
            //Act and Assert
            Assert.True(result.Count == 1);
        }
        [Fact]
        public async Task AddStrategy_ValidStrategy_ReturnsTrue()
        {
            //Arrange
            var tradingStrategy = GetTradingStrategy();
            var result = await _autoTradingStrategyService.AddStrategy(tradingStrategy);
            //Act and Assert
            Assert.True(result);
        }
        [Fact]
        public async Task RemoveStrategy_ValidID_ReturnsTrue()
        {
            //Arrange
            var tradingStrategy = GetTradingStrategy();
            await _autoTradingStrategyService.AddStrategy(tradingStrategy);
            var ID = _autoTradingStrategyService.GetStrategies().Keys.First();
            var resultRemove = await _autoTradingStrategyService.RemoveStrategy(ID);
            //Act and Assert
            Assert.True(resultRemove);
        }
        public static IEnumerable<object[]> AddStrategyReturnFalseData =>
            new List<object[]>
            {
                new object[] { null },
                new object[] { new TradingStrategyDto() { Ticker = "wrong", PriceChange = 10, Quantity = 10, TradeAction = TradeAction.Buy } },
                new object[] { new TradingStrategyDto() { Ticker = "TEST", PriceChange = 0, Quantity = 10, TradeAction = TradeAction.Buy } },
                new object[] { new TradingStrategyDto() { Ticker = "TEST", PriceChange = 10, Quantity = 0, TradeAction = TradeAction.Buy } },
                new object[] { new TradingStrategyDto() { Ticker = "ab", PriceChange = 10, Quantity = 10, TradeAction = TradeAction.Buy } },
                new object[] { new TradingStrategyDto() { Ticker = "", PriceChange = 10, Quantity = 10, TradeAction = TradeAction.Buy } },
                new object[] { new TradingStrategyDto() { Ticker = "abcdef", PriceChange = 10, Quantity = 10, TradeAction = TradeAction.Buy } },
                new object[] { new TradingStrategyDto() { Ticker = null, PriceChange = 10, Quantity = 10, TradeAction = TradeAction.Buy } },

            };

        [Theory, MemberData(nameof(AddStrategyReturnFalseData))]
        public async Task AddStrategy_InvalidStrategy_ReturnsFalse(TradingStrategyDto tradingStrategy)
        {
            //Arrange
            var result = await _autoTradingStrategyService.AddStrategy(tradingStrategy);
            //Act and Assert
            Assert.False(result);
        }
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("wrong")]
        public async Task RemoveStrategy_InValidID_ReturnsFalse(string ID)
        {
            //Arrange
            var result = await _autoTradingStrategyService.RemoveStrategy(ID);
            //Act and Assert
            Assert.False(result);
        }
    }
}