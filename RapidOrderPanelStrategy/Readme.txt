
I want to create new strategy with 2 strategies. (RapidLimitOrderEntryStrategy, ButtonPanel)

RapidLimitOrderEntryStrategy has follow functions:


it can help you to trade easily with ATM.
if you click any point in the chart, open a trade with the price that you clicked.
- in case SetOrderMethod is SpaceMouseLeftClick
	if clicking mouse left  and pressing spacebar, open a trade with SL.

- in case SetOrderMethod is SpaceMouseOver
	set entry order with only pressing spacebar.

- if pressing leftalt and clicking left, close all trades
	can't use middle mouse button.

- If you do Shift+Space+Click, place entry with double size.
	Here, when position is long, point of Click should be above market price.

- add a lockout period where new trades cannot be placed for some amount of time after the previous trade closed
	Default is 2s (PauseDuration)

- I'd like to have functionality that I can enable/disable that automatically places a stop limit order a configurable distance away from each open trade.
	(DistanceForPauseDuration)


- User configurable maximum bid/ask spread - if the bid/ask spread is wider than the maximum allowed, a large message box shows up on the left side of the screen that says "Bid/Ask Spread Too Wide". And if a long trade is on, only a limit sell can be placed. If a short trade is on, on a limit buy can be placed. If no trades are on, no new limit orders can be placed until the spread narrows below the threshold.
bid/ask spread is distance between bid and ask
long trade = long position
(MaxSpread)

- Trailing Stoploss
	TrailingStopTick: Add a configurable move stop limit to breakeven after some number of ticks in profit (default is zero which means this function is disabled)

	TrailingStopType: has two options- Standard, Halfway.
	Standard: When the price takes a 100-tick profit, the stop limit moves to the break-even point and then asks you to remain 100 ticks away from the maximum profit point from that point until now. For example, if profit reaches 150 tick profit, the stop limit will be 50 tick profit. 
	Halfway: in the same scenario, when price goes 100 ticks in profit, the stop limit moves to 50 ticks in profit and then tracks halfway between entry and max profit so far. If profit reaches 150 ticks, the stop limit would be at 75 ticks profit.

so, I want to add these functions of RapidLimitOrderEntryStrategy into ButtonPanel.
Name of new strategy is "RapidOrderPanelStrategy"
	

======================
If you click with LeftAlt, open a trade without SL.
