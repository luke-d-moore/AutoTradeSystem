using AutoTradeSystem;
using AutoTradeSystem.Dtos;

public class AddStrategyResponse :Response
    {
    readonly TradingStrategyDto _TradingStrategyDto;
    public AddStrategyResponse(bool success, string message, TradingStrategyDto tradingStrategy) : base(success, message)
    {
        _TradingStrategyDto = tradingStrategy;
    }
}
