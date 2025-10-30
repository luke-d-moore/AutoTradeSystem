namespace AutoTradeSystem.Services
{
    public interface IPricingService : IHostedService
    {
        public IList<string> GetTickers();
        public IDictionary<string, decimal> GetPrices();
        public Task<decimal> GetCurrentPrice(string Ticker);
        public decimal Sell(string Ticker, int Quantity, decimal OriginalPrice, decimal CurrentPrice);
        public decimal Buy(string Ticker, int Quantity, decimal OriginalPrice, decimal CurrentPrice);

    }
}