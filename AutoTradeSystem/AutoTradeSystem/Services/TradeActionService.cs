using AutoTradeSystem.Dtos;
using AutoTradeSystem.Interfaces;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace AutoTradeSystem.Services
{
    public class TradeActionService : TradeActionServiceBase, ITradeActionService
    {
        private const int _checkRate = 100;
        private readonly ILogger<TradeActionService> _logger;
        private readonly string _exchangeName;
        private readonly string _hostName;
        private readonly ConcurrentQueue<Message> _messagePayloads = new ConcurrentQueue<Message>();
        private readonly IConnectionFactory _connectionFactory;
        private const int _networkRecoveryInterval = 10;
        public string HostName
        {
            get => _hostName;
        }
        public string ExchangeName
        {
            get => _exchangeName;
        }

        public ConcurrentQueue<Message> MessagePayloads => _messagePayloads;

        public TradeActionService(ILogger<TradeActionService> logger, IConfiguration configuration, IConnectionFactory connectionFactory)
        :base(logger, TimeSpan.FromMilliseconds(_checkRate))
        {
            _logger = logger;
            _exchangeName = configuration["RabbitMQExchange"];
            _hostName = configuration["ConnectionHostName"];
            _connectionFactory = connectionFactory;
        }

        public async Task EnqueueMessage(string ticker, int quantity, string action)
        {
            var message = new Message(ticker, quantity, action);
            MessagePayloads.Enqueue(message);
        }

        protected override async Task PublishMessages(CancellationToken cancellationToken)
        {
            await PublishMessagesAsync(cancellationToken);
        }
        public async Task PublishMessagesAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && MessagePayloads.TryDequeue(out var message))
            {
                bool messagePublished = false;

                while (!messagePublished && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
                        using var channel = await connection.CreateChannelAsync(null, cancellationToken).ConfigureAwait(false);

                        var jsonMessage = JsonSerializer.Serialize(message);
                        _logger.LogInformation($"Publishing Message ID : {message?.UniqueID}, Message : {jsonMessage}");
                        var body = Encoding.UTF8.GetBytes(jsonMessage);

                        await channel.BasicPublishAsync(
                            exchange: ExchangeName,
                            routingKey: string.Empty,
                            mandatory: true,
                            body: body).ConfigureAwait(false);

                        _logger.LogInformation($"Message published successfully.");
                        messagePublished = true;
                    }
                    catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogError(ex, $"Application shutting down");
                        // If the application is shutting down while trying to connect, exit gracefully
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to publish message, Retrying connection/publish in {_networkRecoveryInterval} seconds.");
                        await Task.Delay(TimeSpan.FromSeconds(_networkRecoveryInterval), cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
