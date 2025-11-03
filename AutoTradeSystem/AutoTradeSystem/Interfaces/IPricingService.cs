namespace AutoTradeSystem.Interfaces
{
    public interface IPricingService
    {
        public Task<IList<string>> GetTickers();
        public Task<IDictionary<string, decimal>> GetPrices();
        public Task<decimal> GetPriceFromTicker(string ticker);

    }
}