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
        public const bool EXTENDED_DATA = true;
        public const bool NOEXTENDED_TESTS = false;
        public const bool PRINT_CSV = false;
        public const bool VERBOSE = true;

        public static void InitProducts(ref ICollection<Product> products)
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

        public static void InitProductsX(ref ICollection<ProductX> productsX, ICollection<Product> products)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            ICollection<ProductX> productsCol = new List<ProductX>();
            foreach (Product p in products)
                productsCol.Add(new ProductX(p.productID, p.productName, p.category, p.unitPrice, p.unitsInStock));

            watch.Stop();
            if (VERBOSE)
                Console.WriteLine("Collection of {0} productsX init time (StopWatch): {1} msec ({2})" + Environment.NewLine,
                    productsCol.Count(), watch.ElapsedMilliseconds, watch.Elapsed);
            productsX = productsCol.ToList();
        }

        public static void SimpleTest<TQuery>(TQuery query, IQueryExecutor<TQuery> executor, int sourceCount, String description = null, String queryString = null)
        {
            TestHeader(description, queryString);

            if (VERBOSE)
                Console.WriteLine("Query resultCRC for a full source ({0} elements) : {1}", sourceCount, executor.ResultCRC(query));
            Console.WriteLine("Evaluation time: {0} msec" + Environment.NewLine, QueryTester.Test(query, executor, QueryTester.NOOFREPEATS, QueryTester.MAX_TEST_TIME_MSEC, QueryTester.MIN_TEST_TIME_MSEC).medianTimeMsec);

        }

        private static void TestHeader(String description, String queryString)
        {
            if (description != null)
                Console.WriteLine("* * * * * " + description);
            else
                Console.WriteLine("* * * * *");
            if (VERBOSE && queryString != null)
                Console.WriteLine("Query: " + queryString);
        }

        public static void SimpleTest(IEnumerable query, int sourceCount, String description = null, String queryString = null)
        {
            SimpleTest(query, new DeferredEnumerableQueryExecutor(), sourceCount, description, queryString);
        }


        public static ICollection<SizeVsTimeStats> ExtendedTest<TCardConf, TQuery>(Func<TQuery> query, IQueryExecutor<TQuery> executor, ref TCardConf cardConfigurator, IQuerySourceCardinalityManagement<TCardConf> cardManager, String description = null, String queryString = null)
        {
            TestHeader(description, queryString);
            
            ICollection<SizeVsTimeStats> statsList = QueryTester.DefaultSizeVsTimeStats(query, executor, ref cardConfigurator, cardManager);

            Console.WriteLine(StatisticsExporter.FormattedSizeVsTimeStatsCollection(statsList));
            if (PRINT_CSV)
                Console.WriteLine(StatisticsExporter.SizeVsTimeStatsCollection2CSV(statsList));

            return statsList;
        }

        public static void BenchmarkQuery<TCardConf, TQuery>(Func<TQuery> query, IQueryExecutor<TQuery> executor, ref TCardConf cardConfigurator, IQuerySourceCardinalityManagement<TCardConf> cardManager, String description = null, String queryString = null)
        {
            if (NOEXTENDED_TESTS)
                SimpleTest(query(), executor, cardManager.GetFullCardinality(), description, queryString);
            else
                ExtendedTest(query, executor, ref cardConfigurator, cardManager, description, queryString);
        }

        public static void BenchmarkQuery<TSource>(Func<IEnumerable> queryFunc, ref IEnumerable<TSource> source, String description = null, String queryString = null)
        {
            BenchmarkQuery(queryFunc, new DeferredEnumerableQueryExecutor(), ref source, new EnumerableSourceCardinalityManagement<TSource>(source), description, queryString);
        }

        public static void BenchmarkQuery(Func<IEnumerable> queryFunc, ref int count, String description = null, String queryString = null)
        {
            BenchmarkQuery(queryFunc, new DeferredEnumerableQueryExecutor(), ref count, new IntCardinalityManagementStrategy(count), description, queryString);
        }

        public static void BenchmarkQuery<TSource>(Func<Func<IEnumerable>> enumFuncFunc, ref IEnumerable<TSource> source, String description = null, String queryString = null)
        {
            BenchmarkQuery(enumFuncFunc, new FuncEnumerableQueryExecutor(), ref source, new EnumerableSourceCardinalityManagement<TSource>(source), description, queryString);
        }

        public static void BenchmarkQuery(Func<Func<IEnumerable>> enumFuncFunc, ref int count, String description = null, String queryString = null)
        {
            BenchmarkQuery(enumFuncFunc, new FuncEnumerableQueryExecutor(), ref count, new IntCardinalityManagementStrategy(count), description, queryString);
        }

        public static void BenchmarkQuery<TSource>(Func<Func<int>> intFuncFunc, ref IEnumerable<TSource> source, String description = null, String queryString = null)
        {
            BenchmarkQuery(intFuncFunc, new FuncIntQueryExecutor(), ref source, new EnumerableSourceCardinalityManagement<TSource>(source), description, queryString);
        }

        public static void BenchmarkQuery(Func<Func<int>> intFuncFunc, ref int count, String description = null, String queryString = null)
        {
            BenchmarkQuery(intFuncFunc, new FuncIntQueryExecutor(), ref count, new IntCardinalityManagementStrategy(count), description, queryString);
        }
        
        public static void BenchmarkQuery<TSource>(Func<int> intFunc, ref IEnumerable<TSource> source, String description = null, String queryString = null)
        {
            BenchmarkQuery(() => intFunc, ref source, description, queryString);
        }

        public static void BenchmarkQuery(Func<int> intFunc, ref int count, String description = null, String queryString = null)
        {
            BenchmarkQuery(() => intFunc, ref count, description, queryString);
        }


        public static void BenchmarkOptimisations<TSource>(Func<IQueryable<TSource>, IQueryable> queryFunc, ref ICollection<TSource> source, OptimizationApplicator[] optApps, String description = "")
        {
            ICollection<TSource> sourcenoref = source;

            ICollection<SizeVsTimeStats> originalStats = ExtendedTest(() => queryFunc(sourcenoref.AsQueryable()), new DeferredEnumerableQueryExecutor(), ref source, new CollectionSourceCardinalityManagement<TSource>(source), description, queryFunc(source.AsQueryable()).Expression.ToString());
            IList<ICollection<SizeVsTimeStats>> optStats = new List<ICollection<SizeVsTimeStats>>();

            foreach (OptimizationApplicator app in optApps)
            {
                optStats.Add(ExtendedTest(() => queryFunc(app.Apply(sourcenoref)), new DeferredEnumerableQueryExecutor(), ref source, new CollectionSourceCardinalityManagement<TSource>(source), description + " with " + app.ToString(), app.GetOptimizedExpression(queryFunc, source).ToString()));
                TimeStats stats = OptimizationTester.TestOverheadTime(queryFunc, source, app);
                Console.WriteLine("Optimization time: {0} msec", stats.medianTimeMsec);
            }

            optStats.Add(originalStats);

            IList<int> order = Enumerable.Range(0, optStats.Count()).OrderBy(i => optStats[i].OrderBy(stat => stat.sourceSize).Last().timeStats.medianTimeMsec).ToList();
            IList<string> descriptions = optApps.Select(app => app.ToString()).Concat(new string[] { "" }).ToList();

            Console.WriteLine(Environment.NewLine + "* * * R A N K I N G");
            Console.WriteLine(StatisticsExporter.FormattedStatsComparison(originalStats, order.Select(i => optStats[i]).ToList(), order.Select(i => descriptions[i].ToString()).ToList()));
        }

    }
}
 