using AutoTradeSystem.Dtos;
using AutoTradeSystem.Interfaces;
using System.Collections.Concurrent;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Globalization;

namespace AutoTradeSystem.Services
{
    public class TradeActionService : ITradeActionService
    {
        private readonly ILogger<TradeActionService> _logger;
        private readonly IDictionary<string, TradingStrategy> _Strategies = new ConcurrentDictionary<string, TradingStrategy>();
        private readonly string _exchangeName;
        private readonly string _hostName;
        private const int _networkRecoveryInterval = 10;

        public TradeActionService(ILogger<TradeActionService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _exchangeName = configuration["RabbitMQExchange"];
            _hostName = configuration["ConnectionHostName"];
        }

        public async Task PublishMessage(string ticker, int quantity, string action, CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(_networkRecoveryInterval)
            };

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var connection = await factory.CreateConnectionAsync().ConfigureAwait(false);
                    using var channel = await connection.CreateChannelAsync().ConfigureAwait(false);

                    var message = new Message(ticker, quantity, action);
                    var jsonMessage = JsonSerializer.Serialize(message);

                    _logger.LogInformation($"Publishing Message ID : {message.UniqueID}, Message : {jsonMessage} at {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}");
                    var body = Encoding.UTF8.GetBytes(jsonMessage);

                    await channel.BasicPublishAsync(
                        exchange: _exchangeName,
                        routingKey: string.Empty,
                        mandatory: true,
                        body: body).ConfigureAwait(false);

                    _logger.LogInformation($"Message published successfully, at {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}");
                    break;
                }
                catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, $"Application shutting down at {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}");
                    // If the application is shutting down while trying to connect, exit gracefully
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to publish message at {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}, Retrying connection/publish in {_networkRecoveryInterval} seconds.");
                    await Task.Delay(TimeSpan.FromSeconds(_networkRecoveryInterval), cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
