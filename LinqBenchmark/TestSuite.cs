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

        public static void RunAll(ICollection<Product> products)
        {
            Console.WriteLine("* * * TESTS SUITE * * *");
            UniquePriceProblemCompiled(products, null);

        }

        public static void UniquePriceProblemCompiled(IEnumerable<Product> products, IEnumerable<ProductX> productsX)
        {
            Console.WriteLine("\n* * * UNIQUE PRICE AS PROBLEM - COMPILED * * *\n");

//            TestVolatileIndexing.volatileIndexCreation(productsX);
//            TestVolatileIndexing.uniqueUnitPriceQueryOriginal(productsX);
//            TestVolatileIndexing.uniqueUnitPriceVolatileIndex(productsX);
//            TestVolatileIndexing.uniqueUnitPriceAlternatives(productsX);

            Console.WriteLine("\n* * * UNIQUE CATEGORY AS PROBLEM - COMPILED * * *\n");

//            TestVolatileIndexing.nullSomeCategories(products);
            TestVolatileIndexing.uniqueCategoryQueryOriginal(products);
            TestVolatileIndexing.uniqueCategoryAlternatives(products);
            TestVolatileIndexing.uniqueCategoryVolatileIndex(products);
            TestVolatileIndexing.uniqueCategoryPLINQ(products);
            

            Console.WriteLine("\n* * * SAME PRICE AS PROBLEM - COMPILED * * *\n");

//            TestVolatileIndexing.nullSomeCategories(products);
            TestVolatileIndexing.sameUnitPriceQueryOriginal(products);
            TestVolatileIndexing.sameUnitPriceAlternatives(products);
            TestVolatileIndexing.sameUnitPriceVolatileIndex(products);
            TestVolatileIndexing.sameUnitPricePLINQ(products);
            

/**/

        }
 
    }
}
