using System.Text.Json.Serialization;

namespace AutoTradeSystem.Dtos
{
    public class TradingStrategy
    {
        public TradingStrategy() { }
        public TradingStrategy(decimal price, TradingStrategyDto tradingStrategyDto, decimal originalPrice) 
        {
            ActionPrice = price;
            TradingStrategyDto = tradingStrategyDto;
            OriginalPrice = originalPrice;
        }
        [JsonPropertyName("ActionPrice")]
        public decimal ActionPrice { get; set; }
        [JsonPropertyName("TradingStrategy")]
        public TradingStrategyDto TradingStrategyDto { get; set; }
        [JsonPropertyName("OriginalPrice")]
        public decimal OriginalPrice { get; set; }
    }
}
