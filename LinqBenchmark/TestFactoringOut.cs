using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nessos.LinqOptimizer.CSharp;

namespace OptimizableLINQBenchmark
{
    using SampleData;
    using OptimizableLINQ;

    public static class TestFactoringOut
    {

        // Possibly incorrect for empty source
        public static void innerQueryGroupByTest(IEnumerable<Product> products)
        {
            TestingEnvironment.BenchmarkQuery(() => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).GroupBy(u => 0).SelectMany(uEnumerable => products.Where(p => uEnumerable.Contains(p.unitPrice)).Select(p => p.productName)),
                   ref products,
                   "Optimized Ikura With GroupBy-SelectMany",
                   "products.Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).GroupBy(u => 0).SelectMany(uEnumerable => products.Where(p => uEnumerable.Contains(p.unitPrice)).Select(p => p.productName))"
                   );
        }

        public static void innerQueryOriginal(IEnumerable<Product> products)
        {
            TestingEnvironment.BenchmarkQuery(() => from Product p in products
                                                  where (from Product p2 in products where p2.productName == "Ikura" select p2.unitPrice).Contains(p.unitPrice)
                                                  select p.productName,
                           ref products,
                           "Original Ikura Query Expession",
                           "from Product p in products\n where (from Product p2 in products where p2.productName == \"Ikura\" select p2.unitPrice).Contains(p.unitPrice)\nselect p.productName"
                           );

        }

