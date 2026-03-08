using AutoTradeSystem;
using AutoTradeSystem.Dtos;
using System.Text.Json.Serialization;

public class AddStrategyResponse : BaseResponse
{
    [JsonPropertyName("TradingStrategy")]
    public TradingStrategyDto TradingStrategyDto { get; set; }
    public string StrategyID {get; set; }
    public AddStrategyResponse(bool success, string message, TradingStrategyDto tradingStrategy, string strategyID) : base(success, message)
    {
        TradingStrategyDto = tradingStrategy;
        StrategyID = strategyID;
    }
}
