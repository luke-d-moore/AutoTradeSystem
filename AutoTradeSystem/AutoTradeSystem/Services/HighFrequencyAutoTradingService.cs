using AutoTradeSystem.Classes;
using AutoTradeSystem.Dtos;
using System.Collections.Concurrent;

namespace AutoTradeSystem.Services
{
    public class HighFrequencyAutoTradingService : AutoTradingStrategyServiceBase, IHighFrequencyAutoTradingService
    {
        private const int CheckRateMilliseconds = 100;
        private decimal OriginalInvestment = 100000; // set the original invetsment to be 100,000.00
        private readonly ILogger<HighFrequencyAutoTradingService> _logger;
        private readonly IDictionary<string, HighFrequencyAssets> _CurrentAssets;

        public HighFrequencyAutoTradingService(ILogger<HighFrequencyAutoTradingService> logger, IPricingService pricingService)
            : base(CheckRateMilliseconds, logger, pricingService)
        {
            _logger = logger;
            _CurrentAssets = GetCurrentAssets(pricingService.GetTickers().Keys.ToList());
        }

        public decimal GetFundValue()
        {
            var tickers = _pricingService.GetTickers();

            return _CurrentAssets.Sum(x=> x.Value.QuantityOwned * tickers[x.Key]) + OriginalInvestment;
        }

        private IDictionary<string, HighFrequencyAssets> GetCurrentAssets(IList<string> tickers)
        {
            var assets = new Dictionary<string, HighFrequencyAssets>();
            foreach (var ticker in tickers)
            {
                assets.Add(ticker, new HighFrequencyAssets() { PurchasePrice = 0, QuantityOwned = 0 });
            }

            return assets;
        }

        private void BuyAssets(string Ticker, decimal Price)
        {
            //Buy if we have funds
            var asset = _CurrentAssets[Ticker];
            try
            {
                if(OriginalInvestment > 100 * Price)
                {
                    var profit = _pricingService.Buy(Ticker, 100, 0, Price);
                    OriginalInvestment += profit;
                    asset.QuantityOwned = 100;
                    asset.PurchasePrice = Price;
                    _logger.LogInformation("Fund value : {0}, ticker {1}", GetFundValue(), Ticker);
                }
                else
                {
                    var quantity = (int)Math.Floor(OriginalInvestment/ Price);
                    if (quantity == 0) return;
                    var profit = _pricingService.Buy(Ticker, quantity, 0, Price);
                    OriginalInvestment += profit;
                    asset.QuantityOwned = quantity;
                    asset.PurchasePrice = Price;
                    _logger.LogInformation("Fund value : {0}, ticker {1}", GetFundValue(), Ticker);
                }

            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.LogInformation(ex, "Failed to Execute Strategy for {0}, Quantity was invalid", Ticker);
            }
            catch (ArgumentException ex)
            {
                _logger.LogInformation(ex, "Failed to Execute Strategy for {0}, Ticker was invalid", Ticker);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Failed to Execute Strategy for {0}", Ticker);
            }
        }
        private void SellAssets(string Ticker, decimal Price)
        {
            var currentAsset = _CurrentAssets[Ticker];
            //Sell if we have any
            try
            {
                var profit = _pricingService.Sell(Ticker, currentAsset.QuantityOwned, currentAsset.PurchasePrice, Price);
                OriginalInvestment += profit + (currentAsset.PurchasePrice * currentAsset.QuantityOwned);
                currentAsset.QuantityOwned = 0; 
                currentAsset.PurchasePrice = 0;
                _logger.LogInformation("Fund value : {0}, ticker {1}", GetFundValue(), Ticker);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.LogInformation(ex, "Failed to Execute Strategy for {0}, Quantity was invalid", Ticker);
            }
            catch (ArgumentException ex)
            {
                _logger.LogInformation(ex, "Failed to Execute Strategy for {0}, Ticker was invalid", Ticker);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Failed to Execute Strategy for {0}", Ticker);
            }
        }
        protected async override Task<int> CheckTradingStrategies()
        {
            _logger.LogInformation("Fund value : {0}", GetFundValue());
            foreach (var currentAsset in _CurrentAssets)
            {

                var currentPrice = await GetCurrentPrice(currentAsset.Key);

                if (currentPrice == null) continue;

                if (currentAsset.Value.QuantityOwned == 0)
                {
                    BuyAssets(currentAsset.Key, currentPrice.Value);
                    continue;
                }

                if(currentPrice > currentAsset.Value.PurchasePrice && currentAsset.Value.QuantityOwned > 0)
                {
                    SellAssets(currentAsset.Key, currentPrice.Value);
                    continue;
                }
            }

            return 0;
        }
    }
}
