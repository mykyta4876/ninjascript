#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
	public class DayOHLC : Indicator
	{
        #region Variables
		List<double> opens, highs, lows, closes;
        #endregion

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Calculate			= Calculate.OnBarClose;
				IsOverlay			= true;
				BarsRequiredToPlot 	= 0;

				DaysAgo = 1;

				AddPlot(new Stroke(Brushes.Orange, 1), PlotStyle.Hash, "DayOpen");
	            AddPlot(new Stroke(Brushes.Green, 1), PlotStyle.Hash, "DayHigh");
	            AddPlot(new Stroke(Brushes.Red, 1), PlotStyle.Hash, "DayLow");
	            AddPlot(new Stroke(Brushes.Firebrick, 1), PlotStyle.Hash, "DayClose");
			}
			else if (State == State.Configure)
			{
				opens = new List<double>();
				highs = new List<double>();
				lows = new List<double>();
				closes = new List<double>();
			}
		}

		protected override void OnBarUpdate()
		{
			if ( !Bars.BarsType.IsIntraday )
				return;

			if ( Bars.IsFirstBarOfSession ) {
				opens.Insert(0, Open[0]);
				highs.Insert(0, High[0]);
				lows.Insert(0, Low[0]);
				closes.Insert(0, Close[0]);
				//Print(DaysAgo + "  " + Time[0]);
			}

			highs[0] = Math.Max(highs[0], High[0]);
			lows[0] = Math.Min(lows[0], Low[0]);
			closes[0] = Close[0];

			if (DaysAgo >= opens.Count)
				return;

			DayOpen[0] = opens[DaysAgo];
			DayHigh[0] = highs[DaysAgo];
			DayLow[0] = lows[DaysAgo];
			DayClose[0] = closes[DaysAgo];
		}

        #region Properties
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> DayOpen
        {
            get { return Values[0]; }
        }
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> DayHigh
        {
            get { return Values[1]; }
        }
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> DayLow
        {
            get { return Values[2]; }
        }
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> DayClose
        {
            get { return Values[3]; }
        }

		[Range(0, int.MaxValue), NinjaScriptProperty, Display(Name = "Days ago", GroupName = "Parameters", Order = 1)]
        public int DaysAgo { get; set; }
        #endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DayOHLC[] cacheDayOHLC;
		public DayOHLC DayOHLC(int daysAgo)
		{
			return DayOHLC(Input, daysAgo);
		}

		public DayOHLC DayOHLC(ISeries<double> input, int daysAgo)
		{
			if (cacheDayOHLC != null)
				for (int idx = 0; idx < cacheDayOHLC.Length; idx++)
					if (cacheDayOHLC[idx] != null && cacheDayOHLC[idx].DaysAgo == daysAgo && cacheDayOHLC[idx].EqualsInput(input))
						return cacheDayOHLC[idx];
			return CacheIndicator<DayOHLC>(new DayOHLC(){ DaysAgo = daysAgo }, input, ref cacheDayOHLC);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DayOHLC DayOHLC(int daysAgo)
		{
			return indicator.DayOHLC(Input, daysAgo);
		}

		public Indicators.DayOHLC DayOHLC(ISeries<double> input , int daysAgo)
		{
			return indicator.DayOHLC(input, daysAgo);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DayOHLC DayOHLC(int daysAgo)
		{
			return indicator.DayOHLC(Input, daysAgo);
		}

		public Indicators.DayOHLC DayOHLC(ISeries<double> input , int daysAgo)
		{
			return indicator.DayOHLC(input, daysAgo);
		}
	}
}

#endregion
