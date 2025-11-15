using AutoTradeSystem;
using AutoTradeSystem.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using AutoTradeSystem.Interfaces;
using System.Collections.Concurrent;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Serilog.Filters;
using RabbitMQ.Client;
using System.Net.Sockets;
using System.Text;

namespace AutoTradeSystemTests
{
    public class TradeActionServiceTests
    {
        private readonly Mock<ILogger<TradeActionService>> _tradeActionLogger;
        private readonly Mock<IConfiguration> _configuration;
        private readonly Mock<IConnectionFactory> _mockFactory = new Mock<IConnectionFactory>();
        private readonly Mock<IConnection> _mockConnection = new Mock<IConnection>();
        private readonly Mock<IChannel> _mockChannel = new Mock<IChannel>();
        private readonly TradeActionService _service;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public TradeActionServiceTests()
        {
            _tradeActionLogger = new Mock<ILogger<TradeActionService>>();
            _configuration = new Mock<IConfiguration>();
            _configuration.SetupGet(x => x[It.Is<string>(s => s == "RabbitMQExchange")])
                                      .Returns("Exchange");
            _configuration.SetupGet(x => x[It.Is<string>(s => s == "ConnectionHostName")])
                                      .Returns("Queue");
            _configuration.SetupGet(x => x[It.Is<string>(s => s == "NetworkRecoveryIntervalSeconds")])
                          .Returns("10");
            _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_mockConnection.Object);
            _mockConnection.Setup(c => c.CreateChannelAsync(null,It.IsAny<CancellationToken>())).ReturnsAsync(_mockChannel.Object);

            _service = new TradeActionService(_tradeActionLogger.Object, _configuration.Object, _mockFactory.Object);
        }

        [Fact]
        public async Task PublishMessage_PublishedMessage_PublishesMessageAndBreaksLoop()
        {
            // Arrange
            var ticker = "TEST";
            var quantity = 10;
            var action = "BUY";

            // Act
            await _service.PublishMessage(ticker, quantity, action, CancellationToken.None);

            // Assert
            _mockFactory.Verify(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockConnection.Verify(c => c.CreateChannelAsync(null, It.IsAny<CancellationToken>()), Times.Once);

            _tradeActionLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Message published successfully.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public async Task PublishMessage_TransientConnectionFailure_RetriesAndSucceeds()
        {
            // Arrange
            _mockFactory.SetupSequence(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new SocketException())
                .ReturnsAsync(_mockConnection.Object);

            // Act
            await _service.PublishMessage("TEST", 1, "BUY", CancellationToken.None);

            // Assert
            _mockFactory.Verify(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));

            _tradeActionLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Failed to publish message, Retrying connection/publish in {_configuration.Object["NetworkRecoveryIntervalSeconds"]} seconds.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);

            _tradeActionLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Message published successfully.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public async Task PublishMessage_WhenCancellationTokenIsCancelledDuringConnection_ThrowsOperationCanceledException()
        {
            // Arrange
            _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
                .Returns(async (CancellationToken ct) =>
                {
                    await Task.Delay(2000, ct); 
                    return _mockConnection.Object;
                });

            _cancellationTokenSource.CancelAfter(500);

            // Act and Assert
            await Assert.ThrowsAnyAsync<Exception>(()=> _service.PublishMessage("TEST", 1, "BUY", _cancellationTokenSource.Token));

            _tradeActionLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Application shutting down")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}