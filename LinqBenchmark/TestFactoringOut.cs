using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nessos.LinqOptimizer.CSharp;

namespace LINQBenchmark
{
    using LINQSampleData;
    using LINQOptimizer;

    public static class TestFactoringOut
    {

        // Possibly incorrect for empty source
        public static void innerQueryGroupByTest(IEnumerable<Product> products)
        {
            TestingEnvironment.ExtendedTest(() => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).GroupBy(u => 0).SelectMany(uEnumerable => products.Where(p => uEnumerable.Contains(p.unitPrice)).Select(p => p.productName)),
                   ref products,
                   "Optimized Ikura With GroupBy-SelectMany",
                   "products.Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).GroupBy(u => 0).SelectMany(uEnumerable => products.Where(p => uEnumerable.Contains(p.unitPrice)).Select(p => p.productName))"
                   );
        }

        public static void innerQueryOriginal(IEnumerable<Product> products)
        {
            TestingEnvironment.ExtendedTest(() => from Product p in products
                                                  where (from Product p2 in products where p2.productName == "Ikura" select p2.unitPrice).Contains(p.unitPrice)
                                                  select p.productName,
                           ref products,
                           "Original Ikura Query Expession",
                           "from Product p in products\n where (from Product p2 in products where p2.productName == \"Ikura\" select p2.unitPrice).Contains(p.unitPrice)\nselect p.productName"
                           );

        }

