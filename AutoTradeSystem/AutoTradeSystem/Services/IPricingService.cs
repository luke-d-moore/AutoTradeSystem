namespace AutoTradeSystem.Services
{
    public interface IPricingService
    {
        public IDictionary<string, decimal> GetTickers();
        public Task<decimal> GetCurrentPrice(string Ticker);
        public decimal Sell(string Ticker, int Quantity, decimal OriginalPrice, decimal CurrentPrice);
        public decimal Buy(string Ticker, int Quantity, decimal OriginalPrice, decimal CurrentPrice);

    }
}