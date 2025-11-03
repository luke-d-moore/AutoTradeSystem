using AutoTradeSystem.Dtos;

namespace AutoTradeSystem.Interfaces
{
    public interface IAutoTradingStrategyService : IHostedService
    {
        IDictionary<string, TradingStrategy> GetStrategies();
        Task<bool> AddStrategy(TradingStrategyDto strategyDetails);
        Task<bool> RemoveStrategy(string ID);
        Task<bool> UpdateStrategy(string ID, TradingStrategyDto strategyDetails);

    }
}