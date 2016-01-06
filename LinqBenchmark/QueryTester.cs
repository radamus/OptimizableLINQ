using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OptimizableLINQ;

namespace OptimizableLINQBenchmark
{

    public class QueryTester
    {

        public const int NOOFREPEATS = 71;
        public const int MAX_TEST_TIME_MSEC = 10000;
        public const double MIN_TEST_TIME_MSEC = 0.025;

        private static System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        private static List<double> timeList = new List<double>();

        public static TimeStats Test<TQuery>(TQuery query, IQueryExecutor<TQuery> executor, int noOfRepeats = 11, int maxTestTimeMsec = Int32.MaxValue, double minTestTimeMsec = 0)
        {
            timeList.Clear();

            executor.Prepare(ref query);

            for (int i = 0; i < noOfRepeats; i++)
            {
                int count = 0;
                sw.Reset();
                do {
                    sw.Start();
                    executor.Run(ref query);             
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

        public static ICollection<SizeVsTimeStats> AutoSizeVsTimeTest<TCardConf, TQuery>(Func<TQuery> query, IQueryExecutor<TQuery> executor, ref TCardConf cardConfigurator, IQuerySourceCardinalityManagement<TCardConf> cardManager, int minCount = 1, int ratio = 2, int noOfRepeats = 11, int maxTestTimeMsec = Int32.MaxValue, double minTestTimeMsec = 0)
        {
            ICollection<SizeVsTimeStats> stats = new List<SizeVsTimeStats>();
            int srcCount = minCount;
            while (srcCount <= cardManager.GetFullCardinality())
            {
                cardManager.Reduce(ref cardConfigurator, srcCount);
                stats.Add(new SizeVsTimeStats(srcCount, executor.ResultCRC(query()), Test(query(), executor, noOfRepeats, maxTestTimeMsec, minTestTimeMsec)));
                if (stats.Count() > 1 && stats.Last().timeStats.noOfRepeats * 2 < stats.ElementAt(stats.Count() - 2).timeStats.noOfRepeats)
                    break;
                srcCount *= ratio; 
            }
            cardManager.Revert(ref cardConfigurator);
            return stats;
        }

        public static ICollection<SizeVsTimeStats> DefaultSizeVsTimeStats<TCardConf, TQuery>(Func<TQuery> query, IQueryExecutor<TQuery> executor, ref TCardConf cardConfigurator, IQuerySourceCardinalityManagement<TCardConf> cardManager)
        {
            //TODO: Add safe support for minCount = 0
            ICollection<SizeVsTimeStats> statsList = QueryTester.AutoSizeVsTimeTest(query, executor, ref cardConfigurator, cardManager, 1, 2, 
                NOOFREPEATS, MAX_TEST_TIME_MSEC, MIN_TEST_TIME_MSEC);
            statsList = statsList.Concat(QueryTester.AutoSizeVsTimeTest(query, executor, ref cardConfigurator, cardManager, 10, 10, 
                NOOFREPEATS, MAX_TEST_TIME_MSEC, MIN_TEST_TIME_MSEC)).ToList();

            return statsList;
        }

        public static ICollection<SizeVsTimeStats> DefaultSizeVsTimeStats<TSource>(Func<IEnumerable> queryFunc, ref IEnumerable<TSource> source)
        {
            return DefaultSizeVsTimeStats(queryFunc, new DeferredEnumerableQueryExecutor(), ref source, new EnumerableSourceCardinalityManagement<TSource>(source));
        }

        public static ICollection<SizeVsTimeStats> DefaultSizeVsTimeStats(Func<IEnumerable> queryFunc, ref int count)
        {
            return DefaultSizeVsTimeStats(queryFunc, new DeferredEnumerableQueryExecutor(), ref count, new IntCardinalityManagementStrategy(count));
        }

        public static ICollection<SizeVsTimeStats> DefaultSizeVsTimeStats<TSource>(Func<Func<IEnumerable>> enumFuncFunc, ref IEnumerable<TSource> source)
        {
            return DefaultSizeVsTimeStats(enumFuncFunc, new FuncEnumerableQueryExecutor(), ref source, new EnumerableSourceCardinalityManagement<TSource>(source));
        }

        public static ICollection<SizeVsTimeStats> DefaultSizeVsTimeStats(Func<Func<IEnumerable>> enumFuncFunc, ref int count)
        {
            return DefaultSizeVsTimeStats(enumFuncFunc, new FuncEnumerableQueryExecutor(), ref count, new IntCardinalityManagementStrategy(count));
        }

        public static ICollection<SizeVsTimeStats> DefaultSizeVsTimeStats<TSource>(Func<Func<int>> intFuncFunc, ref IEnumerable<TSource> source)
        {
            return DefaultSizeVsTimeStats(intFuncFunc, new FuncIntQueryExecutor(), ref source, new EnumerableSourceCardinalityManagement<TSource>(source));
        }

        public static ICollection<SizeVsTimeStats> DefaultSizeVsTimeStats(Func<Func<int>> intFuncFunc, ref int count)
        {
            return DefaultSizeVsTimeStats(intFuncFunc, new FuncIntQueryExecutor(), ref count, new IntCardinalityManagementStrategy(count));
        }

        public static ICollection<SizeVsTimeStats> DefaultSizeVsTimeStats<TSource>(Func<int> intFunc, ref IEnumerable<TSource> source)
        {
            return DefaultSizeVsTimeStats(() => intFunc, ref source);
        }

        public static ICollection<SizeVsTimeStats> DefaultSizeVsTimeStats(Func<int> intFunc, ref int count)
        {
            return DefaultSizeVsTimeStats(() => intFunc, ref count);
        }

    }

}
