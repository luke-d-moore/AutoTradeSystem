namespace AutoTradeSystem.Server.Services
{
    public abstract class AutoTradingStrategyServiceBase : BackgroundService
    {

        private readonly int _checkRate;

        private readonly ILogger<AutoTradingStrategyServiceBase> _logger;

        protected AutoTradingStrategyServiceBase(int checkRate, ILogger<AutoTradingStrategyServiceBase> logger)
        {
            _checkRate = checkRate;
            _logger = logger;
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
