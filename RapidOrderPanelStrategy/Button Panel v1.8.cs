#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
//using System.Text;
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
using System.Windows.Controls; // Includes ControlTemplate, Border, TextBlock, etc.
using System.Windows.Controls.Primitives; // Includes ToggleButton and related classes


#endregion

namespace NinjaTrader.NinjaScript.Strategies
{
	public class ButtonPanel : Strategy
	{
		
		
        public override string DisplayName { get { return State == State.SetDefaults ? "Button Panel v1.8" : Name; } }
		
		
        System.Windows.Controls.Grid menuGrid;
        System.Windows.Controls.Grid buysellGrid;
        System.Windows.Controls.Grid exitquantGrid;
        System.Windows.Controls.Grid cancelbreakGrid;
		System.Windows.Controls.Button BuyButton;
        System.Windows.Controls.Button SellButton;
        System.Windows.Controls.Button CancelButton;
        System.Windows.Controls.Button BreakevenButton;
        System.Windows.Controls.Button ExitButton;
		System.Windows.Controls.Primitives.ToggleButton NewPushButton;
        QuantityUpDown QuantQUD;


		
	
		
		
        bool IsToolBarButtonAdded;
        int width = 400;
        int FontSize = 24;
		
		public enum EntryModeEnum {None, Current, Previous};
		
		
		private EntryModeEnum	BuyEntryMode = EntryModeEnum.None;
		private EntryModeEnum	SellEntryMode = EntryModeEnum.None;
		
		
		private Order entryOrder;
		private Order stopOrder;
		private Order profitOrder;
		
		private bool stopWasPlaced;
		private bool entryWasPlaced;
		private bool profitWasPlaced;
		private bool wasBreakeven;
		
		private int	addStopQuant;
		private int addProfitQuant;
		
		private bool wasClosed;
		
		private int entryQuantity;
		private int positionQuantity;
		
		private int exitBar;
		
		private bool moveEntryOrder;
		
		private bool joinBidAsk = false;
		
		
		AtmStrategy 		atmStrategy;
		
		
		#region Properties
		
			
		[Display(Name = "ATM Name", Order = 0, GroupName = "Strategy Parameters")]
        [TypeConverter(typeof(ListConverter))]
        public string ATMTemplate
        {
            get; set;
        }

        [Browsable(false)]
        public string[] ATMTemplates
        {
            set { ListConverter.value = value; }
        }
		

		
		private int Quantity;	//DZ
		
		
		
		[NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Offset Ticks", Order = 2, GroupName = "Parameters")]
        public int OffsetTicks
        { get; set; }
		
		
		
		
//DZ - create BreakevenTicks as it is evaluated by part of the logic
//		private int BreakevenTicks;		//DZ
		

		
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
		
		[NinjaScriptProperty]
        [Display(Name = "Buy Plot Color", Order = 11, GroupName = "Parameters")]
        public Brush BuyPlotColor { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Sell Plot Color", Order = 12, GroupName = "Parameters")]
        public Brush SellPlotColor { get; set; }
		
			
		
		
		private void MakeDefaults()
		{
			Quantity = 1;
			OffsetTicks = 8;
//			StopTicks = 10;		//DZ - StopTicks removed
//			ProfitTicks = 15;	//DZ - ProfitTicks removed
//			BreakevenTicks = 4;
			
			LeftMargin = 0;		//DZ - added default
			TopMargin = 0;	//DZ - added default
			ButtonPanelWidth = 400;		//DZ - added default
			ButtonHeight = 50;
			ButtonPanelFontSize = 24;	//DZ - added default
			
			BuyPlotColor = Brushes.Green;
			SellPlotColor = Brushes.Red;
			
			
		}
		
		#endregion
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name										= "Button Panel v1.8";
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
				
				
				ATMTemplates 								= ATM_Templates();
                ATMTemplate 								= "ATM";
				
			}
			else if (State == State.Configure)
			{
					AddPlot(Brushes.Green, "Buy");
					AddPlot(Brushes.Red, "Sell");
			}
			else if (State == State.DataLoaded)
			{
				Account.OrderUpdate 	+= OnOrderUpdate;
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
				
				if(Account != null)
					Account.OrderUpdate 	-= OnOrderUpdate;
			}
		}

