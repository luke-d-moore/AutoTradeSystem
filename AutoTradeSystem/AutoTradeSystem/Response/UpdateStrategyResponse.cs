using AutoTradeSystem;
using AutoTradeSystem.Dtos;
using System.Text.Json.Serialization;

public class UpdateStrategyResponse :Response
{
    [JsonPropertyName("ID")]
    public string id;
    [JsonPropertyName("TradingStrategy")]
    public TradingStrategyDto TradingStrategyDto { get; set; }
    public UpdateStrategyResponse(bool success, string message, string ID, TradingStrategyDto tradingStrategy) : base(success, message)
    {
        id = ID;
        TradingStrategyDto = tradingStrategy;
    }
}
