#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core;
using NinjaTrader.NinjaScript.Indicators;
#endregion

namespace NinjaTrader.NinjaScript.Strategies
{
    public class StrategieVWAPCroise : Strategy
    {
        private Series<double> vwapHaut;
        private Series<double> vwapBas;
        private double sessionHigh;
        private double sessionLow;
        private double cumulVolumeHaut;
        private double cumulVolumeBas;
        private double cumulVwapHaut;
        private double cumulVwapBas;
        private bool vwapInitialises;
        private DateTime currentSessionDate;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Stratégie basée sur le croisement des VWAP cumulés ancrés sur les deux derniers extrêmes de la session en cours";
                Name = "Strategie VWAP Croise Cumules";
                Calculate = Calculate.OnBarClose;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;
                IsFillLimitOnTouch = false;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
                Slippage = 0;
                StartBehavior = StartBehavior.WaitUntilFlat;
                TimeInForce = TimeInForce.Day;
                TraceOrders = false;
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 20;

                AddPlot(new Stroke(Brushes.Blue, 2), PlotStyle.Line, "VWAP Haut");
                AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Line, "VWAP Bas");
            }
            else if (State == State.DataLoaded)
            {
                vwapHaut = new Series<double>(this);
                vwapBas = new Series<double>(this);
                vwapInitialises = false;
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < BarsRequiredToTrade)
                return;

            DateTime currentBarDate = Times[0][0].Date;

            // Check if a new session has started
            if (currentSessionDate != currentBarDate)
            {
                // New session detected, initialize session variables
                currentSessionDate = currentBarDate;
                sessionHigh = High[0];
                sessionLow = Low[0];
                InitialiserVWAP();
                return;
            }

            double typicalPrice = (High[0] + Low[0] + Close[0]) / 3;
            double volume = Volume[0];

            // Update cumulative VWAP calculations
            cumulVolumeHaut += volume;
            cumulVolumeBas += volume;
            cumulVwapHaut += typicalPrice * volume;
            cumulVwapBas += typicalPrice * volume;

            vwapHaut[0] = cumulVwapHaut / cumulVolumeHaut;
            vwapBas[0] = cumulVwapBas / cumulVolumeBas;

            // Display VWAPs on the chart
            Values[0][0] = vwapHaut[0];  // VWAP Haut
            Values[1][0] = vwapBas[0];   // VWAP Bas

            // Trading logic
            if (CrossAbove(vwapHaut, vwapBas, 1))
            {
                EnterShort();
                SetStopLoss(CalculationMode.Price, sessionHigh);
                SetProfitTarget(CalculationMode.Price, sessionLow);
            }
            else if (CrossBelow(vwapHaut, vwapBas, 1))
            {
                EnterLong();
                SetStopLoss(CalculationMode.Price, sessionLow);
                SetProfitTarget(CalculationMode.Price, sessionHigh);
            }

            // Update session extremes
            if (High[0] > sessionHigh)
            {
                sessionHigh = High[0];
                ReinitialiserVWAPHaut(typicalPrice, volume);
            }
            if (Low[0] < sessionLow)
            {
                sessionLow = Low[0];
                ReinitialiserVWAPBas(typicalPrice, volume);
            }
        }

        private void InitialiserVWAP()
        {
            // Initialize VWAP values at the start of a new session
            double typicalPrice = (High[0] + Low[0] + Close[0]) / 3;
            double volume = Volume[0];

            cumulVolumeHaut = volume;
            cumulVolumeBas = volume;
            cumulVwapHaut = typicalPrice * volume;
            cumulVwapBas = typicalPrice * volume;

            vwapHaut[0] = typicalPrice;
            vwapBas[0] = typicalPrice;

            vwapInitialises = true;
        }

        private void ReinitialiserVWAPHaut(double typicalPrice, double volume)
        {
            // Reinitialize the VWAP calculation for the high
            cumulVolumeHaut = volume;
            cumulVwapHaut = typicalPrice * volume;
            vwapHaut[0] = typicalPrice;
        }

        private void ReinitialiserVWAPBas(double typicalPrice, double volume)
        {
            // Reinitialize the VWAP calculation for the low
            cumulVolumeBas = volume;
            cumulVwapBas = typicalPrice * volume;
            vwapBas[0] = typicalPrice;
        }
    }
}
