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
    using OptimisableLINQ;

    public class OptimisationTester
    {

        private static System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        private static List<double> timeList = new List<double>();

        public static TimeStats TestOverheadTime<TSource>(Func<IQueryable<TSource>, IQueryable> queryFunc, IEnumerable<TSource> source, OptimisationApplicator applicator, int noOfRepeats = 11)
        {
            timeList.Clear();

            applicator.Compile(applicator.Apply(queryFunc, source)); //warmup

            for (int i = 0; i < noOfRepeats; i++)
            {
                sw.Reset();

                IQueryable query = applicator.Apply(queryFunc, source);                
                sw.Start();
                applicator.Compile(query);
                sw.Stop();
              
                timeList.Add(Math.Round(sw.Elapsed.TotalMilliseconds, 4));

                if (i % 4 == 0)
                    Thread.Sleep(10);
            }

            return new TimeStats(timeList);
        }

    }

}
