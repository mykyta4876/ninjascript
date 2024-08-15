/*
Copyright Jade Byfield 2024
ZTA StrategyLogger

Logs trade executions and PnL to a CSV file
*/

#region Using declarations
using System;
using System.IO;
using System.Collections.Generic;
using NinjaTrader.Cbi;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using System.ComponentModel.DataAnnotations; 
#endregion

namespace NinjaTrader.NinjaScript.Strategies.zta
{
    public class ZTAStrategyLogger : Strategy
    {
        private string csvFilePath;
        private string realtime_folder = NinjaTrader.Core.Globals.UserDataDir + @"\ZTARealTimeLogs\";
        private bool isHeaderWritten = false;
        private int lastTradeCount = 0;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Strategy here.";
                Name = "ZTAStrategyLogger";
                Calculate = Calculate.OnBarClose;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;
                IsFillLimitOnTouch = false;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
                Slippage = 1;
                StartBehavior = StartBehavior.WaitUntilFlat;
                TimeInForce = TimeInForce.Gtc;
                TraceOrders = false;
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 20;
                IncludeCommission = true;
                IsInstantiatedOnEachOptimizationIteration = true;
                ExcludeWednesdays = false;
            }
            else if (State == State.Configure)
            {
                csvFilePath = realtime_folder + "LiveTradeLogs.csv";
                CreateDirectoryIfNotExists(realtime_folder);
            }
        }

        private void CreateDirectoryIfNotExists(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    Print("Directory created: " + path);
                }
                else
                {
                    Print("Directory already exists: " + path);
                }
            }
            catch (Exception ex)
            {
                Print("Error creating directory: " + ex.Message);
            }
        }

        protected override void OnBarUpdate()
        {
            // Add any logic that needs to run on each bar update
        }

        protected bool canTradeToday()
        {
            return !ExcludeWednesdays || Time[0].DayOfWeek != DayOfWeek.Wednesday;
        }

        protected override void OnPositionUpdate(Cbi.Position position, double averagePrice, int quantity, Cbi.MarketPosition marketPosition)
        {
            if (State == State.Realtime)
            {
                int tradeCount = SystemPerformance.RealTimeTrades.Count;

                if (tradeCount > lastTradeCount)
                {
                    Trade lastTrade = SystemPerformance.RealTimeTrades[tradeCount - 1];
                    WriteTradeToCSV(lastTrade);
                    lastTradeCount = tradeCount;
                }
            }
        }

       private void WriteTradeToCSV(Trade trade)
        {
            bool fileExists = File.Exists(csvFilePath);
            bool bHeader = false;

            CreateDirectoryIfNotExists(realtime_folder);

            if (fileExists)
            {
                try
                {
                    // Open the file to read from
                    using (System.IO.StreamReader file = new System.IO.StreamReader(csvFilePath))
                    {
                        string line;
                        string content = "";
                        while ((line = file.ReadLine()) != null)
                        {
                            content += line;
                        }
                        
                        if (content.Contains("Balance") == true)
                        {
                            bHeader = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    // Handle exceptions if any
                    Print("Error reading file: " + e.Message);
                }
            }

            // Initialize the StreamWriter with append mode
            using (StreamWriter writer = new StreamWriter(csvFilePath, true))
            {
                // If the file doesn't exist or the header hasn't been written, write the header row
                if (!fileExists)
                {
                    writer.WriteLine("FileDate,Symbol,StrategyName,EntryTime,ExitTime,Quantity,Position,Profit,PnL,Balance");
                }
                else // Ensures header is written only once
                {
                    if (!bHeader)
                    {
                        writer.WriteLine("FileDate,Symbol,StrategyName,EntryTime,ExitTime,Quantity,Position,Profit,PnL,Balance");
                    }
                }
                
                double realizedPnL = SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit;
                Print("WriteTradeToCSV: Realized PnL: " + realizedPnL.ToString("N2"));
                
                // Get the cash value of the account
                double accountBalance = Account.Get(AccountItem.CashValue, Currency.UsDollar);
                
                // Write the trade details to the CSV file
                writer.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                    Core.Globals.Now, Instrument.FullName, Name, trade.Entry.Time, trade.Exit.Time, trade.Quantity, trade.Entry.MarketPosition, trade.ProfitCurrency, realizedPnL, accountBalance.ToString()));
            }
        }


        #region Properties
        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Exclude Wednesdays", GroupName = "Specific Days", Order = 1)]
        public bool ExcludeWednesdays { get; set; }
        #endregion
    }
}
