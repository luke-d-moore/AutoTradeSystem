using AutoTradeSystem.Dtos;
using AutoTradeSystem.Interfaces;
using System.Collections.Concurrent;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace AutoTradeSystem.Services
{
    public class AutoTradingStrategyService : AutoTradingStrategyServiceBase, IAutoTradingStrategyService
    {
        private const int _checkRate = 30000;
        private readonly ILogger<AutoTradingStrategyService> _logger;
        private readonly IDictionary<string, TradingStrategy> _Strategies = new ConcurrentDictionary<string, TradingStrategy>();
        private readonly IPricingService _pricingService;
        private readonly ITradeActionService _tradeActionService;

        public AutoTradingStrategyService(ILogger<AutoTradingStrategyService> logger, IPricingService pricingService, ITradeActionService tradeActionService)
            : base(_checkRate, logger)
        {
            _logger = logger;
            _pricingService = pricingService;
            _tradeActionService = tradeActionService;
        }
        public IDictionary<string, TradingStrategy> GetStrategies()
        {
            return _Strategies;
        }

        private async Task<bool> ValidateStrategy(TradingStrategyDto TradingStrategy, string CalledFrom)
        {
            if (TradingStrategy == null)
            {
                _logger.LogError($"Failed to {CalledFrom}, tradingStrategy was null");
                return false;
            }
            if (TradingStrategy.Ticker == null || (TradingStrategy.Ticker.Length > 5 || TradingStrategy.Ticker.Length < 3))
            {
                _logger.LogError($"Failed to {CalledFrom}, Ticker was invalid.");
                return false;
            }
            if (TradingStrategy.PriceChange <= 0)
            {
                _logger.LogError($"Failed to {CalledFrom}, Price change must be greater than 0");
                return false;
            }
            if (TradingStrategy.Quantity <= 0)
            {
                _logger.LogError($"Failed to {CalledFrom}, Quantity must be greater than 0");
                return false;
            }

            var allowedTickers = (await _pricingService.GetTickers().ConfigureAwait(false)).ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (allowedTickers.Contains(TradingStrategy.Ticker))
            {
                TradingStrategy.Ticker = allowedTickers.First(x => x.Equals(TradingStrategy.Ticker, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                _logger.LogError($"Failed to {CalledFrom}, Ticker was invalid. Ticker was : {TradingStrategy.Ticker}");
                return false;
            }

            return true;
        }

        private bool ValidateActionPrice(decimal? ActionPrice, decimal? OriginalPrice, string CalledFrom)
        {
            if (!ActionPrice.HasValue || !OriginalPrice.HasValue)
            {
                _logger.LogError($"Failed to {CalledFrom}, Failed to get ActionPrice ");
                return false;
            }
            return true;
        }

        public async Task<bool> AddStrategy(TradingStrategyDto tradingStrategy)
        {
            if(!await ValidateStrategy(tradingStrategy, "Add Strategy").ConfigureAwait(false)) return false;

            var actionPrice = await GetActionPrice(tradingStrategy).ConfigureAwait(false);

            if(!ValidateActionPrice(actionPrice.ActionPrice, actionPrice.OriginalPrice, "Add Strategy")) return false;

            var id = Guid.NewGuid().ToString();

            var strategy = new TradingStrategy(actionPrice.ActionPrice.Value, tradingStrategy, actionPrice.OriginalPrice.Value);

            if (_Strategies.TryAdd(id, strategy))
            {
                _logger.LogInformation("Strategy Added Successfully {0}", id);
                return true;
            }
            else
            {
                _logger.LogError("Failed to Add Strategy {@strategy}", strategy);
                return false;
            }
        }

        private async Task<(decimal? ActionPrice, decimal? OriginalPrice)> GetActionPrice(TradingStrategyDto tradingStrategy, decimal OriginalPrice = 0m)
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

            decimal? quote = OriginalPrice == 0m ? await _pricingService.GetPriceFromTicker(tradingStrategy.Ticker).ConfigureAwait(false) : OriginalPrice;

            if(quote == null) return (null, null);

            return (quote * multiplyfactor, quote);
        }

        public async Task<bool> RemoveStrategy(string ID)
        {
            await Task.Delay(0);
            if(ID == null) return false;

            if (_Strategies.Remove(ID))
            {
                _logger.LogInformation("Strategy Removed Successfully {0}", ID);
                return true;
            }
            else
            {
                _logger.LogError("Failed to Remove Strategy {0}", ID);
                return false;
            }
        }
        public async Task<bool> UpdateStrategy(string ID, TradingStrategyDto tradingStrategy)
        {
            if (string.IsNullOrEmpty(ID))
            {
                _logger.LogError("Failed to Update Strategy, ID was null");
                return false;
            }

            if (!_Strategies.TryGetValue(ID, out var currentStrategy))
            {
                _logger.LogError("Failed to Update Strategy, ID was not found : {0}", ID);
                return false;
            }

            if (!await ValidateStrategy(tradingStrategy, "Update Strategy").ConfigureAwait(false)) return false;

            var newActionPrice = await GetActionPrice(tradingStrategy, currentStrategy.OriginalPrice).ConfigureAwait(false);
            if (!ValidateActionPrice(newActionPrice.ActionPrice, newActionPrice.OriginalPrice, "Update Strategy")) return false;

            currentStrategy.TradingStrategyDto.TradeAction = tradingStrategy.TradeAction;
            currentStrategy.TradingStrategyDto.Ticker = tradingStrategy.Ticker;
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
                await _tradeActionService.PublishMessage(strategy.Value.TradingStrategyDto.Ticker, strategy.Value.TradingStrategyDto.Quantity, "Sell");
                if (!currentPrices.TryGetValue(strategy.Value.TradingStrategyDto.Ticker, out var currentPrice)) continue;

                if (currentPrice >= strategy.Value.ActionPrice && strategy.Value.TradingStrategyDto.TradeAction == TradeAction.Sell)
                {
                    try
                    {
                        await _tradeActionService.PublishMessage(strategy.Value.TradingStrategyDto.Ticker, strategy.Value.TradingStrategyDto.Quantity, "Sell");
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
                        await _tradeActionService.PublishMessage(strategy.Value.TradingStrategyDto.Ticker, strategy.Value.TradingStrategyDto.Quantity, "Buy");
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
