using AutoTradeSystem.Dtos;
using System.Collections.Concurrent;

namespace AutoTradeSystem.Services
{
    public class AutoTradingStrategyService : AutoTradingStrategyServiceBase, IAutoTradingStrategyService
    {
        private const int CheckRateMilliseconds = 5000;
        private readonly ILogger<AutoTradingStrategyService> _logger;
        private readonly IDictionary<string, TradingStrategy> _Strategies = new ConcurrentDictionary<string, TradingStrategy>();
        private readonly object _CheckStrategiesLock = new object();
        private readonly IPricingService _pricingService;
        private readonly IDictionary<string, decimal> _CurrentPrices = new ConcurrentDictionary<string, decimal>();

        public AutoTradingStrategyService(ILogger<AutoTradingStrategyService> logger, IPricingService pricingService)
            : base(CheckRateMilliseconds, logger)
        {
            _logger = logger;
            _pricingService = pricingService;
        }
        public IDictionary<string, TradingStrategy> GetStrategies()
        {
            return _Strategies;
        }

        private async Task GetCurrentPrices()
        {
            await Task.Delay(1);
            Parallel.ForEach(_CurrentPrices, async currentPrice =>
            {
                var price = await GetCurrentPrice(currentPrice.Key);
                if (price == null)
                {
                    _logger.LogInformation("Failed to get current price for {0}", currentPrice.Key);
                    return;
                }
                _CurrentPrices[currentPrice.Key] = price.Value;
            });
        }

        public async Task<bool> AddStrategy(TradingStrategyDto tradingStrategy)
        {
            if (tradingStrategy == null) return false;
            if (tradingStrategy.Ticker == null || (tradingStrategy.Ticker.Length > 5 || tradingStrategy.Ticker.Length < 3))
            {
                return false;
            }
            if (tradingStrategy.Quantity <= 0)
            {
                return false;
            }
            if (tradingStrategy.PriceChange <= 0)
            {
                return false;
            }
            var actionPrice = await GetActionPrice(tradingStrategy);

            if (actionPrice.ActionPrice == null || actionPrice.OriginalPrice == null)
            {
                return false;
            }

            _CurrentPrices.TryAdd(tradingStrategy.Ticker, actionPrice.OriginalPrice.Value);

            var id = Guid.NewGuid().ToString();

            var strategy = new TradingStrategy(actionPrice.ActionPrice.Value, tradingStrategy, actionPrice.OriginalPrice.Value);

            var added = _Strategies.TryAdd(
                id,
                strategy
                );

            if (added)
            {
                _logger.LogInformation("Strategy Added Successfully {0}", id);
            }
            else
            {
                _logger.LogError("Failed to Add Strategy {@strategy}", strategy);
            }

            return added;
        }

        private async Task<(decimal? ActionPrice, decimal? OriginalPrice)> GetActionPrice(TradingStrategyDto tradingStrategy)
        {
            //we need to keep a record of the price that we will action the strategy
            //PriceMovement is a percentage
            //if Buy then % is a decrease
            //if Sell then % is an increase

            decimal movement =
                tradingStrategy.TradeAction == TradeAction.Sell ?
                tradingStrategy.PriceChange :
                -tradingStrategy.PriceChange;

            decimal multiplyfactor = (100 + movement) / 100.0m;

            decimal? quote = await GetCurrentPrice(tradingStrategy.Ticker);

            if(quote == null) return (null, null);

            return (quote * multiplyfactor, quote);
        }

        private async Task<decimal?> GetCurrentPrice(string ticker)
        {
            //we need to keep a record of the price that we will action the strategy
            //PriceMovement is a percentage
            //if Buy then % is a decrease
            //if Sell then % is an increase

            decimal quote;

            try
            {
                quote = await _pricingService.GetCurrentPrice(ticker);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid Ticker {0}", ticker);
                return null;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Exception Thrown");
                return null;
            }

            return quote;
        }
        public async Task<bool> RemoveStrategy(string ID)
        {
            await Task.Delay(1);
            if(ID == null) return false;
            var removed =_Strategies.Remove(ID);

            if (removed)
            {
                _logger.LogInformation("Strategy Removed Successfully {0}", ID);
            }
            else
            {
                _logger.LogError("Failed to Add Strategy {0}", ID);
            }

            return removed; 
        }

        private void RemoveStrategies(IList<string> IdsToRemove)
        {
            foreach(var id in IdsToRemove)
            {
                if (_Strategies.Remove(id))
                {
                    _logger.LogInformation("Removed Stretegy Successfully {0}", id);
                }
                else
                {
                    _logger.LogError("Failed to Remove Stretegy {0}", id);
                }
            }
        }
        protected async override Task<int> CheckTradingStrategies()
        {
            await GetCurrentPrices();

            lock (_CheckStrategiesLock)
            {
                var IDsToRemove = new List<string>();

                foreach(var strategy in _Strategies)
                {

                    var currentPrice = _CurrentPrices[strategy.Value.TradingStrategyDto.Ticker];

                    if (currentPrice >= strategy.Value.ActionPrice && strategy.Value.TradingStrategyDto.TradeAction == TradeAction.Sell)
                    {
                        try
                        {
                            var profit = _pricingService.Sell(strategy.Value.TradingStrategyDto.Ticker, strategy.Value.TradingStrategyDto.Quantity, strategy.Value.OriginalPrice, currentPrice);
                            _logger.LogInformation("Successfully Executed Strategy for {@strategy} profit : {0}", strategy, profit);
                            IDsToRemove.Add(strategy.Key);
                            continue;
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                            _logger.LogInformation(ex, "Failed to Execute Strategy for {@strategy}, Quantity was invalid", strategy);
                        }
                        catch (ArgumentException ex)
                        {
                            _logger.LogInformation(ex, "Failed to Execute Strategy for {@strategy}, Ticker was invalid", strategy);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogInformation(ex, "Failed to Execute Strategy for {@strategy}", strategy);
                        }
                    }

                    if (currentPrice <= strategy.Value.ActionPrice && strategy.Value.TradingStrategyDto.TradeAction == TradeAction.Buy)
                    {
                        try
                        {
                            var profit = _pricingService.Buy(strategy.Value.TradingStrategyDto.Ticker, strategy.Value.TradingStrategyDto.Quantity, strategy.Value.OriginalPrice, currentPrice);
                            _logger.LogInformation("Successfully Executed Strategy for {@strategy} profit : {0}", strategy, profit);
                            IDsToRemove.Add(strategy.Key);
                            continue;
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                            _logger.LogInformation(ex, "Failed to Execute Strategy for {@strategy}, Quantity was invalid", strategy);
                        }
                        catch (ArgumentException ex)
                        {
                            _logger.LogInformation(ex, "Failed to Execute Strategy for {@strategy}, Ticker was invalid", strategy);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogInformation(ex, "Failed to Execute Strategy for {@strategy}", strategy);
                        }
                    }
                }

                RemoveStrategies(IDsToRemove);
            }

            return 0;
        }
    }
}