		protected override void OnBarUpdate()
		{
			Values[0][0] = Lows[0][0] + OffsetTicks * TickSize;
			Values[1][0] = Highs[0][0] - OffsetTicks * TickSize;
			
			if(moveEntryOrder)
				MoveEntryOrder();
			
			MoveJoinBidAskOrder();
			
			Plots[0].Brush = BuyPlotColor;  //DZ added configurable buy plot color
			Plots[1].Brush = SellPlotColor;  //DZ added configurable sell plot color
			
//			MoveStopToBreakeven(Closes[0][0]);
		}
		
        #region OnOrderUpdate



        #endregion
		
		#region OnOrderUpdate ATM
		
		private void OnOrderUpdate(object sender, OrderEventArgs e)
		{

			
			
				
			//New ATM entry order was placed
			if(e.Order.OrderState == OrderState.Submitted && e.Order == entryOrder)
			{
				atmStrategy	=	e.Order.GetOwnerStrategy() as AtmStrategy;
			}
			
			
			
			//ATM entry order was cancelled
			if(e.Order.OrderState == OrderState.Cancelled && e.Order == entryOrder)
			{
				
//				Print("399 entry was placed = false");
				entryWasPlaced = false;
				
				joinBidAsk = joinBidAsk ? !joinBidAsk : joinBidAsk;
				
			}
			
			//ATM entry order was filled
			if(e.Order.OrderState == OrderState.Filled && e.Order == entryOrder)
			{
				
//				Print("410 entry was placed = false");
//				entryWasPlaced = false;
				
				joinBidAsk = joinBidAsk ? !joinBidAsk : joinBidAsk;
				
			}
			
			//ATM entry order was rejected
			if(e.Order.OrderState == OrderState.Rejected && e.Order == entryOrder)
			{
//				Print("420 entry was placed = false");
				entryWasPlaced = false;
				
				joinBidAsk = joinBidAsk ? !joinBidAsk : joinBidAsk;
				
			}
			
//			Print(e.Order.Name+" "+(atmStrategy != null)+" "+(e.Order.GetOwnerStrategy() == atmStrategy)+" "+entryWasPlaced);
			
			if(atmStrategy != null && e.Order.GetOwnerStrategy() == atmStrategy && entryWasPlaced)
			{
				
				
				
				string[,] orders	=	GetAtmStrategyStopTargetOrderStatus(e.Order.Name, atmStrategy);
				bool	atmStrategyClosed	=	orders.GetLength(1) != 0;
				for (int i = 0; i < orders.GetLength(0); i++)
				{
					if(orders[i, 2] != "Cancelled" && orders[i, 2] != "Filled")
					{
						atmStrategyClosed	=	false;
					}
				}
				
				
				if(atmStrategyClosed)
				{
					
//					Print("entry was placed = false");
					entryWasPlaced	=	false;
					TradeFinish();
				}
			}
			
			
			
			
		}
		
		private string[,] GetAtmStrategyStopTargetOrderStatus(string orderName, NinjaTrader.NinjaScript.AtmStrategy atm)
		{
			if (atm == null)
				return new string[0, 0];
			
			if (Regex.Match(orderName, "^stop[1-9][0-9]*$", RegexOptions.IgnoreCase).Success|| Regex.Match(orderName, "^target[1-9][0-9]*$", RegexOptions.IgnoreCase).Success)
			{
				int idx 	= 0;
				string name = string.Empty;
				
				try
				{
					idx 	= Convert.ToInt32(Regex.Replace(orderName, "[a-zA-Z]*", "")) - 1;
					name 	= Regex.Replace(orderName, "[1-9][0-9]*", "").ToLower();
				}
				catch
				{
					return new string[0, 0];
				}

				if (idx > atm.Brackets.Length - 1)
					return new string[0, 0];

				System.Collections.ObjectModel.Collection<Cbi.Order> orders = (name == "stop" ? atm.GetStopOrders(idx) : atm.GetTargetOrders(idx));
				
				if (orders.Count == 0)
					return new string[0, 0];

				string[,] ordersArray = new string[orders.Count, 3];
				
				for (int i = 0; i < orders.Count; i++)
				{
					ordersArray[i, 0] = orders[i].AverageFillPrice.ToString(System.Globalization.CultureInfo.InvariantCulture);
					ordersArray[i, 1] = orders[i].Filled.ToString(System.Globalization.CultureInfo.InvariantCulture);
					ordersArray[i, 2] = orders[i].OrderState.ToString();
				}

				return ordersArray;
			}
			else 
				return new string[0, 0];
		}
		