        public static void innerQueryTest(IEnumerable<Product> products)
        {
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroupEager(() => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).ToList()).SelectMany(uEnumerable => products.Where(p => uEnumerable.Contains(p.unitPrice)).Select(p => p.productName)),
                               ref products,
                               "Optimized Ikura With AsGroupSelectMany operator",
                               "OptimizerExtensions.AsGroup(() => products.Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).ToList()).SelectMany(uEnumerable => products.Where(p => uEnumerable.Contains(p.unitPrice)).Select(p => p.productName))"
                               );

        }


        public static void suspendedInnerQueryTest(IEnumerable<Product> products)
        {

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).ToMaterializable()).SelectMany(uThunk => products.Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName)),
                               ref products,
                               "Optimized Ikura With AsGroupSuspendedSelectMany operator using Materializable",
                               "OptimizerExtensions.AsGroup(() => products.Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).ToMaterializable()).SelectMany(uThunk => products.Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName))"
                               );

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).ToList()).SelectMany(uThunk => products.Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName)),
                               ref products,
                               "Optimized Ikura With AsGroupSuspendedSelectMany operator using Func",
                               "OptimizerExtensions.AsGroup(() => products.Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).ToList()).SelectMany(uThunk => products.Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName))"
                               );
            
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice)).SelectMany(uThunk => products.Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName)),
                               ref products,
                               "Optimized Ikura With AsGroupSuspendedSelectMany operator using Enumerable",
                               "OptimizerExtensions.AsGroup(products.Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice)).SelectMany(uThunk => products.Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName))"
                               );


        }

        public static void innerQueryOriginalOthersTest(IEnumerable<Product> products)
        {
            innerQueryOriginalPLINQ(products);

            TestingEnvironment.BenchmarkQuery(() => products.AsQueryExpr().Where(p => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).Contains(p.unitPrice)).Select(p => p.productName).Compile(),
                           ref products,
                           "Original Ikura with LinqOptimizer",
                           "products.AsQueryExpr().Where(p => products.Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).Contains(p.unitPrice)).Select(p=>p.productName).Compile()"
                           );

            TestingEnvironment.BenchmarkQuery(() => products.AsParallelQueryExpr().Where(p => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).Contains(p.unitPrice)).Select(p => p.productName).Compile(),
                           ref products,
                           "Original Ikura with Parallel LinqOptimizer",
                           "products.AsParallelQueryExpr().Where(p => products.Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).Contains(p.unitPrice)).Select(p=>p.productName).Compile()"
                           );
        }

        public static IEnumerable<Product> innerQueryOriginalPLINQ(IEnumerable<Product> products)
        {
            TestingEnvironment.BenchmarkQuery(() => products.AsParallel().Where(p => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).Contains(p.unitPrice)).Select(p => p.productName),
                           ref products,
                           "Original Ikura with PLINQ",
                           "products.AsParallel().Where(p => products.Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).Contains(p.unitPrice)).Select(p=>p.productName)"
                           );
            return products;
        }

        public static void suspendedInnerQueryOthersTest(IEnumerable<Product> products)
        {

            suspendedInnerQueryPLINQ(products);

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(products.AsQueryExpr().Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).Compile()).AsQueryExpr().SelectMany(uThunk => products.Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName)).Compile(),
                   ref products,
                   "Optimized Ikura With AsGroupSuspendedSelectMany operator with LinqOptimizer",
                   "OptimizerExtensions.AsGroup(products.AsQueryExpr().Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).Compile())).AsQueryExpr().SelectMany(uThunk => products.Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName)).Compile()"
                   );

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroupSuspendedThreadSafe(products.AsParallelQueryExpr().Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).Compile()).SelectMany(uThunk => products.AsParallelQueryExpr().Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName).Run()),
                               ref products,
                               "Optimized Ikura With AsGroupSuspendedSelectMany operator with Parallel LinqOptimizer",
                               "OptimizerExtensions.AsGroupSuspendedThreadSafe(products.AsParallelQueryExpr().Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).Compile()).SelectMany(uThunk => products.AsParallelQueryExpr().Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName).Run())"
                               );
        }

        public static IEnumerable<Product> suspendedInnerQueryPLINQ(IEnumerable<Product> products)
        {
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroupSuspendedThreadSafe(() => products.AsParallel().Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).ToList()).SelectMany(uThunk => products.AsParallel().Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName)),
                               ref products,
                               "Optimized Ikura With AsGroupSuspendedSelectMany operator with PLINQ",
                               "OptimizerExtensions.AsGroup(() => products.AsParallel().Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).ToList()).SelectMany(uThunk => products.AsParallel().Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName))"
                               );
            return products;
        }

        public static void singleResultOriginalWithLet(IEnumerable<Product> products)
        {
            TestingEnvironment.BenchmarkQuery(() => from Product p in products
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
            TestingEnvironment.BenchmarkQuery(() => from Product p in products
                                                  where (from Product p2 in products select p2.unitPrice).Max() == p.unitPrice
                                                  select p.productName,
                            ref products,
                            "Original Max Query Expession",
                            "from Product p in products\n where (from Product p2 in products select p2.unitPrice).Max() == p.unitPrice\nselect p.productName"
                            );
        }

        public static void singleResultTest(IEnumerable<Product> products)
        {
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroupEager(() => products.Select(p2 => p2.unitPrice).Max()).SelectMany(uMax => products.Where(p => uMax == p.unitPrice).Select(p => p.productName)),
                               ref products,
                               "Optimized Max With AsGroupSelectMany operator",
                               "OptimizerExtensions.AsGroupEager(() => products.Select(p2 => p2.unitPrice).Max()).SelectMany(uMax => products.Where(p => uMax == p.unitPrice).Select(p => p.productName))"
                               );

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroupEager(() => products.Select(p2 => p2.unitPrice).Max()).Select(uMax => products.Where(p => uMax == p.unitPrice).Select(p => p.productName)).First(),
                   ref products,
                   "Optimized Max With AsGroupSelectFirst operator",
                   "OptimizerExtensions.AsGroupEager(() => products.Select(p2 => p2.unitPrice).Max()).Select(uMax => products.Where(p => uMax == p.unitPrice).Select(p => p.productName)).First()"
                   );


            TestingEnvironment.BenchmarkQuery(() => new List<Func<double>>(1) { (() => products.Select(p2 => p2.unitPrice).Max()) }.Select(uFunc => uFunc()).SelectMany(uMax => products.Where(p => uMax == p.unitPrice).Select(p => p.productName)),
                               ref products,
                               "Optimized Max With newListSelectMany operator",
                               "new List<Func<double>> { (() => products.Select(p2 => p2.unitPrice).Max()) }.Select(uFunc => uFunc()).SelectMany(uMax => products.Where(p => uMax == p.unitPrice).Select(p => p.productName))"
                               );

        }

        public static void suspendedSingleResultTest(IEnumerable<Product> products)
        {
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.Select(p2 => p2.unitPrice).Max()).SelectMany(uMaxThunk => products.Where(p => uMaxThunk.Value == p.unitPrice).Select(p => p.productName)),
                               ref products,
                               "Optimized Max With AsGroupSuspendedSelectMany operator",
                               "OptimizerExtensions.AsGroup(() => products.Select(p2 => p2.unitPrice).Max()).SelectMany(uMaxThunk => products.Where(p => uMaxThunk.Value == p.unitPrice).Select(p => p.productName))"
                               );

        }

        public static void singleResultOriginalOthersTest(IEnumerable<Product> products)
        {
            TestingEnvironment.BenchmarkQuery(() => from Product p in products.AsParallel()
                                                  where (from Product p2 in products select p2.unitPrice).Max() == p.unitPrice
                                                  select p.productName,
                            ref products,
                            "Original Max Query Expession with PLINQ",
                            "from Product p in products.AsParallel()\n where (from Product p2 in products select p2.unitPrice).Max() == p.unitPrice\nselect p.productName"
                            );

            TestingEnvironment.BenchmarkQuery(() => products.AsQueryExpr().Where(p => products.Select(p2 => p2.unitPrice).Max() == p.unitPrice).Select(p => p.productName).Compile(),
                            ref products,
                            "Original Max Expession with LinqOptimizer",
                            "products.AsQueryExpr().Where(p => products.Select(p2 => p2.unitPrice).Max() == p.unitPrice).Select(p => p.productName).Compile()"
                            );

            TestingEnvironment.BenchmarkQuery(() => products.AsParallelQueryExpr().Where(p => products.Select(p2 => p2.unitPrice).Max() == p.unitPrice).Select(p => p.productName).Compile(),
                            ref products,
                            "Original Max Expession with ParallelLinqOptimizer",
                            "products.AsParallelQueryExpr().Where(p => products.Select(p2 => p2.unitPrice).Max() == p.unitPrice).Select(p => p.productName).Compile()"
                            );
        }

        public static void suspendedSingleResultOthersTest(IEnumerable<Product> products)
        {
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroupSuspendedThreadSafe(() => products.AsParallel().Select(p2 => p2.unitPrice).Max()).SelectMany(uMaxThunk => products.AsParallel().Where(p => uMaxThunk.Value == p.unitPrice).Select(p => p.productName)),
                               ref products,
                               "Optimized Max With AsGroupSuspendedSelectMany operator with PLINQ",
                               "OptimizerExtensions.AsGroupSuspendedThreadSafe(() => products.AsParallel().Select(p2 => p2.unitPrice).Max()).SelectMany(uMaxThunk => products.AsParallel().Where(p => uMaxThunk.Value == p.unitPrice).Select(p => p.productName))"
                               );

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.AsQueryExpr().Select(p2 => p2.unitPrice).Run().Max()).AsQueryExpr().SelectMany(uMaxThunk => products.Where(p => uMaxThunk.Value == p.unitPrice).Select(p => p.productName)).Compile(),
                               ref products,
                               "Optimized Max With AsGroupSuspendedSelectMany operator with LinqOptimizer",
                               "OptimizerExtensions.AsGroup(() => products.AsQueryExpr().Select(p2 => p2.unitPrice).Run().Max()).AsQueryExpr().SelectMany(uMaxThunk => products.Where(p => uMaxThunk.Value == p.unitPrice).Select(p => p.productName)).Compile()"
                               );

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroupSuspendedThreadSafe(() => products.AsParallelQueryExpr().Select(p2 => p2.unitPrice).Run().Max()).SelectMany(uMaxThunk => products.AsParallelQueryExpr().Where(p => uMaxThunk.Value == p.unitPrice).Select(p => p.productName).Run()),
                               ref products,
                               "Optimized Max With AsGroupSuspendedSelectMany operator with Parallel LinqOptimizer",
                               "OptimizerExtensions.AsGroupSuspendedThreadSafe(() => products.AsParallelQueryExpr().Select(p2 => p2.unitPrice).Run().Max()).SelectMany(uMaxThunk => products.AsParallelQueryExpr().Where(p => uMaxThunk.Value == p.unitPrice).Select(p => p.productName).Run())"
                               );

        }

        public static void singleExpressionOriginal(IEnumerable<Product> products)
        {

            TestingEnvironment.BenchmarkQuery(() => products.Where(p => products.Any(p2 => p2.unitPrice == Math.Round(p.unitPrice / 1.2, 2))).Select(p => p.productName),
                            ref products,
                            "Original single Lambda Expession",
                            "products.Where(p => products.Any(p2 => p2.unitPrice == Math.Round(p.unitPrice / 1.2, 2))).Select(p => p.productName)"
                            );
        }

        public static void singleExpressionTest(IEnumerable<Product> products)
        {

            TestingEnvironment.BenchmarkQuery(() => products.Where(p => OptimizerExtensions.AsGroupEager(() => Math.Round(p.unitPrice / 1.2, 2)).Select(pup => products.Any(p2 => p2.unitPrice == pup)).First()).Select(p => p.productName),
                               ref products,
                               "Optimized single With AsGroupSelectFirst operator",
                               "products.Where(p => OptimizerExtensions.AsGroupEager(() => Math.Round(p.unitPrice / 1.2, 2)).Select(pup => products.Any(p2 => p2.unitPrice == pup)).First()).Select(p => p.productName)"
                               );

        }

        public static void suspendedSingleExpressionTest(IEnumerable<Product> products)
        {

            TestingEnvironment.BenchmarkQuery(() => products.Where(p => OptimizerExtensions.AsGroup(() => Math.Round(p.unitPrice / 1.2, 2)).Select(pup => products.Any(p2 => p2.unitPrice == pup.Value)).First()).Select(p => p.productName),
                               ref products,
                               "Optimized single With AsGroupSuspendedSelectFirst operator",
                               "products.Where(p => OptimizerExtensions.AsGroup(() => Math.Round(p.unitPrice / 1.2, 2)).Select(pup => products.Any(p2 => p2.unitPrice == pup.Value)).First()).Select(p => p.productName)"
                               );

        }

        public static void singleExpressionOriginalOthersTest(IEnumerable<Product> products)
        {

            TestingEnvironment.BenchmarkQuery(() => products.AsParallel().Where(p => products.Any(p2 => p2.unitPrice == Math.Round(p.unitPrice / 1.2, 2))).Select(p => p.productName),
                            ref products,
                            "Original single Lambda Expession with PLINQ",
                            "products.AsParallel().Where(p => products.AsParallel().Any(p2 => p2.unitPrice == Math.Round(p.unitPrice / 1.2, 2))).Select(p => p.productName)"
                            );

            TestingEnvironment.BenchmarkQuery(() => products.AsQueryExpr().Where(p => products.Any(p2 => p2.unitPrice == Math.Round(p.unitPrice / 1.2, 2))).Select(p => p.productName).Compile(),
                            ref products,
                            "Original single Lambda Expession with LinqOptimizer",
                            "products.AsQueryExpr().Where(p => products.Any(p2 => p2.unitPrice == Math.Round(p.unitPrice / 1.2, 2))).Select(p => p.productName).Compile()"
                            );

            TestingEnvironment.BenchmarkQuery(() => products.AsParallelQueryExpr().Where(p => products.Any(p2 => p2.unitPrice == Math.Round(p.unitPrice / 1.2, 2))).Select(p => p.productName).Compile(),
                            ref products,
                            "Original single Lambda Expession with Parallel LinqOptimizer",
                            "products.AsParallelQueryExpr().Where(p => products.Any(p2 => p2.unitPrice == Math.Round(p.unitPrice / 1.2, 2))).Select(p => p.productName).Compile()"
                            );
        }

        public static void suspendedSingleExpressionOthersTest(IEnumerable<Product> products)
        {

            TestingEnvironment.BenchmarkQuery(() => products.AsParallel().Where(p => OptimizerExtensions.AsGroup(() => Math.Round(p.unitPrice / 1.2, 2)).Select(pup => products.Any(p2 => p2.unitPrice == pup.Value)).First()).Select(p => p.productName),
                               ref products,
                               "Optimized single With AsGroupSuspendedSelectFirst operator with PLINQ",
                               "products.AsParallel().Where(p => OptimizerExtensions.AsGroup(() => Math.Round(p.unitPrice / 1.2, 2)).Select(pup => products.AsParallel().Any(p2 => p2.unitPrice == pup.Value)).First()).Select(p => p.productName)"
                               );

            TestingEnvironment.BenchmarkQuery(() => products.AsQueryExpr().Where(p => OptimizerExtensions.AsGroup(() => Math.Round(p.unitPrice / 1.2, 2)).Select(pup => products.Any(p2 => p2.unitPrice == pup.Value)).First()).Select(p => p.productName).Compile(),
                               ref products,
                               "Optimized single With AsGroupSuspendedSelectFirst operator with LinqOptimizer",
                               "products.AsParallelQueryExpr().Where(p => OptimizerExtensions.AsGroup(() => Math.Round(p.unitPrice / 1.2, 2)).Select(pup => products.Any(p2 => p2.unitPrice == pup.Value)).First()).Select(p => p.productName).Compile()"
                               );

            TestingEnvironment.BenchmarkQuery(() => products.AsParallelQueryExpr().Where(p => OptimizerExtensions.AsGroup(() => Math.Round(p.unitPrice / 1.2, 2)).Select(pup => products.Any(p2 => p2.unitPrice == pup.Value)).First()).Select(p => p.productName).Compile(),
                               ref products,
                               "Optimized single With AsGroupSuspendedSelectFirst operator with Parallel LinqOptimizer",
                               "products.AsParallelQueryExpr().Where(p => OptimizerExtensions.AsGroup(() => Math.Round(p.unitPrice / 1.2, 2)).Select(pup => products.Any(p2 => p2.unitPrice == pup.Value)).First()).Select(p => p.productName).Compile()"
                               );

        }

        public static void simplerExpressionOriginal(IEnumerable<Product> products)
        {
            TestingEnvironment.BenchmarkQuery(() => products.Where(p => products.Where(p2 => p2.unitPrice == p.unitPrice / 1.2).Count() == 1).Select(p => p.productName),
                            ref products,
                            "Original simpler Lambda Expession",
                            "products.Where(p => products.Where(p2 => p2.unitPrice == p.unitPrice / 1.2).Count() == 1).Select(p => p.productName)"
                            );

            TestingEnvironment.BenchmarkQuery(() => from p in products
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

            TestingEnvironment.BenchmarkQuery(() => products.Where(p => OptimizerExtensions.AsGroupEager(() => p.unitPrice / 1.2).SelectMany(pup10 => products.Where(p2 => p2.unitPrice == pup10)).Count() == 1).Select(p => p.productName),
                               ref products,
                               "Optimized simpler With AsGroupSelectMany operator",
                               "products.Where(p => OptimizerExtensions.AsGroupEager(() => p.unitPrice / 1.2).SelectMany(pup10 => products.Where(p2 => p2.unitPrice == pup10)).Count() == 1).Select(p => p.productName)"
                               );

            // Permissible variant, because First() is deferred (within lambda). 
            TestingEnvironment.BenchmarkQuery(() => products.Where(p => OptimizerExtensions.AsGroupEager(() => p.unitPrice / 1.2).Select(pup10 => products.Where(p2 => p2.unitPrice == pup10)).First().Count() == 1).Select(p => p.productName),
                   ref products,
                   "Optimized simpler With AsGroupSelectFirst operator",
                   "products.Where(p => OptimizerExtensions.AsGroupEager(() => p.unitPrice / 1.2).Select(pup10 => products.Where(p2 => p2.unitPrice == pup10)).First().Count() == 1).Select(p => p.productName)"
                   );


        }

        public static void suspendedSimplerExpressionTest(IEnumerable<Product> products)
        {

            TestingEnvironment.BenchmarkQuery(() => products.Where(p => OptimizerExtensions.AsGroup(() => p.unitPrice / 1.2).SelectMany(pup10 => products.Where(p2 => p2.unitPrice == pup10.Value)).Count() == 1).Select(p => p.productName),
                               ref products,
                               "Optimized simpler With AsGroupSuspendedSelectMany operator",
                               "products.Where(p => OptimizerExtensions.AsGroup(() => p.unitPrice / 1.2).SelectMany(pup10 => products.Where(p2 => p2.unitPrice == pup10.Value)).Count() == 1).Select(p => p.productName)"
                               );

            // Permissible variant, because First() is deferred (within lambda).
            TestingEnvironment.BenchmarkQuery(() => products.Where(p => OptimizerExtensions.AsGroup(() => p.unitPrice / 1.2).Select(pup10 => products.Where(p2 => p2.unitPrice == pup10.Value)).First().Count() == 1).Select(p => p.productName),
                               ref products,
                               "Optimized simpler With AsGroupSuspendedSelectFirst operator",
                               "products.Where(p => OptimizerExtensions.AsGroup(() => p.unitPrice / 1.2).Select(pup10 => products.Where(p2 => p2.unitPrice == pup10.Value)).First().Count() == 1).Select(p => p.productName)"
                               );

        }

        public static void nessosPythagoreanTriplesOriginal(int max)
        {
/*            
            TestingEnvironment.ExtendedIntTest(() => from a in Enumerable.Range(1, max + 1)
                                                  from b in Enumerable.Range(a, max + 1 - a)
                                                  from c in Enumerable.Range(b, max + 1 - b)
                                                  where a * a + b * b == c * c
                                                  select true,
                             ref max,
                            "Original Pythagorean Triples Query Expession",
                            "from a in Enumerable.Range(1, max + 1)\nfrom b in Enumerable.Range(a, max + 1 - a)\nfrom c in Enumerable.Range(b, max + 1 - b)\nwhere a * a + b * b == c * c\nselect true"
                            );
            */
            TestingEnvironment.BenchmarkQuery(() => Enumerable.Range(1, max + 1).SelectMany(a => Enumerable.Range(a, max + 1 - a).SelectMany(b => Enumerable.Range(b, max + 1 - b).Where(c => a * a + b * b == c * c).Select(r => true))).Count(),
                 ref max,
                "Original Pythagorean Triples Expession",
                "Enumerable.Range(1, max + 1).SelectMany(a => Enumerable.Range(a, max + 1 - a).SelectMany(b => Enumerable.Range(b, max + 1 - b).Where(c => a * a + b * b == c * c).Select(r => true))).Count()"
                );
        }


        public static void nessosPythagoreanTriplesQueryOthersTest(int max) 
        {

            TestingEnvironment.BenchmarkQuery(() => (from a in ParallelEnumerable.Range(1, max + 1)
                                                     from b in Enumerable.Range(a, max + 1 - a)
                                                     from c in Enumerable.Range(b, max + 1 - b)
                                                     where a * a + b * b == c * c
                                                   select true).Count(),
                             ref max,
                            "Original Pythagorean Triples Query Expession with PLINQ",
                            "(from a in ParallelEnumerable.Range(1, max + 1)\nfrom b in Enumerable.Range(a, max + 1 - a)\nfrom c in Enumerable.Range(b, max + 1 - b)\nwhere a * a + b * b == c * c\nselect true).Count()"
                            );

            TestingEnvironment.BenchmarkQuery(() => (from a in QueryExpr.Range(1, max + 1)
                                                     from b in Enumerable.Range(a, max + 1 - a)
                                                     from c in Enumerable.Range(b, max + 1 - b)
                                                     where a * a + b * b == c * c
                                                   select true).Count().Compile(),
                             ref max,
                            "Original Pythagorean Triples Query Expession with LinqOptimizer",
                            "(from a in QueryExpr.Range(1, max + 1)\nfrom b in Enumerable.Range(a, max + 1 - a)\nfrom c in Enumerable.Range(b, max + 1 - b)\nwhere a * a + b * b == c * c\nselect true).Count().Compile()"
                            );

            TestingEnvironment.BenchmarkQuery(() => (from a in Enumerable.Range(1, max + 1).AsParallelQueryExpr()
                                                     from b in Enumerable.Range(a, max + 1 - a)
                                                     from c in Enumerable.Range(b, max + 1 - b)
                                                     where a * a + b * b == c * c
                                                   select true).Count().Compile(),
                             ref max,
                            "Original Pythagorean Triples Query Expession with Parallel LinqOptimizer",
                            "(from a in Enumerable.Range(1, max + 1).AsParallelQueryExpr()\nfrom b in Enumerable.Range(a, max + 1 - a)\nfrom c in Enumerable.Range(b, max + 1 - b)\nwhere a * a + b * b == c * c\nselect true).Count().Compile()"
                            );
        }

        public static void nessosPythagoreanTriplesOriginalOthersTest(int max)
        {

            TestingEnvironment.BenchmarkQuery(() => Enumerable.Range(1, max + 1).AsParallel().SelectMany(a => Enumerable.Range(a, max + 1 - a).SelectMany(b => Enumerable.Range(b, max + 1 - b).Where(c => a * a + b * b == c * c).Select(r => true))).Count(),
                             ref max,
                            "Original Pythagorean Triples Expession with PLINQ",
                            "Enumerable.Range(1, max + 1).AsParallel().SelectMany(a => Enumerable.Range(a, max + 1 - a).SelectMany(b => Enumerable.Range(b, max + 1 - b).Where(c => a * a + b * b == c * c).Select(r => true))).Count()"
            );

            TestingEnvironment.BenchmarkQuery(() => QueryExpr.Range(1, max + 1).SelectMany(a => Enumerable.Range(a, max + 1 - a).SelectMany(b => Enumerable.Range(b, max + 1 - b).Where(c => a * a + b * b == c * c).Select(r => true))).Count().Compile(),
                             ref max,
                            "Original Pythagorean Triples Expession with LinqOptimizer",
                            "QueryExpr.Range(1, max + 1).SelectMany(a => Enumerable.Range(a, max + 1 - a).SelectMany(b => Enumerable.Range(b, max + 1 - b).Where(c => a * a + b * b == c * c).Select(r => true))).Count().Compile()"
            );

            TestingEnvironment.BenchmarkQuery(() => Enumerable.Range(1, max + 1).AsParallelQueryExpr().SelectMany(a => Enumerable.Range(a, max + 1 - a).SelectMany(b => Enumerable.Range(b, max + 1 - b).Where(c => a * a + b * b == c * c).Select(r => true))).Count().Compile(),
                             ref max,
                            "Original Pythagorean Triples Expession with Parallel LinqOptimizer",
                            "Enumerable.Range(1, max + 1).AsParallelQueryExpr().SelectMany(a => Enumerable.Range(a, max + 1 - a).SelectMany(b => Enumerable.Range(b, max + 1 - b).Where(c => a * a + b * b == c * c).Select(r => true))).Count().Compile()"
            );

        }

        public static void nessosPythagoreanTriplesTest(int max)
        {
            TestingEnvironment.BenchmarkQuery(() => Enumerable.Range(1, max + 1).SelectMany(a => OptimizerExtensions.AsGroupEager(() => a * a).SelectMany(asqr => Enumerable.Range(a, max + 1 - a).SelectMany(b => OptimizerExtensions.AsGroupEager(() => asqr + b * b).SelectMany(absqr => Enumerable.Range(b, max + 1 - b).Where(c => absqr == c * c).Select(r => true))))).Count(),
                 ref max,
                "Original Pythagorean Triples Expession with AsGroupSelectMany",
                "Enumerable.Range(1, max + 1).SelectMany(a => OptimizerExtensions.AsGroupEager(() => a * a).SelectMany(asqr => Enumerable.Range(a, max + 1 - a).SelectMany(b => OptimizerExtensions.AsGroup(() => asqr + b * b).SelectMany(absqr => Enumerable.Range(b, max + 1 - b).Where(c => absqr == c * c).Select(r => true))))).Count()"
                );

        }

        public static void nessosPythagoreanTriplesOthersTest(int max)
        {
            TestingEnvironment.BenchmarkQuery(() => Enumerable.Range(1, max + 1).AsParallel().SelectMany(a => OptimizerExtensions.AsGroupEager(() => a * a).SelectMany(asqr => Enumerable.Range(a, max + 1 - a).SelectMany(b => OptimizerExtensions.AsGroupEager(() => asqr + b * b).SelectMany(absqr => Enumerable.Range(b, max + 1 - b).Where(c => absqr == c * c).Select(r => true))))).Count(),
                 ref max,
                "Original Pythagorean Triples Expession with AsGroupSelectMany with PLINQ",
                "Enumerable.Range(1, max + 1).AsParallel().SelectMany(a => OptimizerExtensions.AsGroup(() => a * a).SelectMany(asqr => Enumerable.Range(a, max + 1 - a).SelectMany(b => OptimizerExtensions.AsGroup(() => asqr + b * b).SelectMany(absqr => Enumerable.Range(b, max + 1 - b).Where(c => absqr == c * c).Select(r => true))))).Count()"
                );

            TestingEnvironment.BenchmarkQuery(() => QueryExpr.Range(1, max + 1).SelectMany(a => OptimizerExtensions.AsGroupEager(() => a * a).SelectMany(asqr => Enumerable.Range(a, max + 1 - a).SelectMany(b => OptimizerExtensions.AsGroupEager(() => asqr + b * b).SelectMany(absqr => Enumerable.Range(b, max + 1 - b).Where(c => absqr == c * c).Select(r => true))))).Count().Compile(),
                 ref max,
                "Original Pythagorean Triples Expession with AsGroupSelectMany with LinqOptimizer",
                "QueryExpr.Range(1, max + 1).SelectMany(a => OptimizerExtensions.AsGroup(() => a * a).SelectMany(asqr => Enumerable.Range(a, max + 1 - a).SelectMany(b => OptimizerExtensions.AsGroup(() => asqr + b * b).SelectMany(absqr => Enumerable.Range(b, max + 1 - b).Where(c => absqr == c * c).Select(r => true))))).Count().Compile()"
                );

            TestingEnvironment.BenchmarkQuery(() => Enumerable.Range(1, max + 1).AsParallelQueryExpr().SelectMany(a => OptimizerExtensions.AsGroupEager(() => a * a).SelectMany(asqr => Enumerable.Range(a, max + 1 - a).SelectMany(b => OptimizerExtensions.AsGroupEager(() => asqr + b * b).SelectMany(absqr => Enumerable.Range(b, max + 1 - b).Where(c => absqr == c * c).Select(r => true))))).Count().Compile(),
                 ref max,
                "Original Pythagorean Triples Expession with AsGroupSelectMany with Parallel LinqOptimizer",
                "Enumerable.Range(1, max + 1).AsParallelQueryExpr().SelectMany(a => OptimizerExtensions.AsGroup(() => a * a).SelectMany(asqr => Enumerable.Range(a, max + 1 - a).SelectMany(b => OptimizerExtensions.AsGroup(() => asqr + b * b).SelectMany(absqr => Enumerable.Range(b, max + 1 - b).Where(c => absqr == c * c).Select(r => true))))).Count().Compile()"
                );
        }

        public static void suspendedNessosPythagoreanTriplesTest(int max)
        {
            TestingEnvironment.BenchmarkQuery(() => Enumerable.Range(1, max + 1).SelectMany(a => OptimizerExtensions.AsGroup(() => a * a).SelectMany(asqrThunk => Enumerable.Range(a, max + 1 - a).SelectMany(b => OptimizerExtensions.AsGroup(() => asqrThunk.Value + b * b).SelectMany(absqrThunk => Enumerable.Range(b, max + 1 - b).Where(c => absqrThunk.Value == c * c).Select(r => true))))).Count(),
                 ref max,
                "Original Pythagorean Triples Expession with AsGroupSuspendedSelectMany",
                "Enumerable.Range(1, max + 1).SelectMany(a => OptimizerExtensions.AsGroup(() => a * a).SelectMany(asqrThunk => Enumerable.Range(a, max + 1 - a).SelectMany(b => OptimizerExtensions.AsGroup(() => asqrThunk.Value + b * b).SelectMany(absqrThunk => Enumerable.Range(b, max + 1 - b).Where(c => absqrThunk.Value == c * c).Select(r => true))))).Count()"
                );

        }

        public static void suspendedNessosPythagoreanTriplesOthersTest(int max)
        {
            TestingEnvironment.BenchmarkQuery(() => Enumerable.Range(1, max + 1).AsParallel().SelectMany(a => OptimizerExtensions.AsGroup(() => a * a).SelectMany(asqrThunk => Enumerable.Range(a, max + 1 - a).SelectMany(b => OptimizerExtensions.AsGroup(() => asqrThunk.Value + b * b).SelectMany(absqrThunk => Enumerable.Range(b, max + 1 - b).Where(c => absqrThunk.Value == c * c).Select(r => true))))).Count(),
                 ref max,
                "Original Pythagorean Triples Expession with AsGroupSuspendedSelectMany with PLINQ",
                "Enumerable.Range(1, max + 1).AsParallel().SelectMany(a => OptimizerExtensions.AsGroup(() => a * a).SelectMany(asqrThunk => Enumerable.Range(a, max + 1 - a).SelectMany(b => OptimizerExtensions.AsGroup(() => asqrThunk.Value + b * b).SelectMany(absqrThunk => Enumerable.Range(b, max + 1 - b).Where(c => absqrThunk.Value == c * c).Select(r => true))))).Count()"
                );

            TestingEnvironment.BenchmarkQuery(() => QueryExpr.Range(1, max + 1).SelectMany(a => OptimizerExtensions.AsGroup(() => a * a).SelectMany(asqrThunk => Enumerable.Range(a, max + 1 - a).SelectMany(b => OptimizerExtensions.AsGroup(() => asqrThunk.Value + b * b).SelectMany(absqrThunk => Enumerable.Range(b, max + 1 - b).Where(c => absqrThunk.Value == c * c).Select(r => true))))).Count().Compile(),
                 ref max,
                "Original Pythagorean Triples Expession with AsGroupSuspendedSelectMany with LinqOptimizer",
                "QueryExpr.Range(1, max + 1).SelectMany(a => OptimizerExtensions.AsGroup(() => a * a).SelectMany(asqrThunk => Enumerable.Range(a, max + 1 - a).SelectMany(b => OptimizerExtensions.AsGroup(() => asqrThunk.Value + b * b).SelectMany(absqrThunk => Enumerable.Range(b, max + 1 - b).Where(c => absqrThunk.Value == c * c).Select(r => true))))).Count().Compile()"
                );

            TestingEnvironment.BenchmarkQuery(() => Enumerable.Range(1, max + 1).AsParallelQueryExpr().SelectMany(a => OptimizerExtensions.AsGroup(() => a * a).SelectMany(asqrThunk => Enumerable.Range(a, max + 1 - a).SelectMany(b => OptimizerExtensions.AsGroup(() => asqrThunk.Value + b * b).SelectMany(absqrThunk => Enumerable.Range(b, max + 1 - b).Where(c => absqrThunk.Value == c * c).Select(r => true))))).Count().Compile(),
                 ref max,
                "Original Pythagorean Triples Expession with AsGroupSuspendedSelectMany with Parallel LinqOptimizer",
                "Enumerable.Range(1, max + 1).AsParallelQueryExpr().SelectMany(a => OptimizerExtensions.AsGroup(() => a * a).SelectMany(asqrThunk => Enumerable.Range(a, max + 1 - a).SelectMany(b => OptimizerExtensions.AsGroup(() => asqrThunk.Value + b * b).SelectMany(absqrThunk => Enumerable.Range(b, max + 1 - b).Where(c => absqrThunk.Value == c * c).Select(r => true))))).Count().Compile()"
                );
        }

    }
}
