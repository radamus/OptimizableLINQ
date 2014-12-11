using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

/*          Valid only in case of exactly one Ikura in products collection
            
            TestingEnvironment.ExtendedTest(() => from prodIkura in products.AsQueryable()
                                                  where prodIkura.productName == "Ikura"
                                                  let IkuraPrice = prodIkura.unitPrice
                                                  from prod in products
                                                  where prod.unitPrice == IkuraPrice
                                                  select prod,
                           ref products,
                           "KOCHMAN Ikura Query Expession"
                           );*/
        }

        public static void innerQueryTest(IEnumerable<Product> products)
        {
/*
            TestingEnvironment.ExtendedTest(() => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).GroupSelect(uEnumerable => products.Where(p => uEnumerable.Contains(p.unitPrice)).Select(p => p.productName)),
                               ref products,
                               "Optimized Ikura With GroupSelect operator",
                               "products.Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).GroupSelect(uEnumerable => products.Where(p => uEnumerable.Contains(p.unitPrice)).Select(p => p.productName))"
                               );
*/
            //            ((Func<IEnumerable<double>>) (() => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).ToList())).ToEnumerable().
            TestingEnvironment.ExtendedTest(() => OptimizerExtensions.AsGroup(() => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).ToList()).SelectMany(uEnumerable => products.Where(p => uEnumerable.Contains(p.unitPrice)).Select(p => p.productName)),
                               ref products,
                               "Optimized Ikura With AsGroupSelectMany operator",
                               "OptimizerExtensions.AsGroup(() => products.Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).ToList()).SelectMany(uEnumerable => products.Where(p => uEnumerable.Contains(p.unitPrice)).Select(p => p.productName))"
                               );
