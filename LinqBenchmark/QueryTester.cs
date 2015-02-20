using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OptimisableLINQ;

namespace OptimisableLINQBenchmark
{

    public class QueryTester
    {

        private static System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        private static List<double> timeList = new List<double>();

        public static TimeStats Test<TQuery>(TQuery query, IQueryExecutor<TQuery> executor, int noOfRepeats = 11, int maxTestTimeMsec = Int32.MaxValue, double minTestTimeMsec = 0)
        {
            timeList.Clear();

            executor.prepare(ref query);

            for (int i = 0; i < noOfRepeats; i++)
            {
                int count = 0;
                sw.Reset();
                do {
                    sw.Start();
                    executor.run(ref query);             
                    sw.Stop();
                    count++;
                } while (sw.Elapsed.TotalMilliseconds < minTestTimeMsec);
                timeList.Add(Math.Round(sw.Elapsed.TotalMilliseconds / count, 4));

                if (i % 4 == 0)
                    Thread.Sleep(10);
                if (timeList.Sum() > maxTestTimeMsec)
                    break;
            }

            return new TimeStats(timeList);
                
        }

        public static ICollection<SizeVsTimeStats> AutoSizeVsTimeTest<TCardConf, TQuery>(Func<TQuery> query, IQueryExecutor<TQuery> executor, ref TCardConf cardConfigurator, IQuerySourceCardinalityManagement<TCardConf> cardManager, int ratio = 2, int noOfRepeats = 11, int maxTestTimeMsec = Int32.MaxValue, double minTestTimeMsec = 0)
        {
            ICollection<SizeVsTimeStats> stats = new List<SizeVsTimeStats>();
            int srcCount = 1;
            while (srcCount <= cardManager.GetFullCardinality())
            {
                cardManager.Reduce(ref cardConfigurator, srcCount);
                stats.Add(new SizeVsTimeStats(srcCount, executor.resultCRC(query()), Test(query(), executor, noOfRepeats, maxTestTimeMsec, minTestTimeMsec)));
                srcCount *= ratio; 
            }
            cardManager.Revert(ref cardConfigurator);
            return stats;
        }

    }

}