		private List<Order> GetAtmStopOrders(NinjaTrader.NinjaScript.AtmStrategy atm)
		{
			if (atm == null)
				return null;
			
			return atm.GetStopOrders(0).ToList();
			
		}
		
		private MarketPosition GetAtmStrategyMarketPosition(NinjaTrader.NinjaScript.AtmStrategy atm)
		{
			if (atm == null || atm.State != State.Realtime || atm.Position == null)
				return Cbi.MarketPosition.Flat;

			return atm.Position.MarketPosition;
		}
		
		private double GetAtmStrategyPositionAveragePrice(NinjaTrader.NinjaScript.AtmStrategy atm)
		{
			if (atm == null || atm.State != State.Realtime || atm.Position == null)
				return 0.0;

			return atm.Position.AveragePrice;
		}
		
		private int GetAtmStrategyPositionQuantity(NinjaTrader.NinjaScript.AtmStrategy atm)
		{
			if (atm == null || atm.State != State.Realtime || atm.Position == null)
				return 0;

			return atm.Position.Quantity;
		}
		
		private double GetAtmStrategyRealizedProfitLoss(NinjaTrader.NinjaScript.AtmStrategy atm)
		{
			if (atm == null)
				return 0.0;
			
			lock(atm.Executions)
				return Cbi.SystemPerformance.Calculate(atm.Executions).AllTrades.TradesPerformance.Currency.CumProfit;
		}
		
		private double GetAtmStrategyUnrealizedProfitLoss(NinjaTrader.NinjaScript.AtmStrategy atm)
		{
			if (atm == null || atm.State != State.Realtime || atm.Position == null)
				return 0.0;

			return atm.Position.GetUnrealizedProfitLoss(Cbi.PerformanceUnit.Currency);
		}
		
		#endregion
		
        #region OnExecutionUpdate

        #endregion
		
		#region Trade Methods
		
        private void MoveStopToBreakeven()
		{
			if(atmStrategy != null && 	GetAtmStrategyMarketPosition(atmStrategy) != MarketPosition.Flat)
			{
				List<Order> stopOrders = GetAtmStopOrders(atmStrategy);
				
				foreach(Order stop in stopOrders)
				{
					if(IsWorkingOrder(stop))
					{
						
						stop.StopPriceChanged = entryOrder.AverageFillPrice;
						stop.LimitPriceChanged = entryOrder.AverageFillPrice;
						
						
					}
				}
				
				Account.Change(stopOrders);
			}
		}
		
		private void JoinAskBid(bool isLong)
		{
			if(GetAtmStrategyMarketPosition(atmStrategy) == MarketPosition.Flat)
			{
				if(IsWorkingOrder(entryOrder))
				{
					Account.Cancel(new[] {entryOrder});
					entryOrder = null;
				}
				
				joinBidAsk = true;
				entryWasPlaced = true;
				BuyEntryMode = EntryModeEnum.None;
				SellEntryMode = EntryModeEnum.None;
				UpdateButtons();
				
				OrderAction oa = isLong ? OrderAction.Buy : OrderAction.Sell;
				
				entryOrder = Account.CreateOrder(Instrument, oa, OrderType.Limit, TimeInForce.Gtc, Quantity, isLong ? GetCurrentAsk() : GetCurrentBid(), 0, string.Empty, "Entry", null);
				NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(ATMTemplate, entryOrder);
				
//				SubmitOrderUnmanaged(0, oa, OrderType.Market, Quantity, 0, 0, "", (isLong ? "Long " : "Short ") + " Entry");
				
			}
		}
		
		private void MoveJoinBidAskOrder()
		{
			if(joinBidAsk && IsWorkingOrder(entryOrder))
			{
				double entryPrice = entryOrder.OrderAction == OrderAction.Buy ? GetCurrentAsk() : GetCurrentBid();
				
				entryOrder.LimitPriceChanged = entryPrice;
				Account.Change(new List<Order>{entryOrder});
				
			}
		}
		
