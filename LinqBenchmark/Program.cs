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
    using System.Linq.Expressions;

    class Program
    {
        public const bool EVALUATE_ORIGINALS = true;

        public static ICollection<Product> products;
        public static ICollection<Product> productsBy10;
        public static ICollection<ProductX> productsX;
        public static ICollection<ProductX> productsXBy100;

        static List<int> x = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        static int MAX = 100000;

        static void UsingLambda() {
            var ps = Expression.Parameter(typeof(int), "s");
            var pt = Expression.Parameter(typeof(int), "t");
            var ex1 = Expression.Lambda(
                Expression.Constant(    
                    Expression.Lambda(
                            Expression.Add(ps, pt),
                        pt).Compile()),
                ps);

            var f1a = (Func<int, Func<int, int>>)ex1.Compile();
            var f1b = f1a(100);
            Console.WriteLine();
            Console.WriteLine(f1b(123));
            Console.WriteLine();
            
            Func<IEnumerable<int>, IEnumerable<int>> lambda = l => l.Where(i => i % 2 == 0).Where(i => i > 5);
            var t0 = DateTime.Now.Ticks;
            for (int j = 1; j < MAX; j++)
            {
                var sss = lambda(x).ToList();
            }

            var tn = DateTime.Now.Ticks;
            Console.WriteLine("Using lambda: {0}", tn - t0);
        }

     
        static void SimplestTestingTest()
        {
            TestingEnvironment.SimpleTest(products, products.Count(), "List of all products");

            TimeStats stats = OptimizationTester.TestOverheadTime((productsSrc) => from Product p in productsSrc where (from Product p2 in products select p2.unitPrice).Max() == p.unitPrice select p.productName, products, new OptimizableLINQApplicator());
            Console.WriteLine("OptimizableLINQ optimization time: {0} msec", stats.medianTimeMsec);

            stats = OptimizationTester.TestOverheadTime((productsSrc) => from Product p in productsSrc where (from Product p2 in products select p2.unitPrice).Max() == p.unitPrice select p.productName, products, new ParallelLINQApplicator());
            Console.WriteLine("LinqOptimizer Optimization time: {0} msec", stats.medianTimeMsec);

/*            stats = OptimizationTester.TestOverheadTime((productsSrc) => from Product p in productsSrc where (from Product p2 in products select p2.unitPrice).Max() == p.unitPrice select p.productName, products, new OptimizationCompositionApplicator(new OptimizableLINQApplicator(), new ParallelLINQApplicator()));
            Console.WriteLine("Optimization time: {0} msec", stats.medianTimeMsec);

            stats = OptimizationTester.TestOverheadTime((productsSrc) => from Product p in productsSrc where (from Product p2 in products select p2.unitPrice).Max() == p.unitPrice select p.productName, products, new OptimizationCompositionApplicator(new ParallelLINQApplicator(), new OptimizableLINQApplicator()));
            Console.WriteLine("Optimization time: {0} msec" + Environment.NewLine, stats.medianTimeMsec);

            TestingEnvironment.BenchmarkOptimisations((productsSrc) => from Product p in productsSrc where (from Product p2 in products select p2.unitPrice).Max() == p.unitPrice select p.productName, ref products,
                new OptimizationApplicator[] { new OptimizableLINQApplicator(), new ParallelLINQApplicator(), new OptimizationCompositionApplicator(new ParallelLINQApplicator(), new OptimizableLINQApplicator()) }, "Max problem");
*/              
        }

        static void Main(string[] args)
        {

//            NessosBench.Go();
            
            Console.WriteLine("Is StopWatch resolution high: {0}", System.Diagnostics.Stopwatch.IsHighResolution);
            TestingEnvironment.InitProducts(ref products);
            TestingEnvironment.InitProductsX(ref productsX, products);

//            products = products.Take(100).ToList();

            productsBy10 = products.Take(products.Count() / 10).ToList();
            productsXBy100 = productsX.Take(products.Count() / 100).ToList();

            SimplestTestingTest();

            UsingLambda();
       
//            TestSuite2015.RunAll(products);

/*
            if (TestingEnvironment.EXTENDED_DATA)
                nessosFactoringOutTests(10000);
            else
                nessosFactoringOutTests(100);
/**/

            autoFactoringOutTests();

//            TestVolatileIndexing.sameUnitPriceQueryOriginal(products);     
       
/*            TestFactoringOut.singleExpressionOriginal(products);
            TestVolatileIndexing.uniqueCategoryQueryOriginal(products);
            TestVolatileIndexing.uniqueCategoryOriginalPLINQ(products);

            TestVolatileIndexing.sameUnitPricePLINQ(products);
            TestVolatileIndexing.uniqueCategoryPLINQ(products);

            FactoringOutTests();
//            TestVolatileIndexing.nullSomeCategories(products);            
//            VolatileIndexTests();

/**/
            Console.ReadLine();
        }

        private static void autoFactoringOutTests()
        {
            TestFactoringOut.autoSingleResultTest(products);
            TestFactoringOut.suspendedSingleResultTest(products);

            //TestSuite2015.SamePriceAsProblemRuntime(products);
//            TestFactoringOut.autoInnerQueryTest(products);
//            TestFactoringOut.suspendedInnerQueryTest(products);
//            TestFactoringOut.innerQueryGroupByTest(products);
        }

        private static void FactoringOutTests()
        {
            if (EVALUATE_ORIGINALS)
            {
                TestFactoringOut.innerQueryOriginal(productsBy10);
                TestFactoringOut.innerQueryOriginalOthersTest(productsBy10);
            }
            TestFactoringOut.innerQueryTest(products);
            TestFactoringOut.suspendedInnerQueryTest(products);
            TestFactoringOut.suspendedInnerQueryOthersTest(products);
            //            TestFactoringOut.innerQueryGroupByTest(products);

            if (EVALUATE_ORIGINALS)
            {
                TestFactoringOut.singleResultOriginal(productsBy10);
                TestFactoringOut.singleResultOriginalOthersTest(productsBy10);
            }
            TestFactoringOut.singleResultTest(products);
            TestFactoringOut.suspendedSingleResultTest(products);
            TestFactoringOut.suspendedSingleResultOthersTest(products);

            if (EVALUATE_ORIGINALS)
            {
                TestFactoringOut.singleExpressionOriginal(products);
                TestFactoringOut.singleExpressionOriginalOthersTest(products);
            }
            TestFactoringOut.singleExpressionTest(products);
            TestFactoringOut.suspendedSingleExpressionTest(products);
            TestFactoringOut.suspendedSingleExpressionOthersTest(products);

            /*            if (EVALUATE_ORIGINALS)
                            TestFactoringOut.simplerExpressionOriginal(products);
                        TestFactoringOut.simplerExpressionTest(products);
                        TestFactoringOut.suspendedSimplerExpressionTest(products);


            /**/
        }

        private static void nessosFactoringOutTests(int n)
        {
            if (EVALUATE_ORIGINALS)
            {
                TestFactoringOut.nessosPythagoreanTriplesOriginal(n);
                TestFactoringOut.nessosPythagoreanTriplesOriginalOthersTest(n);
            }
            TestFactoringOut.nessosPythagoreanTriplesTest(n);
            TestFactoringOut.nessosPythagoreanTriplesOthersTest(n);
            
            TestFactoringOut.suspendedNessosPythagoreanTriplesTest(n);
            TestFactoringOut.suspendedNessosPythagoreanTriplesOthersTest(n);
  
              /**/
        }

        private static void VolatileIndexTests()
        {
            TestVolatileIndexing.volatileIndexCreation(products);
            
            if (EVALUATE_ORIGINALS)
            {
                TestFactoringOut.singleExpressionOriginal(productsBy10);
            }
            TestVolatileIndexing.singleExpressionVolatileIndex(products);
            TestVolatileIndexing.singleKeyExpressionVolatileIndex(products);

            if (EVALUATE_ORIGINALS)
            {
                TestVolatileIndexing.uniqueCategoryQueryOriginal(productsBy10);
                TestVolatileIndexing.uniqueCategoryOriginalPLINQ(productsBy10);
            }
            TestVolatileIndexing.uniqueCategoryAlternatives(products);
            TestVolatileIndexing.uniqueCategoryVolatileIndex(products);
            TestVolatileIndexing.uniqueCategoryPLINQ(products);

            if (EVALUATE_ORIGINALS)
            {
                TestVolatileIndexing.sameUnitPriceQueryOriginal(productsBy10);
                TestVolatileIndexing.sameUnitPriceOriginalPLINQ(productsBy10);
            }
            TestVolatileIndexing.sameUnitPriceAlternatives(products);
            TestVolatileIndexing.sameUnitPriceVolatileIndex(products);
            TestVolatileIndexing.sameUnitPricePLINQ(products);
        }

    }
}
