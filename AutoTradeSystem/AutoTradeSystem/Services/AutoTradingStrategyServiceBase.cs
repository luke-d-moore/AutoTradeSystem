namespace AutoTradeSystem.Services
{
    public abstract class AutoTradingStrategyServiceBase : BackgroundService
    {

        private readonly int _checkRate;

        private readonly ILogger<AutoTradingStrategyServiceBase> _logger;
        protected readonly IPricingService _pricingService;

        protected AutoTradingStrategyServiceBase(int checkRate, ILogger<AutoTradingStrategyServiceBase> logger, IPricingService pricingService)
        {
            _checkRate = checkRate;
            _logger = logger;
            _pricingService = pricingService;

        }

        protected async Task<decimal?> GetCurrentPrice(string ticker)
        {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception Thrown");
                return null;
            }

            return quote;
        }

        protected abstract Task CheckTradingStrategies();

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Automatic Trade System Service is starting.");
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await CheckTradingStrategies().ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "An exception occurred");
                    throw;
                }

                await Task.Delay(_checkRate, cancellationToken).ConfigureAwait(false);
            }

            _logger.LogInformation("Automatic Trade System Service is stopping.");
        }
    }
}
