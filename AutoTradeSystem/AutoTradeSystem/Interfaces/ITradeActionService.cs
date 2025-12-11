using AutoTradeSystem.Dtos;

namespace AutoTradeSystem.Interfaces
{
    public interface ITradeActionService
    {
        Task EnqueueMessage(string ticker, int quantity, string action);
    }
}