/*
            TestingEnvironment.ExtendedTest(() => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).Group().SelectMany(uEnumerable => products.Where(p => uEnumerable.Contains(p.unitPrice)).Select(p => p.productName)),
                               ref products,
                               "Optimized Ikura With GroupSelectMany operator",
                               "products.Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).Group().SelectMany(uEnumerable => products.Where(p => uEnumerable.Contains(p.unitPrice)).Select(p => p.productName))"
                               );

            TestingEnvironment.ExtendedTest(() => new List<Func<IEnumerable<double>>>(1) { () => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).ToList() }.Select(uFunc => uFunc()).SelectMany(uEnumerable => products.Where(p => uEnumerable.Contains(p.unitPrice)).Select(p => p.productName)),
                               ref products,
                               "Optimized Ikura With newListSelectMany operator",
                               "new List<Func<IEnumerable<double>>>() { () => products.Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).ToList() }.Select(uFunc => uFunc()).SelectMany(uEnumerable => products.Where(p => uEnumerable.Contains(p.unitPrice)).Select(p => p.productName))"
                               );
*/
        }


        public static void suspendedInnerQueryTest(IEnumerable<Product> products)
        {
/*
            TestingEnvironment.ExtendedTest(() => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).DelayedGroupSelect(uLazy => products.Where(p => uLazy.Value.Contains(p.unitPrice)).Select(p => p.productName)),
                               ref products,
                               "Optimized Ikura With DelayedGroupSelect operator",
                               "products.Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).DelayedGroupSelect(uLazy => products.Where(p => uLazy.Value.Contains(p.unitPrice)).Select(p => p.productName))"
                               );

            TestingEnvironment.ExtendedTest(() => OptimizerExtensions.AsDelayedGroup(() => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).ToList()).SelectMany(uLazy => products.Where(p => uLazy.Value.Contains(p.unitPrice)).Select(p => p.productName)),
                               ref products,
                               "Optimized Ikura With AsDelayedGroupSelectMany operator",
                               "OptimizerExtensions.AsDelayedGroup(() => products.Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).ToList()).SelectMany(uLazy => products.Where(p => uLazy.Value.Contains(p.unitPrice)).Select(p => p.productName))"
                               );
*/
            TestingEnvironment.ExtendedTest(() => OptimizerExtensions.AsGroupSuspended(() => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).ToList()).SelectMany(uThunk => products.Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName)),
                               ref products,
                               "Optimized Ikura With AsGroupSuspendedSelectMany operator",
                               "OptimizerExtensions.AsGroupSuspended(() => products.Where(p2 => p2.productName == \"Ikura\").Select(p2 => p2.unitPrice).ToList()).SelectMany(uThunk => products.Where(p => uThunk.Value.Contains(p.unitPrice)).Select(p => p.productName))"
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
/*          GroupSelect Variant for single results is globally inefficient due to "uEnumerable.First()" 
    * 
            TestingEnvironment.ExtendedTest(() => OptimizerExtensions.AsGroup(() => products.Select(p2 => p2.unitPrice).Max()).GroupSelect(uEnumerable => products.Where(p => uEnumerable.First() == p.unitPrice).Select(p => p.productName)),
                                ref products,
                                "Optimized Max With GroupSelect operator",
                                "OptimizerExtensions.AsGroup(() => products.Select(p2 => p2.unitPrice).Max()).GroupSelect(uEnumerable => products.Where(p => uEnumerable.First() == p.unitPrice).Select(p => p.productName))"
                                );
*/
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
            /*          DelayedGroupSelect Variant for single results is globally inefficient due to "uEnumerable.First()" 
            * 
                        TestingEnvironment.ExtendedTest(() => OptimizerExtensions.AsGroup(() => products.Select(p2 => p2.unitPrice).Max()).DelayedGroupSelect(uLazy => products.Where(p => uLazy.Value.First() == p.unitPrice).Select(p => p.productName)),
                                           ref products,
                                           "Optimized Max With DelayedGroupSelect operator",
                                           "OptimizerExtensions.AsGroup(() => products.Select(p2 => p2.unitPrice).Max()).DelayedGroupSelect(uLazy => products.Where(p => uLazy.Value.First() == p.unitPrice).Select(p => p.productName))"
                                           );
            
            TestingEnvironment.ExtendedTest(() => OptimizerExtensions.AsDelayedGroup(() => products.Select(p2 => p2.unitPrice).Max()).SelectMany(uMaxLazy => products.Where(p => uMaxLazy.Value == p.unitPrice).Select(p => p.productName)),
                               ref products,
                               "Optimized Max With AsDelayedGroupSelectMany operator",
                               "OptimizerExtensions.AsDelayedGroup(() => products.Select(p2 => p2.unitPrice).Max()).SelectMany(uMaxLazy => products.Where(p => uMaxLazy.Value == p.unitPrice).Select(p => p.productName))"
                               );
            */
            TestingEnvironment.ExtendedTest(() => OptimizerExtensions.AsGroupSuspended(() => products.Select(p2 => p2.unitPrice).Max()).SelectMany(uMaxThunk => products.Where(p => uMaxThunk.Value == p.unitPrice).Select(p => p.productName)),
                               ref products,
                               "Optimized Max With AsGroupSuspendedSelectMany operator",
                               "OptimizerExtensions.AsGroupSuspended(() => products.Select(p2 => p2.unitPrice).Max()).SelectMany(uMaxThunk => products.Where(p => uMaxThunk.Value == p.unitPrice).Select(p => p.productName))"
                               );

            TestingEnvironment.ExtendedTest(() => OptimizerExtensions.AsGroupSuspended(() => products.Select(p2 => p2.unitPrice).Max()).Select(uMaxThunk => products.Where(p => uMaxThunk.Value == p.unitPrice).Select(p => p.productName)).First(),
                   ref products,
                   "Optimized Max With AsGroupSuspendedSelectFirst operator",
                   "OptimizerExtensions.AsGroupSuspended(() => products.Select(p2 => p2.unitPrice).Max()).Select(uMaxThunk => products.Where(p => uMaxThunk.Value == p.unitPrice).Select(p => p.productName)).First()"
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
/*
            TestingEnvironment.ExtendedTest(() => from Product p in products
                                                  where (from Product p2 in products where p2.unitPrice == p.unitPrice / 1.2 select p2).Count() == 1
                                                  select p.productName,
                            ref products,
                            "Original simpler Query Expession",
                            "from Product p in products\n where (from Product p2 in products where p2.unitPrice == p.unitPrice / 1.2 select p2).Count() > 0\nselect p.productName"
                            );
*/
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

            TestingEnvironment.ExtendedTest(() => products.Where(p => OptimizerExtensions.AsGroupSuspended(() => p.unitPrice / 1.2).Select(pup10 => products.Where(p2 => p2.unitPrice == pup10.Value)).First().Count() == 1).Select(p => p.productName),
                               ref products,
                               "Optimized simpler With AsGroupSuspendedSelectFirst operator",
                               "products.Where(p => OptimizerExtensions.AsGroupSuspended(() => p.unitPrice / 1.2).Select(pup10 => products.Where(p2 => p2.unitPrice == pup10.Value)).First().Count() == 1).Select(p => p.productName)"
                               );

        }


    }
}
