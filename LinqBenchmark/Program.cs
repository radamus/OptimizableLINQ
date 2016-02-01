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

    class Program
    {
        public const bool EVALUATE_ORIGINALS = true;

        public static ICollection<Product> products;
        public static ICollection<Product> productsBy10;
        public static ICollection<ProductX> productsX;
        public static ICollection<ProductX> productsXBy100;

        static void SimplestTestingTest()
        {
            TestingEnvironment.SimpleTest(products, products.Count(), "List of all products");

            TimeStats stats = OptimizationTester.TestOverheadTime((productsSrc) => from Product p in productsSrc where (from Product p2 in products select p2.unitPrice).Max() == p.unitPrice select p.productName, products, new OptimizableLINQApplicator());
            Console.WriteLine("Optimization time: {0} msec", stats.medianTimeMsec);

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

//            TestSuite2015.RunAll(products);

/*
            if (TestingEnvironment.EXTENDED_DATA)
                nessosFactoringOutTests(10000);
            else
                nessosFactoringOutTests(100);
/**/

//            TestVolatileIndexing.sameUnitPriceVolatileIndexExceptionsTest(products);
//            TestVolatileIndexing.singleExpressionVolatileIndex(products);
       
//            FactoringOutTests();
//            TestVolatileIndexing.nullSomeCategories(products);            
            VolatileIndexTests();

/**/
            Console.ReadLine();
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
            }
            TestVolatileIndexing.uniqueCategoryAlternatives(products);
            TestVolatileIndexing.uniqueCategoryVolatileIndex(products);
            TestVolatileIndexing.uniqueCategoryPLINQ(products);

            if (EVALUATE_ORIGINALS)
            {
                TestVolatileIndexing.sameUnitPriceQueryOriginal(productsBy10);
            }
            TestVolatileIndexing.sameUnitPriceAlternatives(products);
            TestVolatileIndexing.sameUnitPriceVolatileIndex(products);
            TestVolatileIndexing.sameUnitPricePLINQ(products);
        }
    }
}
