using AutoTradeSystem.Classes;
using AutoTradeSystem.Dtos;
using System.Collections.Concurrent;

namespace AutoTradeSystem.Services
{
    public class PricingService : PricingServiceBase, IPricingService
    {
        private const int CheckRateMilliseconds = 60000;
        private readonly ILogger<PricingService> _logger;
        private readonly IConfiguration _configuration;
        //These would be accessed from the database, but here I have hardcoded for testing
        private readonly Dictionary<string, decimal> _tickers = new Dictionary<string, decimal>(
            new Dictionary<string, decimal>()
        {
            { "IBM", 0m },
            { "AMZN", 0m },
            { "AAPL", 0m }
        });

        public PricingService(ILogger<PricingService> logger, IConfiguration configuration) 
            : base(CheckRateMilliseconds, logger)
        {
            _logger = logger;
            _configuration = configuration;
        }
        public async Task<bool> GetLatestPrices()
        {
            foreach (var ticker in _tickers.Keys)
            {
                var price = await PriceChecker.GetPriceFromTicker(ticker, _configuration["Token"]);
                _tickers[ticker] = price;
            }
            return true;
        }
        public async Task<decimal> GetCurrentPrice(string Ticker)
        {
            await Task.Delay(1);
            var ticker = Ticker.ToUpper();
            if (_tickers.Keys.Contains(ticker))
            {
                return _tickers[ticker];
            }
            else
            {
                throw new ArgumentException("Invalid Ticker", "ticker");
            }

        }
        public decimal Buy(string Ticker, int Quantity, decimal OriginalPrice, decimal CurrentPrice)
        {
            if (!_tickers.Keys.Contains(Ticker, StringComparer.OrdinalIgnoreCase))
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
            if (!_tickers.Keys.Contains(Ticker, StringComparer.OrdinalIgnoreCase))
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

        public IDictionary<string, decimal> GetTickers()
        {
            return _tickers;
        }

        protected async override Task<bool> SetCurrentPrices()
        {
            return await GetLatestPrices();
        }
    }
}
