using AutoTradeSystem.Dtos;
using AutoTradeSystem.Interfaces;
using System.Collections.Concurrent;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace AutoTradeSystem.Services
{
    public class TradeActionService : ITradeActionService
    {
        private readonly ILogger<TradeActionService> _logger;
        private readonly IDictionary<string, TradingStrategy> _Strategies = new ConcurrentDictionary<string, TradingStrategy>();
        private readonly string _exchangeName;
        private readonly string _hostName;

        public TradeActionService(ILogger<TradeActionService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _exchangeName = configuration["RabbitMQExchange"];
            _hostName = configuration["ConnectionHostName"];
        }
        public async Task PublishMessage(string ticker, int quantity, string action)
        {
            var factory = new ConnectionFactory { HostName = _hostName };
            using var connection = await factory.CreateConnectionAsync();
            using (var channel = await connection.CreateChannelAsync())
            {
                var message = new Message(ticker, quantity, action);
                var jsonMessage = JsonSerializer.Serialize(message);
                _logger.LogInformation($"Message : {jsonMessage}");
                var body = Encoding.UTF8.GetBytes(jsonMessage);

                await channel.BasicPublishAsync(exchange: _exchangeName, routingKey:string.Empty, body: body);
            }
        }
    }
}
