using AutoTradeSystem;
using AutoTradeSystem.Dtos;

public class GetStrategiesResponse : Response
{
    readonly IDictionary<string, TradingStrategy> _TradingStrategies = new Dictionary<string, TradingStrategy>();
    public GetStrategiesResponse(bool success, string message, IDictionary<string, TradingStrategy> tradingStrategies) : base(success, message)
    {
        _TradingStrategies = tradingStrategies;
    }
}

