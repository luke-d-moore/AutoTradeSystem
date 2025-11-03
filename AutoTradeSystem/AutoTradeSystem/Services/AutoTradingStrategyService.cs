using AutoTradeSystem.Dtos;
using AutoTradeSystem.Interfaces;
using System.Collections.Concurrent;

namespace AutoTradeSystem.Services
{
    public class AutoTradingStrategyService : AutoTradingStrategyServiceBase, IAutoTradingStrategyService
    {
        private const int _checkRate = 5000;
        private readonly ILogger<AutoTradingStrategyService> _logger;
        private readonly IDictionary<string, TradingStrategy> _Strategies = new ConcurrentDictionary<string, TradingStrategy>();
        private readonly IPricingService _pricingService;

        public AutoTradingStrategyService(ILogger<AutoTradingStrategyService> logger, IPricingService pricingService)
            : base(_checkRate, logger)
        {
            _logger = logger;
            _pricingService = pricingService;
        }
        public IDictionary<string, TradingStrategy> GetStrategies()
        {
            return _Strategies;
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

            var allowedTickers = (await _pricingService.GetTickers().ConfigureAwait(false)).ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (allowedTickers.Contains(tradingStrategy.Ticker))
            {
                tradingStrategy.Ticker = allowedTickers.First(x => x.Equals(tradingStrategy.Ticker, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                return false;
            }

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

        private async Task<(decimal? ActionPrice, decimal? OriginalPrice)> GetActionPrice(TradingStrategyDto tradingStrategy, decimal OriginalPrice = 0)
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

            decimal? quote = OriginalPrice == 0 ? await _pricingService.GetPriceFromTicker(tradingStrategy.Ticker).ConfigureAwait(false) : OriginalPrice;

            if(quote == null) return (null, null);

            return (quote * multiplyfactor, quote);
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
                _logger.LogError("Failed to Remove Strategy {0}", ID);
            }

            return removed; 
        }
        public async Task<bool> UpdateStrategy(string ID, TradingStrategyDto tradingStrategy)
        {
            if (string.IsNullOrEmpty(ID))
            {
                _logger.LogError("Failed to Update Strategy, ID was null");
                return false;
            }
            if (tradingStrategy == null)
            {
                _logger.LogError("Failed to Update Strategy, tradingStrategy was null");
                return false;
            }
            if (tradingStrategy.PriceChange <= 0)
            {
                _logger.LogError("Failed to Update Strategy, Price change must be greater than 0");
                return false;
            }
            if (tradingStrategy.Quantity <= 0)
            {
                _logger.LogError("Failed to Update Strategy, Quantity must be greater than 0");
                return false;
            }
            var currentStrategy = new TradingStrategy();
            if (!_Strategies.TryGetValue(ID, out currentStrategy))
            {
                _logger.LogError("Failed to Update Strategy, ID was not found : {0}", ID);
                return false;
            }

            var newActionPrice = await (GetActionPrice(tradingStrategy, currentStrategy.OriginalPrice));
            if (!newActionPrice.ActionPrice.HasValue)
            {
                _logger.LogError("Failed to Update Strategy, update ActionPrice Failed {@strategy}", tradingStrategy);
                return false;
            }

            currentStrategy.TradingStrategyDto.TradeAction = tradingStrategy.TradeAction;
            currentStrategy.TradingStrategyDto.Quantity = tradingStrategy.Quantity;
            currentStrategy.TradingStrategyDto.PriceChange = tradingStrategy.PriceChange;
            currentStrategy.ActionPrice = newActionPrice.ActionPrice.Value;

            _logger.LogInformation("Strategy Updated Successfully {0}", ID);

            return true;
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
            var IDsToRemove = new List<string>();

            var currentPrices = await _pricingService.GetPrices().ConfigureAwait(false);

            if (!currentPrices.Any()) return 0;

            foreach (var strategy in _Strategies)
            {
                decimal currentPrice = 0m;

                if (!currentPrices.TryGetValue(strategy.Value.TradingStrategyDto.Ticker, out currentPrice)) continue;

                if (currentPrice >= strategy.Value.ActionPrice && strategy.Value.TradingStrategyDto.TradeAction == TradeAction.Sell)
                {
                    try
                    {
                        //Publish a message to the queue, with the ticker value, Quantity To be Sold and also the Trade Action
                        //var profit = _tradeActionService.Sell(strategy.Value.TradingStrategyDto.Ticker, strategy.Value.TradingStrategyDto.Quantity, strategy.Value.OriginalPrice);
                        _logger.LogInformation("Successfully Executed Strategy for {@strategy} profit : {0}", strategy);
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
                        //Publish a message to the queue, with the ticker value, Quantity To be Sold and also the Trade Action
                        //var profit = _tradeActionService.Buy(strategy.Value.TradingStrategyDto.Ticker, strategy.Value.TradingStrategyDto.Quantity, strategy.Value.OriginalPrice);
                        _logger.LogInformation("Successfully Executed Strategy for {@strategy} profit : {0}", strategy);
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

            return 0;
        }
    }
}
