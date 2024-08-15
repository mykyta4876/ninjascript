using System;

namespace NinjaTrader.NinjaScript.Strategies.Givanne
{
	public class TimeInfo
	{
		public DateTime Future { get; set; }
	    public DateTime EstTime { get; set; }
	    public TimeZoneInfo EstZone { get; set; }
	}
}