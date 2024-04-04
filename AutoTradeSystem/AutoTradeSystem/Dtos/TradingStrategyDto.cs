namespace AutoTradeSystem.Server.Dtos
{
    public class TradingStrategyDto
    {
        public string Ticker { get; set; }
        public TradeAction TradeAction { get; set; }
        public decimal PriceChange { get; set; }
        public int Quantity { get; set; }
    }
    public enum TradeAction
    {
        Buy,
        Sell
    }
}