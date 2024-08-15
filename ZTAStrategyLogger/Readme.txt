We are seeking a highly skilled NinjaScript developer to enhance our existing NinjaTrader 8 script, the ZTAStrategyLogger. The main goal is to ensure that all trades are logged correctly and consistently, regardless of whether they are exited manually or automatically. The developer will need to ensure accurate and comprehensive trade history logging each day, with the data written to the CSV file matching the data in NinjaTrader.

The script is already working fine and logging trades, we just want it do the following

1) Log the trades from each trading day and ensure accuracy and parity with NinjaTrader's trade performance metrics
2) If the user manually exits an automated trade, the Pnl of the trade as well as the other data still gets logged. This might involve using OnExecutionUpdate as well as OnPositionUpdate.

Responsibilities:
Ensure Comprehensive Trade Logging:

Verify that all trades executed by the strategy are logged correctly to a CSV file.
Identify and resolve any issues where trades might be missing from the log.

Manual Exit Tracking:
Implement functionality to ensure that the PnL of trades initiated by the strategy is logged even when trades are exited manually.
This may involve using the OnExecutionUpdate method instead of or in addition to the OnPositionUpdate method.

Accurate Daily Trade History:
Ensure that the trade history logged each day is accurate and complete.
Verify that the data written to the CSV file matches the trade data in NinjaTrader.

Requirements:
Proven experience with NinjaScript and NinjaTrader 8.
Strong understanding of trade execution, position management, and event handling in NinjaTrader.
Experience with file I/O operations in C# for logging and reporting purposes.
Attention to detail to ensure data accuracy and consistency.
Excellent problem-solving skills and ability to troubleshoot complex issues.

Deliverables:
Updated ZTA StrategyLogger with enhanced logging functionality.
Documentation outlining the changes made and instructions for use.
Verification of accurate and consistent trade logging for both automated and manually exited trades.

How to Apply:
If you have the necessary skills and experience, please apply with:
A brief summary of your experience with NinjaScript and NinjaTrader 8.
Examples of previous work or projects related to trade execution and logging.
An estimate of how long you expect the task to take and your proposed rate.
We look forward to working with an experienced developer to improve the reliability and accuracy of our trade logging system.


1) Keep the strategy name exactly the same. We use this as a base class for all our other strategies
2) Keep any file names and folder paths the same, we build another piece of software that reads from these files
3) We simply want to make sure that the header gets written to the file only once, that each strategy's PnL is successfully written to the file after the trade exits, and we want to see if it's possible to still log the trade that was opened by a strategy, even if the exit was manual

4) One more thing, can you print the account balance to the output logs after each trade ?

And I asked on the NinjaTrader support forums about the manual exit tracking, here’s what they said:
https://forum.ninjatrader.com/node/1310451


I added pnl and balance.
I handled the case of partial fills.

So the ZTAStrategyLogger strategy does the following

1) Gets an update each time a strategy trade finishes, in OnPositionUpdate
2) Logs some information about the trade to a CSV file
I want you to improve this logic so that even if the user manual exits the trade that was started by the strategy, the PnL still gets written
you should log the trade when you right click on the account in Control Center and select “Close all selected account positions”


I also want you to handle the case of partial fills. Strategy makes 1 order to buy 2 contracts. Then, when exiting that long trade, it exits 1 contract each in 2 orders. We still want to log the correct total PnL for that trade, and not just one of the orders


==========================================
1) It's not impossible. I found the original code I used and it's working again, but I would like you to check this for me in the exact way that I need

2) So it's working now with this code I'm about to share below. I will send you 2 files. One is TradeLogger.cs which is the BASE strategy class that has the CSV writing logic. The other is TradeLoggerExample.cs which is a strategy that extends TradeLogger, does that make sense?

3) I want you to test running TradeLoggerExample from the Strategies tab, not from the chart.

The only thing is, you will have to manually create the directory that the CSV file is writing to

Remember again, the strategy you are testing is TradeLoggerExample, NOT TradeLogger. That is just the base logic

Let me know if it works for you from the Strategies tab, and what you think is happening in the code that didn't make it work with ZTAStrategyLogger
