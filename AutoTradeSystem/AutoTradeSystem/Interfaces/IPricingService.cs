using System.Diagnostics;

namespace AutoTradeSystem.Interfaces
{
    public interface IPricingService :IHostedService
    {
        public Task<IDictionary<string, decimal>> GetPrices();
        public IDictionary<string, decimal> GetLatestPrices();
        public decimal GetLatestPriceFromTicker(string Ticker);
        public IList<string> GetLatestTickers();

    }
}