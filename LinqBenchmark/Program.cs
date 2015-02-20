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

    class Program
    {
        public const bool EVALUATE_ORIGINALS = true;

        public static IEnumerable<Product> products;
        public static IEnumerable<Product> productsBy10;

        static void SimplestTestingTest()
        {
            TestingEnvironment.SimpleTest(products, products.Count(), "List of all products");

            TimeStats stats = OptimisationTester.TestOverheadTime((productsSrc) => from Product p in productsSrc where (from Product p2 in products select p2.unitPrice).Max() == p.unitPrice select p.productName, products, new OptimizableLINQApplicator());

            Console.WriteLine("Optimisation time: {0} msec" + Environment.NewLine, stats.medianTimeMsec);
        }

        static void Main(string[] args)
        {

//            NessosBench.Go();
            
            Console.WriteLine("Is StopWatch resolution high: {0}", System.Diagnostics.Stopwatch.IsHighResolution);
            TestingEnvironment.InitProducts(ref products);

            productsBy10 = products.Take(products.Count() / 10).ToList();
            SimplestTestingTest();


            if (TestingEnvironment.EXTENDED_DATA)
                nessosFactoringOutTests(10000);
            else
                nessosFactoringOutTests(100);

            FactoringOutTests();


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
    }
}
