
I think ButtonPanel has good functionalities.
1. set limit order with trailing stop. 
2. custom quantity
we can refer these.


I don't think ButtonPanel has trailing stoploss.
it used ATM for setting static SL and TP, so we don't need ATM.
I found a way for trailing stoploss without error through this strategy.
And Unlike my code, this strategy uses Account class.
I think it's useful for us.

And this strategy support single trade, not multi trades.


===========================
Hello - I'm currently bidding out another project you might consider. As you'll see, it will look similar to a strategy I shared while you were developing our last project but this version does not include the cumulative depth.

I am seeking two modifications to the attached NinjaTrader strategy:
Add a "Counter Trend" Mode:
Implement a new "Counter Trend" mode of behavior that is accessed by toggling the existing 2-state pushbutton between "Trend" and "Counter Trend."
The new "Counter Trend" mode should have the following order placement logic:
- Buy orders: Placed at an Offset number of ticks below the low of the current bar (unlike "Trend" mode, which places buy orders Offset ticks above the low of the current or previous bar).
- Sell orders: Placed at an Offset number of ticks above the high of the current bar (unlike "Trend" mode, which places sell orders Offset ticks below the high of the current or previous bar).
Ensure the new "Counter Trend" mode shares the following functionalities with "Trend" mode:
- Orders are placed using the ATM Strategy specified in the button panel strategy parameters.
- The Breakeven, Cancel, and Exit buttons retain their existing behavior.

Fix Workspace Save Issue:
Resolve the current issue where the button panel strategy cannot be saved as part of a workspace. After the fix, the strategy and its settings should persist across workspace saves and reloads.

Trend mode is the way the strategy behaves currently - for example clicking "BUY" will place a buy stop limit order at a price that is Offset ticks above the low of the current bar. Would like to add "Counter Trend" mode which places orders in the same places but in opposite direction. For example, a Counter Trend Sell would place a sell limit order that is Offset ticks above the low of the current bar - same place as a buy order is placed in Trend mode but this is a sell order instead of a buy order.

Currently, the strategy does not save with the workspace. Other strategies able to be saved with workspaces and I'd like to know if this strategy can be modified to be able to be saved to workspaces as well.
