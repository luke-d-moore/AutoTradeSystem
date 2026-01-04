namespace AutoTradeSystem.Services
{
    public abstract class PricingServiceBase : BackgroundService
    {
        private readonly ILogger<PricingServiceBase> _logger;
        protected PricingServiceBase(ILogger<PricingServiceBase> logger)
        {
            _logger = logger;
        }
        protected abstract Task UpdatePrices(CancellationToken cancellationToken);

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Pricing Service is starting.");
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await UpdatePrices(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get and set prices.");
                }
            }
            _logger.LogInformation("Pricing Service is stopping.");
        }
    }
}
