using AutoTradeSystem;
using AutoTradeSystem.Dtos;
using AutoTradeSystem.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
namespace AutoTradeSystemTests
{
    public class AutoTradingStrategyServiceTests
    {
        private readonly Mock<IPricingService> _pricingService;
        private readonly ILogger<AutoTradingStrategyService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IAutoTradingStrategyService _autoTradingStrategyService;
        private IList<string> _tickers = new List<string>() { "IBM" };

    public AutoTradingStrategyServiceTests()
        {
            _pricingService = new Mock<IPricingService>();
            _pricingService.Setup(x => x.GetTickers()).Returns(Task.FromResult(_tickers));
            _logger = new Mock<ILogger<AutoTradingStrategyService>>().Object;
            _autoTradingStrategyService = new AutoTradingStrategyService(_logger, _pricingService.Object);
        }
        private TradingStrategyDto GetTradingStrategy()
        {
            return new TradingStrategyDto() { Ticker = "IBM", PriceChange = 10, Quantity = 10, TradeAction = TradeAction.Buy };
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
        [Fact]
        public async Task UpdateStrategy_ValidID_ReturnsTrue()
        {
            //Arrange
            var tradingStrategy = GetTradingStrategy();
            var updatedStrategy = GetTradingStrategy();
            updatedStrategy.Quantity = 100;
            updatedStrategy.TradeAction = TradeAction.Sell;
            updatedStrategy.PriceChange = 5;
            await _autoTradingStrategyService.AddStrategy(tradingStrategy);
            var ID = _autoTradingStrategyService.GetStrategies().Keys.First();
            var resultUpdate = await _autoTradingStrategyService.UpdateStrategy(ID, updatedStrategy);
            var updatedStrategyFromService = _autoTradingStrategyService.GetStrategies().First(x => x.Key == ID).Value;
            //Act and Assert
            Assert.True(resultUpdate);
            Assert.True(updatedStrategyFromService.TradingStrategyDto.Quantity == updatedStrategy.Quantity);
            Assert.True(updatedStrategyFromService.TradingStrategyDto.TradeAction == updatedStrategy.TradeAction);
            Assert.True(updatedStrategyFromService.TradingStrategyDto.PriceChange == updatedStrategy.PriceChange);
        }
        public static IEnumerable<object[]> AddStrategyReturnFalseData =>
            new List<object[]>
            {
                new object[] { null },
                new object[] { new TradingStrategyDto() { Ticker = "wrong", PriceChange = 10, Quantity = 10, TradeAction = TradeAction.Buy } },
                new object[] { new TradingStrategyDto() { Ticker = "IBM", PriceChange = 0, Quantity = 10, TradeAction = TradeAction.Buy } },
                new object[] { new TradingStrategyDto() { Ticker = "IBM", PriceChange = 10, Quantity = 0, TradeAction = TradeAction.Buy } },
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

        public static IEnumerable<object[]> UpdateStrategyNullData =>
        new List<object[]>
        {
                new object[] { null, new TradingStrategyDto()},
                new object[] { "", new TradingStrategyDto()},
                new object[] { "abc", null}
        };

        [Theory, MemberData(nameof(UpdateStrategyNullData))]
        public async Task UpdateStrategy_NullData_ReturnsFalse(string ID, TradingStrategyDto updatedStrategy)
        {
            //Arrange
            var resultUpdate = await _autoTradingStrategyService.UpdateStrategy(ID, updatedStrategy);
            //Act and Assert
            Assert.False(resultUpdate);
        }
        public static IEnumerable<object[]> UpdateStrategyReturnFalseData =>
            new List<object[]>
            {
                new object[] {new TradingStrategyDto() { Ticker = "IBM", PriceChange = 0, Quantity = 10, TradeAction = TradeAction.Buy } },
                new object[] {new TradingStrategyDto() { Ticker = "IBM", PriceChange = 10, Quantity = 0, TradeAction = TradeAction.Buy } },
                new object[] {new TradingStrategyDto() { Ticker = "IBM", PriceChange = 10, Quantity = -5, TradeAction = TradeAction.Buy } },
                new object[] {new TradingStrategyDto() { Ticker = "IBM", PriceChange = -10, Quantity = 10, TradeAction = TradeAction.Buy } }
            };

        [Theory, MemberData(nameof(UpdateStrategyReturnFalseData))]
        public async Task UpdateStrategy_InValidData_ReturnsFalse(TradingStrategyDto updatedStrategy)
        {
            //Arrange
            var newTradingStrategy = GetTradingStrategy();
            await _autoTradingStrategyService.AddStrategy(newTradingStrategy);
            var newID = _autoTradingStrategyService.GetStrategies().Keys.First();
            var resultUpdate = await _autoTradingStrategyService.UpdateStrategy(newID, updatedStrategy);
            //Act and Assert
            Assert.False(resultUpdate);
        }

        [Fact]        
        public async Task UpdateStrategy_InValidIDData_ReturnsFalse()
        {
            //Arrange
            var ID = "abc";
            var newTradingStrategy = GetTradingStrategy();
            await _autoTradingStrategyService.AddStrategy(newTradingStrategy);
            var resultUpdate = await _autoTradingStrategyService.UpdateStrategy(ID, newTradingStrategy);
            //Act and Assert
            Assert.False(resultUpdate);
        }
    }
}