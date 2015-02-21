using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizableLINQBenchmark
{
    using SampleData;
    using OptimizableLINQ;

    public static class TestingEnvironment
    {
        public const bool EXTENDED_DATA = false;
        public const bool NOEXTENDED_TESTS = false;
        public const bool PRINT_CSV = false;
        public const bool VERBOSE = true;

        public static void InitProducts(ref IEnumerable<Product> products)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            ICollection<Product> productsCol = new List<Product>();
            SimpleGenerator.fillProducts(ref productsCol);
            if (EXTENDED_DATA)
                SimpleExtendedGenerator.fillProducts(ref productsCol, 1000000);
            
            watch.Stop();
            if (VERBOSE)
                Console.WriteLine("Collection of {0} products loading time (StopWatch): {1} msec ({2})" + Environment.NewLine,
                    productsCol.Count(), watch.ElapsedMilliseconds, watch.Elapsed);
            products = productsCol.ToList();
        }

        public static void SimpleTest<TQuery>(TQuery query, IQueryExecutor<TQuery> executor, int sourceCount, String description = null, String queryString = null)
        {
            if (description != null)
                Console.WriteLine("* * * * * " + description);
            else
                Console.WriteLine("* * * * *");
            if (VERBOSE && queryString != null)
                Console.WriteLine("Query: " + queryString);

            if (VERBOSE)
                Console.WriteLine("Query resultCRC for a full source ({0} elements) : {1}", sourceCount, executor.ResultCRC(query));
            Console.WriteLine("Evaluation time: {0} msec" + Environment.NewLine, QueryTester.Test(query, executor, QueryTester.NOOFREPEATS, QueryTester.MAX_TEST_TIME_MSEC, QueryTester.MIN_TEST_TIME_MSEC).medianTimeMsec);

        }

        public static void SimpleTest(IEnumerable query, int sourceCount, String description = null, String queryString = null)
        {
            SimpleTest(query, new DeferredEnumerableQueryExecutor(), sourceCount, description, queryString);
        }

        public static void ExtendedTest<TCardConf, TQuery>(Func<TQuery> query, IQueryExecutor<TQuery> executor, ref TCardConf cardConfigurator, IQuerySourceCardinalityManagement<TCardConf> cardManager, String description = null, String queryString = null)
        {
            SimpleTest(query(), executor, cardManager.GetFullCardinality(), description, queryString);

            if (!NOEXTENDED_TESTS)
            {
                ICollection<SizeVsTimeStats> statsList = QueryTester.DefaultSizeVsTimeStats(query, executor, ref cardConfigurator, cardManager);

                Console.WriteLine(StatisticsExporter.FormattedSizeVsTimeStatsCollection(statsList));
                if (PRINT_CSV)
                    Console.WriteLine(StatisticsExporter.SizeVsTimeStatsCollection2CSV(statsList));
            }

        }

        public static void ExtendedTest<TSource>(Func<IEnumerable> queryFunc, ref IEnumerable<TSource> source, String description = null, String queryString = null)
        {
            ExtendedTest(queryFunc, new DeferredEnumerableQueryExecutor(), ref source, new EnumerableSourceCardinalityManagement<TSource>(source), description, queryString);
        }

        public static void ExtendedTest(Func<IEnumerable> queryFunc, ref int count, String description = null, String queryString = null)
        {
            ExtendedTest(queryFunc, new DeferredEnumerableQueryExecutor(), ref count, new IntCardinalityManagementStrategy(count), description, queryString);
        }

        public static void ExtendedTest<TSource>(Func<Func<IEnumerable>> enumFuncFunc, ref IEnumerable<TSource> source, String description = null, String queryString = null)
        {
            ExtendedTest(enumFuncFunc, new FuncEnumerableQueryExecutor(), ref source, new EnumerableSourceCardinalityManagement<TSource>(source), description, queryString);
        }

        public static void ExtendedTest(Func<Func<IEnumerable>> enumFuncFunc, ref int count, String description = null, String queryString = null)
        {
            ExtendedTest(enumFuncFunc, new FuncEnumerableQueryExecutor(), ref count, new IntCardinalityManagementStrategy(count), description, queryString);
        }

        public static void ExtendedTest<TSource>(Func<Func<int>> intFuncFunc, ref IEnumerable<TSource> source, String description = null, String queryString = null)
        {
            ExtendedTest(intFuncFunc, new FuncIntQueryExecutor(), ref source, new EnumerableSourceCardinalityManagement<TSource>(source), description, queryString);
        }

        public static void ExtendedTest(Func<Func<int>> intFuncFunc, ref int count, String description = null, String queryString = null)
        {
            ExtendedTest(intFuncFunc, new FuncIntQueryExecutor(), ref count, new IntCardinalityManagementStrategy(count), description, queryString);
        }
        
        public static void ExtendedTest<TSource>(Func<int> intFunc, ref IEnumerable<TSource> source, String description = null, String queryString = null)
        {
            ExtendedTest(() => intFunc, ref source, description, queryString);
        }

        public static void ExtendedTest(Func<int> intFunc, ref int count, String description = null, String queryString = null)
        {
            ExtendedTest(() => intFunc, ref count, description, queryString);
        }
    }
}
