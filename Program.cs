using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using CommandLine.Parsing;
using System.Globalization;
namespace TextfileValidator
{
	class Program
	{
		private static SortedList<DateTime, string> DateAndFileNames;
		
		static void Menu(string[] args)
		{
			//-f C:\Projects\Playground\Eric.Scott\TextfileValidator\bin\Debug\sampleDatafile.txt -d C:\Projects\Playground\Eric.Scott\TextfileValidator\bin\Debug\sampleDefinitionfile.txt -s "|"
		}
	
		static void Main(string[] args)
		{
			
           var options = new Options();
           if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
				DataCreator dc = new DataCreator();
				int[] col = new int[] { 19, 20, 410, 458};

				dc.WriteHistoryFile(@"C:\Projects\Playground\Eric.Scott\TextfileValidator\bin\Debug","BR_History","txt",
					@"C:\Projects\Playground\Eric.Scott\TextfileValidator\bin\Debug", "mkt_ETF_Analytics_Data", "txt", 400, col, null);
					

                Console.WriteLine("first was {0}", options.SampleDataGenArgs[0]);
                Console.ReadLine();
                Environment.Exit(0);
            }
		   Environment.Exit(0);

           // CreateDatesAndFileNames(10);
			string baseFileName = @"C:\Projects\Playground\Eric.Scott\TextfileValidator\bin\Debug\BR_History.txt";
			string ISIN = "GB00B15KY328";
			//string targetFileName = GetTargetFileName(1);
			DateTime asOfDate = DateTime.Now;
			CreateFirstHistoryFile(baseFileName, ISIN, DateAndFileNames.Last().Value, DateAndFileNames.Last().Key);

			for (int i = DateAndFileNames.Count - 1; i > 0 ; i--)
			{
				CreateFirstHistoryFile(DateAndFileNames.ElementAt(i).Value, ISIN, DateAndFileNames.ElementAt(i - 1).Value, DateAndFileNames.ElementAt(i -1).Key);
				Console.WriteLine("Created File {0} for Date: {1}", DateAndFileNames.ElementAt(i-1).Value, DateAndFileNames.ElementAt(i-1).Key);
			}
				
			Console.ReadLine();
		}

		private static void CreateFirstHistoryFile(string baseFileName, string ISIN, string targetFileName, DateTime asOfDate)
		{
			using (System.IO.StreamWriter wfile =
						new StreamWriter(targetFileName, true))
			{
				StreamReader r = new StreamReader(baseFileName);
				string headerLine = r.ReadLine();
				string line;
				wfile.WriteLine(headerLine);

				//find ISINs I want to create Mock data for
				bool direction1 = GetRanadomDirection();
				bool direction2 = GetRanadomDirection();
				if (direction1 != direction2)
					Console.WriteLine("the two directions are not equal");
				while ((line = r.ReadLine()) != null)
				{
					string[] items = line.Split(new char[] { Char.Parse("|") });
					if (items.Contains(ISIN))
					{
						items[12] = asOfDate.ToShortDateString();
						items[19] = CreateData(double.Parse(items[19]),direction1).ToString();
						items[20] = CreateData(double.Parse(items[20]), direction2).ToString();
						// clearing out all other data.
						for (int i = 21; i < items.Length; i++)
							items[i] = string.Empty;

						StringBuilder sb = new StringBuilder(line.Length);
						foreach (string item in items)
							sb.Append(item + "|");
						sb.Remove(sb.Length - 1, 1);

						wfile.WriteLine(sb.ToString());

					}
				}
			}
		}

		private static bool GetRanadomDirection()
		{
			Random gen = new Random();
			int prob = gen.Next(100);
			bool increase = prob % 2 == 0;
			return increase;

		}


		private static void CreateHistory(string ISIN, string filename)
		{
			Random gen = new Random();
			int prob = gen.Next(100);
			bool increase = prob <= 49;

			StreamReader r = new StreamReader(@"C:\Projects\Playground\Eric.Scott\TextfileValidator\bin\Debug\BR_History.txt");
			string headerLine = r.ReadLine();
			string[] headers = headerLine.Split(new char[] { Char.Parse("|") });

			string line; //List<string> writethese = new List<string>();
			
			Console.WriteLine("writing file {0}", filename);
			Console.WriteLine("Modifing data in this direction: {0}", increase);
			using (System.IO.StreamWriter wfile =
						new StreamWriter(filename, true))
			{
				wfile.WriteLine(headerLine);
				while ((line = r.ReadLine()) != null)
					{
						string[] items = line.Split(new char[] { Char.Parse("|") });
						if (items.Contains(ISIN))
						{
							Console.WriteLine("found ISIN {0} in file",ISIN);
							Console.WriteLine(items[2]);
							items[19] = CreateData(double.Parse(items[19]), increase).ToString();
							StringBuilder sb = new StringBuilder(line.Length);

							//string writethisline = string.Empty;
							foreach (string items2 in items)
								sb.Append(items2 + "|");
							sb.Remove(sb.Length - 1, 1);
					
							wfile.WriteLine(sb.ToString());
						}
					}
				}
			
		}

		private static double CreateData(double original, bool increase)
		{
			
			double updated;
			if (increase)
				updated = original * 1.1;
			else
				updated = original * .90;
			return updated;

		}

