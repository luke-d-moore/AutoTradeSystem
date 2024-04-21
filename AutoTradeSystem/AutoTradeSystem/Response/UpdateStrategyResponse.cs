using AutoTradeSystem;
using AutoTradeSystem.Dtos;

public class UpdateStrategyResponse :Response
{
    readonly string _id;
    readonly TradingStrategyDto _TradingStrategyDto;
    public UpdateStrategyResponse(bool success, string message, string ID, TradingStrategyDto tradingStrategy) : base(success, message)
    {
        _id = ID;
        _TradingStrategyDto = tradingStrategy;
    }
}
