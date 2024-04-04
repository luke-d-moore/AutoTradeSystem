namespace AutoTradeSystem.Dtos
{
    public class TradingStrategy
    {
        public TradingStrategy(decimal price, TradingStrategyDto tradingStrategyDto, decimal originalPrice) 
        {
            ActionPrice = price;
            TradingStrategyDto = tradingStrategyDto;
            OriginalPrice = originalPrice;
        }
        public decimal ActionPrice { get; set; }
        public TradingStrategyDto TradingStrategyDto { get; set; }
        public decimal OriginalPrice { get; set; }
    }
}
