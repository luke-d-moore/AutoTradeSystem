using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AutoTradeSystem.Dtos
{
    [RequiredEitherNotBoth(
    propertyAName: nameof(PriceChange),
    propertyBName: nameof(ActionPrice),
    ErrorMessage = "You must provide either a PriceChange OR ActionPrice (greater than 0), but not both."
)]
    public class TradingStrategyDto
    {
        [Required(ErrorMessage = "Ticker is required.")]
        [StringLength(5, MinimumLength = 3, ErrorMessage = "Invalid Ticker length. Ticker must be between 3 and 5 characters.")]
        [JsonPropertyName("Ticker")]
        public string Ticker { get; set; }

        [JsonPropertyName("TradeAction")]
        [EnumDataType(typeof(TradeAction), ErrorMessage = "Invalid Trade Action. Valid values are 0 (Buy) or 1 (Sell).")]
        public TradeAction TradeAction { get; set; }

        [JsonPropertyName("PriceChange")]
        //[Required(ErrorMessage = "Price Change is required.")]
        public decimal PriceChange { get; set; }

        [JsonPropertyName("ActionPrice")]
        //[Required(ErrorMessage = "Action Price is required.")]
        public decimal ActionPrice { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        [JsonPropertyName("Quantity")]
        [Required(ErrorMessage = "Quantity is required.")]
        public int Quantity { get; set; }
    }

    public enum TradeAction
    {
        Buy = 0,
        Sell = 1
    }
}