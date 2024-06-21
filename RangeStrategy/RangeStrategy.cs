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

namespace NinjaTrader.NinjaScript.Strategies
{
	[CategoryOrder(typeof(Custom.Resource), "Parameters", 1)]
	

	public class RangeStrategy : Strategy
	{
		#region Variables

		bool _triggerTime;

		//---------------------------
				
		private double _highRange;
		private double _lowRange;
		private double _halfRange;

		bool _activateTrades = false;
		bool _stopTrades = false;
		bool _activateFisrtOperation = false;
		int _counterbars = 0;
		int _counterCandlesRange=0;

		string _lastTrade   = "";

		double _stopShort   = 0;
		double _stopLong    = 0;
		double _targetLong  = 0;		
		double _targetShort = 0;
		
		//---------------------------
		private double _qty;
		private double _profit;
		private double _stop;
		private double _entry;
		private double _capital;
		private int _actualBEValue;

		private string _debugin;

		private Order _entryOrder = null;
		private Order _targetOrder = null;
		private Order _stopOrder = null;

		private System.Windows.Controls.Button BenableStrategy;
		private System.Windows.Controls.Button BStopStrategy;		
		private System.Windows.Controls.Grid myGrid;

		public enum TypeOrder { Long, Short };

		public enum TypeDirection { SameDirection , OppositeDirection };

		private NinjaTrader.Gui.Tools.SimpleFont _myFont2;

		private int _countOperation = 0;

		int _counterOperation = 0;

		#endregion