		private void PlaceMarketOrder(bool isLong)
		{
			if(GetAtmStrategyMarketPosition(atmStrategy) == MarketPosition.Flat)
			{
				if(IsWorkingOrder(entryOrder))
				{
					Account.Cancel(new[] {entryOrder});
					entryOrder = null;
				}
				entryWasPlaced = true;
				BuyEntryMode = EntryModeEnum.None;
				SellEntryMode = EntryModeEnum.None;
				UpdateButtons();
				
				OrderAction oa = isLong ? OrderAction.Buy : OrderAction.Sell;
				
				entryOrder = Account.CreateOrder(Instrument, oa, OrderType.Market, TimeInForce.Gtc, Quantity, 0, 0, string.Empty, "Entry", null);
				NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(ATMTemplate, entryOrder);
				
//				SubmitOrderUnmanaged(0, oa, OrderType.Market, Quantity, 0, 0, "", (isLong ? "Long " : "Short ") + " Entry");
				
			}
		}
		
		private void PlaceEntryOrder(bool isLong)
		{
			if(GetAtmStrategyMarketPosition(atmStrategy) != MarketPosition.Flat)
				return;
			
//			if(IsWorkingOrder(entryOrder) && ((entryOrder.OrderAction == OrderAction.Buy && !isLong || entryOrder.OrderAction == OrderAction.Sell && isLong)))
//			{
				Account.Cancel(new[] {entryOrder});
				entryOrder = null;
			
			if(isLong)
			{
				SellEntryMode = EntryModeEnum.None;
			}
			else
			{
				BuyEntryMode = EntryModeEnum.None;
			}
			
			UpdateButtons();
//			}
			
			entryWasPlaced = true;
			double entryPrice = isLong ? Lows[0][0] + OffsetTicks * TickSize : Highs[0][0] - OffsetTicks * TickSize;
			
			OrderAction oa = isLong ? OrderAction.Buy : OrderAction.Sell;
			
			entryOrder = Account.CreateOrder(Instrument, oa, OrderType.StopMarket, TimeInForce.Gtc, Quantity, entryPrice, entryPrice, string.Empty, "Entry", null);
			AtmStrategy.StartAtmStrategy(ATMTemplate, entryOrder);
			
			
			moveEntryOrder = true;
			
		}
		
		private void MoveEntryOrder()
		{
			if(moveEntryOrder && IsWorkingOrder(entryOrder))
			{
				double entryPrice = entryOrder.OrderAction == OrderAction.Buy ? Lows[0][0] + OffsetTicks * TickSize : Highs[0][0] - OffsetTicks * TickSize;
				
				if((entryOrder.OrderAction == OrderAction.Buy && entryPrice < entryOrder.StopPrice) ||
					(entryOrder.OrderAction == OrderAction.Sell && entryPrice > entryOrder.StopPrice))
				{
					
					entryOrder.StopPriceChanged = entryPrice;
					entryOrder.LimitPriceChanged = entryPrice;
					Account.Change(new List<Order>{entryOrder});
				}
			}
		}
		
		private void MoveEntryOrderToPreviousBar(bool isLong)
		{
			if(IsWorkingOrder(entryOrder))
			{
				Account.Cancel(new[] {entryOrder});
				entryOrder = null;
				
				double entryPrice = isLong ? Lows[0][1] + OffsetTicks * TickSize : Highs[0][1] - OffsetTicks * TickSize;
				
				OrderAction oa = isLong ? OrderAction.Buy : OrderAction.Sell;
				OrderType ot = (isLong && entryPrice < Closes[0][0]) || (!isLong && entryPrice > Closes[0][0]) ? OrderType.Limit : OrderType.StopMarket;
				
				entryOrder = Account.CreateOrder(Instrument, oa, ot, TimeInForce.Gtc, Quantity, entryPrice, ot == OrderType.StopMarket ? entryPrice : 0, string.Empty, "Entry", null);
				AtmStrategy.StartAtmStrategy(ATMTemplate, entryOrder);
				
				moveEntryOrder = false;
			}
		}
		

		
		private int PnlTicks(Order entry, double close)
        {
            if (entryOrder == null)
                return 0;

            double pnl = 0;

            if (entry.OrderAction == OrderAction.Buy || entry.OrderAction == OrderAction.BuyToCover)
                pnl = close - entry.AverageFillPrice;
            if (entry.OrderAction == OrderAction.Sell || entry.OrderAction == OrderAction.SellShort)
                pnl = entry.AverageFillPrice - close;

            int pnl_ticks = (int)(Instrument.MasterInstrument.RoundToTickSize(pnl) / TickSize);

            return (pnl_ticks);
        }
		
		private bool IsWorkingOrder(Order order)
		{
			return order != null && order.OrderState != OrderState.Filled && order.OrderState != OrderState.Cancelled && order.OrderState != OrderState.Rejected;
		}
		
