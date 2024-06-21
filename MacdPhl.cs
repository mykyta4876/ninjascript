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
	public class MacdPhl : Strategy
	{
		public	Series<double>		macs;
		public	Series<double>		Deltas;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "MacdPhl";
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
			}
			else if (State == State.DataLoaded)
			{
				macs = new Series<double>(this);
				Deltas = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom strategy logic here.
			
			if(CurrentBar < 1) return;
			MACD indicMACD = MACD(Input, Fast, Slow, Smoothing);
			Deltas[0] 	= indicMACD.Diff[0];
			
			
			
			double bull = ProtectedHighsLows(Input, 1)[0];
			
			//Print("Macdhl. bull: " + bull);
			
			// Get the current bar's time
			DateTime currentBarTime = Time[0];
			
			// Extract the hour component from the current bar's time
			int currentHour = (currentBarTime.Hour + 18) % 24;
			
			if (CrossAbove(Deltas, 0, 1))
			{
				if (bull > 0)
				{
					if (Position.Quantity == 0 && currentHour > 4 && currentHour < 10)
						if (Deltas[1] < Deltas[0] && Deltas[2] < Deltas[1] && Deltas[3] < Deltas[2] && Deltas[4] < Deltas[3])
						{
							EnterLong();
							Print("Macdhl.CrossAbove Deltas[0]: " + Deltas[0] + " Time: " + currentBarTime + " bull: " + bull + " Position.Quantity: " + Position.Quantity + " CurrentBar: " + CurrentBar);
						}
				}
				
				if (Position.Quantity > 0)
				{
					Print("Macdhl.CrossAbove CurrentBar: " + CurrentBar);
					ExitLong();
					ExitShort();
				}
			}
			else if (CrossBelow(Deltas, 0, 1))
			{
				if (bull == 0)
				{
					if (Position.Quantity == 0 && currentHour > 4 && currentHour < 10)
						if (Deltas[1] > Deltas[0] && Deltas[2] > Deltas[1] && Deltas[3] > Deltas[2] && Deltas[4] > Deltas[3])
						{
							EnterShort();
							Print("Macdhl.CrossBelow Deltas[0]: " + Deltas[0] + " Time: " + currentBarTime + " bull: " + bull + " Position.Quantity: " + Position.Quantity + " CurrentBar: " + CurrentBar);
						}
				}
				
				if (Position.Quantity > 0)
				{
					Print("Macdhl.CrossBelow CurrentBar: " + CurrentBar);
					ExitLong();
					ExitShort();
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
