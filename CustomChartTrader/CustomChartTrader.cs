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
using System.Text.RegularExpressions;
#endregion

namespace NinjaTrader.NinjaScript.Strategies
{
	public class CustomChartTrader : Strategy
	{
        public override string DisplayName { get { return State == State.SetDefaults ? "CustomChartTrader" : Name; } }
		
        System.Windows.Controls.Grid menuGrid;
        System.Windows.Controls.Grid mktGrid;
        System.Windows.Controls.Grid exitquantGrid;
        System.Windows.Controls.Grid askGrid;
        System.Windows.Controls.Grid bidGrid;
        System.Windows.Controls.Grid revcloseGrid;
        System.Windows.Controls.Grid posentryGrid;
        System.Windows.Controls.Grid tpslGrid;
		System.Windows.Controls.Button BuyMktButton;
        System.Windows.Controls.Button SellMktButton;
        System.Windows.Controls.Button SellAskButton;
        System.Windows.Controls.Button BuyAskButton;
        System.Windows.Controls.Button SellBidButton;
        System.Windows.Controls.Button BuyBidButton;
        System.Windows.Controls.Button RevButton;
        System.Windows.Controls.Button CloseButton;
        System.Windows.Controls.Button PnlLabel;
        System.Windows.Controls.Button PosSizeLabel;
        System.Windows.Controls.Button EntryLabel;
        System.Windows.Controls.Button BidAskLabel;
        System.Windows.Controls.Button RatioLabel;
        System.Windows.Controls.Button TpLabel;
        System.Windows.Controls.Button SlLabel;
		//System.Windows.Controls.Label PnlLabel;
        QuantityUpDown QuantQUD;

        bool IsToolBarButtonAdded;
        int width = 400;
        int FontSize = 24;
		
		#region Properties		
		private int Quantity;	//DZ		
		
//DZ - add LeftMargin, TopMargin, ButtonPanelWidth, ButtonHeight and Fontsize parameters so users can customize the look and position of the button panel
		[NinjaScriptProperty]
        [Range(0, 5000)]
        [Display(Name = "Distance: Buttons to Chart's Left Edge", Order = 6, GroupName = "Parameters")]
        public int LeftMargin
        { get; set; }
		
		[NinjaScriptProperty]
        [Range(0, 5000)]
        [Display(Name = "Distance: Buttons to Chart's Top Edge", Order = 7, GroupName = "Parameters")]
        public int TopMargin
        { get; set; }
		
		[NinjaScriptProperty]
        [Range(100, 5000)]
        [Display(Name = "Button Panel Width", Order = 8, GroupName = "Parameters")]
        public int ButtonPanelWidth
        { get; set; }

		[NinjaScriptProperty]
        [Range(0, 5000)]
        [Display(Name = "Button Panel Height", Order = 9, GroupName = "Parameters")]
        public int ButtonHeight
        { get; set; }
				
		[NinjaScriptProperty]
        [Range(0, 100)]
        [Display(Name = "Button Panel Font Size", Order = 10, GroupName = "Parameters")]
        public int ButtonPanelFontSize
        { get; set; }
		
		private void MakeDefaults()
		{
			Quantity = 1;
			
			LeftMargin = 0;		//DZ - added default
			TopMargin = 0;	//DZ - added default
			ButtonPanelWidth = 300;		//DZ - added default
			ButtonHeight = 30;
			ButtonPanelFontSize = 15;	//DZ - added default			
		}
		#endregion
		
        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
        //protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time)
        {
			UpdateInfoLabelByOrderUpdate();
			
			MarketPosition pos = Position.MarketPosition;
			
			if (pos != MarketPosition.Flat)
			{
				double posPrice = Position.AveragePrice;
				if (orderState == OrderState.Submitted)
				{
					if (order.OrderType == OrderType.Limit)
					{
						double maxProfit = 0;
						if (order.OrderAction == OrderAction.Sell && pos == MarketPosition.Long)
						{
							maxProfit = (order.LimitPrice - posPrice) * order.Quantity * Instrument.MasterInstrument.PointValue;
						}
						else if (order.OrderAction == OrderAction.Buy && pos == MarketPosition.Short)
						{
							maxProfit = (posPrice - order.LimitPrice ) * order.Quantity * Instrument.MasterInstrument.PointValue;
						}
					}
				}
			}
        }
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name										= "CustomChartTrader";
				Calculate									= Calculate.OnEachTick;
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
				IsInstantiatedOnEachOptimizationIteration	= true;
				IsUnmanaged = true;
				
