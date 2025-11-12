using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AutoTradeSystem.Dtos
{
    public class TradingStrategyDto
    {
        [JsonPropertyName("Ticker")]
        public string Ticker { get; set; }
        [JsonPropertyName("TradeAction")]
        [EnumDataType(typeof(TradeAction), ErrorMessage = "Invalid Trade Action. Valid values are 0 or 1.")]
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