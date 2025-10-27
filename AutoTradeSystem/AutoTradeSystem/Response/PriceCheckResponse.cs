using System.Text.Json.Serialization;

namespace AutoTradeSystem.Response
{
    public class PriceCheckResponse
    {
        [JsonPropertyName("c")]
        public decimal currentPrice { get; set; }

        [JsonPropertyName("h")]
        public decimal HighPrice { get; set; }

        [JsonPropertyName("l")]
        public decimal LowPrice { get; set; }

        [JsonPropertyName("o")]
        public decimal OpenPrice { get; set; }

        [JsonPropertyName("pc")]
        public decimal PreviousClosePrice { get; set; }
    }

}
