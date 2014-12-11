﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LINQOptimizer;

namespace LINQBenchmark
{

    public class QueryTester
    {

        private static System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        private static List<double> timeList = new List<double>();

        public static TimeStats Test(IEnumerable query, int noOfRepeats = 11, int maxTestTimeMsec = Int32.MaxValue, double minTestTimeMsec = 0)
        {
            timeList.Clear();
            
            for (int i = 0; i < noOfRepeats; i++)
            {
                int count = 0;
                sw.Reset();
                do {
                    sw.Start();
                    foreach (object item in query)
                    {
                    }
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

        public static SizeVsTimeStats TestReducedSource<TSource>(Func<IEnumerable> queryFunc, ref IEnumerable<TSource> source, int reducedCollectionCount, int noOfRepeats = 11, int maxTestTimeMsec = Int32.MaxValue, double minTestTimeMsec = 0)
        {
            IEnumerable<TSource> srcCopy = source;
            source = source.Take(reducedCollectionCount).ToList();
            SizeVsTimeStats stats = new SizeVsTimeStats(reducedCollectionCount, queryFunc().Count(), Test(queryFunc(), noOfRepeats, maxTestTimeMsec, minTestTimeMsec));
            source = srcCopy;
            return stats;
        }

        public static ICollection<SizeVsTimeStats> AutoSizeVsTimeTest<TSource>(Func<IEnumerable> queryFunc, ref IEnumerable<TSource> source, int ratio = 2, int noOfRepeats = 11, int maxTestTimeMsec = Int32.MaxValue, double minTestTimeMsec = 0)
        {
            ICollection<SizeVsTimeStats> stats = new List<SizeVsTimeStats>();
            int srcCount = 1;
            while (srcCount <= source.Count())
            {
                stats.Add(TestReducedSource(queryFunc, ref source, srcCount, noOfRepeats, maxTestTimeMsec, minTestTimeMsec));
                srcCount *= ratio;
            }
            
            return stats;
        }

    }
}