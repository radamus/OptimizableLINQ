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
        public int resultSize { get; private set; }
        public TimeStats timeStats { get; private set; }

        internal SizeVsTimeStats(int sourceSize, int resultSize, TimeStats timeStats)
        {
            this.sourceSize = sourceSize;
            this.resultSize = resultSize;
            this.timeStats = timeStats;
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
            return stats.sourceSize + CSV_SEPARATOR + stats.resultSize + CSV_SEPARATOR + TimeStats2CSV(stats.timeStats);
        }

        public static String SizeVsTimeStatsCollection2CSV(ICollection<SizeVsTimeStats> stats)
        {
            StringBuilder res = new StringBuilder();

            res.Append(SizeVsTimeStats2CSVHeader() + Environment.NewLine);
            foreach (SizeVsTimeStats s in stats)
                res.Append(SizeVsTimeStats2CSV(s) + Environment.NewLine);

            return res.ToString();
        }

        private const String formatString = "{0, -11}{1, -10}{2, -13}{3, -13}{4, -15}";

        public static String FormattedSizeVsTimeStatsCollection(ICollection<SizeVsTimeStats> stats)
        {
            StringBuilder res = new StringBuilder();

            res.Append(String.Format(formatString + Environment.NewLine,
                 "srcSize", "resCRC", "med[msec]", "min[msec]", "noOfRepeats"));
            
            foreach (SizeVsTimeStats s in stats)
                res.Append(String.Format(formatString + Environment.NewLine,
                 s.sourceSize, s.resultSize, s.timeStats.medianTimeMsec, s.timeStats.minTimeMsec, s.timeStats.noOfRepeats));

            return res.ToString();
        }

    }
}
