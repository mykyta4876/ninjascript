namespace NinjaTrader.NinjaScript.Strategies.Givanne
{
	public class Configurator
	{
		public string InstanceId { get; set; }
	    public string DataSeries { get; set; }
	    public bool IsPrint { get; set; }
	    public bool IsInSession { get; set; }
	    public bool MoveStopLoss { get; set; }
	    public bool EnableTrailing { get; set; }
	}
}