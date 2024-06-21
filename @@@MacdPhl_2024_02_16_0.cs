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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class MacdPhl_2024_02_16_0 : Strategy
	{
		public	Series<double>		Deltas;
		public	Series<double>		macd15Diff;
		public	Series<double>		macd60Diff;
		public	Series<double>		vCMO;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "MacdPhl_2024_02_16_0";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				
				Fast					= 12;
				Slow					= 26;
				Smoothing				= 9;
			}
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Second, 15);
				AddDataSeries(Data.BarsPeriodType.Second, 60);
			}
			else if (State == State.DataLoaded)
			{
				Deltas = new Series<double>(this);
				macd15Diff = new Series<double>(this);
				macd60Diff = new Series<double>(this);
				vCMO = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom strategy logic here.
			
			if(CurrentBar < 12) return;
			Deltas[0] = MACD(BarsArray[0], Fast, Slow, Smoothing).Diff[0];
			
			// Get the current bar's time
			DateTime currentBarTime = Time[0];
			
			// Extract the hour component from the current bar's time
			int currentHour = (currentBarTime.Hour + 18) % 24;
			
			double bull = ProtectedHighsLows(BarsArray[0], 1)[0];
			macd60Diff[0] = ChaikinVolatility(10, 10)[0];
			vCMO[0] = CMO(BarsArray[0], 10)[0];
			
			// Access MACD values from the secondary data series (15-second chart)
	        int macd15sFast = MACD(BarsArray[1], 12, 26, 9).Fast;
	        int macd15sSlow = MACD(BarsArray[1], 12, 26, 9).Slow;
			macd15Diff[0] = MACD(BarsArray[1], 12, 26, 9).Diff[0];
			//macd60Diff[0] = MACD(BarsArray[2], 12, 26, 9).Diff[0];
			
			if (CrossAbove(Deltas, 0, 1))
			{
				//if (macd15Diff[0] > 0 && bull > 0)
				//if (macd15sFast > macd15sSlow)
				if (macd15Diff[0] >= 0 && vCMO[0] > vCMO[1] && vCMO[1] > vCMO[2] && vCMO[2] > vCMO[3])
				{
					if (Position.Quantity == 0 && currentHour > 4 && currentHour < 10)
					//if (Position.Quantity == 0)
						if (Deltas[1] < Deltas[0] && Deltas[2] < Deltas[1] && Deltas[3] < Deltas[2] && Deltas[4] < Deltas[3])
						{
							EnterLong();
							Print("Macdhl.CrossAbove Deltas[0]: " + Deltas[0] + " Time: " + currentBarTime + " Position.Quantity: " + Position.Quantity + " CurrentBar: " + CurrentBar);
						}
				}
				
				if (Position.Quantity > 0)
				{
					Print("Macdhl.CrossAbove CurrentBar: " + CurrentBar);
					ExitShort();
				}
			}
			else if (CrossBelow(Deltas, 0, 1))
			{
				//if (macd15Diff[0] < 0 && bull == 0)
				//if (macd15sFast < macd15sSlow)
				if (macd15Diff[0] <= 0 && vCMO[0] < vCMO[1] && vCMO[1] < vCMO[2] && vCMO[2] < vCMO[3])
				{
					if (Position.Quantity == 0 && currentHour > 4 && currentHour < 10)
					//if (Position.Quantity == 0)
						if (Deltas[1] > Deltas[0] && Deltas[2] > Deltas[1] && Deltas[3] > Deltas[2] && Deltas[4] > Deltas[3])
						{
							EnterShort();
							Print("Macdhl.CrossBelow Deltas[0]: " + Deltas[0] + " Time: " + currentBarTime + " Position.Quantity: " + Position.Quantity + " CurrentBar: " + CurrentBar);
						}
				}
				
				if (Position.Quantity > 0)
				{
					Print("Macdhl.CrossBelow CurrentBar: " + CurrentBar);
					ExitLong();
				}
			}
		}
		
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Fast", Order=1, GroupName="Parameters")]
		public int Fast
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Slow", Order=2, GroupName="Parameters")]
		public int Slow
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Smoothing", Order=3, GroupName="Parameters")]
		public int Smoothing
		{ get; set; }

		#endregion

	}
}
