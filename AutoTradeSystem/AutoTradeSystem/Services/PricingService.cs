namespace AutoTradeSystem.Server.Services
{
    public class PricingService : IPricingService
    {
        private readonly List<string> _tickers = new List<string>() { "ABC", "GOOGL", "AMZN", "TEST"};
        //These would be accessed from the database, but here I have hardcoded for testing
        public async Task<decimal> GetCurrentPrice(string Ticker)
        {
            await Task.Delay(100);
            if (_tickers.Contains(Ticker, StringComparer.OrdinalIgnoreCase))
            {
                var rand = new Random();
                return new decimal(rand.NextDouble()*100.0);
            }
            else
            {
                throw new ArgumentException("Invalid Ticker", "ticker");
            }

        }
        public decimal Buy(string Ticker, int Quantity, decimal OriginalPrice, decimal CurrentPrice)
        {
            if (!_tickers.Contains(Ticker, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Invalid Ticker", "ticker");
            }
            if (Quantity <= 0)
            {
                throw new ArgumentOutOfRangeException("quantity", Quantity, "Quantity must be greater than 0.");
            }

            var Difference = OriginalPrice - CurrentPrice; // for buy this is positive as original > current

            return Difference * Quantity;
        }
        public decimal Sell(string Ticker, int Quantity, decimal OriginalPrice, decimal CurrentPrice)
        {
            if (!_tickers.Contains(Ticker, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Invalid Ticker", "ticker");
            }
            if (Quantity <= 0)
            {
                throw new ArgumentOutOfRangeException("quantity", Quantity, "Quantity must be greater than 0.");
            }

            var Difference = CurrentPrice - OriginalPrice; // for sell this is negative as original < current

            return Difference * Quantity;
        }
    }
}
