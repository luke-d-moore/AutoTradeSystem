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
        private readonly HashSet<string> _tickers = new HashSet<string>() { "IBM", "AMZN", "AAPL" };
        private readonly Dictionary<string, decimal> _prices = new Dictionary<string, decimal>();
        public PricingService(ILogger<PricingService> logger, IConfiguration configuration) 
            : base(CheckRateMilliseconds, logger)
        {
            _logger = logger;
            _configuration = configuration;

            foreach(var ticker in _tickers)
            {
                _prices.TryAdd(ticker, 0m);
            }
        }
        public async Task<bool> GetLatestPrices()
        {
            var tasks = _tickers.Select(ticker => Task.Run(async () => _prices[ticker] = await PriceChecker.GetPriceFromTicker(ticker, _configuration["Token"])));

            await Task.WhenAll(tasks);

            return true;
        }
        public async Task<decimal> GetCurrentPrice(string Ticker)
        {
            await Task.Delay(1);
            var ticker = Ticker.ToUpper();
            if (_prices.Keys.Contains(ticker))
            {
                return _prices[ticker];
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

        public IList<string> GetTickers()
        {
            return _tickers.ToList();
        }

        public IDictionary<string, decimal> GetPrices()
        {
            return _prices;
        }

        protected async override Task<bool> SetCurrentPrices()
        {
            return await GetLatestPrices();
        }
    }
}
