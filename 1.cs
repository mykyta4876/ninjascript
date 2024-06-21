// Ensure you have the necessary using directives at the top of your file
using System;
using NinjaTrader.Cbi;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Strategies;
using NinjaTrader.Core.FloatingPoint;
using System.Windows.Media;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.Gui.Chart;
using System.Collections.Generic;
using NinjaTrader.Data;

namespace NinjaTrader.NinjaScript.Indicators
{
	public class ProtectedHighsLows : Indicator
	{
	    private int pivotStrength = 1; // Adjust based on your requirement
	    private double lastPivotHighPrice = double.NaN;
	    private double lastPivotLowPrice = double.NaN;
	    private int lastPivotHighBar = -1;
	    private int lastPivotLowBar = -1;
	    private Brush bullColor = Brushes.DodgerBlue;
	    private Brush bearColor = Brushes.Red;

		private List<double> ph = new List<double>();
		private List<double> pl = new List<double>();
		
		private List<DateTime> pht = new List<DateTime>();
		private List<DateTime> plt = new List<DateTime>();
		
		private double lastHigh = double.NaN;
		private double lastLow = double.NaN;
		
		private int lastHighIndex = -1;
		private int lastLowIndex = -1;
		
		private double trackHigh = double.NaN;
		private double trackLow = double.NaN;
		
		private int trackHighIndex = -1;
		private int trackLowIndex = -1;
		
	    protected override void OnStateChange()
	    {
	        if (State == State.SetDefaults)
	        {
	            Description = @"Protected Highs & Lows";
	            Name = "ProtectedHighsLows";
	            Calculate = Calculate.OnEachTick; // For real-time update, consider OnPriceChange or OnEachTick
	            IsOverlay = true;
	            AddPlot(Brushes.Gray, "TrailPrice"); // Placeholder for the trail price
	        }
	    }
	
	    protected override void OnBarUpdate()
	    {
			if (LowestBar(Low, ps) == 0 && pl.Count == 0)
			{
				pl.Add(Low[pivotStrength]);

				// Accessing time 'ps' bars ago
				DateTime timePsBarsAgo = BarsArray[0].GetTime(pivotStrength); // Accessing the time of the bar 'ps' bars ago
				plt.Add(timePsBarsAgo);
				
				if (lastLow == double.NaN)
				{
					lastLow = Low[pivotStrength];
					lastLowIndex = CurrentBar - pivotStrength;
				}
				else if (Low[pivotStrength] < lastLow)
				{
					lastLow = Low[pivotStrength];
					lastLowIndex = CurrentBar - pivotStrength;					
				}				
			}
			
			if (HighestBar(High, ps) == 0 && ph.Count == 0)
			{
				ph.Add(High[pivotStrength]);

				// Accessing time 'ps' bars ago
				DateTime timePsBarsAgo = BarsArray[0].GetTime(pivotStrength); // Accessing the time of the bar 'ps' bars ago
				pht.Add(timePsBarsAgo);
				
				if (lastHigh == double.NaN)
				{
					lastHigh = Low[pivotStrength];
					lastHighIndex = CurrentBar - pivotStrength;
				}
				else if (Low[pivotStrength] < lastHigh)
				{
					lastHigh = Low[pivotStrength];
					lastHighIndex = CurrentBar - pivotStrength;					
				}				
			}
			
			if ((High[ps] > trackHigh || double.IsNaN(trackHigh) || lastLowIndex >= trackHighIndex) && !double.IsNaN(HighestBar(High, ps)))
			{
			    trackHigh = High[ps];
			    trackHighIndex = CurrentBar - ps;
			}
			
			if ((Low[ps] < trackLow || double.IsNaN(trackLow) || lastHighIndex >= trackLowIndex) && !double.IsNaN(LowestBar(Low, ps)))
			{
			    trackLow = Low[ps];
			    trackLowIndex = CurrentBar - ps;
			}

	        // Skip the historical bars needed for the pivot strength calculation
	        if (CurrentBar < pivotStrength) return;
	
	        // Identify pivot highs and lows
	        CheckForPivotHighs();
	        CheckForPivotLows();
	
	        // Example drawing logic for the last identified pivot high/low
	        DrawLastPivots();
	    }
	