		#region Parameters

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name = "Take Profit in Points", Order = 1, GroupName = "Parameters")]
		public double TakeProfit
		{ get; set; }


		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name = "Range Start Time", Order = 2, GroupName = "Parameters")]
		public DateTime InitRange
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name = "Range End Time", Order = 3, GroupName = "Parameters")]
		public DateTime EndRange
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name = "Initial Contract Quantity", Order = 4, GroupName = "Parameters")]
		public double InitialQuantity
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name = "Arithmetic Increment", Order = 5, GroupName = "Parameters")]
		public int ContractIncrement
		{ get; set; }


		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name = "Recovery Gap of Point", Order = 6, GroupName = "Parameters")]
		public double RecoveryGap
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Trade Direction", Order = 7, GroupName = "Parameters")]
		public TypeDirection TypeDirection_ { get; set; }

		#endregion


		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				SetStrategyConfiguration();
				SetParameters();
				SetVaribles();
				ClearOutputWindow();
			}
			else if (State == State.Configure)
			{
				AddIndicatorInChart();
			}
			if (State == State.SetDefaults)
			{
				//AddPlot(new Stroke(Brushes.Green), PlotStyle.Line, "Stop");
				//AddPlot(new Stroke(Brushes.Blue) , PlotStyle.Line, "Price");
			}
			else if (State == State.Historical)
			{
				ActivateButtons();
			}
			else if (State == State.Terminated)
			{
				DeactivateButtons();
			}
		}

		#region OnStateChange
		protected void SetStrategyConfiguration()
		{
			Name      = "Range-Strategy";
			Calculate = Calculate.OnPriceChange;
			EntriesPerDirection = 1;
			EntryHandling = EntryHandling.UniqueEntries;
			IsExitOnSessionCloseStrategy = true;
			//ExitOnSessionCloseSeconds = 30;
			IsFillLimitOnTouch = true;
			Slippage = 0;
			MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
			OrderFillResolution = OrderFillResolution.Standard;
			StartBehavior = StartBehavior.WaitUntilFlat;
			TimeInForce = TimeInForce.Gtc;
			TraceOrders = false;
			RealtimeErrorHandling = RealtimeErrorHandling.IgnoreAllErrors;
			StopTargetHandling = StopTargetHandling.PerEntryExecution;
			IsInstantiatedOnEachOptimizationIteration = false; // Disable this property for performance gains in Strategy Analyzer optimizations 			
			BarsRequiredToTrade = 20;
		}
		protected void SetParameters()
		{
			TakeProfit        = 50;
			InitRange         = DateTime.Parse("17:00", System.Globalization.CultureInfo.InvariantCulture);
			EndRange          = DateTime.Parse("03:30", System.Globalization.CultureInfo.InvariantCulture);
			InitialQuantity   = 4;
			ContractIncrement = 1;
			RecoveryGap       = 3;
			TypeDirection_    = TypeDirection.SameDirection;
		}
		protected void SetVaribles()
		{
			_capital     = SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit;
			_stop        = 0;
			_profit      = 0;
			_triggerTime = false;						
		}
		protected void AddIndicatorInChart()
		{


		}
        #endregion

        #region InMyButtonClick
        protected void OnMyButtonClick(object sender, RoutedEventArgs rea) 
		{
			System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
			if (button != null)
			{
				ClickManagement(button.Name);
			}
		}
        #endregion

        protected override void OnBarUpdate()
		{
			if (CurrentBar <= BarsRequiredToTrade)
			{				
				Calculate = Calculate.OnPriceChange;
				return;
			}		

			try
			{
                if (IsFirstTickOfBar)
                {
					_counterbars++;
					_counterCandlesRange++;
				}
				//_debugin += "Close[0]==" + Close[0];

				//---------------------------------------------
				if (_activateTrades && _activateFisrtOperation)
				{
					if (LongCondition() && !InMarKet())
					{
						_countOperation = 0;
						_qty = InitialQuantity;
						EnterLong(Convert.ToInt32(_qty), @"Long_");
						_counterbars = 0;
						_debugin += "FirtsLong" + "	\n";
					}

					if (ShortCondition() && !InMarKet())
					{
						_countOperation = 0;
						_qty = InitialQuantity;
						EnterShort(Convert.ToInt32(_qty), @"Short_");
						_counterbars = 0;
						_debugin += "FirtsShort" + "	\n";
					}
					_activateFisrtOperation = false;
				}
				///---------------------------------------------								
				SetRange();				
				DrawStopProfit();				
				//Debug();
				_debugin = "";
			}
			catch (Exception e)
			{
				Print(e);
			}
			

			if (Time[0].Hour == 6 && Time[0].Minute == 45)
		    {
		        Draw.VerticalLine(this, "SixFortyFiveAM", Time[0], Brushes.Lime, DashStyleHelper.Dash, 4);
		    }
			if (Time[0].Hour == 8 && Time[0].Minute == 0)
		    {
		        Draw.VerticalLine(this, "EigthAM", Time[0], Brushes.Red, DashStyleHelper.Dash, 4);
		    }
	
			// Calculate the High and Low of the day
		    double highOfDay = CurrentDayOHL().CurrentHigh[0];
		    double lowOfDay = CurrentDayOHL().CurrentLow[0];

			// Draw a horizontal line for the High of day
		    Draw.HorizontalLine(this, "HighOfDay", highOfDay, Brushes.Red);
			// Text box positioning (adjust as needed)
			int xPosition = 5; // Adjust X-axis offset
			int yPosition = (int)highOfDay - 0; // Adjust Y-axis offset relative to line
			// Text box content
			string labelText = "Today's High Liqudity"; // Replace with your desired label
			// Draw the text box
			Draw.Text(this, "TextOverlay", labelText, xPosition, yPosition, Brushes.White);
		    
				
				
			// Draw a horizontal line for the Low of day
		    Draw.HorizontalLine(this, "LowOfDay", lowOfDay, Brushes.Lime);
			// Text box positioning (adjust as needed)
			int xPosition1 = 5; // Adjust X-axis offset
			int yPosition1 = (int)lowOfDay - 0; // Adjust Y-axis offset relative to line
			// Text box content
			string labelText1 = "Today's Low Liqudity"; // Replace with your desired label
			// Draw the text box
			Draw.Text(this, "TextOverlay1", labelText1, xPosition1, yPosition1, Brushes.White);
				
				
			// Calculate the high and low of yesterday
			double highOfYesterday = PriorDayOHLC().PriorHigh[0];
			double lowOfYesterday = PriorDayOHLC().PriorLow[0];
				
			
				
			// Draw a horizontal line for the high of yesterday
			Draw.HorizontalLine(this, "HighOfYesterday", highOfYesterday, Brushes.Red);
			// Text box positioning (adjust as needed)
			int xPosition2 = 5; // Adjust X-axis offset
			int yPosition2= (int)highOfYesterday - 0; // Adjust Y-axis offset relative to line
			// Text box content
			string labelText2 = "Yesterday's High Liqudity"; // Replace with your desired label
			// Draw the text box
			Draw.Text(this, "TextOverlay2", labelText2, xPosition2, yPosition2, Brushes.White);
				

			// Draw a horizontal line for the low of yesterday
			Draw.HorizontalLine(this, "LowOfYesterday", lowOfYesterday, Brushes.Lime);
			// Text box positioning (adjust as needed)
			int xPosition3 = 5; // Adjust X-axis offset
			int yPosition3= (int)lowOfYesterday  - 0; // Adjust Y-axis offset relative to line
			// Text box content
			string labelText3 = "Yesterday's Low Liqudity"; // Replace with your desired label
			// Draw the text box
			Draw.Text(this, "TextOverlay3", labelText3, xPosition3, yPosition3, Brushes.White);
				
		/*		
			// Calculate the high and low of yesterday
			double highOf2DaysAgo = PriorDayOHLC().PriorHigh[1];
			double lowOf2DaysAgo = PriorDayOHLC().PriorLow[1];
				
			// Draw a horizontal line for the high of highOf2DaysAgo
			Draw.HorizontalLine(this, "highOf2DaysAgo", highOf2DaysAgo, Brushes.Red);
			// Text box positioning (adjust as needed)
			int xPosition15 = 5; // Adjust X-axis offset
			int yPosition15= (int)highOf2DaysAgo - 0; // Adjust Y-axis offset relative to line
			// Text box content
			string labelText15 = "2 Day's High Liqudity"; // Replace with your desired label
			// Draw the text box
			Draw.Text(this, "TextOverlay15", labelText15, xPosition15, yPosition15, Brushes.White);
				

			// Draw a horizontal line for the low of lowOf2DaysAgo
			Draw.HorizontalLine(this, "lowOf2DaysAgo", lowOf2DaysAgo, Brushes.Red);
			// Text box positioning (adjust as needed)
			int xPosition16 = 5; // Adjust X-axis offset
			int yPosition16= (int)lowOf2DaysAgo - 0; // Adjust Y-axis offset relative to line
			// Text box content
			string labelText16 = "2 Day's Low Liqudity"; // Replace with your desired label
			// Draw the text box
			Draw.Text(this, "TextOverlay16", labelText16, xPosition16, yPosition16, Brushes.White);
		*/		
//////////////////////////////000000000000000000000///////////////////////////			
				
			// Calculate the high and low of ppLiquidity
			double ppLiquidity = (PriorDayOHLC().PriorHigh[0] + PriorDayOHLC().PriorLow[0]+PriorDayOHLC().PriorClose[0])/3.0;
				
			// Calculate the high and low of R1 Liquidity
			double R1Liquidity = ((ppLiquidity * 2) - PriorDayOHLC().PriorLow[0]);
			// Calculate the high and low of R2 Liquidity
			double R2Liquidity = (ppLiquidity + (PriorDayOHLC().PriorHigh[0] - PriorDayOHLC().PriorLow[0]));
			// Calculate the high and low of S2 Liquidity
			double S2Liquidity = (ppLiquidity - (PriorDayOHLC().PriorHigh[0] - PriorDayOHLC().PriorLow[0]));
			// Calculate the high and low of R3 Liquidity
			double R3Liquidity = (ppLiquidity - S2Liquidity + R2Liquidity);
			// Calculate the high and low of S1 Liquidity
			double S1Liquidity = ((ppLiquidity * 2) - PriorDayOHLC().PriorHigh[0]);			
			// Calculate the high and low of S3 Liquidity
			double S3Liquidity = (ppLiquidity - R2Liquidity + S2Liquidity);
				
				
			// Draw a horizontal line for the high of ppLiquidity
			Draw.HorizontalLine(this, "ppLiquidity", ppLiquidity, Brushes.White);
			// Text box positioning (adjust as needed)
			int xPosition4 = 5; // Adjust X-axis offset
			int yPosition4= (int)ppLiquidity - 0; // Adjust Y-axis offset relative to line
			// Text box content
			string labelText4 = "PP_Liquidity"; // Replace with your desired label
			// Draw the text box
			Draw.Text(this, "TextOverlay4", labelText4, xPosition4, yPosition4, Brushes.White);
				

			// Draw a horizontal line for the high of ppLiquidity
			Draw.HorizontalLine(this, "R1Liquidity", R1Liquidity, Brushes.Red);
			// Text box positioning (adjust as needed)
			int xPosition5 = 5; // Adjust X-axis offset
			int yPosition5= (int)R1Liquidity - 0; // Adjust Y-axis offset relative to line
			// Text box content
			string labelText5 = "R1 Liquidity"; // Replace with your desired label
			// Draw the text box
			Draw.Text(this, "TextOverlay5", labelText5, xPosition5, yPosition5, Brushes.White);
				
						
			// Draw a horizontal line for the high of R2Liquidity
			Draw.HorizontalLine(this, "R2Liquidity", R2Liquidity, Brushes.Red);
			// Text box positioning (adjust as needed)
			int xPosition6 = 5; // Adjust X-axis offset
			int yPosition6= (int)R2Liquidity - 0; // Adjust Y-axis offset relative to line
			// Text box content
			string labelText6 = "R2 Liquidity"; // Replace with your desired label
			// Draw the text box
			Draw.Text(this, "TextOverlay6", labelText6, xPosition6, yPosition6, Brushes.White);
				
						
			// Draw a horizontal line for the high of R3Liquidity
			Draw.HorizontalLine(this, "R3Liquidity", R3Liquidity, Brushes.Red);
			// Text box positioning (adjust as needed)
			int xPosition7 = 5; // Adjust X-axis offset
			int yPosition7= (int)R3Liquidity - 0; // Adjust Y-axis offset relative to line
			// Text box content
			string labelText7 = "R3 Liquidity"; // Replace with your desired label
			// Draw the text box
			Draw.Text(this, "TextOverlay7", labelText7, xPosition7, yPosition7, Brushes.White);
				
			
			
			// Draw a horizontal line for the high of R1Liquidity
			Draw.HorizontalLine(this, "S1Liquidity", S1Liquidity, Brushes.Lime);
			// Text box positioning (adjust as needed)
			int xPosition8 = 5; // Adjust X-axis offset
			int yPosition8= (int)S1Liquidity - 0; // Adjust Y-axis offset relative to line
			// Text box content
			string labelText8 = "S1 Liquidity"; // Replace with your desired label
			// Draw the text box
			Draw.Text(this, "TextOverlay8", labelText8, xPosition8, yPosition8, Brushes.White);
				
				
			
			// Draw a horizontal line for the high of R2Liquidity
			Draw.HorizontalLine(this, "S2Liquidity", S2Liquidity, Brushes.Lime);
			// Text box positioning (adjust as needed)
			int xPosition9 = 5; // Adjust X-axis offset
			int yPosition9= (int)S2Liquidity - 0; // Adjust Y-axis offset relative to line
			// Text box content
			string labelText9 = "S2 Liquidity"; // Replace with your desired label
			// Draw the text box
			Draw.Text(this, "TextOverlay9", labelText9, xPosition9, yPosition9, Brushes.White);
				
				
		
			// Draw a horizontal line for the high of R3Liquidity
			Draw.HorizontalLine(this, "S3Liquidity", S3Liquidity, Brushes.Lime);
			// Text box positioning (adjust as needed)
			int xPosition10 = 5; // Adjust X-axis offset
			int yPosition10= (int)S3Liquidity - 0; // Adjust Y-axis offset relative to line
			// Text box content
			string labelText10 = "S3 Liquidity"; // Replace with your desired label
			// Draw the text box
			Draw.Text(this, "TextOverlay10", labelText10, xPosition10, yPosition10, Brushes.White);
				
///////////////////////////////////////////	
				/*
			// Calculate the high and low of the prior week (assuming weekly data)
			double highOfPriorWeek = Highest(High, 1)[1];  // Highest high of the last week (one week ago)
			double lowOfPriorWeek = Lowest(Low, 1)[1];   // Lowest low of the last week (one week ago)
			
			// Draw a horizontal line for the high of the prior week
			Draw.HorizontalLine(this, "HighOfPriorWeek", highOfPriorWeek, Brushes.Red);
			// Text box positioning (adjust as needed)
			int xPosition4 = 5; // Adjust X-axis offset
			int yPosition4= (int)highOfPriorWeek - 0; // Adjust Y-axis offset relative to line
			// Text box content (Update label for prior week)
			string labelText4 = "Last Week's COT High Liquidity"; 
			// Draw the text box
			Draw.Text(this, "TextOverlay4", labelText4, xPosition4, yPosition4, Brushes.White);
			
				
			// Draw a horizontal line for the low of the prior week
			Draw.HorizontalLine(this, "LowOfPriorWeek", lowOfPriorWeek, Brushes.Lime);
			// Text box positioning (adjust as needed)
			int xPosition5 = 5; // Adjust X-axis offset
			int yPosition5= (int)lowOfPriorWeek  - 0; // Adjust Y-axis offset relative to line
			// Text box content (Update label for prior week)
			string labelText5 = "Last Week's COT Low Liquidity"; 
			// Draw the text box
			Draw.Text(this, "TextOverlay5", labelText5, xPosition5, yPosition5, Brushes.White);
				*/
//////////////////////////////////////////////////	

		}

        #region CheckErrors
        private void CheckErrors()
        {
			if (InLong() && _stopOrder != null)
            {
				if (_stopOrder.OrderState != OrderState.Working )
                {	
					ExitLong((int)_qty, "Stop_", "Long_");
				}
            }
			if (InShort() && _stopOrder != null)
            {
				if (_stopOrder.OrderState != OrderState.Working )
				{
					ExitShort((int)_qty, "Stop_", "Short_");
				}
			}
            
        }
        #endregion

        #region CalcQuantity
        private void CalcQty()
		{
			_qty = InitialQuantity + (ContractIncrement * _countOperation);//Math.Pow(ContractIncrement, _countOperation);
			_debugin += "Operation=" + (_countOperation + 1) + "---Quantity = " + _qty + "	\n";
		}
        #endregion     

        #region CloseTrades
        private void CloseEntry()
		{
			if (_stopTrades)
            {
				if (InLong())
                {
					ExitLong((int)_qty, "ExitLong", "Long_");
					_debugin += "------LongStop------" + "	\n";
                }
				if (InShort())
                {
					ExitShort((int)_qty, "ExitShort", "Short_");
					_debugin += "-----ShortStop-----" + "	\n";
				}
				if (!InMarKet())
                {
					CancelOrder(_entryOrder);
                }
            }
		}
        #endregion

        #region SetRange
        private void SetRange()
        {
			if (EnableTradingHour(InitRange.TimeOfDay))
			{				
				_counterCandlesRange = 0;
				//Draw.VerticalLine(this, "VL1" + CurrentBar, 0, Brushes.Blue, DashStyleHelper.Dash, 1);
			}

			if (EnableTradingHour(EndRange.TimeOfDay))
			{
				_highRange = High[HighestBar(High, _counterCandlesRange + 1)];
				_lowRange  = Low[LowestBar (Low , _counterCandlesRange + 1)];				
				_halfRange = Math.Round(_lowRange + Math.Abs(_highRange - _lowRange)/ 2 , 2);
				_debugin  += "High=" + _highRange + "--Low=" + _lowRange + "--Half=" + _halfRange +"--Counter=" + _counterCandlesRange + "	\n";
				//Draw.VerticalLine(this, "VL2" + CurrentBar, 0, Brushes.Blue, DashStyleHelper.Dash, 1);
				///	
				Draw.Line(this, "R1" + CurrentBar, true, _counterCandlesRange +0, _highRange, 0, _highRange, Brushes.Aqua, DashStyleHelper.Solid, 1);
				// Text box positioning (adjust as needed)
				int xPosition12 = 5; // Adjust X-axis offset
				int yPosition12 = (int)_highRange - 0; // Adjust Y-axis offset relative to line
				// Text box content
				string labelText12 = "Asia High Liqudity"; // Replace with your desired label
				// Draw the text box
				Draw.Text(this, "TextOverlay12", labelText12, xPosition12, yPosition12, Brushes.White);
				///
				Draw.Line(this, "R1" + CurrentBar, true, _counterCandlesRange +0, _highRange, 0, _highRange, Brushes.Aqua, DashStyleHelper.Solid, 1);
				///
				Draw.Line(this, "R2" + CurrentBar, true, _counterCandlesRange + 0, _lowRange, 0, _lowRange, Brushes.Aqua, DashStyleHelper.Solid, 1) ;
				///
				Draw.Line(this, "R3" + CurrentBar, true, _counterCandlesRange +0, _halfRange, 0, _halfRange, Brushes.BlueViolet , DashStyleHelper.Solid, 1);
				// Text box positioning (adjust as needed)
				int xPosition13 = 5; // Adjust X-axis offset
				int yPosition13 = (int)_lowRange - 0; // Adjust Y-axis offset relative to line
				// Text box content
				string labelText13 = "Asia Low Liqudity"; // Replace with your desired label
				// Draw the text box
				Draw.Text(this, "TextOverlay13", labelText13, xPosition13, yPosition13, Brushes.White);
			}
		}
        #endregion

        #region OrderConditions
        protected bool LongCondition()
		{
			bool condition1 = Close[0] < _halfRange ;
			_debugin += "LongCondition=" + condition1 + "---Half=" + _halfRange + "---Close="+ Close[0] + "	\n"; 
			return condition1;
		}
		protected bool ShortCondition()
		{
			bool condition1 = Close[0] > _halfRange;
			_debugin += "ShortCondition=" + condition1 + "---Half=" + _halfRange + "---Close=" + Close[0] + "	\n";
			return condition1;
		}
		#endregion

		#region Profit/Stop
		protected void SetStopProfit(string Side)
		{
			if (Side == "Long")
			{
				_stop   = _stopLong;
				_profit = _targetLong;				
				if (Close[0] > _stop)
                {
					ExitLongStopMarket(0, true, (int)_qty, _stop, @"Stop_", @"Long_");
					ExitLongLimit(0, true, (int)_qty, _profit, @"Target_", @"Long_");
				}
				else
                {
					ExitLong((int)_qty, "Stop_", "Long_");
                }
			
				_debugin += "StopProfitLong---" + _countOperation + "---Stop=" + _stop + "---Target=" + _profit + "	\n";
			}

			if (Side == "Short")
			{
				_stop   = _stopShort;
				_profit = _targetShort;
				if (Close[0] < _stop)
				{
					ExitShortStopMarket(0, true, (int)_qty, _stop  , @"Stop_", @"Short_");
					ExitShortLimit     (0, true, (int)_qty, _profit, @"Target_", @"Short_");
				}
                else
                {
					ExitShort((int)_qty, "Stop_", "Short_");
				}

									
				_debugin += "--StopProfitShort---" + _countOperation + "---Stop=" + _stop + "---Target=" + _profit + "	\n";
			}
			DrawStopProfit();
		}
		#endregion	

		#region OnOrderUpdate-------------
		protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
		{
			if (order.Name == "Long_" || order.Name == "Short_")
				_entryOrder = order;

			if (order.Name == "Target_")
			{
				_targetOrder = order;
			}
			if (order.Name == "Stop_")
				_stopOrder = order;
			//__________________________________________________
			//if (_entryOrder != null && (_entryOrder.OrderState == OrderState.Cancelled || _entryOrder.OrderState == OrderState.Rejected))
			//	_entryOrder = null;
			//
			//if (_targetOrder != null && (_targetOrder.OrderState == OrderState.Cancelled || _targetOrder.OrderState == OrderState.Rejected))
			//	_targetOrder = null;
			//
			//if (_stopOrder != null && (_stopOrder.OrderState == OrderState.Cancelled || _stopOrder.OrderState == OrderState.Rejected))
			//	_stopOrder = null;
			//
			//if (order.OrderState == OrderState.Cancelled)
			//	_debugin += " |OrderCancell=" + order.Name + "| ";

			//--------------Control----------------------------------

			if (order.OrderState == OrderState.Unknown)
			{
				_debugin += order.Name + "-Unknown" + "\n	";
			}
			if (order.OrderState == OrderState.Cancelled)
			{
				_debugin += order.Name + "-Cancelled" + "\n	";
			}
			if (order.OrderState == OrderState.Rejected)
			{
				_debugin += order.Name + "-Rejected" + "\n	";
				if (InLong())
                {
					ExitLong((int)_qty, "Stop_", "Long_");
				}
				if (InShort())
                {
					ExitShort((int)_qty, "Stop_", "Short_");
				}
			}
			if (order.OrderState == OrderState.ChangePending)
			{
				_debugin += order.Name + "-ChangePending" + "\n	";
			}
			if (order.OrderState == OrderState.Working)
			{
				_debugin += order.Name + "--Working = " + "--" + order.IsLiveUntilCancelled + "--" + order.FromEntrySignal + "--"
					+ order.LimitPrice + "--" + order.StopPrice + "--" + order.Quantity + "--" + order.AverageFillPrice + "\n	";
			}
		}
		#endregion

		#region OnExecutionUpdate---------
		protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{
			if (execution.Order.OrderState == OrderState.Filled)
			{
				if (execution.Order.Name == "Long_")
				{
					if (_countOperation == 0)
                    {
						_entry       = execution.Order.AverageFillPrice;
						_stopShort   = _entry;
						_stopLong    = _entry - RecoveryGap;
						_targetLong  = _entry + TakeProfit;
						_targetShort = _entry - (RecoveryGap + TakeProfit);
					}
						
					_countOperation++;
					_lastTrade = "Long";
					_debugin += "LongExecuted"+ "	\n";
					SetStopProfit("Long");
				}
				if (execution.Order.Name == "Short_")
				{
                    if (_countOperation == 0)
                    {
						_entry = execution.Order.AverageFillPrice;
						_stopLong    = _entry;
						_stopShort   = _entry + RecoveryGap;
						_targetShort = _entry - TakeProfit;
						_targetLong  = _entry + (RecoveryGap + TakeProfit);
					}						
					_countOperation++;
					_lastTrade = "Short";
					_debugin += "ShortExecuted" + "	\n";
					SetStopProfit("Short");
				}

				if (execution.Order.Name == "Target_")
				{
					OrderComplete();
				}
				if (execution.Order.Name == "Stop_")
				{

					if (_countOperation <=10)
                    {
						CalcQty();
						if (TypeDirection_ == TypeDirection.OppositeDirection)
						{
							bool _sw = true;
							if (_lastTrade == "Long" && _sw)
							{
								_debugin += "StopTouchGo--Short" + "	\n";
								EnterShort(Convert.ToInt32(_qty), @"Short_");
								_sw = false;
							}
							if (_lastTrade == "Short" && _sw)
							{
								_debugin += "StopTouchGo--Long" + "	\n";
								EnterLong(Convert.ToInt32(_qty), @"Long_");
								_sw = false;
							}
						}

						if (TypeDirection_ == TypeDirection.SameDirection)
						{
							bool _sw = true;
							if (_lastTrade == "Long" && _sw)
							{
								_debugin += "StopTouchGo--SameLong" + "	\n";
								if (Close[0] < _entry)
								{
									EnterLongStopMarket(0, true, Convert.ToInt32(_qty), _entry, @"Long_");
								}
								else
								{
									EnterLong(Convert.ToInt32(_qty), @"Long_");
								}

								_sw = false;
							}
							if (_lastTrade == "Short" && _sw)
							{
								_debugin += "StopTouchGo--SameShort" + "	\n";
								if (Close[0] > _entry)
								{
									EnterShortStopMarket(0, true, Convert.ToInt32(_qty), _entry, @"Short_");
								}
								else
								{
									EnterShort(Convert.ToInt32(_qty), @"Short_");
								}
								_sw = false;
							}
						}
					}
					
				}			
				//-----------------------------------------------------------------------------------------------------------				
				DrawStopProfit();
			}

		}

        
        #endregion
		
        #region CancelOrder
        protected void CancelOrders(string from)
		{
			//if (_entryOrder != null)
			//	CancelOrder(_entryOrder);
			//if (_targetOrder != null)
			//	CancelOrder(_targetOrder);
			//if (_stopOrder != null)
			//	CancelOrder(_stopOrder);
			//_condTriggerTime = true;
			//_countOperation = 0;
			//_debugin += " |AllOrdersCancel=" + from + "| ";
		}

		#endregion

		#region Draw
		protected void DrawStopProfit()
		{
			if (InMarKet())
			{
				if (_stop > 0)
				{
					Draw.Line(this, "L1", true, _counterbars , _stopLong    , -100, _stopLong   , Brushes.Orange, DashStyleHelper.Solid, 2);
					Draw.Line(this, "L2", true, _counterbars , _stopShort   , -100, _stopShort  , Brushes.Orange, DashStyleHelper.Solid, 2);
					Draw.Line(this, "L3", true, _counterbars , _targetLong  , -100, _targetLong , Brushes.GreenYellow , DashStyleHelper.Solid, 2);
					Draw.Line(this, "L4", true, _counterbars , _targetShort , -100, _targetShort, Brushes.GreenYellow , DashStyleHelper.Solid, 2);
				}
			}
		}
		protected void DrawArrow(string side)
		{
			if (side == "Long")
			{
				Draw.ArrowUp(this, "ArrowUp" + CurrentBars[0], true, 0, Low[0] - 4 * TickSize, Brushes.Gold);
			}
			if (side == "Short")
			{
				Draw.ArrowDown(this, "ArrowDown" + CurrentBars[0], true, 0, High[0] + 4 * TickSize, Brushes.Gold);
			}
		}		
		#endregion		

		#region Logical


		protected bool EnableTradingHour(TimeSpan _start)//TimeSpan _start)
		{
			int Hour   = _start.Hours;
			int Minute = _start.Minutes;

			bool condition1 = Times[0][0].Hour   == Hour;
			bool condition2 = Times[0][0].Minute == Minute + (State == State.Realtime ? 1 : 0);

			return (condition1 && condition2 );
		}

		protected void OrderComplete()
		{
			CancelOrders("OrderComplete");
		}
		protected bool InLong()
		{
			return Position.MarketPosition == MarketPosition.Long;
		}

		protected bool InShort()
		{
			return Position.MarketPosition == MarketPosition.Short;
		}
		protected bool InMarKet()
		{
			return Position.MarketPosition != MarketPosition.Flat;
		}

		protected void Debug()
		{
			if (_debugin != "")
				Print(Time[0].Day + "-" + Time[0].Month + "|" + Time[0].Hour + ":" + Time[0].Minute + ":" + Time[0].Second + "| " + _debugin);
		}


		#endregion

		#region Buttons
		protected void DeactivateButtons()
		{
			if (ChartControl == null)
				return;

			// Again, we need to use a Dispatcher to interact with the UI elements
			ChartControl.Dispatcher.InvokeAsync((() =>
			{
				if (myGrid != null)
				{
					if (BenableStrategy != null)
					{
						myGrid.Children.Remove(BenableStrategy);
						BenableStrategy.Click -= OnMyButtonClick;
						BenableStrategy = null;
					}
					if (BStopStrategy != null)
					{
						myGrid.Children.Remove(BStopStrategy);
						BStopStrategy.Click -= OnMyButtonClick;
						BStopStrategy = null;
					}
					
				}
			}));
		}

		protected void ActivateButtons()
		{
			ChartControl.Dispatcher.InvokeAsync((() =>
			{
				// Grid already exists
				if (UserControlCollection.Contains(myGrid))
					return;

				// Add a control grid which will host our custom buttons
				myGrid = new System.Windows.Controls.Grid
				{
					Name = "MyGrid",
					// Align the control to the top right corner of the chart
					HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
					VerticalAlignment   = VerticalAlignment.Top,
				};

				// Define the two columns in the grid, one for each button
				System.Windows.Controls.ColumnDefinition column1 = new System.Windows.Controls.ColumnDefinition();
				System.Windows.Controls.ColumnDefinition column2 = new System.Windows.Controls.ColumnDefinition();
				
				// Add the columns to the Grid
				myGrid.ColumnDefinitions.Add(column1);
				myGrid.ColumnDefinitions.Add(column2);
			

				// Define the custom Buy Button control object
				BenableStrategy = new System.Windows.Controls.Button
				{
					Name      = "Start_",
					Content   = "Start",
					Foreground = Brushes.Black,
					Background = Brushes.White
				};

				BStopStrategy = new System.Windows.Controls.Button
				{
					Name       = "Stop_",
					Content    = "Stop",
					Foreground = Brushes.Black,
					Background = Brushes.White,
				};

				

				// Subscribe to each buttons click event to execute the logic we defined in OnMyButtonClick()
				BenableStrategy.Click += OnMyButtonClick;
				BStopStrategy.Click += OnMyButtonClick;
				

				// Define where the buttons should appear in the grid
				System.Windows.Controls.Grid.SetColumn(BenableStrategy, 0);
				System.Windows.Controls.Grid.SetColumn(BStopStrategy  , 1);
				

				// Add the buttons as children to the custom grid
				myGrid.Children.Add(BenableStrategy);
				myGrid.Children.Add(BStopStrategy);			

				// Finally, add the completed grid to the custom NinjaTrader UserControlCollection
				UserControlCollection.Add(myGrid);

			}));
		}
		protected void ClickManagement(string buttonName)
		{

			Print(buttonName + " Clicked");

			if (true)
			{
				if (buttonName == "Start_")
				{
					if (_activateTrades == false)
                    {
						_activateTrades            = true;
						_activateFisrtOperation    = true;
						BenableStrategy.Background = Brushes.GreenYellow;
						BStopStrategy.Background   = Brushes.White;
						_debugin += "PressStart_" + "	\n";
						//OnBarUpdate();
					}                    
				}

				if (buttonName == "Stop_")
				{					
					if (_activateTrades == true)
					{
						_activateTrades = false;
						_stopTrades     = true;
						CloseEntry();
						BStopStrategy.Background   = Brushes.GreenYellow;
						BenableStrategy.Background = Brushes.White;
						_debugin += "PressStop_" + "	\n";
					}                 
				}				
			}			
		} 


        #endregion
		
		
    }
}