        public static void innerQueryTest(IEnumerable<Product> products)
        {
            TestingEnvironment.ExtendedTest(() => OptimizerExtensions.AsGroup(() => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).ToList()).SelectMany(uEnumerable => products.Where(p => uEnumerable.Contains(p.unitPrice)).Select(p => p.productName)),
                               ref products,
                               "Optimized Ikura With AsGroupSelectMany operator",
                               "OptimizerExtensions.AsGroup(() => products.Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).ToList()).SelectMany(uEnumerable => products.Where(p => uEnumerable.Contains(p.unitPrice)).Select(p => p.productName))"
                               );

        }


        public static void suspendedInnerQueryTest(IEnumerable<Product> products)
        {

            TestingEnvironment.ExtendedTest(() => OptimizerExtensions.AsGroupSuspended(() => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).ToList()).SelectMany(uThunk => products.Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName)),
                               ref products,
                               "Optimized Ikura With AsGroupSuspendedSelectMany operator using Func",
                               "OptimizerExtensions.AsGroupSuspended(() => products.Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).ToList()).SelectMany(uThunk => products.Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName))"
                               );
            
            TestingEnvironment.ExtendedTest(() => OptimizerExtensions.AsGroupSuspended(products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice)).SelectMany(uThunk => products.Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName)),
                               ref products,
                               "Optimized Ikura With AsGroupSuspendedSelectMany operator using Enumerable",
                               "OptimizerExtensions.AsGroupSuspended(products.Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice)).SelectMany(uThunk => products.Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName))"
                               );


        }

        public static void innerQueryWithLinqOptimizerTest(IEnumerable<Product> products)
        {

            TestingEnvironment.ExtendedTest(() => products.AsQueryExpr().Where(p => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).Contains(p.unitPrice)).Select(p => p.productName).Compile(),
                           ref products,
                           "Original Ikura with LinqOptimizer",
                           "products.AsQueryExpr().Where(p => products.Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).Contains(p.unitPrice)).Select(p=>p.productName).Compile()"
                           );

            TestingEnvironment.ExtendedTest(() => products.AsParallelQueryExpr().Where(p => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).Contains(p.unitPrice)).Select(p => p.productName).Compile(),
                           ref products,
                           "Original Ikura with Parallel LinqOptimizer",
                           "products.AsParallelQueryExpr().Where(p => products.Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).Contains(p.unitPrice)).Select(p=>p.productName).Compile()"
                           );

            TestingEnvironment.ExtendedTest(() => OptimizerExtensions.AsGroupSuspended(products.AsQueryExpr().Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).Compile()).AsQueryExpr().SelectMany(uThunk => products.Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName)).Compile(),
                               ref products,
                               "Optimized Ikura With AsGroupSuspendedSelectMany operator with LinqOptimizer",
                               "OptimizerExtensions.AsGroupSuspended(products.AsQueryExpr().Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).Compile())).AsQueryExpr().SelectMany(uThunk => products.Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName)).Compile()"
                               );

            TestingEnvironment.ExtendedTest(() => OptimizerExtensions.AsGroupSuspended(products.AsParallelQueryExpr().Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).Compile()).AsParallelQueryExpr().SelectMany(uThunk => products.Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName)).Compile(),
                               ref products,
                               "Optimized Ikura With AsGroupSuspendedSelectMany operator with Parallel LinqOptimizer",
                               "OptimizerExtensions.AsGroupSuspended(products.AsParallelQueryExpr().Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).Compile()).AsParallelQueryExpr().SelectMany(uThunk => products.Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName)).Compile()"
                               );

        }

        public static void innerQueryWithPLINQ(IEnumerable<Product> products)
        {
            TestingEnvironment.ExtendedTest(() => OptimizerExtensions.AsGroupSuspended(() => products.AsParallel().Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).ToList()).SelectMany(uThunk => products.Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName)),
                               ref products,
                               "Optimized Ikura With AsGroupSuspendedSelectMany operator with PLINQ - variant 0",
                               "OptimizerExtensions.AsGroupSuspended(() => products.AsParalllel().Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).ToList()).SelectMany(uThunk => products.Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName))"
                               );

            TestingEnvironment.ExtendedTest(() => OptimizerExtensions.AsGroupSuspended(() => products.AsParallel().Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).ToList()).AsParallel().SelectMany(uThunk => products.Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName)),
                               ref products,
                               "Optimized Ikura With AsGroupSuspendedSelectMany operator with PLINQ - variant 1",
                               "OptimizerExtensions.AsGroupSuspended(() => products.AsParalllel().Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).ToList()).AsParallel().SelectMany(uThunk => products.Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName))"
                               );

            TestingEnvironment.ExtendedTest(() => OptimizerExtensions.AsGroupSuspended(() => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).ToList()).AsParallel().SelectMany(uThunk => products.Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName)),
                               ref products,
                               "Optimized Ikura With AsGroupSuspendedSelectMany operator with PLINQ - variant 2",
                               "OptimizerExtensions.AsGroupSuspended(() => products.Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).ToList()).AsParallel().SelectMany(uThunk => products.Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName))"
                               );

            TestingEnvironment.ExtendedTest(() => OptimizerExtensions.AsGroupSuspendedThreadSafe(() => products.AsParallel().Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).ToList()).SelectMany(uThunk => products.AsParallel().Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName)),
                               ref products,
                               "Optimized Ikura With AsGroupSuspendedSelectMany operator with PLINQ - variant 3",
                               "OptimizerExtensions.AsGroupSuspended(() => products.AsParalllel().Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).ToList()).SelectMany(uThunk => products.AsParallel().Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName))"
                               );

            TestingEnvironment.ExtendedTest(() => OptimizerExtensions.AsGroupSuspendedThreadSafe(() => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).ToList()).SelectMany(uThunk => products.AsParallel().Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName)),
                    ref products,
                    "Optimized Ikura With AsGroupSuspendedSelectMany operator with PLINQ - variant 4",
                    "OptimizerExtensions.AsGroupSuspended(() => products.Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).ToList()).SelectMany(uThunk => products.AsParallel().Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName))"
                    );

            TestingEnvironment.ExtendedTest(() => products.AsParallel().Where(p => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).Contains(p.unitPrice)).Select(p => p.productName),
                           ref products,
                           "Original Ikura with PLINQ",
                           "products.AsParallel().Where(p => products.Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).Contains(p.unitPrice)).Select(p=>p.productName)"
                           );

 
        }

        public static void singleResultOriginalWithLet(IEnumerable<Product> products)
        {
            TestingEnvironment.ExtendedTest(() => from Product p in products
                                let maxUnitPrice = (from Product p2 in products select p2.unitPrice).Max()
                                where maxUnitPrice == p.unitPrice
                                select p.productName,
                            ref products,
                            "Original Max Query Expession with let",
                            "from Product p in products\n let maxUnitPrice = (from Product p2 in products select p2.unitPrice).Max()\n where maxUnitPrice == p.unitPrice\nselect p.productName"
                            );
        }

        // 
        public static void singleResultOriginal(IEnumerable<Product> products)
        {
            TestingEnvironment.ExtendedTest(() => from Product p in products
                                                  where (from Product p2 in products select p2.unitPrice).Max() == p.unitPrice
                                                  select p.productName,
                            ref products,
                            "Original Max Query Expession",
                            "from Product p in products\n where (from Product p2 in products select p2.unitPrice).Max() == p.unitPrice\nselect p.productName"
                            );
        }

        public static void singleResultTest(IEnumerable<Product> products)
        {
            TestingEnvironment.ExtendedTest(() => OptimizerExtensions.AsGroup(() => products.Select(p2 => p2.unitPrice).Max()).SelectMany(uMax => products.Where(p => uMax == p.unitPrice).Select(p => p.productName)),
                               ref products,
                               "Optimized Max With AsGroupSelectMany operator",
                               "OptimizerExtensions.AsGroup(() => products.Select(p2 => p2.unitPrice).Max()).SelectMany(uMax => products.Where(p => uMax == p.unitPrice).Select(p => p.productName))"
                               );

            TestingEnvironment.ExtendedTest(() => OptimizerExtensions.AsGroup(() => products.Select(p2 => p2.unitPrice).Max()).Select(uMax => products.Where(p => uMax == p.unitPrice).Select(p => p.productName)).First(),
                   ref products,
                   "Optimized Max With AsGroupSelectFirst operator",
                   "OptimizerExtensions.AsGroup(() => products.Select(p2 => p2.unitPrice).Max()).Select(uMax => products.Where(p => uMax == p.unitPrice).Select(p => p.productName)).First()"
                   );


            TestingEnvironment.ExtendedTest(() => new List<Func<double>>(1) { (() => products.Select(p2 => p2.unitPrice).Max()) }.Select(uFunc => uFunc()).SelectMany(uMax => products.Where(p => uMax == p.unitPrice).Select(p => p.productName)),
                               ref products,
                               "Optimized Max With newListSelectMany operator",
                               "new List<Func<double>> { (() => products.Select(p2 => p2.unitPrice).Max()) }.Select(uFunc => uFunc()).SelectMany(uMax => products.Where(p => uMax == p.unitPrice).Select(p => p.productName))"
                               );

        }

        public static void suspendedSingleResultTest(IEnumerable<Product> products)
        {
            TestingEnvironment.ExtendedTest(() => OptimizerExtensions.AsGroupSuspended(() => products.Select(p2 => p2.unitPrice).Max()).SelectMany(uMaxThunk => products.Where(p => uMaxThunk.Value == p.unitPrice).Select(p => p.productName)),
                               ref products,
                               "Optimized Max With AsGroupSuspendedSelectMany operator",
                               "OptimizerExtensions.AsGroupSuspended(() => products.Select(p2 => p2.unitPrice).Max()).SelectMany(uMaxThunk => products.Where(p => uMaxThunk.Value == p.unitPrice).Select(p => p.productName))"
                               );

        }


        public static void singleExpressionOriginal(IEnumerable<Product> products)
        {

            TestingEnvironment.ExtendedTest(() => products.Where(p => products.Any(p2 => p2.unitPrice == Math.Round(p.unitPrice / 1.2, 2))).Select(p => p.productName),
                            ref products,
                            "Original single Lambda Expession",
                            "products.Where(p => products.Any(p2 => p2.unitPrice == Math.Round(p.unitPrice / 1.2, 2))).Select(p => p.productName)"
                            );
        }

        public static void singleExpressionTest(IEnumerable<Product> products)
        {

            TestingEnvironment.ExtendedTest(() => products.Where(p => OptimizerExtensions.AsGroup(() => Math.Round(p.unitPrice / 1.2, 2)).Select(pup => products.Any(p2 => p2.unitPrice == pup)).First()).Select(p => p.productName),
                               ref products,
                               "Optimized single With AsGroupSelectFirst operator",
                               "products.Where(p => OptimizerExtensions.AsGroup(() => Math.Round(p.unitPrice / 1.2, 2)).Select(pup => products.Any(p2 => p2.unitPrice == pup)).First()).Select(p => p.productName)"
                               );

        }

        public static void suspendedSingleExpressionTest(IEnumerable<Product> products)
        {

            TestingEnvironment.ExtendedTest(() => products.Where(p => OptimizerExtensions.AsGroupSuspended(() => Math.Round(p.unitPrice / 1.2, 2)).Select(pup => products.Any(p2 => p2.unitPrice == pup.Value)).First()).Select(p => p.productName),
                               ref products,
                               "Optimized single With AsGroupSuspendedSelectFirst operator",
                               "products.Where(p => OptimizerExtensions.AsGroupSuspended(() => Math.Round(p.unitPrice / 1.2, 2)).Select(pup => products.Any(p2 => p2.unitPrice == pup.Value)).First()).Select(p => p.productName)"
                               );

        }

        public static void simplerExpressionOriginal(IEnumerable<Product> products)
        {
            TestingEnvironment.ExtendedTest(() => products.Where(p => products.Where(p2 => p2.unitPrice == p.unitPrice / 1.2).Count() == 1).Select(p => p.productName),
                            ref products,
                            "Original simpler Lambda Expession",
                            "products.Where(p => products.Where(p2 => p2.unitPrice == p.unitPrice / 1.2).Count() == 1).Select(p => p.productName)"
                            );

            TestingEnvironment.ExtendedTest(() => from p in products
                                                  let pup10 = p.unitPrice / 1.2
                                                  where (from p2 in products where p2.unitPrice == pup10 select p2).Count() == 1
                                                  select p.productName,
                            ref products,
                            "Original simpler Query Expession with let",
                            "from Product p in products\nlet pup10 = p.unitPrice / 1.2\nwhere (from Product p2 in products where p2.unitPrice == pup10 select p2).Count() > 0\nselect p.productName"
                            );
        }

        public static void simplerExpressionTest(IEnumerable<Product> products)
        {

            TestingEnvironment.ExtendedTest(() => products.Where(p => OptimizerExtensions.AsGroup(() => p.unitPrice / 1.2).SelectMany(pup10 => products.Where(p2 => p2.unitPrice == pup10)).Count() == 1).Select(p => p.productName),
                               ref products,
                               "Optimized simpler With AsGroupSelectMany operator",
                               "products.Where(p => OptimizerExtensions.AsGroup(() => p.unitPrice / 1.2).SelectMany(pup10 => products.Where(p2 => p2.unitPrice == pup10)).Count() == 1).Select(p => p.productName)"
                               );

            // Permissible variant, because First() is deferred (within lambda). 
            TestingEnvironment.ExtendedTest(() => products.Where(p => OptimizerExtensions.AsGroup(() => p.unitPrice / 1.2).Select(pup10 => products.Where(p2 => p2.unitPrice == pup10)).First().Count() == 1).Select(p => p.productName),
                   ref products,
                   "Optimized simpler With AsGroupSelectFirst operator",
                   "products.Where(p => OptimizerExtensions.AsGroup(() => p.unitPrice / 1.2).Select(pup10 => products.Where(p2 => p2.unitPrice == pup10)).First().Count() == 1).Select(p => p.productName)"
                   );


        }

        public static void suspendedSimplerExpressionTest(IEnumerable<Product> products)
        {

            TestingEnvironment.ExtendedTest(() => products.Where(p => OptimizerExtensions.AsGroupSuspended(() => p.unitPrice / 1.2).SelectMany(pup10 => products.Where(p2 => p2.unitPrice == pup10.Value)).Count() == 1).Select(p => p.productName),
                               ref products,
                               "Optimized simpler With AsGroupSuspendedSelectMany operator",
                               "products.Where(p => OptimizerExtensions.AsGroupSuspended(() => p.unitPrice / 1.2).SelectMany(pup10 => products.Where(p2 => p2.unitPrice == pup10.Value)).Count() == 1).Select(p => p.productName)"
                               );

            // Permissible variant, because First() is deferred (within lambda).
            TestingEnvironment.ExtendedTest(() => products.Where(p => OptimizerExtensions.AsGroupSuspended(() => p.unitPrice / 1.2).Select(pup10 => products.Where(p2 => p2.unitPrice == pup10.Value)).First().Count() == 1).Select(p => p.productName),
                               ref products,
                               "Optimized simpler With AsGroupSuspendedSelectFirst operator",
                               "products.Where(p => OptimizerExtensions.AsGroupSuspended(() => p.unitPrice / 1.2).Select(pup10 => products.Where(p2 => p2.unitPrice == pup10.Value)).First().Count() == 1).Select(p => p.productName)"
                               );

        }


    }
}
