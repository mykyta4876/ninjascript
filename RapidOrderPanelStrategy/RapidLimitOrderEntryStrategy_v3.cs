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
        private DateTime lastTime;
        private bool isMessageShowed = false;
        
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
                DistanceForPauseDuration = 40;
                MaxSpread = 0;
                Quantity = 1;
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
            /*
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
            */
            
            if (EnableStopLimit)
            {
                if (execution.Order.OrderAction == OrderAction.Buy)
                {
                    if (Position.MarketPosition == MarketPosition.Long)
                    {
                        double stopPrice = price - StopLossDistance * TickSize;
                        stopPrice = Instrument.MasterInstrument.RoundToTickSize(stopPrice);
    
                        string stopOrderId = "Stop_" + executionId;
                        stopOrders[stopOrderId] = SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.StopMarket, Position.Quantity, 0, stopPrice, stopOrderId, "StopLoss");    
                    }
                    else
                    {
                        CancelAllStopLossOrders(OrderAction.BuyToCover);
                    }
                }
                else if (execution.Order.OrderAction == OrderAction.SellShort)
                {
                    if (Position.MarketPosition == MarketPosition.Short)
                    {
                        double stopPrice = price + StopLossDistance * TickSize;
                        stopPrice = Instrument.MasterInstrument.RoundToTickSize(stopPrice);
    
                        string stopOrderId = "Stop_" + executionId;
                        stopOrders[stopOrderId] = SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.StopMarket, Position.Quantity, 0, stopPrice, stopOrderId, "StopLoss");
                    }
                    else
                    {
                        CancelAllStopLossOrders(OrderAction.Sell);
                    }                   
                }
            }
        }

        protected override void OnBarUpdate()
        {
            // Your main strategy logic here
            
            if (BarsInProgress != 0)
                return;

            if (MaxSpread == 0)
                return;
            
            double bid = GetCurrentBid();
            double ask = GetCurrentAsk();
            double spread = ask - bid;
            
            if (spread > MaxSpread)
            {
                if (isMessageShowed == false)
                {
                    ShowMessageBox("Bid/Ask Spread Too Wide");
                    isMessageShowed = true;
                }
            }
            else
            {
                if (isMessageShowed == true)
                {
                    HideMessageBox();
                    isMessageShowed = false;
                }
            }
        }

        private void OnMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Print("OnMouseLeftButtonDown start");
            if (isPaused) return;
            
            if (isSpaceBarPressed)
            {
                double yValue = ChartingExtensions.ConvertToVerticalPixels(e.GetPosition(ChartControl as IInputElement).Y, ChartControl.PresentationSource);
                double price = Instrument.MasterInstrument.RoundToTickSize(chartScale.GetValueByY((float)yValue));
                
                Print("OnMouseLeftButtonDown price=" + price);
                
                double bid = GetCurrentBid();
                double ask = GetCurrentAsk();
                
                if (price < bid)
                {
                    if (buyLimitOrder == null || buyLimitOrder.OrderState == OrderState.Cancelled || buyLimitOrder.OrderState == OrderState.Rejected)
                    {
                        // Get the current time
                        DateTime nowTime = DateTime.Now;
                        if (lastTime.AddSeconds(PauseDuration) > nowTime && bid - DistanceForPauseDuration < price)
                        {
                            Print("OnMouseLeftButtonDown delay: lastTime=" + lastTime.ToString("HH:mm:ss.fff") + ", nowTime=" + nowTime.ToString("HH:mm:ss.fff"));
                            return;
                        }
                        
                        if (isMessageShowed == true && (Position.MarketPosition == MarketPosition.Long || Position.MarketPosition == MarketPosition.Flat))
                        {
                            Print("OnMouseLeftButtonDown : isMessageShowed == true, MarketPosition.Long || Flat");
                            return;
                        }
                        
                        buyLimitOrder = SubmitOrderUnmanaged(BarsInProgress, OrderAction.Buy, OrderType.Limit, Quantity, price, 0, "", "Buy Limit");
                        
                    }
                    else
                    {
                        ChangeOrder(buyLimitOrder, Quantity, price, 0);
                    }
                }
                else if (price > ask)
                {
                    if (sellLimitOrder == null || sellLimitOrder.OrderState == OrderState.Cancelled || sellLimitOrder.OrderState == OrderState.Rejected)
                    {
                        // Get the current time
                        DateTime nowTime = DateTime.Now;
                        if (lastTime.AddSeconds(PauseDuration) > nowTime && ask + DistanceForPauseDuration > price)
                        {
                            Print("OnMouseLeftButtonDown delay: lastTime=" + lastTime.ToString("HH:mm:ss.fff") + ", nowTime=" + nowTime.ToString("HH:mm:ss.fff"));
                            return;
                        }
                        
                        if (isMessageShowed == true && (Position.MarketPosition == MarketPosition.Short || Position.MarketPosition == MarketPosition.Flat))
                        {
                            Print("OnMouseLeftButtonDown : isMessageShowed == true, MarketPosition.Short || Flat");
                            return;
                        }
                        
                        sellLimitOrder = SubmitOrderUnmanaged(BarsInProgress, OrderAction.SellShort, OrderType.Limit, Quantity, price, 0, "", "Sell Limit");
                    }
                    else
                    {
                        ChangeOrder(sellLimitOrder, Quantity, price, 0);
                    }
                }
            }
        }

        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
        //protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time)
        {
            if (order == buyLimitOrder || order == sellLimitOrder)
            {
                //if (order.OrderState == OrderState.Filled || order.OrderState == OrderState.Cancelled || order.OrderState == OrderState.Rejected)
                if (order.OrderState == OrderState.Filled)
                {
                    DateTime nowTime = DateTime.Now;
                    lastTime = nowTime;
                    
                    if (order == buyLimitOrder) buyLimitOrder = null;
                    if (order == sellLimitOrder) sellLimitOrder = null;

                    // Pause the strategy for 2 seconds
                    //PauseStrategy(PauseDuration * 1000);
                }
            }
            //else if (stopOrders.ContainsKey(order.Name) && (order.OrderState == OrderState.Filled || order.OrderState == OrderState.Cancelled || order.OrderState == OrderState.Rejected))
            else if (stopOrders.ContainsKey(order.Name) && (order.OrderState == OrderState.Filled))
            {
                // Remove stop orders from the dictionary when they are filled, cancelled, or rejected
                stopOrders.Remove(order.Name);
                
                // Pause the strategy for 2 seconds
                //PauseStrategy(PauseDuration * 1000);
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

        private void CancelAllStopLossOrders(OrderAction orderAction)
        {
            foreach (Order order in Orders)
            {
                if (order != null && order.OrderType == OrderType.StopMarket && order.OrderAction == orderAction)
                {
                    CancelOrder(order);
                    stopOrders.Remove(order.Name);
                }
            }
        }
        
        private void ShowMessageBox(string message)
        {
            Print("ShowMessageBox");
            Draw.TextFixed(this, "SpreadWarning", "Bid/Ask Spread Too Wide", TextPosition.TopLeft, Brushes.Red, new SimpleFont("Arial", 16), Brushes.Transparent, Brushes.Transparent, 100);
        }

        private void HideMessageBox()
        {
            Print("HideMessageBox");
            RemoveDrawObject("SpreadWarning");
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
            
            [NinjaScriptProperty]
            [Display(ResourceType = typeof(Custom.Resource), Name="Distance For Pause Duration (ticks)", Order=4, GroupName=par1)]
            public int DistanceForPauseDuration
            { get; set; }
            
            [NinjaScriptProperty]
            [Display(ResourceType = typeof(Custom.Resource), Name = "Maximum Bid/Ask Spread", Order = 5, GroupName=par1)]
            public double MaxSpread { get; set; }
    
            [NinjaScriptProperty]
            [Display(ResourceType = typeof(Custom.Resource), Name = "Quantity", Order = 6, GroupName=par1)]
            public int Quantity { get; set; }
    
            #endregion
            
        #endregion
    }
}
