## NinjaTrader Strategy Proposal

### Strategy Overview:

1. **Mark London Session**: Define the high and low points of the London session, which runs from 2:00 AM to 8:59 AM EST.

2. **Entry Orders**:
    - **Buy Limit**: Place a buy limit order at the London session high.
    - **Sell Limit**: Place a sell limit order at the London session low.

3. **Fair Value Gap Orders**:
    - **Buy Limit**: Place another buy limit order if the price forms a 3-candle pattern where the high of the first candle and the low of the third candle do not touch (Fair Value Gap).
    - **Sell Limit**: Place another sell limit order if the price forms a 3-candle pattern where the low of the first candle and the high of the third candle do not touch (Fair Value Gap).

4. **Variables**:
    - **Take Profit**: Default set to 20 ticks.
    - **Stop Loss**: Default set to 30 ticks below the entry point.

5. **Add-ons**:
    - Three buttons at the top right corner of the platform to:
        1. Enable the strategy to only look for "Long Orders".
        2. Enable the strategy to only look for "Short Orders".
        3. Close all orders.

6. create a license for this project.

Note: We have code samples for fair value gap function and buttons if you need it 
---

This strategy will help automate trading decisions based on the predefined rules and patterns observed during the London session.
