using NinjaTrader.Cbi;

namespace NinjaTrader.NinjaScript.Strategies
{
    public class TradingDataContainer
    {
        public double EntryPrice { get; set; }
        public int BarsOrderSubmit { get; set; }
        public Order LongOrder { get; set; }
        public Order ShortOrder { get; set; }
    }
}