				MakeDefaults();
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{
			}
			else if (State == State.Historical)
			{
				if (!IsToolBarButtonAdded)
                {
                    AddToolBar();
                }
			}
			else if (State == State.Terminated)
			{
                DisposeCleanUp();
			}
		}

		protected override void OnBarUpdate()
		{
		    // Ensure the strategy runs in real-time or historical data
		    if (State == State.Historical)
		        return;
			
		    // Check if the ChartControl is available before updating the PnL label
		    if (ChartControl != null)
		    {
		        TriggerCustomEvent(x => UpdateInfoLabelByBarUpdate(), null);
		    }			
		}
		
		#region Trade Methods
		private void PlaceMarketOrder(bool isLong, string strType)
		{
			OrderType ot = strType == "mkt" ? OrderType.Market : OrderType.Limit;
			OrderAction oa = isLong ? OrderAction.Buy : OrderAction.Sell;
			double limitPrice = strType == "bid" ? GetCurrentBid() : (strType == "ask" ? GetCurrentAsk() : 0);
			
			SubmitOrderUnmanaged(0, oa, OrderType.Market, Quantity, limitPrice, 0, "", (isLong ? "Long " : "Short ") + " Entry");
		}
		
        private void ClosePositions()
        {
            Print("ClosePositions: start");
		
            // Flatten all positions manually
            if (Position.MarketPosition == MarketPosition.Long)
            {
                SubmitOrderUnmanaged(0, OrderAction.SellShort, OrderType.Market, Position.Quantity, 0, 0, "", "Close Long Position");
            }
            else if (Position.MarketPosition == MarketPosition.Short)
            {
                SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.Market, Position.Quantity, 0, 0, "", "Close Short Position");
            }

            // Close all open orders
            // foreach (Order order in ActiveOrders)
            foreach (Order order in Orders)
            {
				if (order.Name == "Close Long Position" || order.Name == "Close Short Position")
					continue;
				
                CancelOrder(order);
            }
			
