namespace AutoTradeSystem.Services
{
    public interface ITradeActionService
    {
        public Task<decimal> Buy(string ticker, int Quantity, decimal OriginalPrice);
        public Task<decimal> Sell(string ticker, int Quantity, decimal OriginalPrice);

    }
}