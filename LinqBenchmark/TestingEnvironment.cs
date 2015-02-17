using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimisableLINQBenchmark
{
    using SampleData;
    using OptimisableLINQ;

    public static class TestingEnvironment
    {
        public const bool NOEXTENDED_TESTS = false;
        public const bool PRINT_CSV = false;
        public const bool VERBOSE = true;

        public const int NOOFREPEATS = 71;
        public const int MAX_TEST_TIME_MSEC = 10000;
        public const double MIN_TEST_TIME_MSEC = 0.025;


        public static void InitProducts(ref IEnumerable<Product> products)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            ICollection<Product> productsCol = new List<Product>();
            SimpleGenerator.fillProducts(ref productsCol);
            SimpleExtendedGenerator.fillProducts(ref productsCol, 1000000);
            //var products = productsCol.AsQueryable();
            watch.Stop();
            if (VERBOSE)
                Console.WriteLine("Collection of {0} products loading time (StopWatch): {1} msec ({2})" + Environment.NewLine,
                    productsCol.Count(), watch.ElapsedMilliseconds, watch.Elapsed);
            products = productsCol.ToList();
        }

        public static void SimpleTest(IEnumerable query, int sourceCount, String description = null, String queryString = null)
        {
            if (description != null)
                Console.WriteLine("* * * * * " + description);
            else
                Console.WriteLine("* * * * *");
            if (VERBOSE && queryString != null)
                Console.WriteLine("Query: " + queryString);

            if (VERBOSE)
                Console.WriteLine("Query result count for a full source ({0} elements) : {1}", sourceCount, query.Count());
            Console.WriteLine("Evaluation time: {0} msec" + Environment.NewLine, QueryTester.Test(query, NOOFREPEATS, MAX_TEST_TIME_MSEC, MIN_TEST_TIME_MSEC).medianTimeMsec);

        }

        public static void ExtendedTest<TSource>(Func<IEnumerable> queryFunc, ref IEnumerable<TSource> source, String description = null, String queryString = null)
        {
            SimpleTest(queryFunc(), source.Count(), description, queryString);

            if (!NOEXTENDED_TESTS)
            {

                ICollection<SizeVsTimeStats> statsList = QueryTester.AutoSizeVsTimeTest(queryFunc, ref source, 2, NOOFREPEATS, MAX_TEST_TIME_MSEC, MIN_TEST_TIME_MSEC);
                statsList = statsList.Concat(QueryTester.AutoSizeVsTimeTest(queryFunc, ref source, 10, NOOFREPEATS, MAX_TEST_TIME_MSEC, MIN_TEST_TIME_MSEC)).ToList();

                Console.WriteLine(StatisticsExporter.FormattedSizeVsTimeStatsCollection(statsList));
                if (PRINT_CSV)
                    Console.WriteLine(StatisticsExporter.SizeVsTimeStatsCollection2CSV(statsList));
            }
        }

        public static void ExtendedIntTest(Func<IEnumerable> queryFunc, ref int count, String description = null, String queryString = null)
        {
            SimpleTest(queryFunc(), count, description, queryString);

            if (!NOEXTENDED_TESTS)
            {

                ICollection<SizeVsTimeStats> statsList = QueryTester.AutoSizeVsTimeIntTest(queryFunc, ref count, 2, NOOFREPEATS, MAX_TEST_TIME_MSEC, MIN_TEST_TIME_MSEC);
                statsList = statsList.Concat(QueryTester.AutoSizeVsTimeIntTest(queryFunc, ref count, 10, NOOFREPEATS, MAX_TEST_TIME_MSEC, MIN_TEST_TIME_MSEC)).ToList();

                Console.WriteLine(StatisticsExporter.FormattedSizeVsTimeStatsCollection(statsList));
                if (PRINT_CSV)
                    Console.WriteLine(StatisticsExporter.SizeVsTimeStatsCollection2CSV(statsList));
            }
        }

        public static void SimpleTest(Func<IEnumerable> enumFunc, int sourceCount, String description = null, String queryString = null)
        {
            if (description != null)
                Console.WriteLine("* * * * * " + description);
            else
                Console.WriteLine("* * * * *");
            if (VERBOSE && queryString != null)
                Console.WriteLine("Query: " + queryString);

            if (VERBOSE)
                Console.WriteLine("Query result count for a full source ({0} elements) : {1}", sourceCount, enumFunc().Count());
            Console.WriteLine("Evaluation time: {0} msec" + Environment.NewLine, EnumFuncFuncTester.Test(enumFunc, NOOFREPEATS, MAX_TEST_TIME_MSEC, MIN_TEST_TIME_MSEC).medianTimeMsec);

        }

        public static void ExtendedTest<TSource>(Func<Func<IEnumerable>> enumFuncFunc, ref IEnumerable<TSource> source, String description = null, String queryString = null)
        {
            SimpleTest(enumFuncFunc(), source.Count(), description, queryString);

            if (!NOEXTENDED_TESTS)
            {

                ICollection<SizeVsTimeStats> statsList = EnumFuncFuncTester.AutoSizeVsTimeTest(enumFuncFunc, ref source, 2, NOOFREPEATS, MAX_TEST_TIME_MSEC, MIN_TEST_TIME_MSEC);
                statsList = statsList.Concat(EnumFuncFuncTester.AutoSizeVsTimeTest(enumFuncFunc, ref source, 10, NOOFREPEATS, MAX_TEST_TIME_MSEC, MIN_TEST_TIME_MSEC)).ToList();

                Console.WriteLine(StatisticsExporter.FormattedSizeVsTimeStatsCollection(statsList));
                if (PRINT_CSV)
                    Console.WriteLine(StatisticsExporter.SizeVsTimeStatsCollection2CSV(statsList));
            }
        }

        public static void ExtendedIntTest(Func<Func<IEnumerable>> enumFuncFunc, ref int count, String description = null, String queryString = null)
        {
            SimpleTest(enumFuncFunc(), count, description, queryString);

            if (!NOEXTENDED_TESTS)
            {

                ICollection<SizeVsTimeStats> statsList = EnumFuncFuncTester.AutoSizeVsTimeIntTest(enumFuncFunc, ref count, 2, NOOFREPEATS, MAX_TEST_TIME_MSEC, MIN_TEST_TIME_MSEC);
                statsList = statsList.Concat(EnumFuncFuncTester.AutoSizeVsTimeIntTest(enumFuncFunc, ref count, 10, NOOFREPEATS, MAX_TEST_TIME_MSEC, MIN_TEST_TIME_MSEC)).ToList();

                Console.WriteLine(StatisticsExporter.FormattedSizeVsTimeStatsCollection(statsList));
                if (PRINT_CSV)
                    Console.WriteLine(StatisticsExporter.SizeVsTimeStatsCollection2CSV(statsList));
            }
        }
    }
}
