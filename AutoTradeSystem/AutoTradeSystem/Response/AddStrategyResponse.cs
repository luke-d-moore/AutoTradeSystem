using AutoTradeSystem;
using AutoTradeSystem.Dtos;
using System.Text.Json.Serialization;

public class AddStrategyResponse : BaseResponse
{
    [JsonPropertyName("TradingStrategy")]
    public TradingStrategyDto TradingStrategyDto { get; set; }
    public AddStrategyResponse(bool success, string message, TradingStrategyDto tradingStrategy) : base(success, message)
    {
        TradingStrategyDto = tradingStrategy;
    }
}