		private static bool IsWeekend(DateTime dt)
		{
			if (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday)
				return true;
			return false;
		}

		//private static void ValidateFile(string[] args)
		//{
		//	Validator v = new Validator();
		//	System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
		//	var options = new Options();
		//	if (CommandLine.Parser.Default.ParseArguments(args, options))
		//	{
		//		timer.Start();
		//		Console.WriteLine("Reading file {0} delimiting by {1}", options.InputFile, Char.Parse(options.Delimiter));
		//		Console.WriteLine();
		//		string line;
		//		string[] items;
		//		Int64 lineCount = 0;
		//		Int64 itemCount = 0;
		//		ItemDefinition def = new ItemDefinition();
		//		int trues = 0;
		//		int falses = 0;
		//		bool result;
		//		TimeSpan ts = new TimeSpan();
		//		StreamReader r2 = new StreamReader(options.DefinitionFilePath);
		//		string line2 = r2.ReadLine();
		//		r2.Close();
		//		List<ItemDefinition> types = line2.Split(new char[] { Char.Parse(options.Delimiter) }).Select(type => new ItemDefinition(type)).ToList();
		//		int numTypes = types.Count();
		//		StreamReader r = new StreamReader(options.InputFile);

		//		int iStartRow;
		//		if (Int32.TryParse(options.StartRow, out iStartRow))
		//		{
		//			while (iStartRow > 0)
		//			{
		//				r.ReadLine();
		//				iStartRow--;
		//			}
		//		}

		//		while ((line = r.ReadLine()) != null)
		//		{
		//			lineCount++;
		//			items = line.Split(new char[] { Char.Parse(options.Delimiter) });
		//			if (items.Count() != numTypes)
		//			{
		//				Console.WriteLine("ERROR:  expected more or less items in line # {0}; expected {1}, got {2}", lineCount, numTypes, items.Count());
		//			}

		//			for (int i = 0; i < numTypes; i++)
		//			{
		//				result = v.IsValid(items[i], types[i]); // takes about 150 ticks to do this.  threading overhead will not speed ths up
		//				if (result)
		//					trues++;
		//				else
		//				{
		//					Console.WriteLine("failure:  row #{0}  item#{1} value: {2} is not {3}", lineCount, i, items[i],types[i].originalType);
		//					Console.WriteLine("the line is");
		//					Console.WriteLine(line);//r.Close();
		//					falses++;
		//				}
		//				//Console.WriteLine("Item: {0} validation test is: {1} for type {2}", items[i], v.IsValid(items[i], def), types[i]);
		//				itemCount++;
		//			}
		//		}
		//		ts = timer.Elapsed;
		//		timer.Stop();
		//		r.Close();
		//		Console.WriteLine("Validation completed");
		//		Console.WriteLine("Line Count: {0}\nItem Count: {1}\nPasses: {2}\nFailures: {3}\nRun time: {4}", lineCount, itemCount, trues.ToString(), falses.ToString(),
		//					String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds));
		//		Console.WriteLine("Validation answer: {0}", falses == 0);
		//	}
		//	else
		//	{
		//		Console.WriteLine("you are missing required arguments");
		//	}
		//	Console.ReadLine();
		//}

	}

	
	class MainOptions
	{
		[Option('c', "datafilepath", Required = true, HelpText = "Show the Headers of the file")]
		public string function { get; set; }
		[HelpOption]
		public string GetUsage()
		{
			var help = new HelpText
			{
				Heading = new HeadingInfo("<>", "<>"),
				Copyright = new CopyrightInfo("<>", 2017),
				AdditionalNewLineAfterOption = false,
				AddDashesToOption = true
			};

			help.AddPreOptionsLine("<>");
			help.AddPreOptionsLine("Usage: app -pSomeone");
			help.AddOptions(this);
			return help;
		}

	}

	class Options
	{
		[OptionArray('c', "sampledataArgs", Required = false, HelpText = "Input file | number of days of samplen")]
		public string[] SampleDataGenArgs { get; set; }

		[Option('f', "datafilepath", Required = false, HelpText = "The TEXT data file to be validated")]
		public string InputFile { get; set; }

		[Option('d', "defintionfilepath", Required = false, HelpText = "the definition file to be validated")]
		public string DefinitionFilePath { get; set; }

		[Option('r', "startrow", Required = false, HelpText = "What row of the file does the data start on.")]
		public string StartRow { get; set; }

		[Option('s', "delimiter", Required = false, HelpText = "the Delimiter or separator in the file. Note | need to be in quotes")]
		public string Delimiter { get; set; }

		//[Option('v', "verbose", DefaultValue = true, HelpText = "Prints all the messages to standard output.")]
		//public bool Verbose { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			var help = new HelpText
			{
				Heading = new HeadingInfo("<>", "<>"),
				Copyright = new CopyrightInfo("<>", 2017),
				AdditionalNewLineAfterOption = false,
				AddDashesToOption = true
			};

			help.AddPreOptionsLine("<>");
			help.AddPreOptionsLine("Usage: app -pSomeone");
			help.AddOptions(this);
			return help;
		}
	}
}
