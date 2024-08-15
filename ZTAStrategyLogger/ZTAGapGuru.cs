//** Copyright (c) 1998-2024 Capstone Trading Systems All rights reserved. **
//This is for the Gap Fill Combo strategy from the book, "Seven Trading Systems for the S&P Futures"
//http://capstonetradingsystems.com

#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml.Serialization;
using System.IO;
using NinjaTrader.Cbi;
using NinjaTrader.Core;
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

namespace NinjaTrader.NinjaScript.Strategies.JBAlgos.Profitable
{
	[Description("")]
    public class ZTAGapGuru : ZTAStrategyLogger
    {
        #region Inputs
		private BarsPeriodType _Data2_PeriodType = BarsPeriodType.Minute;
		private int _Data2_PeriodValue = 405;
		private int _L1 = 100;
		private TimeSpan _LongExTime = new TimeSpan(16, 14, 0);
		private TimeSpan _ShortExTime = new TimeSpan(16, 14, 0);
		string _HolidaysFile = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\holidays.txt";
        #endregion
		
		#region Variables
		List<double> HO, OL, dailyOpens, dailyCloses;
		public List<double> dailyHighs, dailyLows;
		public DayOHLC ohlc0, ohlc1;
		Holidays h;
        #endregion
						
		DateTime ToEst(DateTime dt)
		{
			return TimeZoneInfo.ConvertTime(dt, TimeZoneInfo.Local, Core.Globals.Est);
		}
		
		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				Calculate							= Calculate.OnBarClose;
				EntriesPerDirection					= 1;
				EntryHandling						= EntryHandling.UniqueEntries;
				IsExitOnSessionCloseStrategy		= true;
				ExitOnSessionCloseSeconds			= 30;
				TimeInForce							= TimeInForce.Day;
				BarsRequiredToTrade					= 0;
				IsEnabled							= true;
				Name = "ZTAGapGuru";
				StopLoss = 80;
		        GapFillTP = 40;
		        GapContTP = 80; 
				_cntrcts = 1;
			} 
			else if (State == State.Configure)
			{	
				AddDataSeries(_Data2_PeriodType, _Data2_PeriodValue);	
							
				h = new Holidays(_HolidaysFile);		
									
				ohlc0 = DayOHLC(0);
				ohlc1 = DayOHLC(1);
					
				HO = new List<double>();
				OL = new List<double>();
				dailyOpens = new List<double>();
				dailyHighs = new List<double>();
				dailyLows = new List<double>();
				dailyCloses = new List<double>();
			}
			else if (State == State.Realtime)
			{

			}
		}
		
		protected override void OnPositionUpdate(Position position, double averagePrice, int quantity, MarketPosition marketPosition)
                {
					
					base.OnPositionUpdate(position, averagePrice, quantity, marketPosition);

						
                }    

        protected override void OnBarUpdate()
        {
			
			if ( !ohlc1.DayClose.IsValidDataPoint(0) )
				return;
			double dhigh0 = ohlc0.DayHigh[0];
			double dlow0 = ohlc0.DayLow[0];
			double dopen0 = ohlc0.DayOpen[0];
			double dclose1 = ohlc1.DayClose[0];		
			double dlow1 = ohlc1.DayLow[0];
			double dhigh1 = ohlc1.DayHigh[0];
			
			if ( Bars.IsFirstBarOfSession ) {
				dailyOpens.Insert(0, Open[0]);
				dailyHighs.Insert(0, High[0]);
				dailyLows.Insert(0, Low[0]);
				dailyCloses.Insert(0, Close[0]);
			}
			
			if (dailyCloses.Count < 1)
				return;
			dailyHighs[0] = Math.Max(dailyHighs[0], High[0]);
			dailyLows[0] = Math.Min(dailyLows[0], Low[0]);
			dailyCloses[0] = Close[0];
			if (dailyCloses.Count < 2)
				return;

			if ( Bars.IsFirstBarOfSession ) {
				SetStopLoss(CalculationMode.Currency, _cntrcts * StopLoss);
				
				if ((dopen0 < dhigh1) && (dopen0 > dlow1))
					SetProfitTarget(CalculationMode.Currency, _cntrcts * GapFillTP);
				else 
					SetProfitTarget(CalculationMode.Currency, _cntrcts * GapContTP);
								
				if (!h.IsHoliday(ToEst(Time[0]))) {
				
				if ((Open[0] < Close[1]) 
					&& (Open[0] > dlow1) 
					&& (dclose1 > SMA(Closes[1],_L1)[1]))
					EnterLong(_cntrcts, "GapGuruFill LE");

				if ((Open[0] > Close[1]) 
					&& (Open[0] < dhigh1) 
					&& (dclose1 < SMA(Closes[1],_L1)[1]))
					EnterShort(_cntrcts, "GapGuruFill SE");
				
				if ((Open[0] < dlow1) 
						&& (dclose1 > SMA(Closes[1],_L1)[1]))
					EnterShort(_cntrcts, "GapGuruCont SE");
				
				if ((Open[0] >dhigh1) 
						&& (dclose1 < SMA(Closes[1],_L1)[1]))
					EnterLong(_cntrcts, "GapGuruCont LE");
			}
			}

			if ((Position.MarketPosition == MarketPosition.Long) && (ToEst(Time[0]).TimeOfDay == _LongExTime))
				ExitLong();
			if ((Position.MarketPosition == MarketPosition.Short) && (ToEst(Time[0]).TimeOfDay == _ShortExTime))
				ExitShort();
			}
		
		
		[Range(0, int.MaxValue), NinjaScriptProperty]
	        [Display(ResourceType = typeof(Custom.Resource), Name = "Quantity", GroupName = "Trade Settings", Order = 9)]
	        public int _cntrcts
	        { get; set; }	
			
		 [Range(0, int.MaxValue), NinjaScriptProperty]
	        [Display(ResourceType = typeof(Custom.Resource), Name = "Stop Loss", GroupName = "Trade Settings", Order = 9)]
	        public double StopLoss
	        { get; set; }
			
		[Range(0, int.MaxValue), NinjaScriptProperty]
	        [Display(ResourceType = typeof(Custom.Resource), Name = "Gap Fill TP", GroupName = "Trade Settings", Order = 9)]
	        public double GapFillTP
	        { get; set; }
			
		[Range(0, int.MaxValue), NinjaScriptProperty]
	        [Display(ResourceType = typeof(Custom.Resource), Name = "Gap Cont. TP", GroupName = "Trade Settings", Order = 9)]
	        public double GapContTP
	        { get; set; }	
		}
	
	
    }
