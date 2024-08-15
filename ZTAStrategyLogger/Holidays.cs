#region Using declarations
using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
#endregion

namespace NinjaTrader.NinjaScript.Strategies
{
    public class Holidays
    {
		List<DateTime> holidays;

        public Holidays(string holidays_file)
        {
			holidays = new List<DateTime>();

			if ( File.Exists(holidays_file) ) {
				string[] lines = File.ReadAllLines(holidays_file);

				if ((lines != null) && (lines.Length > 0)) {
					foreach (string line in lines) {
						if ( string.IsNullOrEmpty(line) )
							continue;

//						string[] columns = line.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
//						if ((columns != null) && (columns.Length > 1)) {
							try {
//								holidays.Add(DateTime.ParseExact(columns[0], "yyyyMMdd", CultureInfo.InvariantCulture));
								holidays.Add(DateTime.ParseExact(line, "yyyyMMdd", CultureInfo.InvariantCulture));
							}
							catch {}
//						}
					}
				}
			}
        }

        public bool IsHoliday(DateTime t)
        {
			if (holidays == null)
				return false;

			return holidays.Contains(t.Date);
        }
    }
}
