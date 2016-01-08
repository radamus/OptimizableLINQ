using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizableLINQBenchmark
{

    using SampleData;
    using OptimizableLINQ;

    public static class TestSuite2015
    {

        public static void RunAll(ICollection<Product> products)
        {
            Console.WriteLine("* * * TESTS SUITE * * *");
            SamePriceAsProblemCompiled(products);
            SamePriceAsProblemRuntime(products);
        }

        public static void SamePriceAsProblemCompiled(IEnumerable<Product> products)
        {
            Console.WriteLine("* * * SAME PRICE AS PROBLEM - COMPILED * * *");
            
            TestFactoringOut.innerQueryOriginal(products);
            TestFactoringOut.innerQueryOriginalPLINQ(products);
            TestFactoringOut.suspendedInnerQueryTest(products);
            TestFactoringOut.suspendedInnerQueryPLINQ(products);
        }

        public static void SamePriceAsProblemRuntime(ICollection<Product> products)
        {
            Console.WriteLine("* * * SAME PRICE AS PROBLEM - RUNTIME * * *");

            TestingEnvironment.BenchmarkOptimisations((productsSrc) => from Product p in productsSrc
                                                                       where (from Product p2 in products where p2.productName == "Ikura" select p2.unitPrice).Contains(p.unitPrice)
                                                                       select p.productName, ref products,
                new OptimizationApplicator[] { new OptimizableLINQApplicator(), new ParallelLINQApplicator(), new OptimizationCompositionApplicator(new ParallelLINQApplicator(), new OptimizableLINQApplicator()) }, "SamePrice problem");

        }

        public static void MaxPriceAsProblemCompiled(IEnumerable<Product> products)
        {

        }

        public static void MaxPriceAsProblemRuntime(ICollection<Product> products)
        {

        }

        public static void PromoProductsProblemCompiled(IEnumerable<Product> products)
        {

        }

        public static void PythagorianTriplesProblemCompiled()
        {

        }
    }

    public static class TestSuite2016
    {

        public static void RunAll(ICollection<ProductX> products)
        {
            Console.WriteLine("* * * TESTS SUITE * * *");
            UniquePriceProblemCompiled(products);

        }

        public static void UniquePriceProblemCompiled(IEnumerable<ProductX> products)
        {
            Console.WriteLine("* * * UNIQUE PRICE AS PROBLEM - COMPILED * * *");



//            TestVolatileIndexing.uniqueUnitPriceQueryOriginal(products);
//            TestVolatileIndexing.uniqueUnitPriceVolatileIndex(products);
//           TestVolatileIndexing.uniqueUnitPriceAlternatives(products);

            TestVolatileIndexing.nullSomeUnitPrices(products, false, true);
            TestVolatileIndexing.sameUnitPriceVolatileIndex(products);
            TestVolatileIndexing.sameUnitPriceAlternatives(products);
            TestVolatileIndexing.sameUnitPriceQueryOriginal(products);

        }
 
    }
}
