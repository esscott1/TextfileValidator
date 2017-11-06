using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyprTextToolLib
{
    public class SeparateFiles
    {
		public string[] sFilenames { get; set; }


		private List<string> GetLastTradeDayOfMonth()
		{
			int count = 0;
			List<string> lastdaysofmonth = new List<string>();
			DateTime today = DateTime.Today;
			int Month = today.Month;
			int Year = today.Year;
			for (int y = 2000; y < 2018; y++)
			{
				for (int m = 1; m < 13; m++)
				{
					DateTime endOfMonth = new DateTime(y,
										   m,
										   DateTime.DaysInMonth(y,
																m));

					while (endOfMonth.DayOfWeek == DayOfWeek.Saturday || endOfMonth.DayOfWeek == DayOfWeek.Sunday)
						endOfMonth = endOfMonth.AddDays(-1);
					lastdaysofmonth.Add(endOfMonth.ToString(@"yyyMMdd"));
					//	Console.WriteLine("last day of month number {0} is {1}", endOfMonth.Month, endOfMonth.ToString(@"yyyMMdd"));
					count = count + 1;
				}
			}
			Console.WriteLine("found {0} last trading days", count);
			return lastdaysofmonth;

		}

    }
}