		private void UpdateButtons()
		{
			ChartControl.Dispatcher.InvokeAsync((Action)(() =>
            {
				BuyButton.Content = BuyEntryMode == EntryModeEnum.None  ? "BUY" : BuyEntryMode == EntryModeEnum.Current ? "BUY CURRENT" : "BUY PREVIOUS";
                BuyButton.Background = BuyEntryMode == EntryModeEnum.None  ? Brushes.Gray : Brushes.Red;
				
				SellButton.Content = SellEntryMode == EntryModeEnum.None  ? "SELL" : SellEntryMode == EntryModeEnum.Current ? "SELL CURRENT" : "SELL PREVIOUS";
                SellButton.Background = SellEntryMode == EntryModeEnum.None  ? Brushes.Gray : Brushes.Red;

            }));
		}
		
		private void TradeFinish()
        {
			
//			Print("Trade Finish entry was placed = false");
//            entryOrder = null;
            entryWasPlaced = false;
//            entryQuantity = 0;
			moveEntryOrder = false;
			
			BuyEntryMode = EntryModeEnum.None;
			SellEntryMode = EntryModeEnum.None;
			
			UpdateButtons();

            CancelAllOrders();

        }
		
		private void CancelAllOrders()
        {
//            if (IsWorkingOrder(stopOrder))
//            {
//                CancelOrder(stopOrder);
//            }

//            if (IsWorkingOrder(profitOrder))
//            {
//                CancelOrder(profitOrder);
//            }

        }
		
