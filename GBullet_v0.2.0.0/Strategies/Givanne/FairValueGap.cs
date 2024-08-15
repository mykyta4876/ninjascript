using System;

namespace NinjaTrader.NinjaScript.Strategies
{
    public enum FairValueGapType
    {
        Resistance,
        Support
    }
    
    public class FairValueGap
    {
        private double upperPrice, lowerPrice, threshold;
        private string tag;
        private FairValueGapType type;
        private bool filled;
        private DateTime gapStartTime;
        private DateTime fillTime;
        
        public FairValueGap(){}
        
        public double UpperPrice
        {
            get { return upperPrice; }
            set { upperPrice = value; }
        }

        public double LowerPrice
        {
            get { return lowerPrice; }
            set { lowerPrice = value; }
        }

        public double Threshold
        {
            get { return threshold; }
            set { threshold = value; }
        }

        public string Tag
        {
            get { return tag; }
            set { tag = value; }
        }

        public FairValueGapType Type
        {
            get { return type; }
            set { type = value; }
        }

        public bool Filled
        {
            get { return filled; }
            set { filled = value; }
        }

        public DateTime GapStartTime
        {
            get { return gapStartTime; }
            set { gapStartTime = value; }
        }

        public DateTime FillTime
        {
            get { return fillTime; }
            set { fillTime = value; }
        }
    }
    
}