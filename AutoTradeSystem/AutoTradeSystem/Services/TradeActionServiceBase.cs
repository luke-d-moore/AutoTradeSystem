namespace AutoTradeSystem.Services
{
    public abstract class TradeActionServiceBase : BackgroundService
    {

        private readonly ILogger<TradeActionServiceBase> _logger;
        private readonly TimeSpan _checkRateTimeSpan;

        protected TradeActionServiceBase(ILogger<TradeActionServiceBase> logger, TimeSpan checkRate)
        {
            _logger = logger;
            _checkRateTimeSpan = checkRate;
        }
        protected abstract Task PublishMessages(CancellationToken cancellationToken);

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Trade Action Service is starting.");
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await PublishMessages(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Publish Messages Failed.");
                }

                await Task.Delay(_checkRateTimeSpan, cancellationToken).ConfigureAwait(false);
            }
            _logger.LogInformation("Trade Action Service is stopping.");
        }
    }
}
