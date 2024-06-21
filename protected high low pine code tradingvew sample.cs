// Ensure you have the necessary using directives at the top of your file
#region Using declarations
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Strategies;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

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
        private string labelType = "MSS";
        private int trail_width = 2;
        
        private List<double> ph = new List<double>();
        private List<double> pl = new List<double>();
        
        private List<DateTime> pht = new List<DateTime>();
        private List<DateTime> plt = new List<DateTime>();
        
        private double trail_price = double.NaN;
        private Brush trail_color;

        private double lastHigh = double.NaN;
        private double lastLow = double.NaN;
        
        private int lastHighIndex = -1;
        private int lastLowIndex = -1;
        
        private double trackHigh = double.NaN;
        private double trackLow = double.NaN;
        
        private int trackHighIndex = -1;
        private int trackLowIndex = -1;
        
        private bool bull = false;
        
        private Series<bool>    bosBear;
        private Series<bool>    bosBull;
        private Series<bool>    mssBear;
        private Series<bool>    mssBull;
        private Series<bool>    change;
        
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Protected Highs & Lows";
                Name = "ProtectedHighsLows";
                Calculate = Calculate.OnBarClose; // For real-time update, consider OnPriceChange or OnEachTick
                IsOverlay = true;
                AddPlot(Brushes.Gray, "TrailPrice"); // Placeholder for the trail price
                AddPlot(Brushes.Blue, "Low");
                AddPlot(Brushes.Red, "High");
            }
            else if (State == State.DataLoaded)
            {
                bosBear     = new Series<bool>(this);
                bosBull     = new Series<bool>(this);
                mssBear     = new Series<bool>(this);
                mssBull     = new Series<bool>(this);
                change      = new Series<bool>(this);
            }
        }
        
        private void ClearAll()
        {
            pl = new List<double>();
            ph = new List<double>();
        }
        
        protected override void OnBarUpdate()
        {
            // Skip the historical bars needed for the pivot strength calculation
            if (CurrentBar < pivotStrength) return;
            
            Print("CurrentBar: " + CurrentBar);
            Print("low[ps]: " + Low[pivotStrength]);
            Print("ta.pivotlow(low, ps, ps): " + PivotLow(Low, pivotStrength, pivotStrength));
            Print("pl.Count: " + pl.Count);
            
            if (double.IsNaN(PivotLow(Low, pivotStrength, pivotStrength)) && pl.Count == 0)
            {
                pl.Insert(0, Low[pivotStrength]);
                
                if (double.IsNaN(lastLow))
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
            
            if (double.IsNaN(PivotHigh(High, pivotStrength, pivotStrength)) && ph.Count == 0)
            {
                ph.Insert(0, High[pivotStrength]);
                //ph.Add(High[pivotStrength]);
                
                if (double.IsNaN(lastHigh))
                {
                    lastHighIndex = CurrentBar - pivotStrength;
                    lastHigh = High[pivotStrength];
                }
                else if (High[pivotStrength] < lastHigh)
                {
                    lastHighIndex = CurrentBar - pivotStrength; 
                    lastHigh = High[pivotStrength];             
                }               
            }
            
            Print("lastHigh: " + lastHigh);
            Print("lastLow: " + lastLow);
            
            if ((High[pivotStrength] > trackHigh || double.IsNaN(trackHigh) || lastLowIndex >= trackHighIndex) && !double.IsNaN(PivotHigh(High, pivotStrength, pivotStrength)))
            {
                trackHighIndex = CurrentBar - pivotStrength;
                trackHigh = High[pivotStrength];
            }
            
            if ((Low[pivotStrength] < trackLow || double.IsNaN(trackLow) || lastHighIndex >= trackLowIndex) && !double.IsNaN(PivotLow(Low, pivotStrength, pivotStrength)))
            {
                trackLowIndex = CurrentBar - pivotStrength;
                trackLow = Low[pivotStrength];
            }
            
            Print("trackHigh: " + trackHigh);
            Print("trackLow: " + trackLow);
            
            bosBear[0] = false;
            bosBull[0] = false;
            mssBear[0] = false;
            mssBull[0] = false;
            change[0] = false;
            
            Print("label: 1");
            
            if (ph.Count > 0)
            {
                if (Close[0] > ph[0])
                {
                    bool save = false;
                    if (labelType == "MSS" && !bull)
                    {
                        save = true;
                    }
                    else if (labelType == "BOS" && bull)
                    {
                        save = true;
                    }
                    else if (labelType == "All")
                    {
                        save = true;
                    }
                    
                    if (save)
                    {
                        Draw.Text(this, "Label", false, bull ? "BOS" : "MSS", 0, ph[0], 0, Brushes.Black, new Gui.Tools.SimpleFont() { Size = 14}, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
                    }
                    
                    if (bull)
                    {
                        bosBull[0] = true;
                    }
                    else
                    {
                        mssBull[0] = true;
                    }
                    
                    bull = true;
                    change[0] = true;
                    
                    Values[2][0] = ph[0];
                    
                    ClearAll();
                    
                    if (! double.IsNaN(trackLow))
                    {
                        pl.Insert(0, trackLow);
                        lastHigh = double.NaN;
                    }
                }
            }
            
            Print("label: 2");
            
            if (pl.Count > 0)
            {
                if (Close[0] < pl[0])
                {
                    bool save = false;
                    if (labelType == "MSS" && bull)
                    {
                        save = true;
                    }
                    else if (labelType == "BOS" && !bull)
                    {
                        save = true;
                    }
                    else if (labelType == "All")
                    {
                        save = true;
                    }
                    
                    if (save)
                    {
                        Draw.Text(this, "Label", false, !bull ? "BOS" : "MSS", 0, pl[0], 0, Brushes.Black, new Gui.Tools.SimpleFont() { Size = 14}, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
                    }
                    
                    if (!bull)
                    {
                        bosBear[0] = true;
                    }
                    else
                    {
                        mssBear[0] = true;
                    }
                    
                    bull = false;
                    change[0] = true;
                    
                    Values[1][0] = pl[0];
                    
                    ClearAll();
                    
                    if (! double.IsNaN(trackHigh))
                    {
                        ph.Insert(0, trackHigh);
                        lastLow = double.NaN;
                    }
                }
            }
            
            Print("label: 3");
            
            if (change[1])
            {
                if (bosBear[1] || mssBear[1])
                {
                    trail_price = trackHigh;
                    trail_color = bearColor;
                }
                else if (bosBull[1] || mssBull[1])
                {
                    trail_price = trackLow;
                    trail_color = bullColor;
                }
            }
            
            // Set the bar color
            //PlotBrushes[0][0] = bull ? bullColor : bearColor;
            
            // Plot the trailPrice
            //Plot("Trail PHL", trail_price, trail_color, trail_width);
            //AddPlot(new Stroke(trail_color, trail_width), PlotStyle.Bar, "Trail PHL");
            
            
            /*
            // Identify pivot highs and lows
            CheckForPivotHighs();
            CheckForPivotLows();
            */
            
            Print("ProtectedHighLows. bull: " + bull);
            // Example drawing logic for the last identified pivot high/low
            //DrawLastPivots();
        }
    
        // Define a method to find pivot lows
        private double PivotLow(ISeries<double> lows, int period, int currentIndex)
        {
            double lowValue = lows[currentIndex]; // Get the low value at the current index
        
            for (int i = 1; i <= period; i++)
            {
                if (lows[currentIndex - i] < lowValue)
                    return double.NaN; // If any of the lows in the lookback period are lower, it's not a pivot low
            }
        
            return lowValue; // If it's the lowest in the lookback period, it's a pivot low
        }
        
        // Define a method to find pivot highs
        private double PivotHigh(ISeries<double> highs, int period, int currentIndex)
        {
            double highValue = highs[currentIndex]; // Get the high value at the current index
        
            for (int i = 1; i <= period; i++)
            {
                if (highs[currentIndex - i] > highValue)
                    return double.NaN; // If any of the highs in the lookback period are higher, it's not a pivot high
            }
        
            return highValue; // If it's the highest in the lookback period, it's a pivot high
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
