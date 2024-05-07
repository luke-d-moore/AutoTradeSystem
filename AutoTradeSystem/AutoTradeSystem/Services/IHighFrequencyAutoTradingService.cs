using AutoTradeSystem.Dtos;

namespace AutoTradeSystem.Services
{
    public interface IHighFrequencyAutoTradingService : IHostedService
    {
        public decimal GetFundValue();
    }
}