            // Print a message to the output window
            Print("ClosePositions: All trades closed.");
        }
		
        private void RevPositions()
        {
            Print("RevPositions: start");
			int qty = Position.Quantity;
			MarketPosition pos = Position.MarketPosition;
			
            // Flatten all positions manually
            if (pos == MarketPosition.Long)
            {
                SubmitOrderUnmanaged(0, OrderAction.SellShort, OrderType.Market, qty * 2, 0, 0, "", "Close Long Position");
            }
            else if (pos == MarketPosition.Short)
            {
                SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.Market, qty * 2, 0, 0, "", "Close Short Position");
            }

            // Close all open orders
            // foreach (Order order in ActiveOrders)
            foreach (Order order in Orders)
            {
				if (order.Name == "Close Long Position" || order.Name == "Close Short Position")
					continue;
				
                CancelOrder(order);
            }
			
			
            // Print a message to the output window
            Print("RevPositions: All trades closed.");
        }
		#endregion
		
		#region Toolbar
		
		#region AddToolbar

        private void AddToolBar()
        {
            // Use this.Dispatcher to ensure code is executed on the proper thread
			width = ButtonPanelWidth;		//DZ
			FontSize = ButtonPanelFontSize;	//DZ

            if (ChartControl != null)
            {
                ChartControl.Dispatcher.InvokeAsync((Action)(() =>
                {
                    // Grid already exists
                    if (UserControlCollection.Contains(menuGrid))
                        return;

                    AddControls();

                    UserControlCollection.Add(menuGrid);
                }));
            }
        }
        #endregion

        #region CleanToolBar
        private void DisposeCleanUp()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (menuGrid != null)
                {
                    UserControlCollection.Remove(menuGrid);
                }
            }));
        }
        #endregion

        #region AddControlsToToolbar
        private void AddControls()
        {
            menuGrid = new System.Windows.Controls.Grid
            {
                Name = "MenuGrid",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };
			
			mktGrid = new System.Windows.Controls.Grid();
			exitquantGrid = new System.Windows.Controls.Grid();
			askGrid = new System.Windows.Controls.Grid();
			bidGrid = new System.Windows.Controls.Grid();
			revcloseGrid = new System.Windows.Controls.Grid();
			posentryGrid = new System.Windows.Controls.Grid();
			tpslGrid = new System.Windows.Controls.Grid();

            InitColumns();
            AddRows();

        }

        #endregion

        #region Rows

        private void InitColumns()
        {
            System.Windows.Controls.ColumnDefinition column1 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(LeftMargin) };
            System.Windows.Controls.ColumnDefinition column2 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width) };

            System.Windows.Controls.RowDefinition row1 = new System.Windows.Controls.RowDefinition() { Height = new GridLength(TopMargin) };
            System.Windows.Controls.RowDefinition row2 = new System.Windows.Controls.RowDefinition() { Height = new GridLength(ButtonHeight) };
            System.Windows.Controls.RowDefinition row3 = new System.Windows.Controls.RowDefinition() { Height = new GridLength(ButtonHeight) };
            System.Windows.Controls.RowDefinition row4 = new System.Windows.Controls.RowDefinition() { Height = new GridLength(ButtonHeight) };
            System.Windows.Controls.RowDefinition row5 = new System.Windows.Controls.RowDefinition() { Height = new GridLength(ButtonHeight) };
            System.Windows.Controls.RowDefinition row6 = new System.Windows.Controls.RowDefinition() { Height = new GridLength(ButtonHeight) };
            System.Windows.Controls.RowDefinition row7 = new System.Windows.Controls.RowDefinition() { Height = new GridLength(ButtonHeight) };
            System.Windows.Controls.RowDefinition row8 = new System.Windows.Controls.RowDefinition() { Height = new GridLength(ButtonHeight) };

            // Add the columns to the Grid
            menuGrid.ColumnDefinitions.Add(column1);
            menuGrid.ColumnDefinitions.Add(column2);
            menuGrid.RowDefinitions.Add(row1);
            menuGrid.RowDefinitions.Add(row2);
            menuGrid.RowDefinitions.Add(row3);
            menuGrid.RowDefinitions.Add(row4);
            menuGrid.RowDefinitions.Add(row5);
            menuGrid.RowDefinitions.Add(row6);
            menuGrid.RowDefinitions.Add(row7);
            menuGrid.RowDefinitions.Add(row8);

			System.Windows.Controls.ColumnDefinition row2_column1 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0.5) };
			System.Windows.Controls.ColumnDefinition row2_column2 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0) };
			System.Windows.Controls.ColumnDefinition row2_column3 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0.5) };
			
			mktGrid.ColumnDefinitions.Add(row2_column1);
			mktGrid.ColumnDefinitions.Add(row2_column2);
			mktGrid.ColumnDefinitions.Add(row2_column3);
			
			System.Windows.Controls.ColumnDefinition row3_column1 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0.5) };
			System.Windows.Controls.ColumnDefinition row3_column2 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0) };
			System.Windows.Controls.ColumnDefinition row3_column3 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0.5) };
			
			askGrid.ColumnDefinitions.Add(row3_column1);
			askGrid.ColumnDefinitions.Add(row3_column2);
			askGrid.ColumnDefinitions.Add(row3_column3);
			
			System.Windows.Controls.ColumnDefinition row4_column1 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0.5) };
			System.Windows.Controls.ColumnDefinition row4_column2 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0) };
			System.Windows.Controls.ColumnDefinition row4_column3 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0.5) };
			
			bidGrid.ColumnDefinitions.Add(row4_column1);
			bidGrid.ColumnDefinitions.Add(row4_column2);
			bidGrid.ColumnDefinitions.Add(row4_column3);
			
			System.Windows.Controls.ColumnDefinition row5_column1 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0.5) };
			System.Windows.Controls.ColumnDefinition row5_column2 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0) };
			System.Windows.Controls.ColumnDefinition row5_column3 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0.5) };
			
			revcloseGrid.ColumnDefinitions.Add(row5_column1);
			revcloseGrid.ColumnDefinitions.Add(row5_column2);
			revcloseGrid.ColumnDefinitions.Add(row5_column3);
			
			System.Windows.Controls.ColumnDefinition row6_column1 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0.5) };
			System.Windows.Controls.ColumnDefinition row6_column2 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0) };
			System.Windows.Controls.ColumnDefinition row6_column3 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0.5) };

			exitquantGrid.ColumnDefinitions.Add(row6_column1);
			exitquantGrid.ColumnDefinitions.Add(row6_column2);
			exitquantGrid.ColumnDefinitions.Add(row6_column3);
			
			System.Windows.Controls.ColumnDefinition row7_column1 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0.5) };
			System.Windows.Controls.ColumnDefinition row7_column2 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0) };
			System.Windows.Controls.ColumnDefinition row7_column3 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0.5) };

			posentryGrid.ColumnDefinitions.Add(row7_column1);
			posentryGrid.ColumnDefinitions.Add(row7_column2);
			posentryGrid.ColumnDefinitions.Add(row7_column3);
			
			System.Windows.Controls.ColumnDefinition row8_column1 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0.5) };
			System.Windows.Controls.ColumnDefinition row8_column2 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0) };
			System.Windows.Controls.ColumnDefinition row8_column3 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0.5) };

			tpslGrid.ColumnDefinitions.Add(row8_column1);
			tpslGrid.ColumnDefinitions.Add(row8_column2);
			tpslGrid.ColumnDefinitions.Add(row8_column3);
        }

        private void AddRows()
        {
			BuyMktButton = new System.Windows.Controls.Button()
            {
                Name = "BuyMktButton",
                // Content = BuyEntryMode == EntryModeEnum.None ? "BUY" : BuyEntryMode == EntryModeEnum.Current ? "BUY CURR" : "BUY PREV",		//DZ - shortened labels
                Content = "BUY Mkt",
                Foreground = Brushes.White,
                // Background = BuyEntryMode == EntryModeEnum.None ? Brushes.Gray : Brushes.Blue,		//DZ - changed color
				Background = Brushes.Gray,
                FontWeight = FontWeights.Bold,
                FontSize = FontSize,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinWidth = 0,
            };

            SellMktButton = new System.Windows.Controls.Button()
            {
                Name = "SellMktButton",
                // Content = SellEntryMode == EntryModeEnum.None ? "SELL" : SellEntryMode == EntryModeEnum.Current ? "SELL CURR" : "SELL PREV",	//DZ - shortened labels
				Content = "SELL Mkt",
                Foreground = Brushes.White,
                // Background = SellEntryMode == EntryModeEnum.None ? Brushes.Gray : Brushes.Red,
				Background = Brushes.Gray,
                FontWeight = FontWeights.Bold,
                FontSize = FontSize,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinWidth = 0,
            };

			BuyAskButton = new System.Windows.Controls.Button()
            {
                Name = "BuyAskButton",
                Content = "Buy Ask",
                Foreground = Brushes.White,
                Background = Brushes.Gray,
                FontWeight = FontWeights.Bold,
                FontSize = FontSize,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinWidth = 0,
            };

            SellAskButton = new System.Windows.Controls.Button()
            {
                Name = "SellAskButton",
                Content = "Sell Ask",
                Foreground = Brushes.White,
                Background = Brushes.Gray,
                FontWeight = FontWeights.Bold,
                FontSize = FontSize,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinWidth = 0,
            };
			
			BuyBidButton = new System.Windows.Controls.Button()
            {
                Name = "BuyAskButton",
                Content = "Buy Bid",
                Foreground = Brushes.White,
                Background = Brushes.Gray,
                FontWeight = FontWeights.Bold,
                FontSize = FontSize,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinWidth = 0,
            };

            SellBidButton = new System.Windows.Controls.Button()
            {
                Name = "SellBidButton",
                Content = "Sell Bid",
                Foreground = Brushes.White,
                Background = Brushes.Gray,
                FontWeight = FontWeights.Bold,
                FontSize = FontSize,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinWidth = 0,
            };
			
            RevButton = new System.Windows.Controls.Button()
            {
                Name = "RevButton",
                Content = "Rev",
                Foreground = Brushes.White,
                Background = Brushes.Gray,
                FontWeight = FontWeights.Bold,
//                Width = width * 0.5,
                FontSize = FontSize,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinWidth = 0,
            };

            CloseButton = new System.Windows.Controls.Button()
            {
                Name = "CloseButton",
                Content = "Close",
                Foreground = Brushes.White,
                Background = Brushes.Gray,
                FontWeight = FontWeights.Bold,
//                Width = width * 0.5,
                FontSize = FontSize,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinWidth = 0,
            };

            PnlLabel = new System.Windows.Controls.Button()
            {
                Content = "PnL",
                Foreground = Brushes.White,
                Background = Brushes.Gray,
                FontWeight = FontWeights.Bold,
//                Width = width * 0.5,
                FontSize = FontSize,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinWidth = 0,
            };
            
            PosSizeLabel = new System.Windows.Controls.Button()
            {
                Content = "Flat",		//DZ - shortened labels
                Foreground = Brushes.White,
                Background = Brushes.Gray,
                FontWeight = FontWeights.Bold,
                FontSize = FontSize,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinWidth = 0,
            };
			
			EntryLabel = new System.Windows.Controls.Button()
            {
                Content = "Entry",		//DZ - shortened labels
                Foreground = Brushes.White,
                Background = Brushes.Gray,
                FontWeight = FontWeights.Bold,
                FontSize = FontSize,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinWidth = 0,
            };

            QuantQUD = new QuantityUpDown()
            {
                Name = "QuantQUD",
                Minimum = 1,
                Value = Quantity,
                VerticalAlignment = VerticalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
            };

            TpLabel = new System.Windows.Controls.Button()
            {
                Content = "TP",		//DZ - shortened labels
                Foreground = Brushes.White,
                Background = Brushes.Green,
                FontWeight = FontWeights.Bold,
                FontSize = FontSize,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinWidth = 0,
            };
			
			SlLabel = new System.Windows.Controls.Button()
            {
                Content = "SL",		//DZ - shortened labels
                Foreground = Brushes.White,
                Background = Brushes.Red,
                FontWeight = FontWeights.Bold,
                FontSize = FontSize,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinWidth = 0,
            };

            System.Windows.Controls.Grid.SetColumn(BuyMktButton, 0);
            System.Windows.Controls.Grid.SetRow(BuyMktButton, 0);
			
			System.Windows.Controls.Grid.SetColumn(SellMktButton, 2);
            System.Windows.Controls.Grid.SetRow(SellMktButton, 0);

            System.Windows.Controls.Grid.SetColumn(SellAskButton, 2);
            System.Windows.Controls.Grid.SetRow(SellAskButton, 0);
			
			System.Windows.Controls.Grid.SetColumn(BuyAskButton, 0);
            System.Windows.Controls.Grid.SetRow(BuyAskButton, 0);
			
            System.Windows.Controls.Grid.SetColumn(SellBidButton, 2);
            System.Windows.Controls.Grid.SetRow(SellBidButton, 0);
			
			System.Windows.Controls.Grid.SetColumn(BuyBidButton, 0);
            System.Windows.Controls.Grid.SetRow(BuyBidButton, 0);
			
            System.Windows.Controls.Grid.SetColumn(RevButton, 0);
            System.Windows.Controls.Grid.SetRow(RevButton, 0);
			
            System.Windows.Controls.Grid.SetColumn(CloseButton, 2);
            System.Windows.Controls.Grid.SetRow(CloseButton, 0);

            System.Windows.Controls.Grid.SetColumn(PosSizeLabel, 0);
            System.Windows.Controls.Grid.SetRow(PosSizeLabel, 0);
			
            System.Windows.Controls.Grid.SetColumn(EntryLabel, 2);
            System.Windows.Controls.Grid.SetRow(EntryLabel, 0);

            System.Windows.Controls.Grid.SetColumn(PnlLabel, 2);
            System.Windows.Controls.Grid.SetRow(PnlLabel, 0);

            System.Windows.Controls.Grid.SetColumn(QuantQUD, 0);
            System.Windows.Controls.Grid.SetRow(QuantQUD, 0);
			
            System.Windows.Controls.Grid.SetColumn(TpLabel, 0);
            System.Windows.Controls.Grid.SetRow(TpLabel, 0);
			
            System.Windows.Controls.Grid.SetColumn(SlLabel, 2);
            System.Windows.Controls.Grid.SetRow(SlLabel, 0);
		
			mktGrid.Children.Add(BuyMktButton);
			mktGrid.Children.Add(SellMktButton);
			System.Windows.Controls.Grid.SetRow(mktGrid, 1);
			System.Windows.Controls.Grid.SetColumn(mktGrid, 1);
            menuGrid.Children.Add(mktGrid);
			
			askGrid.Children.Add(SellAskButton);
			askGrid.Children.Add(BuyAskButton);
			System.Windows.Controls.Grid.SetRow(askGrid, 2);
			System.Windows.Controls.Grid.SetColumn(askGrid, 1);
            menuGrid.Children.Add(askGrid);
			
			bidGrid.Children.Add(SellBidButton);
			bidGrid.Children.Add(BuyBidButton);
			System.Windows.Controls.Grid.SetRow(bidGrid, 3);
			System.Windows.Controls.Grid.SetColumn(bidGrid, 1);
            menuGrid.Children.Add(bidGrid);
			
			revcloseGrid.Children.Add(RevButton);
			revcloseGrid.Children.Add(CloseButton);
			System.Windows.Controls.Grid.SetRow(revcloseGrid, 4);
			System.Windows.Controls.Grid.SetColumn(revcloseGrid, 1);
            menuGrid.Children.Add(revcloseGrid);
			
			exitquantGrid.Children.Add(QuantQUD);
			exitquantGrid.Children.Add(PnlLabel);
			System.Windows.Controls.Grid.SetRow(exitquantGrid, 5);
			System.Windows.Controls.Grid.SetColumn(exitquantGrid, 1);
            menuGrid.Children.Add(exitquantGrid);
			
			posentryGrid.Children.Add(PosSizeLabel);
			posentryGrid.Children.Add(EntryLabel);
			System.Windows.Controls.Grid.SetRow(posentryGrid, 6);
			System.Windows.Controls.Grid.SetColumn(posentryGrid, 1);
            menuGrid.Children.Add(posentryGrid);
			
			tpslGrid.Children.Add(TpLabel);
			tpslGrid.Children.Add(SlLabel);
			System.Windows.Controls.Grid.SetRow(tpslGrid, 7);
			System.Windows.Controls.Grid.SetColumn(tpslGrid, 1);
            menuGrid.Children.Add(tpslGrid);
			
            BuyMktButton.Click += BuyMktButtonClick;
			SellMktButton.Click += SellMktButtonClick;
            CloseButton.Click += CloseButtonClick;
            SellAskButton.Click += SellAskButtonClick;
            BuyAskButton.Click += BuyAskButtonClick;
            SellBidButton.Click += SellBidButtonClick;
            BuyBidButton.Click += BuyBidButtonClick;
            RevButton.Click += RevButtonClick;
            QuantQUD.ValueChanged += QuantQUDValueChanged;
        }
        #endregion

        void BuyMktButtonClick(object sender, EventArgs e)
        {
			TriggerCustomEvent(x => PlaceMarketOrder(true, "mkt"), null);
        }

        void SellMktButtonClick(object sender, EventArgs e)
        {
			TriggerCustomEvent(x => PlaceMarketOrder(false, "mkt"), null);
        }
		
		void SellAskButtonClick(object sender, EventArgs e)
        {
			TriggerCustomEvent(x => PlaceMarketOrder(false, "ask"), null);
        }
		
		void BuyAskButtonClick(object sender, EventArgs e)
        {
			TriggerCustomEvent(x => PlaceMarketOrder(true, "ask"), null);
        }
		
		void SellBidButtonClick(object sender, EventArgs e)
        {
			TriggerCustomEvent(x => PlaceMarketOrder(false, "bid"), null);
        }
		
		void BuyBidButtonClick(object sender, EventArgs e)
        {
			TriggerCustomEvent(x => PlaceMarketOrder(true, "bid"), null);
        }
		
		void CloseButtonClick(object sender, EventArgs e)
        {
			ClosePositions();
        }
		
		void RevButtonClick(object sender, EventArgs e)
        {
			RevPositions();
        }
		
		void QuantQUDValueChanged(object sender, EventArgs e)
        {
            if (ChartControl != null)
            {
                ChartControl.Dispatcher.InvokeAsync((Action)(() =>
                {
					Quantity = QuantQUD.Value;
                }));
            }
        }
		
		void UpdateInfoLabelByOrderUpdate()
		{
			if (ChartControl != null)
            {
                ChartControl.Dispatcher.InvokeAsync((Action)(() =>
                {
					MarketPosition pos = Position.MarketPosition;
					PosSizeLabel.Content = pos == MarketPosition.Flat ? "Flat" : Position.Quantity.ToString();
					PosSizeLabel.Background = pos == MarketPosition.Long ? Brushes.Green : pos == MarketPosition.Short ? Brushes.Red : Brushes.Gray;
					
					EntryLabel.Content = pos == MarketPosition.Flat ? "Entry" : Position.AveragePrice.ToString();
                }));
            }
		}
		
		void UpdateInfoLabelByBarUpdate()
		{
		    // Ensure the ChartControl is available and the PnL label is ready for update
		    if (ChartControl != null && PnlLabel != null)
		    {
		        // Use Dispatcher to update the UI asynchronously
		        ChartControl.Dispatcher.InvokeAsync((Action)(() =>
		        {
		            double pnl = Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0]);
					//double pnl = SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit - SystemPerformance.AllTrades.TradesPerformance.Currency.RealizedProfit;
		            PnlLabel.Content = Position.MarketPosition == MarketPosition.Flat ? "PnL" : pnl.ToString();
		            PnlLabel.Background = pnl > 0 ? Brushes.Green : pnl < 0 ? Brushes.Red : Brushes.Gray;
		        }));
		    }
		}
		
		#endregion
	}
}