	    private void CheckForPivotHighs()
	    {
	        // Check if the current bar qualifies as a pivot high
	        if (High[0] > High[1] && High[0] >= High[2]) // Simplified condition
	        {
	            lastPivotHighPrice = High[0];
	            lastPivotHighBar = CurrentBar;
	            // Logic to handle protected highs could be added here
	        }
	    }
	
	    private void CheckForPivotLows()
	    {
	        // Check if the current bar qualifies as a pivot low
	        if (Low[0] < Low[1] && Low[0] <= Low[2]) // Simplified condition
	        {
	            lastPivotLowPrice = Low[0];
	            lastPivotLowBar = CurrentBar;
	            // Logic to handle protected lows could be added here
	        }
	    }
		
		// Define a method to detect pivot lows
		private bool IsPivotLow(int currentIndex, ISeries<double> lows, int ps)
		{
		    // Check if the current bar is within the valid range to check for pivot lows
		    if (currentIndex >= ps && currentIndex <= lows.Count - ps - 1)
		    {
		        // Determine if the current low is lower than the lows of the preceding and subsequent bars
		        double currentLow = lows[currentIndex];
		        bool isPivotLow = true;
		        
		        for (int i = currentIndex - ps; i <= currentIndex + ps; i++)
		        {
		            if (i != currentIndex && lows[i] <= currentLow)
		            {
		                isPivotLow = false;
		                break;
		            }
		        }
		        
		        return isPivotLow;
		    }
		    
		    return false;
		}
		
	    private void DrawLastPivots()
	    {
	        // If we have a pivot high, draw it
	        if (!double.IsNaN(lastPivotHighPrice) && lastPivotHighBar == CurrentBar)
	        {
	            Draw.Dot(this, "PivotHigh" + CurrentBar, false, 0, lastPivotHighPrice, bullColor);
	        }
	
	        // If we have a pivot low, draw it
	        if (!double.IsNaN(lastPivotLowPrice) && lastPivotLowBar == CurrentBar)
	        {
	            Draw.Dot(this, "PivotLow" + CurrentBar, false, 0, lastPivotLowPrice, bearColor);
	        }
	    }
	
	    #region Properties
	    [NinjaScriptProperty]
	    public int PivotStrength
	    {
	        get { return pivotStrength; }
	        set { pivotStrength = Math.Max(1, value); } // Ensure pivot strength is at least 1
	    }
	    #endregion
	}

}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ProtectedHighsLows[] cacheProtectedHighsLows;
		public ProtectedHighsLows ProtectedHighsLows(int pivotStrength)
		{
			return ProtectedHighsLows(Input, pivotStrength);
		}

		public ProtectedHighsLows ProtectedHighsLows(ISeries<double> input, int pivotStrength)
		{
			if (cacheProtectedHighsLows != null)
				for (int idx = 0; idx < cacheProtectedHighsLows.Length; idx++)
					if (cacheProtectedHighsLows[idx] != null && cacheProtectedHighsLows[idx].PivotStrength == pivotStrength && cacheProtectedHighsLows[idx].EqualsInput(input))
						return cacheProtectedHighsLows[idx];
			return CacheIndicator<ProtectedHighsLows>(new ProtectedHighsLows(){ PivotStrength = pivotStrength }, input, ref cacheProtectedHighsLows);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ProtectedHighsLows ProtectedHighsLows(int pivotStrength)
		{
			return indicator.ProtectedHighsLows(Input, pivotStrength);
		}

		public Indicators.ProtectedHighsLows ProtectedHighsLows(ISeries<double> input , int pivotStrength)
		{
			return indicator.ProtectedHighsLows(input, pivotStrength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ProtectedHighsLows ProtectedHighsLows(int pivotStrength)
		{
			return indicator.ProtectedHighsLows(Input, pivotStrength);
		}

		public Indicators.ProtectedHighsLows ProtectedHighsLows(ISeries<double> input , int pivotStrength)
		{
			return indicator.ProtectedHighsLows(input, pivotStrength);
		}
	}
}

#endregion
