using AutoTradeSystem.Dtos;

namespace AutoTradeSystem.Interfaces
{
    public interface ITradeActionService
    {
        Task PublishMessage(string ticker, int quantity, string action);
    }
}