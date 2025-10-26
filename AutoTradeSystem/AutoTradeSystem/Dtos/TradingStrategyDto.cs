using System.Text.Json.Serialization;

namespace AutoTradeSystem.Dtos
{
    public class TradingStrategyDto
    {
        [JsonPropertyName("Ticker")]
        public string Ticker { get; set; }
        [JsonPropertyName("TradeAction")]
        public TradeAction TradeAction { get; set; }
        [JsonPropertyName("PriceChange")]
        public decimal PriceChange { get; set; }
        [JsonPropertyName("Quantity")]
        public int Quantity { get; set; }
    }
    public enum TradeAction
    {
        Buy,
        Sell
    }
}