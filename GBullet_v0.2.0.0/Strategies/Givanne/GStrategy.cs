#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Strategies.Givanne;
using NinjaTrader.NinjaScript.Strategies.Givanne.Gui;

#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{

    [Gui.CategoryOrder("FVG Data Series", 1)]
    [Gui.CategoryOrder("ATR", 2)]
    [Gui.CategoryOrder("Trade", 3)]
    [Gui.CategoryOrder("FVG Data Series Label", 4)]
    [Gui.CategoryOrder("FVG Colors", 5)]
    [Gui.CategoryOrder("LD SESSION SIVER BULLET (EST TIME)", 6)]
    [Gui.CategoryOrder("AM SESSION SIVER BULLET (EST TIME)", 7)]
    [Gui.CategoryOrder("PM SESSION SIVER BULLET (EST TIME)", 8)]
    [Gui.CategoryOrder("Risk Management", 9)]

    public class GStrategy : Strategy
	{
        // TODO Too much properties
        public TradingDataContainer TradingDataContainer { get; set; } = new TradingDataContainer();
        public TechnicalIndicators TechnicalIndicators { get; set; } = new TechnicalIndicators();
		public ChartUserInterface ChartUserInterface { get; set; } = new ChartUserInterface();
		public TimeInfo TimeInfo { get; set; } = new TimeInfo();
		public Configurator Configurator { get; set; } = new Configurator();


        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "G Bullet";
				Calculate									= Calculate.OnBarClose;
                IsOverlay									= true;
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
				RealtimeErrorHandling						= RealtimeErrorHandling.IgnoreAllErrors;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 256;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration   = true;

                UpBrush 					= Brushes.DarkGreen; // Si cambio esto a transparente va a quedar mucho mejor
                DownBrush					= Brushes.Maroon; // Aca lo mismo
                UpAreaBrush 				= Brushes.DarkGreen;
                DownAreaBrush 				= Brushes.Maroon;
                ChartUserInterface.BrushSettings.FillBrush 					= Brushes.DimGray;
                DisplayCE 					= false;
                UseATR 						= true;
                ActiveAreaOpacity 			= 13;
                FilledAreaOpacity 			= 4;
                MinimumFVGSize 				= 1;
                ATRPeriod 					= 10;
                ImpulseFactor 				= 1.1;
				this.Configurator.EnableTrailing				= false;
				
                LDStartTime = DateTime.Parse("03:00", System.Globalization.CultureInfo.InvariantCulture);
                LDStopTime = DateTime.Parse("04:00", System.Globalization.CultureInfo.InvariantCulture);

                AMStartTime = DateTime.Parse("10:00", System.Globalization.CultureInfo.InvariantCulture);
                AMStopTime = DateTime.Parse("11:00", System.Globalization.CultureInfo.InvariantCulture);

                PMStartTime = DateTime.Parse("14:00", System.Globalization.CultureInfo.InvariantCulture);
                PMStopTime = DateTime.Parse("15:00", System.Globalization.CultureInfo.InvariantCulture);

				RR = 2;
				goLong = true; // Para mi medio al pedo... pero bueno
				goShort = true; // idem
				

                LDBackColor = Brushes.AntiqueWhite;
                AMBackColor = Brushes.AntiqueWhite;
                PMBackColor = Brushes.AntiqueWhite;
				

                
            }
            else if (State == State.Configure)
            {
                // Helps keep track of draw object tags
                // if multiple instances are present on the same chart
                Configurator.InstanceId = Guid.NewGuid().ToString();
				
                ChartUserInterface.BrushSettings.LateDayBrush = LDBackColor.Clone();
                ChartUserInterface.BrushSettings.MorningBrush = AMBackColor.Clone();
                ChartUserInterface.BrushSettings.AfternoonBrush = PMBackColor.Clone();
                ChartUserInterface.BrushSettings.LateDayBrush.Opacity = 0.1;
                ChartUserInterface.BrushSettings.MorningBrush.Opacity = 0.1;
                ChartUserInterface.BrushSettings.AfternoonBrush.Opacity = 0.1;
                ChartUserInterface.BrushSettings.LateDayBrush.Freeze();
                ChartUserInterface.BrushSettings.MorningBrush.Freeze();
                ChartUserInterface.BrushSettings.AfternoonBrush.Freeze();
				
				
            }
            else if (State == State.DataLoaded)
            {
				if (ChartControl != null)
					CreateWPFControls();
				
				this.ChartUserInterface.ButtonPanel.LongLimitEnabled = true;
				this.ChartUserInterface.ButtonPanel.LongConditionTriggerEnabled=true;
				
                // Add ATR
                TechnicalIndicators.Atr = ATR(Close, ATRPeriod);
				
            }
			else if (State == State.Terminated)	
			{
				if (ChartControl != null)
					RemoveWPFControls();
			}
        }

		protected override void OnBarUpdate()
		{
			try
			{				
				
				// Functionality can be outside the main function
				#region Para el background
				
				DateTime thisTime = Time[0];
				
	            if (thisTime.TimeOfDay.TotalMilliseconds >= LDStartTime.TimeOfDay.TotalMilliseconds && thisTime.TimeOfDay.TotalMilliseconds <= LDStopTime.TimeOfDay.TotalMilliseconds)
	            {
	                BackBrushes[0] = ChartUserInterface.BrushSettings.LateDayBrush;
	                Configurator.IsInSession = true;
	            } 
	            else if (thisTime.TimeOfDay.TotalMilliseconds >= AMStartTime.TimeOfDay.TotalMilliseconds && thisTime.TimeOfDay.TotalMilliseconds <= AMStopTime.TimeOfDay.TotalMilliseconds)
	            {
	                BackBrushes[0] = ChartUserInterface.BrushSettings.MorningBrush;
	                Configurator.IsInSession = true;
	            } 
	            else if (thisTime.TimeOfDay.TotalMilliseconds >= PMStartTime.TimeOfDay.TotalMilliseconds && thisTime.TimeOfDay.TotalMilliseconds <= PMStopTime.TimeOfDay.TotalMilliseconds)
	            {
	                BackBrushes[0] = ChartUserInterface.BrushSettings.AfternoonBrush;
	                Configurator.IsInSession = true;
	            }
				else 
				{
					Configurator.IsInSession = false;
					CancelOrder(TradingDataContainer.LongOrder);
					CancelOrder(TradingDataContainer.ShortOrder);
				}
				#endregion

	
				if(Position.MarketPosition == MarketPosition.Flat)
				{	
		            // FVG only applies if there's been an impulse move
		            if (Configurator.IsInSession && ((UseATR && Math.Abs(High[1] - Low[1]) >= ImpulseFactor * this.TechnicalIndicators.Atr.Value[0]) || !UseATR)) //If you remove it, the code inside the if statement will not be executed when UseATR is false
		            {
		                this.TimeInfo.Future = Time[0].AddDays(1);
		
		                // Fair value gap while going UP
		                if (Low[0] > High[2] && Low[0] - High[2] >= (MinimumFVGSize * TickSize) && goLong)
		                // IDEA: Potential FVG filtering based on ATR: && (Math.Abs(Lows[iDataSeries][0] - Highs[iDataSeries][2]) >= ImpulseFactor * atr.Value[0]))
		                {
		                    //Print("Up FVG Found.");
		
		                    FairValueGap fairValueGap = new FairValueGap()
		                    {
			                    Tag = "FVG UP", 
			                    Type = FairValueGapType.Support, 
			                    LowerPrice = High[2], 
			                    UpperPrice = Low[0], 
			                    GapStartTime = Time[2]
		                    };
							
		                    //Print("Drawing Up FVG [" + fvg.gapStartTime + ", " + fvg.lowerPrice + ", " + fvg.upperPrice + "]");
		                    
							Draw.Rectangle(this, "FVG UP", false, fairValueGap.GapStartTime, fairValueGap.LowerPrice, this.TimeInfo.Future, fairValueGap.UpperPrice, UpBrush, UpAreaBrush, ActiveAreaOpacity, true);
		                   
							if (DisplayCE) 
								Draw.Line(this,"FVGU MidLVL", false, fairValueGap.GapStartTime, fairValueGap.Threshold, this.TimeInfo.Future, fairValueGap.Threshold, UpBrush, DashStyleHelper.DashDotDot, 1);
		                    
							
							double ValorTick = Instrument.MasterInstrument.PointValue * TickSize; // Esto me da el valor de 1 Tick
							double TotalTicksForSL = (fairValueGap.Threshold - Low[2])/TickSize;
							int NumOfContracts	= (int)Math.Floor(RiskA / (TotalTicksForSL * ValorTick));
							
							SetStopLoss("Enter Long", CalculationMode.Price, Low[2], false);
							SetProfitTarget("Enter Long", CalculationMode.Ticks, (TotalTicksForSL * RR));
		                    EnterLongLimit(0,true, NumOfContracts, fairValueGap.Threshold, "Enter Long");
		                }
						
		                // Fair value gap while going DOWN
		                if (High[0] < Low[2] && Low[2] - High[0] >= (MinimumFVGSize * TickSize) && goShort)
		                // IDEA: Potential FVG filtering based on ATR : && (Math.Abs(Highs[iDataSeries][0] - Lows[iDataSeries][2]) >= ImpulseFactor * atr.Value[0]))
		                {
		                    //Print("Down FVG Found.");
							
		                    FairValueGap fairValueGap = new FairValueGap()
		                    {
			                    Tag = "FVG Down", 
			                    Type = FairValueGapType.Resistance, 
			                    LowerPrice = High[0], 
			                    UpperPrice = Low[2], 
			                    GapStartTime = Time[2]
		                    };
		                    
							//Print("Drawing Down FVG [" + fvg.gapStartTime + ", " + fvg.upperPrice + ", " + fvg.lowerPrice + "]");
		                    
							Draw.Rectangle(this, "FVG Down", false, fairValueGap.GapStartTime, fairValueGap.UpperPrice, this.TimeInfo.Future, fairValueGap.LowerPrice, DownBrush, DownAreaBrush, ActiveAreaOpacity, true);
		                    
							if (DisplayCE) 
								Draw.Line(this, "FVGD MidLVL", false, fairValueGap.GapStartTime, fairValueGap.Threshold, this.TimeInfo.Future, fairValueGap.Threshold, DownBrush, DashStyleHelper.DashDotDot, 1);
							
							double ValorTick = Instrument.MasterInstrument.PointValue * TickSize; // Esto me da el valor de 1 Tick
							double TotalTicksForSL = (High[2] - fairValueGap.Threshold)/TickSize;
							int NumOfContracts	= (int)Math.Floor(RiskA / (TotalTicksForSL * ValorTick));
							
							SetStopLoss("Enter Short", CalculationMode.Price, High[2], false);
							SetProfitTarget("Enter Short", CalculationMode.Ticks, (TotalTicksForSL * RR));
	                        EnterShortLimit(0, true, NumOfContracts, fairValueGap.Threshold, "Enter Short"); // Puede saltar error con el "enytrOrder" siendo asigando NO en Position Update
		                }
		            }
				}
				else 
				{
					if(Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0]) >= RiskA && Configurator.MoveStopLoss) // Si hicimos 1:1 ==> mover SL 
					{
						Configurator.MoveStopLoss = false;
						
						if(Position.MarketPosition == MarketPosition.Long)
						{
							SetStopLoss("Enter Long", CalculationMode.Price, TradingDataContainer.EntryPrice, false);
						}
						
						if(Position.MarketPosition == MarketPosition.Short)
						{
							SetStopLoss("Enter Short", CalculationMode.Price, TradingDataContainer.EntryPrice, false);
						}
						
						//Print("Movemos el SL a " + PrecioEntrada);
					}
				}
			}
			catch(Exception e){
				Print(e);
			}
			
        }

        protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
        {
			Configurator.MoveStopLoss = true;
		}

		
        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
        {
		  // Assign entryOrder in OnOrderUpdate() to ensure the assignment occurs when expected.
		  // This is more reliable than assigning Order objects in OnBarUpdate, as the assignment is not gauranteed to be complete if it is referenced immediately after submitting
		  if (order.Name == "Enter Long")
		      TradingDataContainer.LongOrder = order;
		  
		  if(order.Name == "Enter Short")
			  TradingDataContainer.ShortOrder = order;
		  
		 
		  // Evaluates for all updates to myEntryOrder.
		  if (TradingDataContainer.LongOrder != null && TradingDataContainer.LongOrder == order)
		  {
		      // Check if myEntryOrder is cancelled.
		      if (TradingDataContainer.LongOrder.OrderState == OrderState.Cancelled)
		      {
		          TradingDataContainer.LongOrder = null;
		      }
		  } 
		  else if (TradingDataContainer.ShortOrder != null && TradingDataContainer.ShortOrder == order)
		  {
		      // Check if myEntryOrder is cancelled.
		      if (TradingDataContainer.ShortOrder.OrderState == OrderState.Cancelled)
		      {
		          TradingDataContainer.ShortOrder = null;
		      }		  	
		  }	
        }



        protected override void OnPositionUpdate(Cbi.Position position, double averagePrice, int quantity, Cbi.MarketPosition marketPosition)
        {
			if(Position.MarketPosition == MarketPosition.Long) 
			{
				CancelOrder(TradingDataContainer.ShortOrder);
				RemoveDrawObject("FVG UP");
			}
			
			if(Position.MarketPosition == MarketPosition.Short)
			{
				CancelOrder(TradingDataContainer.LongOrder);
				RemoveDrawObject("FVG Down");
			}
			
			TradingDataContainer.EntryPrice = averagePrice;
			
        }

		
		#region Botones
		
		private void CreateWPFControls()
		{
			// if the script has already added the controls, do not add a second time.
			if (UserControlCollection.Contains(this.ChartUserInterface.ButtonPanel.ButtonGrid))
				return;
			
			// when making WPF changes to the UI, run the code on the UI thread of the chart
			ChartControl.Dispatcher.InvokeAsync((() =>
			{
				// this buttonGrid will contain the buttons
				this.ChartUserInterface.ButtonPanel.ButtonGrid = new System.Windows.Controls.Grid
				{
					Background	= Brushes.Red,
					Name				= "ButtonsGrid",
					HorizontalAlignment	= HorizontalAlignment.Right,
					VerticalAlignment = VerticalAlignment.Top
				};

				// add 3 columns to the grid
				for (int i = 0; i < 3; i++)
					this.ChartUserInterface.ButtonPanel.ButtonGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());

				#region Use case 1, immediately submitting a market order

				this.ChartUserInterface.ButtonPanel.LongMarketButton = new System.Windows.Controls.Button
				{
					Name		= "LongMarketButton",
					Content		= "Long Enabled",
					Foreground	= Brushes.White,
					Background	= Brushes.LightGreen
				};

				var longMarketButton = this.ChartUserInterface.ButtonPanel.LongMarketButton; 
				longMarketButton.Click += OnButtonClick;				
				this.ChartUserInterface.ButtonPanel.ButtonGrid.Children.Add(longMarketButton);
				System.Windows.Controls.Grid.SetColumn(longMarketButton, 0);
				#endregion

				#region Use case 2, submitting a limit order with limit price updated to indicator plot value

				var longLimitButton = this.ChartUserInterface.ButtonPanel.LongLimitButton; 
				longLimitButton = new System.Windows.Controls.Button
				{
					Name		= "LongLimitButton",
					Content		= "Short Enabled",
					Foreground	= Brushes.White,
					Background	= Brushes.Red
				};
				
				longLimitButton.Click += OnButtonClick;
				this.ChartUserInterface.ButtonPanel.ButtonGrid.Children.Add(longLimitButton);
				System.Windows.Controls.Grid.SetColumn(longLimitButton, 1);
				#endregion

				#region Use case 3, setting a trigger to submit a market order after conditions are met

				var longConditionalButton = this.ChartUserInterface.ButtonPanel.LongConditionalButton;
				longConditionalButton = new System.Windows.Controls.Button
				{
					Name		= "LongConditionalButton",
					Content		= "Close All",
					Foreground	= Brushes.White,
					Background	= Brushes.Magenta
				};

				longConditionalButton.Click += OnButtonClick;
				this.ChartUserInterface.ButtonPanel.ButtonGrid.Children.Add(longConditionalButton);
				System.Windows.Controls.Grid.SetColumn(longConditionalButton, 2);
				#endregion

				// add our button grid to the main UserControlCollection over the chart
				UserControlCollection.Add(this.ChartUserInterface.ButtonPanel.ButtonGrid);
			}));
		}


		private void OnButtonClick(object sender, RoutedEventArgs rea)
		{
			System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;

			#region Use case 1, immediately submitting a market order
			
			if (button == this.ChartUserInterface.ButtonPanel.LongMarketButton)
			{
				var longConditionTriggerEnabled = this.ChartUserInterface.ButtonPanel.LongConditionTriggerEnabled;
				longConditionTriggerEnabled= !longConditionTriggerEnabled;
				goLong = longConditionTriggerEnabled;
				
						// change the button text so we know the order is submitted
				ChartControl.Dispatcher.InvokeAsync((() =>
				{
					button.Content		= longConditionTriggerEnabled ? "Long Enabled" : "Long Disabled";
					button.Background	= longConditionTriggerEnabled ? Brushes.LightGreen : Brushes.Gray;
					button.Foreground	= longConditionTriggerEnabled ? Brushes.Black : Brushes.White;
				}));
				ForceRefresh();
				
			}
			#endregion

			#region Use case 2, submitting a limit order with limit price updated to indicator plot value
			
			if (button == this.ChartUserInterface.ButtonPanel.LongLimitButton)
			{
				var longLimitEnabled = this.ChartUserInterface.ButtonPanel.LongLimitEnabled;
				longLimitEnabled = !longLimitEnabled;
				goShort = longLimitEnabled;
				
				// change the button text so we know the order is submitted
				ChartControl.Dispatcher.InvokeAsync((() =>
				{
					button.Content		= longLimitEnabled ? "Short Enabled" : "Short Disabled";
					button.Background	= longLimitEnabled ? Brushes.Red : Brushes.Gray;
					button.Foreground	= longLimitEnabled ? Brushes.Black : Brushes.White;
				}));
				ForceRefresh();
			}
			#endregion

			#region Use case 3, setting a trigger to submit a market order after conditions are met
			
			if (button == this.ChartUserInterface.ButtonPanel.LongConditionalButton)
			{
				ExitLong();
				ExitShort();
				
			}
			#endregion
		}

		
		
		private void RemoveWPFControls()
		{
			// when disabling the script, remove the button click handler methods from the click events
			// set the buttons to null so the garbage collector knows to clean them up and free memory
			ChartControl.Dispatcher.InvokeAsync((() =>
			{
				if (this.ChartUserInterface.ButtonPanel.ButtonGrid != null)
				{
					if (this.ChartUserInterface.ButtonPanel.LongLimitButton != null)
					{
						this.ChartUserInterface.ButtonPanel.LongLimitButton.Click -= OnButtonClick;
						this.ChartUserInterface.ButtonPanel.LongLimitButton = null;
					}
					if (this.ChartUserInterface.ButtonPanel.LongConditionalButton != null)
					{
						this.ChartUserInterface.ButtonPanel.LongConditionalButton.Click -= OnButtonClick;
						this.ChartUserInterface.ButtonPanel.LongConditionalButton = null;
					}
					if (this.ChartUserInterface.ButtonPanel.LongMarketButton != null)
					{
						this.ChartUserInterface.ButtonPanel.LongMarketButton.Click -= OnButtonClick;
						this.ChartUserInterface.ButtonPanel.LongMarketButton = null;
					}

					UserControlCollection.Remove(this.ChartUserInterface.ButtonPanel.ButtonGrid);
				}
			}));
		}
		
		#endregion
		
		

        #region Properties
		
		#region Dire
		
		[NinjaScriptProperty]
        [Display(Name = "Long", Order = 1, GroupName = "Direction")]
        public bool goLong
        { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Short", Order = 2, GroupName = "Direction")]
        public bool goShort
        { get; set; }
		
		#endregion

		#region ATR
		
        [NinjaScriptProperty]
        [Display(Name = "Use ATR", Description = "If enabled, ATR settings will be used to filter FVGs.", Order = 10, GroupName = "ATR")]
        public bool UseATR
        { get; set; }
		
        [NinjaScriptProperty]
        [Range(3, int.MaxValue)]
        [Display(Name = "ATR Period", Description = "The ATR period on which to calculate its value (usually 14) ", Order = 40, GroupName = "ATR")]
        public int ATRPeriod
        { get; set; }
		
        [NinjaScriptProperty]
        [Range(0.1, double.MaxValue)]
        [Display(Name = "ATR Min Level", Description = "The ATR minimum level to know if there is enough volatility ", Order = 50, GroupName = "ATR")]
        public double ImpulseFactor
        { get; set; }
		
		#endregion
		
		#region Trade

        [NinjaScriptProperty]
        [Display(Name = "Risk Amount ($)", Description = "Amount of ticks ", Order = 20, GroupName = "Trade")]
        public int RiskA
        { get; set; }
		
        [NinjaScriptProperty]
        [Display(Name = "R:R", Description = "Risk to Reward target for TP", Order = 30, GroupName = "Trade")]
        public int RR
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "Minimum FVG Size (Ticks)", Order = 60, GroupName = "Trade")]
        public int MinimumFVGSize
        { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Show Mean Threshold", Order = 80, GroupName = "Trade")]
        public bool DisplayCE
        { get; set; }
		

		#endregion
		
		#region FVG Color
        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bearish FVG (Border) Color", Order = 100, GroupName = "FVG Colors")]
        public Brush DownBrush
        { get; set; }

        [Browsable(false)]
        public string DownBrushSerializable
        {
            get { return Serialize.BrushToString(DownBrush); }
            set { DownBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bearish FVG (Area) Color", Order = 110, GroupName = "FVG Colors")]
        public Brush DownAreaBrush
        { get; set; }

        [Browsable(false)]
        public string DownBrushAreaSerializable
        {
            get { return Serialize.BrushToString(DownAreaBrush); }
            set { DownAreaBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bullish FVG (Border) Color", Order = 200, GroupName = "FVG Colors")]
        public Brush UpBrush
        { get; set; }

        [Browsable(false)]
        public string UpBrushSerializable
        {
            get { return Serialize.BrushToString(UpBrush); }
            set { UpBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bullish FVG (Area) Color", Order = 210, GroupName = "FVG Colors")]
        public Brush UpAreaBrush
        { get; set; }

        [Browsable(false)]
        public string UpAreaBrushSerializable
        {
            get { return Serialize.BrushToString(UpAreaBrush); }
            set { UpAreaBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Range(1, 100)]
        [Display(Name = "Active Gap Opacity", Order = 300, GroupName = "FVG Colors")]
        public int ActiveAreaOpacity
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, 100)]
        [Display(Name = "Filled Gap Opacity", Order = 400, GroupName = "FVG Colors")]
        public int FilledAreaOpacity
        { get; set; }

		#endregion
		
		#region LD Session
		
        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start", Description = "", Order = 1, GroupName = "LD SESSION SIVER BULLET (EST TIME)")]
        public DateTime LDStartTime
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "End", Description = "", Order = 2, GroupName = "LD SESSION SIVER BULLET (EST TIME)")]
        public DateTime LDStopTime
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Back Color", Description = "", Order = 3, GroupName = "LD SESSION SIVER BULLET (EST TIME)")]
        public Brush LDBackColor
        { get; set; }

        [Browsable(false)]
        public string LDBackColorBrushSerializable
        {
            get { return Serialize.BrushToString(LDBackColor); }
            set { LDBackColor = Serialize.StringToBrush(value); }
        }
		
		#endregion
		
		#region AM session
		
        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start", Description = "", Order = 1, GroupName = "AM SESSION SIVER BULLET (EST TIME)")]
        public DateTime AMStartTime
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "End", Description = "", Order = 2, GroupName = "AM SESSION SIVER BULLET (EST TIME)")]
        public DateTime AMStopTime
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Back Color", Description = "", Order = 3, GroupName = "AM SESSION SIVER BULLET (EST TIME)")]
        public Brush AMBackColor
        { get; set; }

		
        [Browsable(false)]
        public string AMBackColorBrushSerializable
        {
            get { return Serialize.BrushToString(AMBackColor); }
            set { AMBackColor = Serialize.StringToBrush(value); }
        }
		
		#endregion
		
		#region PM Session
		
        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start", Description = "", Order = 1, GroupName = "PM SESSION SIVER BULLET (EST TIME)")]
        public DateTime PMStartTime
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "End", Description = "", Order = 2, GroupName = "PM SESSION SIVER BULLET (EST TIME)")]
        public DateTime PMStopTime
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Back Color", Description = "", Order = 3, GroupName = "PM SESSION SIVER BULLET (EST TIME)")]
        public Brush PMBackColor
        { get; set; }
		
        [Browsable(false)]
        public string PMBackColorBrushSerializable
        {
            get { return Serialize.BrushToString(PMBackColor); }
            set { PMBackColor = Serialize.StringToBrush(value); }
        }
		#endregion

        #endregion
    }
}
