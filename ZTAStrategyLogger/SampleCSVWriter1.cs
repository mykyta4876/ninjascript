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
using NinjaTrader.NinjaScript.Strategies.zta;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies.zta
{
	public class SampleCSVWriter1 : ZTAStrategyLogger
	{
		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "SampleCSVWriter1";
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
			}
			else if (State == State.Configure)
			{
			}
    	
		}
		
		protected override void OnPositionUpdate(Position position, double averagePrice, int quantity, MarketPosition marketPosition)
                {
					
					base.OnPositionUpdate(position, averagePrice, quantity, marketPosition);

						
                } 

		protected override void OnBarUpdate()
		{
			
			if(Position.MarketPosition == MarketPosition.Flat  && Close[0] > Open[0]){
				EnterLong();
			}
			
			if(Position.MarketPosition == MarketPosition.Long && BarsSinceEntryExecution() >= 0){
				ExitLong();
			}
		}
	}
}
