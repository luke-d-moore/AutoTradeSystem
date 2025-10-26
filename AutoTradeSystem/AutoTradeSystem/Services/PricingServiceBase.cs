namespace AutoTradeSystem.Services
{
    public abstract class PricingServiceBase : BackgroundService
    {

        private readonly int _checkRate;
        private readonly ILogger<PricingServiceBase> _logger;

        protected PricingServiceBase(int checkRate, ILogger<PricingServiceBase> logger)
        {
            _checkRate = checkRate;
            _logger = logger;
        }

        protected abstract Task<bool> SetCurrentPrices();

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Pricing Service is starting.");
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await SetCurrentPrices().ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "An exception occurred");
                    throw;
                }

                await Task.Delay(_checkRate, cancellationToken).ConfigureAwait(false);
            }

            _logger.LogInformation("Pricing Service is stopping.");
        }
    }
}
