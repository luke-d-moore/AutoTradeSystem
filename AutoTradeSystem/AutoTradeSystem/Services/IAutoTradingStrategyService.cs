using AutoTradeSystem.Server.Dtos;

namespace AutoTradeSystem.Server.Services
{
    public interface IAutoTradingStrategyService :IHostedService
    {
        IDictionary<string, TradingStrategy> GetStrategies();
        Task<bool> AddStrategy(TradingStrategyDto strategyDetails);
        Task<bool> RemoveStrategy(string ID);

    }
}