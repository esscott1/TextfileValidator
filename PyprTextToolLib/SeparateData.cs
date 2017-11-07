using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PyprTextToolLib
{
	public class SeparateData
	{
		public string OutputDirectory { get; set; }
		public string[] sFileNames { get; set; }
		public delegate void Del(string message);
		private Dictionary<string, string> dTableMap;
		private bool UseParallelForEach { get; set; }

		private Dictionary<string, List<string>> FileLineCache { get; set; }

		public SeparateData(string outputDirectory, string[] fileNames, bool? useParallelForEach = false)
		{
			if (useParallelForEach == null)
				useParallelForEach = false;
			UseParallelForEach = useParallelForEach.Value;
			dTableMap = new Dictionary<string, string>();
			FileLineCache = new Dictionary<string, List<string>>();
			OutputDirectory = outputDirectory;
			sFileNames = fileNames;
			InitDictionary(dTableMap);
 			
		}

		public Task Process(IProgress<int> prog, IProgress<int> subProg, CancellationToken ct)
		{
			return Task.Run(() =>
				{
					int total = sFileNames.Count();
					foreach (string esFileName in sFileNames)
					{
						if (ct.IsCancellationRequested)
						{
							throw new OperationCanceledException();
						}

						 ParseFile(esFileName, subProg, ct);
						
						prog.Report(1);
						total--;

					}
				}, ct);
		}
		

		private void ParseFile(string sFileName, IProgress<int> prog, CancellationToken ct)
		{
			//return Task.Run(() =>
			//	{
					string sDate = sFileName.Substring(sFileName.Length - 12, 8);
					using (StreamReader SR = new StreamReader(sFileName))
					{
						while (SR.Peek() > 0)
						{
							string sLine = SR.ReadLine();
							if (UseParallelForEach)
							{
								Parallel.ForEach(dTableMap, (Pair) =>
									{
										ExtractData(sLine, Pair, sDate);
									});
							}
							else
							{
								foreach (KeyValuePair<string, string> Pair in dTableMap)
								{
									ExtractData(sLine, Pair, sDate);
								}
							}
							if (ct.IsCancellationRequested)
							{
								FlushCache();
								throw new OperationCanceledException();
							}
							prog.Report(1);
						}
						FlushCache();
					}
				//}, ct);
		}

		private void ExtractData(string sLineData, KeyValuePair<string, string> Pair, string sDate)
		{
			string sFileName = Pair.Key;
			string sColumDef = Pair.Value;
			string[] oColumDef = sColumDef.Split(','); // the column number I'm after in the Line.
			string[] oLineData = sLineData.Split('|'); // the array of data in the file I care about.

			string sNewLineForTable = "";
			foreach (string colNo in oColumDef)
			{
				sNewLineForTable += oLineData[int.Parse(colNo)] + "|";
			}
					
			sNewLineForTable = sNewLineForTable.Substring(0, sNewLineForTable.LastIndexOf('|'));
			sFileName = sFileName.Substring(0, sFileName.Length - 4) + "_" + sDate + ".txt";
			eAddLines(sFileName, sNewLineForTable);
		}

		private void eAddLines(string sFileName, string sLinesOfData)
		{
			if (!FileLineCache.ContainsKey(sFileName))
				FileLineCache.Add(sFileName, new List<string>());
			FileLineCache[sFileName].Add(sLinesOfData);

			if (FileLineCache[sFileName].Count > 20)
			{
				using (StreamWriter SW = new StreamWriter(sFileName, true))
				{
					foreach (string line in FileLineCache[sFileName])
						SW.WriteLine(line);
				}
				FileLineCache[sFileName].Clear();
			}
		}

		/// <summary>
		///  not thread safe
		/// </summary>
		private void FlushCache()
		{
			foreach (KeyValuePair<string, List<string>> kvp in FileLineCache)
			{
				using (StreamWriter SW = new StreamWriter(kvp.Key, true))
				{
					foreach (string line in kvp.Value)
						SW.WriteLine(line);
				}
			}
			FileLineCache.Clear();
		}

		private void InitDictionary(Dictionary<string, string> dTableMap)
		{
			// Xref population file
			string sFileT0 = OutputDirectory+"\\" + "XrefPopulation.txt";
			string sCoulumnT0 = "0,1,2,5,6,7,8";
			dTableMap.Add(sFileT0, sCoulumnT0);

			//dbo.MarkitETPNAV
			string sFileT1 = OutputDirectory+"\\" + "MarkitETPNAV.txt";
			string sCoulumnT1 = "0,5,8,3,7,12,15,16,17";
			dTableMap.Add(sFileT1, sCoulumnT1);

			//dbo.MarkitETPAUM
			string sFileT2 = OutputDirectory+"\\" + "MarkitETPAUM.txt";
			string sCoulumnT2 = "0,5,8,12,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67,68,69,70,71,72,73,74";
			dTableMap.Add(sFileT2, sCoulumnT2);

			//dbo.MarkitETPPricing
			string sFileT3 = OutputDirectory+"\\" + "MarkitETPPricing.txt";
			string sCoulumnT3 = "0,5,8,140,141,142,145,146,147,148,149,150,154,155,156,175,176,177,178,179,180,181,182,183,184,185,186,187,188,189,226,227,228,229,230,231,232,233,234,235,236,237,238,239,240,241,242,243,244,245,246,247,248,249,298,299,300,301,302,303,304,305,306,307,308,309,870,871,869";
			dTableMap.Add(sFileT3, sCoulumnT3);

			//dbo.MarkitETPSharesOutstanding
			string sFileT4 = OutputDirectory+"\\" + "MarkitETPSharesOutstanding.txt";
			string sCoulumnT4 = "0,5,8,12,18,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89";
			dTableMap.Add(sFileT4, sCoulumnT4);

			//dbo.MarkitETPTopHoldingsPercent
			//string sFileT5 = OutputDirectory+"\\" + "MarkitETPTopHoldingsPercent.txt";
			//string sCoulumnT5 = "0,5,8,818";
			//dTableMap.Add(sFileT5, sCoulumnT5);

			//dbo.MarkitETPDividends
			string sFileT6 = OutputDirectory+"\\" + "MarkitETPDividends.txt";
			string sCoulumnT6 = "0,5,8,12,4,869,14";
			dTableMap.Add(sFileT6, sCoulumnT6);

			//dbo.MarkitETPBenchmark - MarkitETPBRUKBenchmark
			string sFileT7 = OutputDirectory+"\\" + "MarkitETPBRUKBenchmark.txt";
			string sCoulumnT7 = "0,5,8,9,10,11,12";
			dTableMap.Add(sFileT7, sCoulumnT7);

			//dbo.MarkitETPSplits
			//string sFileT8 = OutputDirectory+"\\" + "MarkitETPSplits.txt";
			//string sCoulumnT8 = "0,5,8,13";
			//dTableMap.Add(sFileT8, sCoulumnT8);

			//dbo.MarkitETPPricingVolumeOther
			string sFileT9 = OutputDirectory+"\\" + "MarkitETPPricingVolumeOther.txt";
			string sCoulumnT9 = "0,5,8,334,335,336,337,338,339,340,341,342,343,344,345";
			dTableMap.Add(sFileT9, sCoulumnT9);

			//dbo.MarkitETPPremiumDiscount
			//string sFileT10 = OutputDirectory+"\\" + "MarkitETPPremiumDiscount.txt";
			//string sCoulumnT10 = "0,5,8,159,160,161,162,163,164,165,166,167,168,169,170,171,172,173,174";
			//dTableMap.Add(sFileT10, sCoulumnT10);

			//dbo.MarkitETPPerformance
			string sFileT11 = OutputDirectory+"\\" + "MarkitETPPerformance.txt";
			string sCoulumnT11 = "0,5,8,12,384,385,386,387,388,389,390,391,392,393,394,395,396,397,398,399,400,401,402,403,404,405,406,407,408,409,410,411,412,413,414,415,416,417,418,419,420,421,422,423,424,425,426,427,428,429,430,431,432,433,434,435,436,437,438,439,440,441,442,443,444,445,446,447,448,449,450,451,452,453,454,455,456,457,458,459,460,461,462,463,464,465,466,467,468,469,470,471,472,473,474,475,476,477,478,479,480,481,482,483,484,485,486,487,488,489,490,491,492,493,494,495,496,497,498,499,500,501,502,503,504,505";
			dTableMap.Add(sFileT11, sCoulumnT11);

			//dbo.MarkitETPBenchmarkPerformance
			string sFileT12 = OutputDirectory+"\\" + "MarkitETPBenchmarkPerformance.txt";
			string sCoulumnT12 = "0,5,8,382,383,636,637,638,639,640,641,642,643,644,645,646,647,648,649,650,651,652,653,654,655,656,657,658,659,660,661,662,663,664,665,666,667,668,669,670,671,672,673,674,675";
			dTableMap.Add(sFileT12, sCoulumnT12);

			//dbo.MarkitETPRegionalAllocation - MarkitETPBRUKRegionalAllocation
			string sFileT13 = OutputDirectory+"\\" + "MarkitETPBRUKRegionalAllocation.txt";
			string sCoulumnT13 = "0,5,8,819,820,821,822,823,824,825,12";
			dTableMap.Add(sFileT13, sCoulumnT13);

			//dbo.MarkitETPAssetClassBreakdown - MarkitETPBRUKAssetClassBreakdown
			string sFileT14 = OutputDirectory+"\\" + "MarkitETPBRUKAssetClassBreakdown.txt";
			string sCoulumnT14 = "0,5,8,826,827,828,829,830,831,832,833,834,835,836,837,12";
			dTableMap.Add(sFileT14, sCoulumnT14);

			//dbo.MarkitETPSectorAllocation - MarkitETPBRUKSectorAllocation
			string sFileT15 = OutputDirectory+"\\" + "MarkitETPBRUKSectorAllocation.txt";
			string sCoulumnT15 = "0,5,8,838,839,840,841,842,843,844,845,846,847,848,12";
			dTableMap.Add(sFileT15, sCoulumnT15);

			//dbo.MarkitETPEconomicAllocation
			//string sFileT16 = OutputDirectory+"\\" + "MarkitETPEconomicAllocation.txt";
			//string sCoulumnT16 = "0,5,8,849,850,851,852";
			//dTableMap.Add(sFileT16, sCoulumnT16);

			//dbo.MarkitETPRiskMeasures - MarkitETPBRUKRiskMeasures
			string sFileT17 = OutputDirectory+"\\" + "MarkitETPBRUKRiskMeasures.txt";
			string sCoulumnT17 = "0,5,8,853,854,855,856,857,858,859,860,861,12";
			dTableMap.Add(sFileT17, sCoulumnT17);

			//dbo.MarkitETPListingPerformance
			string sFileT18 = OutputDirectory+"\\" + "MarkitETPListingPerformance.txt";
			string sCoulumnT18 = "0,5,8,506,507,508,509,510,511,512,513,514,515,516,517,518,519,520,521,522,523,524,525,526,527,528,529,530,531,532,533,534,535,536,537,538,539,540,541,542,543,544,545,546,547,548,549,550,551,552,553,554,555,556,557,571,572,573,574,575,576,577,578,579,580,581,582,583,584,585,586,587,588,589,590,591,592,593,594,595,596,597,598,599,600,601,602,603,604,605,606,607,608,609,610,611";
			dTableMap.Add(sFileT18, sCoulumnT18);

			//dbo.MarkitETPListingPerformanceSupplemental
			string sFileT19 = OutputDirectory+"\\" + "MarkitETPListingPerformanceSupplemental.txt";
			string sCoulumnT19 = "0,5,6,8,558,559,560,561,562,563,564,565,566,567,568,569,570,610,611,612,613,614,615,616,617,618,619,620,621,622,623,624,625,626,627,628,629,630,631,632,633,634,635";
			dTableMap.Add(sFileT19, sCoulumnT19);

			//2017/09/27
			//dbo.MarkitETPTrackingDifference
			string sFileT20 = OutputDirectory+"\\" + "MarkitETPTrackingDifference.txt";
			string sCoulumnT20 = "0,5,8,12,676,677,678,679,680,681,682,683,684,685,686,687,688,689,690,691,692,693,694,695,696,697,698,699,700,701,702,703,704,705,706,707,708,709,710,711,712,713,714,715,716,717,718,719,720,721,722,723,724,725,726,727,728,729,730,731,732,733,734,735,736,737,738,739,740,741,742,743,744,745,746,747,748,749";
			dTableMap.Add(sFileT20, sCoulumnT20);

			//2017/09/27
			//dbo.MarkitETPTrackingError
			string sFileT21 = OutputDirectory+"\\" + "MarkitETPTrackingError.txt";
			string sCoulumnT21 = "0,5,8,12,750,751,752,753,754,755,756,757,758,759,760,761,762,763,764,765,766,767,768,769,770,771,772,773,774,775,776,777,778,779,780,781,782,783,784,785,786,787,788,789,790,791,792,793,794,795,796,797,798,799,800,801,802,803,804,805,806,807,808,809,810,811,812,813,814,815,816,817";
			dTableMap.Add(sFileT21, sCoulumnT21);
		}

	}
}
