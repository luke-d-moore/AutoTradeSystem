using AutoTradeSystem.Dtos;

namespace AutoTradeSystem.Services
{
    public interface IAutoTradingStrategyService :IHostedService
    {
        IDictionary<string, TradingStrategy> GetStrategies();
        Task<bool> AddStrategy(TradingStrategyDto strategyDetails);
        Task<bool> RemoveStrategy(string ID);

    }
}