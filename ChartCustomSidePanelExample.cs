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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class ChartCustomSidePanelExample : Indicator
	{
		private System.Windows.Controls.Button		button1, button2;
		private System.Windows.Controls.Grid		buttonGrid, chartGrid;
		private NinjaTrader.Gui.Chart.ChartTab		chartTab;
		private Gui.Chart.ChartTrader				chartTrader;
		private int									chartTraderStartColumn;
		private NinjaTrader.Gui.Chart.Chart			chartWindow;
		private bool								panelActive;	
		private System.Windows.Controls.TabItem		tabItem;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"Enter the description for your new custom Indicator here.";
				Name						= "ChartCustomSidePanelExample";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= true;
				DisplayInDataBox			= false;
				DrawOnPricePanel			= true;
				IsSuspendedWhileInactive	= true;
			}
			else if (State == State.DataLoaded)
			{
				ClearOutputWindow();
				panelActive = false;
			}
			else if (State == State.Historical)
			{
				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync((Action)(() =>
					{
						CreateWPFControls();
					}));
				}
			}
			else if (State == State.Terminated)
			{
				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync((Action)(() =>
					{
						DisposeWPFControls();
					}));
				}
			}
		}

		protected void Button1_Click(object sender, RoutedEventArgs e)
		{
			Draw.TextFixed(this, "infobox", "Button 1 clicked", TextPosition.BottomLeft, Brushes.Green, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			ChartControl.InvalidateVisual();
		}

		protected void Button2_Click(object sender, RoutedEventArgs e)
		{
			Draw.TextFixed(this, "infobox", "Button 2 clicked", TextPosition.BottomLeft, Brushes.DarkRed, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			ChartControl.InvalidateVisual();
		}

		protected void CreateWPFControls()
		{
			chartWindow			= System.Windows.Window.GetWindow(ChartControl.Parent) as Chart;
			chartGrid			= chartWindow.MainTabControl.Parent as System.Windows.Controls.Grid;
			chartTrader			= chartWindow.FindFirst("ChartWindowChartTraderControl") as Gui.Chart.ChartTrader;

			buttonGrid = new System.Windows.Controls.Grid();

			buttonGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition() { Height = new GridLength(50) });
			buttonGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition() { Height = new GridLength(50) });
			buttonGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition() { Height = new GridLength(50) });

			System.Windows.Controls.TextBlock label = new System.Windows.Controls.TextBlock()
			{
				FontFamily			= ChartControl.Properties.LabelFont.Family,
				FontSize			= 13,
				Foreground			= ChartControl.Properties.ChartText,
				HorizontalAlignment	= HorizontalAlignment.Center,
				Margin				= new Thickness(5, 5, 5, 5),
				Text				= string.Format("{0} {1} {2}", Instrument.FullName, BarsPeriod.Value, BarsPeriod.BarsPeriodType)
			};

			System.Windows.Controls.Grid.SetRow(label, 0);
			buttonGrid.Children.Add(label);

			button1 = new System.Windows.Controls.Button()
			{
				Content				= "Button 1",
				HorizontalAlignment	= HorizontalAlignment.Center
			};
			button1.Click += Button1_Click;

			System.Windows.Controls.Grid.SetRow(button1, 1);
			buttonGrid.Children.Add(button1);

			button2 = new System.Windows.Controls.Button()
			{
				Content				= "Button 2",
				HorizontalAlignment	= HorizontalAlignment.Center
			};
			button2.Click += Button2_Click;

			System.Windows.Controls.Grid.SetRow(button2, 2);
			buttonGrid.Children.Add(button2);

			if (TabSelected())
				InsertWPFControls();

			chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
		}

		private void DisposeWPFControls()
		{
			if (button1 != null)
				button1.Click -= Button1_Click;

			if (button2 != null)
				button2.Click -= Button2_Click;

			if (chartWindow != null)
				chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;

			RemoveWPFControls();
		}

		private void InsertWPFControls()
		{
			if (panelActive)
				return;

			chartTraderStartColumn = System.Windows.Controls.Grid.GetColumn(chartTrader);

			// a new column is added to the right of ChartTrader
			chartGrid.ColumnDefinitions.Insert((chartTraderStartColumn + 2), new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(125) });

			// all items to the right of the ChartTrader are shifted to the right
			for (int i = 0; i < chartGrid.Children.Count; i++)
				if (System.Windows.Controls.Grid.GetColumn(chartGrid.Children[i]) > chartTraderStartColumn)
					System.Windows.Controls.Grid.SetColumn(chartGrid.Children[i], System.Windows.Controls.Grid.GetColumn(chartGrid.Children[i]) + 1);

			// and then we set our new grid to be within the new column of the chart grid (and on the same row as the MainTabControl)
			System.Windows.Controls.Grid.SetColumn(buttonGrid, System.Windows.Controls.Grid.GetColumn(chartTrader) + 2);
			System.Windows.Controls.Grid.SetRow(buttonGrid, System.Windows.Controls.Grid.GetRow(chartWindow.MainTabControl));

			chartGrid.Children.Add(buttonGrid);

			// let the script know the panel is active
			panelActive = true;
		}

		protected override void OnBarUpdate() { }

		private void RemoveWPFControls()
		{
			if (!panelActive)
				return;

			// remove the column of our added grid
			chartGrid.ColumnDefinitions.RemoveAt(System.Windows.Controls.Grid.GetColumn(buttonGrid));
			// then remove the grid
			chartGrid.Children.Remove(buttonGrid);

			// if the childs column is 1 (so we can move it to 0) and the column is to the right of the column we are removing, shift it left
			for (int i = 0; i < chartGrid.Children.Count; i++)
				if ( System.Windows.Controls.Grid.GetColumn(chartGrid.Children[i]) > 0 && System.Windows.Controls.Grid.GetColumn(chartGrid.Children[i]) > System.Windows.Controls.Grid.GetColumn(buttonGrid) )
					System.Windows.Controls.Grid.SetColumn(chartGrid.Children[i], System.Windows.Controls.Grid.GetColumn(chartGrid.Children[i]) - 1);

			panelActive = false;
		}

		private bool TabSelected()
		{
			bool tabSelected = false;

			// loop through each tab and see if the tab this indicator is added to is the selected item
			foreach (System.Windows.Controls.TabItem tab in chartWindow.MainTabControl.Items)
				if ((tab.Content as ChartTab).ChartControl == ChartControl && tab == chartWindow.MainTabControl.SelectedItem)
					tabSelected = true;

			return tabSelected;
		}

		private void TabChangedHandler(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count <= 0)
				return;

			tabItem = e.AddedItems[0] as System.Windows.Controls.TabItem;
			if (tabItem == null)
				return;

			chartTab = tabItem.Content as NinjaTrader.Gui.Chart.ChartTab;
			if (chartTab == null)
				return;

			if (TabSelected())
				InsertWPFControls();
			else
				RemoveWPFControls();
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ChartCustomSidePanelExample[] cacheChartCustomSidePanelExample;
		public ChartCustomSidePanelExample ChartCustomSidePanelExample()
		{
			return ChartCustomSidePanelExample(Input);
		}

		public ChartCustomSidePanelExample ChartCustomSidePanelExample(ISeries<double> input)
		{
			if (cacheChartCustomSidePanelExample != null)
				for (int idx = 0; idx < cacheChartCustomSidePanelExample.Length; idx++)
					if (cacheChartCustomSidePanelExample[idx] != null &&  cacheChartCustomSidePanelExample[idx].EqualsInput(input))
						return cacheChartCustomSidePanelExample[idx];
			return CacheIndicator<ChartCustomSidePanelExample>(new ChartCustomSidePanelExample(), input, ref cacheChartCustomSidePanelExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ChartCustomSidePanelExample ChartCustomSidePanelExample()
		{
			return indicator.ChartCustomSidePanelExample(Input);
		}

		public Indicators.ChartCustomSidePanelExample ChartCustomSidePanelExample(ISeries<double> input )
		{
			return indicator.ChartCustomSidePanelExample(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ChartCustomSidePanelExample ChartCustomSidePanelExample()
		{
			return indicator.ChartCustomSidePanelExample(Input);
		}

		public Indicators.ChartCustomSidePanelExample ChartCustomSidePanelExample(ISeries<double> input )
		{
			return indicator.ChartCustomSidePanelExample(input);
		}
	}
}

#endregion
