using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LINQBenchmark
{

    using LINQSampleData;

    class Program
    {
        public const bool EVALUATE_ORIGINALS = false;

        public static IEnumerable<Product> products;

        static void SimplestTestingTest()
        {
            TestingEnvironment.SimpleTest(products, products.Count(), "List of all products");
        }

        static void TempTest()
        {   
            var res = QueryTester.TestReducedSource(() => products.SelectMany(p => products.Where(pw => pw.unitPrice >= 90).
                Where(pup => products.Where(p2 => p2.productName == pup.productName).Average(p2=>p2.unitPrice) > p.unitPrice).
                Select(pup => p.productName + " bezsensowny " + pup.productName)), ref products, 100);

            Console.WriteLine("For {0} count: {1}", 100, res.resultSize);

            res = QueryTester.TestReducedSource(() => products.SelectMany(p => products.Where(pw => pw.unitPrice >= 90).
                Where(pup => products.Where(p2 => p2.productName == p.productName).Average(p2=>p2.unitPrice) > pup.unitPrice).
                Select(pup => p.productName + " bezsensowny " + pup.productName)), ref products, 1000);

            Console.WriteLine("For {0} count: {1}", 1000, res.resultSize);

            res = QueryTester.TestReducedSource(() => products.SelectMany(p => products.Where(pw => pw.unitPrice >= 90).
                Where(pup => products.Where(p2 => p2.productName == p.productName).Average(p2=>p2.unitPrice) > pup.unitPrice).
                Select(pup => p.productName + " bezsensowny " + pup.productName)), ref products, 10000);

            Console.WriteLine("For {0} count: {1}", 10000, res.resultSize);

            res = QueryTester.TestReducedSource(() => products.SelectMany(p => products.Where(pw => pw.unitPrice >= 90).
                Where(pup => products.Where(p2 => p2.productName == p.productName).Average(p2 => p2.unitPrice) > pup.unitPrice).
                Select(pup => p.productName + " bezsensowny " + pup.productName)), ref products, products.Count());

            Console.WriteLine("For {0} count: {1}", products.Count(), res.resultSize);
        }

        static void Main(string[] args)
        {
//            NessosBench.Go();
            
            Console.WriteLine("Is StopWatch resolution high: {0}", System.Diagnostics.Stopwatch.IsHighResolution);
            TestingEnvironment.InitProducts(ref products);
//            SimplestTestingTest();
//            TempTest();



            TestFactoringOut.innerQueryWithPLINQ(products);
            TestFactoringOut.innerQueryWithLinqOptimizerTest(products);
            if (EVALUATE_ORIGINALS)
                TestFactoringOut.innerQueryOriginal(products);
            TestFactoringOut.innerQueryTest(products);
            TestFactoringOut.suspendedInnerQueryTest(products);
//            TestFactoringOut.innerQueryGroupByTest(products);

/*           if (EVALUATE_ORIGINALS)
                TestFactoringOut.singleResultOriginal(products);           
            TestFactoringOut.singleResultTest(products);
            TestFactoringOut.suspendedSingleResultTest(products);

            if (EVALUATE_ORIGINALS)
                TestFactoringOut.singleExpressionOriginal(products);
            TestFactoringOut.singleExpressionTest(products);
            TestFactoringOut.suspendedSingleExpressionTest(products);

            if (EVALUATE_ORIGINALS)
                TestFactoringOut.simplerExpressionOriginal(products);
            TestFactoringOut.simplerExpressionTest(products);
            TestFactoringOut.suspendedSimplerExpressionTest(products);

/**/
            Console.ReadLine();
        }
    }
}
