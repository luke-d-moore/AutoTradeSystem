using AutoTradeSystem;
using AutoTradeSystem.Dtos;
using System.Text.Json.Serialization;

public class GetStrategiesResponse : Response
{
    [JsonPropertyName("TradingStrategies")]
    public IDictionary<string, TradingStrategy> TradingStrategies { get; set; }
    public GetStrategiesResponse(bool success, string message, IDictionary<string, TradingStrategy> tradingStrategies) : base(success, message)
    {
        TradingStrategies = tradingStrategies;
    }
}

