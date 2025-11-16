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
        private const int _checkRate = 500;
        private readonly ILogger<AutoTradingStrategyService> _logger;
        private readonly ConcurrentDictionary<string, TradingStrategy> _Strategies = new ConcurrentDictionary<string, TradingStrategy>();
        private readonly IPricingService _pricingService;
        private readonly ITradeActionService _tradeActionService;
        private HashSet<TradeAction> _validActions = new HashSet<TradeAction>() { TradeAction.Buy, TradeAction.Sell };
        public ConcurrentDictionary<string, TradingStrategy> Strategies
        {
            get { return _Strategies; }
        }
        public HashSet<TradeAction> ValidActions
        {
            get { return _validActions; }
        }
        public AutoTradingStrategyService(ILogger<AutoTradingStrategyService> logger, 
            IPricingService pricingService, 
            ITradeActionService tradeActionService)
            : base(_checkRate, logger)
        {
            _logger = logger;
            _pricingService = pricingService;
            _tradeActionService = tradeActionService;
        }
        public IDictionary<string, TradingStrategy> GetStrategies()
        {
            return Strategies;
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
            if (!ValidActions.Contains(TradingStrategy.TradeAction))
            {
                _logger.LogError($"Failed to {CalledFrom}, Invalid Trade Action. Valid values are 0 (Buy) or 1 (Sell).");
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

            var allowedTickers = _pricingService.GetLatestTickers().ToHashSet(StringComparer.OrdinalIgnoreCase);

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

            if (Strategies.TryAdd(id, strategy))
            {
                _logger.LogInformation($"Strategy Added Successfully {id}");
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

            decimal? quote = OriginalPrice == 0m ? 
                _pricingService.GetLatestPriceFromTicker(tradingStrategy.Ticker) : 
                OriginalPrice;

            if(quote == null) return (null, null);

            return (quote * multiplyfactor, quote);
        }
        public async Task<bool> RemoveStrategy(string ID)
        {
            await Task.Delay(0);
            if(ID == null) return false;

            if (Strategies.TryRemove(ID, out var removedStrategy))
            {
                _logger.LogInformation($"Strategy Removed Successfully {ID}");
                return true;
            }
            else
            {
                _logger.LogError($"Failed to Remove Strategy {ID}");
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

            if (!Strategies.TryGetValue(ID, out var currentStrategy))
            {
                _logger.LogError($"Failed to Update Strategy, ID was not found : {ID}");
                return false;
            }

            if (!await ValidateStrategy(tradingStrategy, "Update Strategy").ConfigureAwait(false)) return false;

            var newActionPrice = await GetActionPrice(
                tradingStrategy, 
                currentStrategy.OriginalPrice)
                .ConfigureAwait(false);
            if (!ValidateActionPrice(newActionPrice.ActionPrice, newActionPrice.OriginalPrice, "Update Strategy")) return false;

            currentStrategy.TradingStrategyDto.TradeAction = tradingStrategy.TradeAction;
            currentStrategy.TradingStrategyDto.Ticker = tradingStrategy.Ticker;
            currentStrategy.TradingStrategyDto.Quantity = tradingStrategy.Quantity;
            currentStrategy.TradingStrategyDto.PriceChange = tradingStrategy.PriceChange;
            currentStrategy.ActionPrice = newActionPrice.ActionPrice.Value;

            _logger.LogInformation($"Strategy Updated Successfully {ID}");

            return true;
        }
        private void RemoveStrategies(IList<string> IdsToRemove)
        {
            foreach(var id in IdsToRemove)
            {
                if (Strategies.TryRemove(id, out var removedStrategy))
                {
                    _logger.LogInformation($"Removed Stretegy Successfully {id}");
                }
                else
                {
                    _logger.LogError($"Failed to Remove Stretegy {id}");
                }
            }
        }
        protected async override Task<int> CheckTradingStrategies(CancellationToken cancellationToken)
        {
            var IDsToRemove = new List<string>();

            var currentPrices = _pricingService.GetLatestPrices();

            if (!currentPrices.Any())
            {
                _logger.LogError("Failed to get current prices");
                return 0;
            }

            foreach (var strategy in Strategies)
            {

                if (!currentPrices.TryGetValue(strategy.Value.TradingStrategyDto.Ticker, out var currentPrice)) continue;

                if (currentPrice >= strategy.Value.ActionPrice && 
                    strategy.Value.TradingStrategyDto.TradeAction == TradeAction.Sell)
                {
                    try
                    {
                        await _tradeActionService.PublishMessage(
                            strategy.Value.TradingStrategyDto.Ticker, 
                            strategy.Value.TradingStrategyDto.Quantity, 
                            strategy.Value.TradingStrategyDto.TradeAction.ToString(), 
                            cancellationToken);
                        _logger.LogInformation("Successfully Executed Strategy for {@strategy}", strategy);
                        IDsToRemove.Add(strategy.Key);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to Execute Strategy for {@strategy}", strategy);
                    }
                }

                if (currentPrice <= strategy.Value.ActionPrice && 
                    strategy.Value.TradingStrategyDto.TradeAction == TradeAction.Buy)
                {
                    try
                    {
                        await _tradeActionService.PublishMessage(
                            strategy.Value.TradingStrategyDto.Ticker, 
                            strategy.Value.TradingStrategyDto.Quantity, 
                            strategy.Value.TradingStrategyDto.TradeAction.ToString(), 
                            cancellationToken);
                        _logger.LogInformation("Successfully Executed Strategy for {@strategy}", strategy);
                        IDsToRemove.Add(strategy.Key);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to Execute Strategy for {@strategy}", strategy);
                    }
                }
            }

            RemoveStrategies(IDsToRemove);

            return 0;
        }
    }
}