		private void ClosePosition(string name)
        {
//            if (Positions[0] != null && Positions[0].MarketPosition != MarketPosition.Flat && !wasClosed)
//            {
//                wasClosed = true;
//                OrderAction oa = Positions[0].MarketPosition == MarketPosition.Long ? OrderAction.Sell : OrderAction.Buy;
//                SubmitOrderUnmanaged(0, oa, OrderType.Market, Positions[0].Quantity, 0, 0, "", name);
//            }
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
			
			buysellGrid = new System.Windows.Controls.Grid();
			exitquantGrid = new System.Windows.Controls.Grid();
			cancelbreakGrid = new System.Windows.Controls.Grid();

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

            // Add the columns to the Grid
            menuGrid.ColumnDefinitions.Add(column1);
            menuGrid.ColumnDefinitions.Add(column2);
            menuGrid.RowDefinitions.Add(row1);
            menuGrid.RowDefinitions.Add(row2);
            menuGrid.RowDefinitions.Add(row3);
            menuGrid.RowDefinitions.Add(row4);

			System.Windows.Controls.ColumnDefinition row2_column1 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0.5) };
			System.Windows.Controls.ColumnDefinition row2_column2 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0) };
			System.Windows.Controls.ColumnDefinition row2_column3 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0.5) };
			
			buysellGrid.ColumnDefinitions.Add(row2_column1);
			buysellGrid.ColumnDefinitions.Add(row2_column2);
			buysellGrid.ColumnDefinitions.Add(row2_column3);
			
			System.Windows.Controls.ColumnDefinition row3_column1 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0.5) };
			System.Windows.Controls.ColumnDefinition row3_column2 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0) };
			System.Windows.Controls.ColumnDefinition row3_column3 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0.5) };
			
			cancelbreakGrid.ColumnDefinitions.Add(row3_column1);
			cancelbreakGrid.ColumnDefinitions.Add(row3_column2);
			cancelbreakGrid.ColumnDefinitions.Add(row3_column3);
			
			System.Windows.Controls.ColumnDefinition row4_column1 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0.25) };
			System.Windows.Controls.ColumnDefinition row4_column2 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0.25) };
			System.Windows.Controls.ColumnDefinition row4_column3 = new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(width * 0.5) };

			exitquantGrid.ColumnDefinitions.Add(row4_column1);
			exitquantGrid.ColumnDefinitions.Add(row4_column2);
			exitquantGrid.ColumnDefinitions.Add(row4_column3);
			
        }

        private void AddRows()
        {
			BuyButton = new System.Windows.Controls.Button()
            {
                Name = "BuyButton",
                Content = BuyEntryMode == EntryModeEnum.None ? "BUY" : BuyEntryMode == EntryModeEnum.Current ? "BUY CURR" : "BUY PREV",		//DZ - shortened labels
                Foreground = Brushes.White,
                Background = BuyEntryMode == EntryModeEnum.None ? Brushes.Gray : Brushes.Blue,		//DZ - changed color
                FontWeight = FontWeights.Bold,
                FontSize = FontSize,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinWidth = 0,
            };

            SellButton = new System.Windows.Controls.Button()
            {
                Name = "SellButton",
                Content = SellEntryMode == EntryModeEnum.None ? "SELL" : SellEntryMode == EntryModeEnum.Current ? "SELL CURR" : "SELL PREV",	//DZ - shortened labels
                Foreground = Brushes.White,
                Background = SellEntryMode == EntryModeEnum.None ? Brushes.Gray : Brushes.Red,
                FontWeight = FontWeights.Bold,
                FontSize = FontSize,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinWidth = 0,
            };

            CancelButton = new System.Windows.Controls.Button()
            {
                Name = "CancelButton",
                Content = "CANCEL",
                Foreground = Brushes.White,
                Background = Brushes.Gray,
                FontWeight = FontWeights.Bold,
                FontSize = FontSize,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinWidth = 0,
            };
			
			BreakevenButton = new System.Windows.Controls.Button()
            {
                Name = "BreakevenButton",
                Content = "BREAKEVEN",
                Foreground = Brushes.White,
                Background = Brushes.Gray,
                FontWeight = FontWeights.Bold,
                FontSize = FontSize,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinWidth = 0,
            };

            ExitButton = new System.Windows.Controls.Button()
            {
                Name = "ExitButton",
                Content = "EXIT",
                Foreground = Brushes.White,
                Background = Brushes.Gray,
                FontWeight = FontWeights.Bold,
//                Width = width * 0.5,
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

			
            // Replace QuantLabel with NewPushButton
NewPushButton = new System.Windows.Controls.Primitives.ToggleButton()
{
    Name = "NewPushButton",
    Foreground = Brushes.White,
    Background = Brushes.Gray, // Fixed to gray
    FontWeight = FontWeights.Bold,
    FontSize = FontSize - 6,
    HorizontalContentAlignment = HorizontalAlignment.Center,
    VerticalContentAlignment = VerticalAlignment.Center,
    Padding = new Thickness(5),
    Cursor = System.Windows.Input.Cursors.Arrow
};


// Apply a ControlTemplate to define the visual structure of the button
ControlTemplate toggleButtonTemplate = new ControlTemplate(typeof(System.Windows.Controls.Primitives.ToggleButton));
FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
border.SetValue(Border.BackgroundProperty, Brushes.Gray); // Fixed gray background
border.SetValue(Border.BorderBrushProperty, Brushes.Black); // Optional border color
border.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
border.SetValue(Border.BorderThicknessProperty, new Thickness(1));

FrameworkElementFactory contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

// Add the ContentPresenter inside the Border
border.AppendChild(contentPresenter);

// Set the visual tree
toggleButtonTemplate.VisualTree = border;

// Assign the custom ControlTemplate to the button
NewPushButton.Template = toggleButtonTemplate;


// Use a TextBlock to support multi-line content
TextBlock buttonContent = new TextBlock()
{
    Text = "Trend",
    TextAlignment = TextAlignment.Center,
    TextWrapping = TextWrapping.Wrap, // Enable multi-line wrapping
    FontSize = FontSize - 6,
    Foreground = Brushes.White
};

// Set the TextBlock as the content of the ToggleButton
NewPushButton.Content = buttonContent;

// Attach event handlers for Checked/Unchecked states
NewPushButton.Checked += NewPushButtonChecked;
NewPushButton.Unchecked += NewPushButtonUnchecked;

// Add the button to the grid
System.Windows.Controls.Grid.SetColumn(NewPushButton, 1);
System.Windows.Controls.Grid.SetRow(NewPushButton, 0);
exitquantGrid.Children.Add(NewPushButton);

	
	
            
            System.Windows.Controls.Grid.SetColumn(BuyButton, 0);
            System.Windows.Controls.Grid.SetRow(BuyButton, 0);
			
			System.Windows.Controls.Grid.SetColumn(SellButton, 2);
            System.Windows.Controls.Grid.SetRow(SellButton, 0);

            System.Windows.Controls.Grid.SetColumn(CancelButton, 2);
            System.Windows.Controls.Grid.SetRow(CancelButton, 0);
			
			System.Windows.Controls.Grid.SetColumn(BreakevenButton, 0);
            System.Windows.Controls.Grid.SetRow(BreakevenButton, 0);
			
            System.Windows.Controls.Grid.SetColumn(ExitButton, 2);
            System.Windows.Controls.Grid.SetRow(ExitButton, 0);

            System.Windows.Controls.Grid.SetColumn(QuantQUD, 0);
			
//            System.Windows.Controls.Grid.SetColumn(QuantLabel, 1);
			
			buysellGrid.Children.Add(BuyButton);
			buysellGrid.Children.Add(SellButton);
			System.Windows.Controls.Grid.SetRow(buysellGrid, 1);
			System.Windows.Controls.Grid.SetColumn(buysellGrid, 1);
            menuGrid.Children.Add(buysellGrid);
			
			cancelbreakGrid.Children.Add(CancelButton);
			cancelbreakGrid.Children.Add(BreakevenButton);
			System.Windows.Controls.Grid.SetRow(cancelbreakGrid, 2);
			System.Windows.Controls.Grid.SetColumn(cancelbreakGrid, 1);
            menuGrid.Children.Add(cancelbreakGrid);
			
			exitquantGrid.Children.Add(QuantQUD);
//			exitquantGrid.Children.Add(QuantLabel);
			exitquantGrid.Children.Add(ExitButton);
			System.Windows.Controls.Grid.SetRow(exitquantGrid, 3);
			System.Windows.Controls.Grid.SetColumn(exitquantGrid, 1);
            menuGrid.Children.Add(exitquantGrid);
			
			
			
           

            BuyButton.Click += BuyButtonClick;
			SellButton.Click += SellButtonClick;
			BuyButton.MouseRightButtonDown += BuyRightButtonClick;
			SellButton.MouseRightButtonDown += SellRightButtonClick;
            ExitButton.Click += ExitButtonClick;
            CancelButton.Click += CancelButtonClick;
            BreakevenButton.Click += BreakevenButtonClick;
			
            QuantQUD.ValueChanged += QuantQUDValueChanged;

        }

        #endregion

		

        void BuyButtonClick(object sender, EventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;

			
			if(Position.MarketPosition != MarketPosition.Flat)
				return;
			
			BuyEntryMode = BuyEntryMode == EntryModeEnum.None ? EntryModeEnum.Current : BuyEntryMode == EntryModeEnum.Current ? EntryModeEnum.Previous : EntryModeEnum.Current;
			
			if(BuyEntryMode == EntryModeEnum.Current)
				TriggerCustomEvent(x => PlaceEntryOrder(true), null);
			else if(BuyEntryMode == EntryModeEnum.Previous)
				TriggerCustomEvent(x =>MoveEntryOrderToPreviousBar(true), null);
			
            if (ChartControl != null)
            {
                ChartControl.Dispatcher.InvokeAsync((Action)(() =>
                {
					
			
                    btn.Content = BuyEntryMode == EntryModeEnum.None  ? "BUY" : BuyEntryMode == EntryModeEnum.Current ? "BUY CURR" : "BUY PREV";	//DZ - shortened names
                    btn.Background = BuyEntryMode == EntryModeEnum.None  ? Brushes.Gray : Brushes.Green;		//DZ - changed color
                }));
            }
        }

        void SellButtonClick(object sender, EventArgs e)
        {
			
			
			if(Position.MarketPosition != MarketPosition.Flat)
				return;
			
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;

			SellEntryMode = SellEntryMode == EntryModeEnum.None ? EntryModeEnum.Current : SellEntryMode == EntryModeEnum.Current ? EntryModeEnum.Previous : EntryModeEnum.Current;
			
			if(SellEntryMode == EntryModeEnum.Current)
				TriggerCustomEvent(x => PlaceEntryOrder(false), null);
			else if(SellEntryMode == EntryModeEnum.Previous)
				TriggerCustomEvent(x => MoveEntryOrderToPreviousBar(false), null);

            if (ChartControl != null)
            {
                ChartControl.Dispatcher.InvokeAsync((Action)(() =>
                {
					btn.Content = SellEntryMode == EntryModeEnum.None  ? "SELL" : SellEntryMode == EntryModeEnum.Current ? "SELL CURR" : "SELL PREV";		//DZ - shortened names
                    btn.Background = SellEntryMode == EntryModeEnum.None  ? Brushes.Gray : Brushes.Red;

                }));
            }
        }
		
		void BuyRightButtonClick(object sender, MouseEventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;

			TriggerCustomEvent(x => JoinAskBid(true), null);
			
//			e.SuppressKeyPress = true;
			
            if (ChartControl != null)
            {
                ChartControl.Dispatcher.InvokeAsync((Action)(() =>
                {
                   
                }));
            }
        }
		
		void SellRightButtonClick(object sender, MouseEventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;

			TriggerCustomEvent(x => JoinAskBid(false), null);
			
//			e.SuppressKeyPress = true;
			
            if (ChartControl != null)
            {
                ChartControl.Dispatcher.InvokeAsync((Action)(() =>
                {
                   
                }));
            }
        }
		
		void CancelButtonClick(object sender, EventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;

			if(IsWorkingOrder(entryOrder))
			{
				Account.Cancel(new[] {entryOrder});
				
				TradeFinish();
			}

            if (ChartControl != null)
            {
                ChartControl.Dispatcher.InvokeAsync((Action)(() =>
                {
//                    btn.Content = TradeShorts ? "Short On" : "Short Off";
//                    btn.Background = TradeShorts ? green : Brushes.Red;

//                    ChangeMenuCheckBox(btn.Name, TradeShorts);

                }));
            }
        }
		
		void BreakevenButtonClick(object sender, EventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;

			MoveStopToBreakeven();
        }
		
		void ExitButtonClick(object sender, EventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;

//			ClosePosition("Exit");
			if(atmStrategy != null)
			{
				atmStrategy.CloseStrategy("Exit");
				TradeFinish();
			}

            if (ChartControl != null)
            {
                ChartControl.Dispatcher.InvokeAsync((Action)(() =>
                {
//                    btn.Content = TradeShorts ? "Short On" : "Short Off";
//                    btn.Background = TradeShorts ? green : Brushes.Red;

//                    ChangeMenuCheckBox(btn.Name, TradeShorts);

                }));
            }
        }
		
		void QuantQUDValueChanged(object sender, EventArgs e)
        {
			

            if (ChartControl != null)
            {
                ChartControl.Dispatcher.InvokeAsync((Action)(() =>
                {
//                    btn.Content = TradeShorts ? "Short On" : "Short Off";
//                    btn.Background = TradeShorts ? green : Brushes.Red;

//                    ChangeMenuCheckBox(btn.Name, TradeShorts);
					
					Quantity = QuantQUD.Value;

                }));
            }
        }
		
		
private void NewPushButtonChecked(object sender, RoutedEventArgs e)
{
    var btn = sender as System.Windows.Controls.Primitives.ToggleButton;

    if (btn.Content is TextBlock content)
    {
        content.Text = "Counter\nTrend";
    }

    btn.Background = Brushes.DarkGray; // Ensure background remains consistent
    Print("NewPushButton is set to Counter Trend mode.");
}

private void NewPushButtonUnchecked(object sender, RoutedEventArgs e)
{
    var btn = sender as System.Windows.Controls.Primitives.ToggleButton;

    if (btn.Content is TextBlock content)
    {
        content.Text = "Trend";
    }

    btn.Background = Brushes.DarkGray; // Ensure background remains consistent
    Print("NewPushButton is set to Trend mode.");
}


		
		
		
		

		
		#endregion
		
		#region ListConverter
		
        public sealed class ListConverter : StringConverter
        {

            public static string[] value;
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            { return true; }
            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            { return true; }
            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            { return new StandardValuesCollection(value); }


        }

        #endregion
		
		#region GetATMTemplates

        private string[] ATM_Templates()
        {
            List<string> atmTemplates = new List<string>();
            var serializer = new XmlSerializer(typeof(AtmStrategy));
            List<AtmStrategy> atmList = new List<AtmStrategy>();
            string[] ATMfiles = System.IO.Directory.GetFiles(NinjaTrader.Core.Globals.UserDataDir + "templates/AtmStrategy");

            foreach (string ATMfile in ATMfiles)
            {


                using (System.IO.StreamReader streamReader = new System.IO.StreamReader(ATMfile))
                {
                    string line = "";
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (line.Contains("<Template>"))
                        {
                            int start = line.IndexOf(">") + 1;
                            int end = line.IndexOf("<", start);
                            string result = line.Substring(start, end - start);
                            atmTemplates.Add(result);
                            break;
                        }

                    }
                }
            }

            return (atmTemplates.ToArray());

        }

        #endregion
		
		
	}
}