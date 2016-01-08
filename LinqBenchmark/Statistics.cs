using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizableLINQBenchmark
{

    public class TimeStats
    {
        public int noOfRepeats { get; private set; }
        public double minTimeMsec { get; private set; }
        public double medianTimeMsec { get; private set; }

        internal TimeStats(List<double> times)
            : this(
                times.Count(),
                times.OrderBy(t => t).ElementAt(0),
                times.OrderBy(t => t).ElementAt(times.Count / 2)
                )
        { }

        internal TimeStats(int noOfRepeats, double minTimeMsec, double medianTimeMsec)
        {
            this.noOfRepeats = noOfRepeats;
            this.minTimeMsec = minTimeMsec;
            this.medianTimeMsec = medianTimeMsec;
        }
    }

    public class SizeVsTimeStats
    {
        public int sourceSize { get; private set; }
        public int resultCRC { get; private set; }
        public TimeStats timeStats { get; private set; }
        public Exception caughtException { get; private set; }

        internal SizeVsTimeStats(int sourceSize, int resultCRC, TimeStats timeStats)
        {
            this.sourceSize = sourceSize;
            this.resultCRC = resultCRC;
            this.timeStats = timeStats;
            this.caughtException = null;
        }
        
        internal SizeVsTimeStats(int sourceSize, Exception e)
        {
            this.sourceSize = sourceSize;
            this.caughtException = e;
        }

    }


    class StatisticsExporter
    {
        public const String CSV_SEPARATOR = ",";

        private static String TimeStats2CSV(TimeStats timeStats)
        {
            return timeStats.medianTimeMsec + CSV_SEPARATOR + timeStats.minTimeMsec + CSV_SEPARATOR + timeStats.noOfRepeats;
        }

        private static string SizeVsTimeStats2CSVHeader()
        {
            return "srcSize" + CSV_SEPARATOR + "resCRC" + CSV_SEPARATOR + "med[msec]" + CSV_SEPARATOR + "min[msec]" + CSV_SEPARATOR + "noOfRepeats";
        }

        private static String SizeVsTimeStats2CSV(SizeVsTimeStats stats)
        {
            return stats.sourceSize + CSV_SEPARATOR + stats.resultCRC + CSV_SEPARATOR + TimeStats2CSV(stats.timeStats);
        }

        public static String SizeVsTimeStatsCollection2CSV(ICollection<SizeVsTimeStats> stats)
        {
            StringBuilder res = new StringBuilder();

            res.Append(SizeVsTimeStats2CSVHeader() + Environment.NewLine);
            foreach (SizeVsTimeStats s in stats)
                res.Append(SizeVsTimeStats2CSV(s) + Environment.NewLine);

            return res.ToString();
        }

        private const String formatString = "{0, -11}{1, -13}{2, -13}{3, -13}{4, -15}";

        public static String FormattedSizeVsTimeStatsCollection(ICollection<SizeVsTimeStats> stats)
        {
            StringBuilder res = new StringBuilder();

            res.Append(String.Format(formatString + Environment.NewLine,
                 "srcSize", "resCRC", "med[msec]", "min[msec]", "noOfRepeats"));

            foreach (SizeVsTimeStats s in stats)
            {
                if (s.caughtException == null)
                    res.Append(String.Format(formatString + Environment.NewLine,
                     s.sourceSize, s.resultCRC, s.timeStats.medianTimeMsec, s.timeStats.minTimeMsec, s.timeStats.noOfRepeats));
                else
                    res.Append(String.Format("{0, -11}{1, -30}" + Environment.NewLine,
                     s.sourceSize, s.caughtException.Message));
            }

            return res.ToString();
        }

        public static String FormattedStatsComparison(ICollection<SizeVsTimeStats> orgStats, IList<ICollection<SizeVsTimeStats>> optStatsList, IList<String> descriptions)
        {
            StringBuilder res = new StringBuilder();

            res.Append(String.Format(formatString + Environment.NewLine,
                 "CRC-CHECK", "MinSizeTime", "MaxSizeTime", "GainSrcSize", "Optimisation"));

            IEnumerable<int> sizes = optStatsList.SelectMany(stats => stats.Select(s => s.sourceSize)).GroupBy(size => size).Where(g => g.Count() == optStatsList.Count()).Select(g => g.Key).OrderBy(size => size);

            for (int i = 0; i < optStatsList.Count(); i++)
            {
                ICollection<SizeVsTimeStats> optStats = optStatsList[i];
               
                res.Append(String.Format(formatString + Environment.NewLine,
                     sizes.All(size => optStats.First(stat => stat.sourceSize == size).resultCRC == orgStats.First(stat => stat.sourceSize == size).resultCRC) ? "CRC-OK" : "CRC-ERROR",
                     optStats.First(stat => stat.sourceSize == sizes.First()).timeStats.medianTimeMsec, optStats.First(stat => stat.sourceSize == sizes.Last()).timeStats.medianTimeMsec,
                     optStats.Where(stat => orgStats.Select(ostat => ostat.sourceSize).Contains(stat.sourceSize)).Where(stat => stat.timeStats.medianTimeMsec < orgStats.First(ostat => ostat.sourceSize == stat.sourceSize).timeStats.medianTimeMsec).Select(stat => stat.sourceSize).DefaultIfEmpty(Int32.MaxValue).OrderBy(size => size).First(),
                     descriptions[i]));
            }

            return res.ToString();
        }

    }
}
