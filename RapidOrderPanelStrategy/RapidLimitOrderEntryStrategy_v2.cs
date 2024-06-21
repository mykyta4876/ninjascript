#region Using declarations
using System;
using System.Globalization;
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
using NinjaTrader.NinjaScript.Indicators.GB;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
using System.IO;
#endregion

namespace NinjaTrader.NinjaScript.Strategies
{
    public class RapidLimitOrderEntryStrategy : Strategy
    {
        private Order buyLimitOrder;
        private Order sellLimitOrder;
        private ChartScale chartScale;
        private bool isSpaceBarPressed = false;
        private bool isPaused = false;
        private Dictionary<string, Order> stopOrders = new Dictionary<string, Order>();

        protected override void OnStateChange()
        {
            Print("OnStateChange start" + State);
            if (State == State.SetDefaults)
            {
                Description = @"Rapid limit order entry and management via space bar + left-click";
                Name = "RapidLimitOrderEntry";
                Calculate = Calculate.OnEachTick;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = false;
                ExitOnSessionCloseSeconds = 30;
                IsFillLimitOnTouch = false;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
                Slippage = 0;
                StartBehavior = StartBehavior.WaitUntilFlat;
                TimeInForce = TimeInForce.Gtc;
                TraceOrders = false;
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 20;
                IsUnmanaged                                 = true;
                IsInstantiatedOnEachOptimizationIteration = false;
                IsAdoptAccountPositionAware = true;
                
                PauseDuration = 2.0; // Default pause duration
                EnableStopLimit = true; // Default to enabled
                StopLossDistance = 20; // Default stop loss distance in ticks
            }
            else if (State == State.Configure)
            {
                // Configure logic
            }
            else if (State == State.Realtime)
            {
                ChartControl.Dispatcher.InvokeAsync(() =>
                {
                    Print("Adding mouse event");
                    ChartControl.MouseLeftButtonDown += OnMouseLeftButtonDown;
                    ChartControl.PreviewKeyDown += OnPreviewKeyDown;
                    ChartControl.PreviewKeyUp += OnPreviewKeyUp;
                });
            }
            else if (State == State.Historical)
            {
                if (ChartControl != null)
                {
                    foreach (ChartScale scale in ChartPanel.Scales)
                        if (scale.ScaleJustification == ScaleJustification)
                            chartScale = scale;
                    
                }
            }
            else if (State == State.Terminated)
            {
                if (ChartControl != null)
                {
                    ChartControl.MouseLeftButtonDown -= OnMouseLeftButtonDown;
                    ChartControl.PreviewKeyDown -= OnPreviewKeyDown;
                    ChartControl.PreviewKeyUp -= OnPreviewKeyUp;
                }
            }
        }

        protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
        {
            Print("OnExecutionUpdate OrderAction= " + execution.Order.OrderAction);
            
            if (EnableStopLimit && (execution.Order.OrderAction == OrderAction.Buy || execution.Order.OrderAction == OrderAction.SellShort))
            {
                // Place a stop limit order for each trade
                double stopPrice = marketPosition == MarketPosition.Long ?
                    price - StopLossDistance * TickSize :
                    price + StopLossDistance * TickSize;
                stopPrice = Instrument.MasterInstrument.RoundToTickSize(stopPrice);

                string stopOrderId = "Stop_" + executionId;
                stopOrders[stopOrderId] = SubmitOrderUnmanaged(BarsInProgress, marketPosition == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover, OrderType.StopMarket, quantity, 0, stopPrice, stopOrderId, "StopLoss");
            }
        }

        protected override void OnBarUpdate()
        {
            // Your main strategy logic here
        }

        private void OnMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Print("OnMouseLeftButtonDown start");
            if (isPaused) return;
            
            if (isSpaceBarPressed)
            {
                ChartControl chartControl = sender as ChartControl;
                // if (chartControl != null)
                if (true)
                {
                    
                    double yValue = ChartingExtensions.ConvertToVerticalPixels(e.GetPosition(ChartControl as IInputElement).Y, ChartControl.PresentationSource);
                    double price = Instrument.MasterInstrument.RoundToTickSize(chartScale.GetValueByY((float)yValue));
                    
                    Print("OnMouseLeftButtonDown price=" + price);
                    
                    if (price < GetCurrentBid())
                    {
                        if (buyLimitOrder == null || buyLimitOrder.OrderState == OrderState.Cancelled || buyLimitOrder.OrderState == OrderState.Rejected)
                        {
                            buyLimitOrder = SubmitOrderUnmanaged(BarsInProgress, OrderAction.Buy, OrderType.Limit, 1, price, 0, "", "Buy Limit");
                        }
                        else
                        {
                            ChangeOrder(buyLimitOrder, 1, price, 0);
                        }
                    }
                    else if (price > GetCurrentAsk())
                    {
                        if (sellLimitOrder == null || sellLimitOrder.OrderState == OrderState.Cancelled || sellLimitOrder.OrderState == OrderState.Rejected)
                        {
                            sellLimitOrder = SubmitOrderUnmanaged(BarsInProgress, OrderAction.SellShort, OrderType.Limit, 1, price, 0, "", "Sell Limit");
                        }
                        else
                        {
                            ChangeOrder(sellLimitOrder, 1, price, 0);
                        }
                    }
                }
            }
        }

        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
        //protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time)
        {
            if (order == buyLimitOrder || order == sellLimitOrder)
            {
                if (order.OrderState == OrderState.Filled || order.OrderState == OrderState.Cancelled || order.OrderState == OrderState.Rejected)
                {
                    if (order == buyLimitOrder) buyLimitOrder = null;
                    if (order == sellLimitOrder) sellLimitOrder = null;

                    // Pause the strategy for 2 seconds
                    PauseStrategy(PauseDuration * 1000);
                }
            }
            else if (stopOrders.ContainsKey(order.Name) && (order.OrderState == OrderState.Filled || order.OrderState == OrderState.Cancelled || order.OrderState == OrderState.Rejected))
            {
                // Remove stop orders from the dictionary when they are filled, cancelled, or rejected
                stopOrders.Remove(order.Name);
                
                // Pause the strategy for 2 seconds
                PauseStrategy(PauseDuration * 1000);
            }
        }
        
        private async void PauseStrategy(double milliseconds)
        {
            isPaused = true;
            await Task.Delay((int)milliseconds);
            isPaused = false;
        }

        private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Print("OnPreviewKeyDown start" + e.Key);
            //if (e.Key == Key.LeftCtrl)
            if (e.Key == Key.Space)
            {
                isSpaceBarPressed = true;
            }
        }

        private void OnPreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Print("OnPreviewKeyUp start" + e.Key);
            //if (e.Key == Key.LeftCtrl)
            if (e.Key == Key.Space)
            {
                isSpaceBarPressed = false;
            }
        }

        #region Properties
            
            #region Parameters
            
            const string par1   = "Parameters";
        
            [NinjaScriptProperty]
            [Display(ResourceType = typeof(Custom.Resource), Name="Pause Duration (seconds)", Order=1, GroupName=par1)]
            public double PauseDuration
            { get; set; }
            
            [NinjaScriptProperty]
            [Display(ResourceType = typeof(Custom.Resource), Name="Enable Stop Limit", Order=2, GroupName=par1)]
            public bool EnableStopLimit
            { get; set; }
            
            [NinjaScriptProperty]
            [Display(ResourceType = typeof(Custom.Resource), Name="Stop Loss Distance (ticks)", Order=3, GroupName=par1)]
            public int StopLossDistance
            { get; set; }
            
            #endregion
            
        #endregion
    }
}
