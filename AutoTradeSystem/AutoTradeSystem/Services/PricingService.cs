using AutoTradeSystem.Dtos;
using System.Collections.Concurrent;

namespace AutoTradeSystem.Services
{
    public class PricingService : PricingServiceBase, IPricingService
    {
        private const int CheckRateMilliseconds = 500;
        private readonly ILogger<PricingService> _logger;
        private Random _rand = new Random();
        //These would be accessed from the database, but here I have hardcoded for testing
        private readonly ConcurrentDictionary<string, decimal> _tickers = new ConcurrentDictionary<string, decimal>(
            new Dictionary<string, decimal>()
        {
            { "ABC", 50.00m },
            { "GOOGL", 100.00m },
            { "AMZN", 100.00m },
            { "TEST", 75.00m }
        });

        public PricingService(ILogger<PricingService> logger) 
            : base(CheckRateMilliseconds, logger)
        {
            _logger = logger;
        }
        public async Task<bool> ChangePrices()
        {
            await Task.Delay(1);
            foreach (var ticker in _tickers.Keys)
            {
                if (_tickers.Keys.Contains(ticker))
                {
                    var percChange = new decimal(_rand.NextDouble() / 100);
                    var currentPrice = _tickers[ticker];
                    if (PriceIncreases(_rand))
                    {
                        _tickers[ticker] += currentPrice * percChange;
                    }
                    else
                    {
                        _tickers[ticker] -= currentPrice * percChange;
                    }
                }
            }
            return true;
        }
        private bool PriceIncreases(Random random) 
        {
            return random.NextDouble() > 0.5;
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
            return await ChangePrices();
        }
    }
}
