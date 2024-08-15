
#region Using declarations
using System;
using System.IO;
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
	public class TradeLogger : Strategy
	{
		#region TRADELOG 1/4 - declarations: put this code block in the beginning of your strategy class and edit the path to reflect your folders here below:
			//private string positionUpdate_csv = @"C:\Users\TbotUser\Documents\Realtime_Log\positionUpdates\positionUpdates.csv";
			private string csvFilePath = NinjaTrader.Core.Globals.UserDataDir + @"\TradeLoggerLogs\TradeLogs.csv";
			private string realtime_folder = NinjaTrader.Core.Globals.UserDataDir + @"\TradeLoggerLogs\";
			//private string backtest_folder = @"C:\Users\TbotUser\Documents\Realtime_Log\Backtest_Log\";
//			private string quick_backtest_folder = @"C:\Users\TbotUser\Documents\Realtime_Log\AllTrades_Log\";
//			private string walkforward_folder = @"C:\Users\TbotUser\Documents\Realtime_Log\WFO_Log\"; // not very useful multithreading it seems.
		
			bool isHeaderWritten = false;
			private List<Trade> trades; // List to store executed trades during the day
			private int lastTradeCount, tradesCount, count = 0;
		#endregion
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "TradeLogger";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 1;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				IncludeCommission = true;
				//Instrument = Instrument.GetInstrument("ES 09-23");
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				TP = 280;
				SL = 200;
			}
			else if (State == State.Configure)
			{
				#region TRADELOG 2/4: - stateconfig: put this code block in State.Configure
					IncludeTradeHistoryInBacktest 				= true;
					csvFilePath = realtime_folder + this.Name + ".csv";
//					csvFilePath = tradeLogBacktestOnly ? backtest_folder + this.Name + ".csv" : csvFilePath;
//					csvFilePath = tradeLogWFOOnly ? walkforward_folder + this.Name + ".csv" : csvFilePath;
					trades = new List<Trade>();
				#endregion
				
				SetProfitTarget(CalculationMode.Ticks, TP);
				SetStopLoss(CalculationMode.Ticks, SL);
			}
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
		
		
		#region Properties
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Profit Target Ticks", Order = 0)]
	    public double TP
	    { get; set; }
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Stop Loss Ticks", Order = 0)]
	    public double SL
	    { get; set; }
		#endregion
		
		#region TRADELOG 3/4 - logging_realtime_OnPosition_Update: Put this code block after OnBarUpdate
		protected override void OnPositionUpdate(Cbi.Position position, double averagePrice,
		      int quantity, Cbi.MarketPosition marketPosition)
		{
			
			Print(this.Name + " onPosiitionUpdate()");
////			 Logging entries to master entry file:
//			if (position.MarketPosition != MarketPosition.Flat && positionUpdate == true)
//			{
////				message = this.Name + " entered a " +  Enterposition +" position";
//				bool fileExists = File.Exists(positionUpdate_csv);
//				// Initialize the StreamWriter
//				StreamWriter writer = new StreamWriter(positionUpdate_csv, true);

//				// If the file doesn't exist or the header hasn't been written, write the header row
//				if (!fileExists)// || !isHeaderWritten)
//				{
//					// Write the header row
//					writer.WriteLine("EntryTime,Symbol,StrategyName,PositionUpdate");
//					isHeaderWritten = true;
//				}
				
//				writer.WriteLine(Time[0] + "," + Instrument.FullName + "," + this.Name + "," + position.MarketPosition);
//				writer.Close();
//			}
		

			
		  if (position.MarketPosition == MarketPosition.Flat)
		  {
			tradesCount = tradeLogRTOnly ? SystemPerformance.RealTimeTrades.TradesCount : SystemPerformance.AllTrades.TradesCount;
			count = tradeLogRTOnly ? SystemPerformance.RealTimeTrades.Count : SystemPerformance.AllTrades.Count;
			
			
			if (count > 0 && count - lastTradeCount == 1) //tradesCount - lastTradeCount == 1 && Position.MarketPosition == MarketPosition.Flat && 
			{
				
				Trade lastTrade = tradeLogRTOnly ? SystemPerformance.RealTimeTrades[count - 1] : SystemPerformance.AllTrades[count - 1];

				trades.Add(lastTrade);

				lastTradeCount ++;
			}
		  }
			
			
			#region writeNetProfittoCSV
			
			double netPL = tradeLogRTOnly ? SystemPerformance.RealTimeTrades.TradesPerformance.NetProfit : SystemPerformance.AllTrades.TradesPerformance.NetProfit;
//			Print("Net PL at " + Time[0] + " is " + netPL);
//			Print("trades.Count is " + trades.Count);
			
			if (trades.Count > 0 && dontLog == false)
			{	

//				lastTradeCount = 0;
				// Check if the file exists
				bool fileExists = File.Exists(csvFilePath);
				// Initialize the StreamWriter
				StreamWriter writer = new StreamWriter(csvFilePath, true);

				// If the file doesn't exist or the header hasn't been written, write the header row
				if (!fileExists)// || !isHeaderWritten)
				{
					// Write the header row
					writer.WriteLine("fileDate,Symbol,StrategyName,EntryTime,ExitTime,Position,Profit");
					isHeaderWritten = true;
				}
				
				
				// Write the trade details to the CSV file
	//			writer.WriteLine($"{Time[0]},{Instrument.Id},{netPL},{quantity},{price}");
				foreach (Trade trade in trades)
           		{

					
					writer.WriteLine(Time[0] + "," + Instrument.FullName + "," + this.Name + "," + trade.Entry.Time + "," + trade.Exit.Time + "," + trade.Entry.MarketPosition + "," + trade.ProfitCurrency);
				}
				// Close the StreamWriter when done
				writer.Close();
				// Clear the list of trades for the next trading day
                trades.Clear();
//				executions.Clear();
			}
			
			#endregion	
			
		  	int firstCount = tradeLogRTOnly ? SystemPerformance.RealTimeTrades.Count : SystemPerformance.AllTrades.Count;
			Print("The strategy has taken " + firstCount + " trades.");
		}
		#endregion
		
		#region TRADELOG 4/4 - logging_UI: Put this code block just next to the other Properties region at end of strategy.
		// if none of below are selected the default is that it prints to the quick_backtest_folder!
		// if you don't want the strategy to log the trades at all check the box "Do not log"
		[NinjaScriptProperty]
        [Display(Order = 270, Name = "Only log realtime trades", GroupName="Trade Logging")]
        public bool tradeLogRTOnly { get; set; }
		
		[NinjaScriptProperty]
        [Display(Order = 270, Name = "Log Backtest", GroupName="Trade Logging")]
        public bool tradeLogBacktestOnly { get; set; }
		
		[NinjaScriptProperty]
        [Display(Order = 270, Name = "Log WFO", GroupName="Trade Logging")]
        public bool tradeLogWFOOnly { get; set; }
		
		[NinjaScriptProperty]
        [Display(Order = 270, Name = "Log position update", GroupName="Trade Logging")]
        public bool positionUpdate { get; set; }
		
		[NinjaScriptProperty]
        [Display(Order = 270, Name = "Do not log", GroupName="Trade Logging")]
        public bool dontLog { get; set; }
		#endregion
		
	}
}





