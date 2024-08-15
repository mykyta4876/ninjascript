using System.Windows.Controls;

namespace NinjaTrader.NinjaScript.Strategies.Givanne.Gui
{
    public class ButtonPanel
    {
        public Grid ButtonGrid {get; set;}
        public Button LongConditionalButton { get; set; }
        public Button LongLimitButton { get; set; }
        public Button LongMarketButton { get; set; }
        public bool LongConditionTriggerEnabled { get; set; }
        public bool LongLimitEnabled { get; set; }
    }
}
