using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;

namespace TextfileValidator
{
	class DataCreator
	{
		public string TemplateFile { get; set; }
		public string KeyColumnName { get; set; }
		public int DaysOfHistory { get; set; }
		public string HistoryFileName { get; set; }
		public double ChangePercent { get; set; }
		private SortedList<DateTime, string> DateAndFileNames { get; set; }

		public DataCreator() { }

		private static bool GetRanadomDirection()
		{
			Random gen = new Random();
			int prob = gen.Next(100);
			bool increase = prob % 2 == 0;
			return increase;

		}

		public void WriteHistoryFile(string BaseFilePath, string BaseFileName, string BasefileType,
			string TargetFilePath, string TargetFileName, string TargetFileType,
			int DayOfHistory, int[] HistoryForColumnNo, KeyValuePair<int, string>? subsetOn)
		{
			SortedList<DateTime, string> filesNames = CreateDatesAndFileNames(DayOfHistory, TargetFilePath, TargetFileName, TargetFileType);
			//@"C:\Projects\Playground\Eric.Scott\TextfileValidator\bin\Debug",
			//"eBR_History", "txt");

			// adding in today's date with the Base file
			filesNames.Add(DateTime.Now, BaseFilePath + @"\" + BaseFileName + "." + BasefileType);

			// create history
			for (int i = filesNames.Count - 1; i > 0; i--)
			{
				CreateHistoryFile(filesNames.ElementAt(i).Value, filesNames.ElementAt(i - 1).Value, filesNames.ElementAt(i - 1).Key, HistoryForColumnNo);
				Console.WriteLine("\r{0} of files left to create", i);
			}
		}
		
		private bool IsANumber(string value, out double iValue)
		{
			return double.TryParse(value, out iValue);
		}

		private void CreateHistoryFile(string baseFileName, string targetFileName, DateTime asOfDate, int [] HistoryForColumnNo)
		{
			using (System.IO.StreamWriter wfile =
						new StreamWriter(targetFileName, true))
			{
				StreamReader r = new StreamReader(baseFileName);
				string headerLine = r.ReadLine();
				string line;
				wfile.WriteLine(headerLine);

				//find ISINs I want to create Mock data for
				while ((line = r.ReadLine()) != null)
				{
					string[] items = line.Split(new char[] { Char.Parse("|") });
					//if (items.Contains("GB00B15KY328"))
					//{
						items[12] = asOfDate.ToShortDateString();
						for (int j = 0; j < items.Length; j++)
						{
							if (HistoryForColumnNo.Contains(j))
							{
								items[j] = GetHistoricalValue(items[j], .1, GetRanadomDirection());
							}
							else if (j > 20)
							{
								items[j] = string.Empty;
							}
						}
						//items[12] = asOfDate.ToShortDateString();
						
						//items[19] = GetHistoricalValue(items[19], .1, true);
						//items[20] = GetHistoricalValue(items[20], .1, true);
						//// clearing out all other data.
						//for (int i = 21; i < items.Length; i++)
						//	items[i] = string.Empty;

						StringBuilder sb = new StringBuilder(line.Length);
						foreach (string item in items)
							sb.Append(item + "|");
						sb.Remove(sb.Length - 1, 1);

						wfile.WriteLine(sb.ToString());
					//}
					
				}
			}
		}
		
		

		private string GetHistoricalValue(string value, double changePercent, bool changeUp)
		{
			ChangePercent = changePercent;
			double iValue; string result;
			if(IsANumber(value, out iValue))
			{
				if (changeUp)
					result = (iValue * (1 + changePercent)).ToString();
				else
					result = (iValue * (1 - changePercent)).ToString();
				return result;
			}
			return value;
		}

		private SortedList<DateTime, string> CreateDatesAndFileNames(int DaysInPast, string FilePath, string FileName, string fileType)
		{
			SortedList<DateTime, string> dateAndFileNames = new SortedList<DateTime, string>();
			
			System.Globalization.Calendar cal = CultureInfo.InvariantCulture.Calendar;
			SortedList<int, string> sDates = new SortedList<int, string>();
			int d = 1; int fileCount = 0;
			while (fileCount < DaysInPast)
			{
				DateTime dt = DateTime.Now.AddDays(-d);
				d++;
				if (cal.GetDayOfWeek(dt) != DayOfWeek.Sunday && (cal.GetDayOfWeek(dt) != DayOfWeek.Saturday))
				{
					string sDate = dt.ToString("yyyyMMdd");
					string fileName = FilePath + @"\"+ FileName +"_" + sDate + "." + fileType;
					dateAndFileNames.Add(dt, fileName);
					fileCount++;
				}
			}
			return dateAndFileNames;
		}

	}
}
