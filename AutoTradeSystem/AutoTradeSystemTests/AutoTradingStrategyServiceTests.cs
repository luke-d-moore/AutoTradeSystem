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
            var tradingStrategy = new TradingStrategyDto() { Ticker = "TEST", PriceChange = 10, Quantity = 10, TradeAction = TradeAction.Buy };
            await _autoTradingStrategyService.AddStrategy(tradingStrategy);
            var result = _autoTradingStrategyService.GetStrategies();
            //Act and Assert
            Assert.True(result.Count == 1);
        }
        [Fact]
        public async Task AddStrategy_ValidStrategy_ReturnsTrue()
        {
            //Arrange
            var tradingStrategy = new TradingStrategyDto() { Ticker = "TEST", PriceChange = 10, Quantity = 10, TradeAction = TradeAction.Buy };
            var result = await _autoTradingStrategyService.AddStrategy(tradingStrategy);
            //Act and Assert
            Assert.True(result);
        }
        [Fact]
        public async Task RemoveStrategy_ValidID_ReturnsTrue()
        {
            //Arrange
            var tradingStrategy = new TradingStrategyDto() { Ticker = "TEST", PriceChange = 10, Quantity = 10, TradeAction = TradeAction.Buy };
            await _autoTradingStrategyService.AddStrategy(tradingStrategy);
            var ID = _autoTradingStrategyService.GetStrategies().Keys.First();
            var resultRemove = await _autoTradingStrategyService.RemoveStrategy(ID);
            //Act and Assert
            Assert.True(resultRemove);
        }
        [Fact]
        public async Task AddStrategy_NullStrategy_ReturnsFalse()
        {
            //Arrange
            var tradingStrategy = new TradingStrategyDto();
            tradingStrategy = null;
            var result = await _autoTradingStrategyService.AddStrategy(tradingStrategy);
            //Act and Assert
            Assert.False(result);
        }
        [Fact]
        public async Task AddStrategy_InValidQuantity_ReturnsFalse()
        {
            //Arrange
            var tradingStrategy = new TradingStrategyDto() { Ticker = "TEST", PriceChange = 10, Quantity = 0, TradeAction = TradeAction.Buy };
            var result = await _autoTradingStrategyService.AddStrategy(tradingStrategy);
            //Act and Assert
            Assert.False(result);
        }
        [Fact]
        public async Task AddStrategy_InValidTicker_ReturnsFalse()
        {
            //Arrange
            var tradingStrategy = new TradingStrategyDto() { Ticker = "wrong", PriceChange = 10, Quantity = 10, TradeAction = TradeAction.Buy };
            var result = await _autoTradingStrategyService.AddStrategy(tradingStrategy);
            //Act and Assert
            Assert.False(result);
        }
        [Fact]
        public async Task AddStrategy_InValidPriceChange_ReturnsFalse()
        {
            //Arrange
            var tradingStrategy = new TradingStrategyDto() { Ticker = "TEST", PriceChange = 0, Quantity = 10, TradeAction = TradeAction.Buy };
            var result = await _autoTradingStrategyService.AddStrategy(tradingStrategy);
            //Act and Assert
            Assert.False(result);
        }
        [Fact]
        public async Task RemoveStrategy_InValidID_ReturnsFalse()
        {
            //Arrange
            var result = await _autoTradingStrategyService.RemoveStrategy("");
            //Act and Assert
            Assert.False(result);
        }
    }
}