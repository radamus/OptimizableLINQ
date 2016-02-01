using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizableLINQBenchmark
{

    using SampleData;
    using OptimizableLINQ;


    class TestVolatileIndexing
    {

        public static void volatileIndexCreation(IEnumerable<Product> products)
        {
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToVolatileIndex(p => p.unitPrice)).SelectMany(prodPriceIdxThunk => prodPriceIdxThunk.Value.ToString()),
                           ref products, "Volatile Index Creation", "on unitprice");
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToRelaxedVolatileIndex(p => p.unitPrice)).SelectMany(prodPriceIdxThunk => prodPriceIdxThunk.Value.ToString()),
                           ref products, "Relaxed Volatile Index Creation", "on unitprice");
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToPartlyRelaxedVolatileIndex(p => p.unitPrice)).SelectMany(prodPriceIdxThunk => prodPriceIdxThunk.Value.ToString()),
                           ref products, "Partly Relaxed Volatile Index Creation", "on unitprice");
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToLookup(p => p.unitPrice)).SelectMany(prodPriceIdxThunk => prodPriceIdxThunk.Value.ToString()),
                           ref products, "Lookup Creation", "on unitprice");
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToSlowVolatileIndex(p => p.unitPrice)).SelectMany(prodPriceIdxThunk => prodPriceIdxThunk.Value.ToString()),
                           ref products, "DEPRECATED: Slow Volatile Index Creation", "on unitprice");
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToAlmostVolatileIndex(p => p.unitPrice)).SelectMany(prodPriceIdxThunk => prodPriceIdxThunk.Value.ToString()),
                           ref products, "DEPRECATED: Almost Volatile Index Creation", "on unitprice");

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToVolatileIndex(p => p.category)).SelectMany(prodPriceIdxThunk => prodPriceIdxThunk.Value.ToString()),
                           ref products, "Volatile Index Creation", "on category");
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToRelaxedVolatileIndex(p => p.category)).SelectMany(prodPriceIdxThunk => prodPriceIdxThunk.Value.ToString()),
                           ref products, "Relaxed Volatile Index Creation", "on category");
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToPartlyRelaxedVolatileIndex(p => p.category)).SelectMany(prodPriceIdxThunk => prodPriceIdxThunk.Value.ToString()),
                           ref products, "Partly Relaxed Volatile Index Creation", "on category");
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToLookup(p => p.category)).SelectMany(prodPriceIdxThunk => prodPriceIdxThunk.Value.ToString()),
                           ref products, "Lookup Index Creation", "on category");
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToSlowVolatileIndex(p => p.category)).SelectMany(prodPriceIdxThunk => prodPriceIdxThunk.Value.ToString()),
                           ref products, "DEPRECATED: Slow Volatile Index Creation", "on category");
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToAlmostVolatileIndex(p => p.category)).SelectMany(prodPriceIdxThunk => prodPriceIdxThunk.Value.ToString()),
                           ref products, "DEPRECATED: Almost Volatile Index Creation", "on category");
        }

        public static void uniqueCategoryQueryOriginal(IEnumerable<Product> products)
        {
/*            TestingEnvironment.BenchmarkQuery(() => from p in products
                                                    where (from p2 in products where p.category.Equals(p2.category) select p2).Count() == 1
                                                    select p.productName,
                           ref products,
                           "Original Unique category Query Expession",
                           "from Product p in products\n where (from Product p2 in products where p.category.Equals(p2.category) select p2).Count() == 1\nselect p.productName"
                           );
*/
            TestingEnvironment.BenchmarkQuery(() => products.Where(p => products.Where(p2 => p.category.Equals(p2.category)).Count() == 1).Select(p => p.productName),
                           ref products,
                           "Original Unique category Lambda Expession",
                           "products.Where(p => products.Where(p2 => p.category.Equals(p2.category)).Count() == 1).Select(p => p.productName)"
                           );
/*
            TestingEnvironment.BenchmarkQuery(() => products.Where(p => products.Where(p2 => p.category == p2.category).Count() == 1).Select(p => p.productName),
                           ref products,
                           "Original Unique category (==) Lambda Expession",
                           "products.Where(p => products.Where(p2 => p.category == p2.category).Count() == 1).Select(p => p.productName)"
                           );
 */
        }

        public static void uniqueCategoryAlternatives(IEnumerable<Product> products)
        {
            /*
                        TestingEnvironment.BenchmarkQuery(() => products.OrderBy(p => p.category),
                                       ref products,
                                       "OrderBy category Lambda Expession",
                                       "products.OrderBy(p => p.category)"
                                       );

                        TestingEnvironment.BenchmarkQuery(() => () => products.OrderBy(p => p.category).Aggregate(new { res = new List<Product>(100000), lastCategory = new List<string>(1) { null } },
                                            (acc, p) => { if (!p.category.Equals(acc.lastCategory[0])) { acc.res.Add(p); acc.lastCategory[0] = p.category; } else if (acc.res.Count() != 0 && acc.res.Last().category == p.category) { acc.res.RemoveAt(acc.res.Count() - 1); } return acc; },
                                            (acc) => acc.res),
                                       ref products,
                                       "OrderBy alternative Unique category (==) Lambda Expession",
                                       "products.OrderBy(p => p.category).Aggregate(new { res = new List<Product>(), lastCategory = new List<string>(1) { null } },\n(acc, p) => { if (!p.category.Equals(acc.lastCategory[0])) { acc.res.Add(p); acc.lastCategory[0] = p.category; } else if (acc.res.Count() != 0 && acc.res.Last().category == p.category) { acc.res.RemoveAt(acc.res.Count() - 1); } return acc; },\n(acc) => acc.res"
                                       );
            */
            TestingEnvironment.BenchmarkQuery(() => products.GroupBy(p => p.category).Where(prodGroup => prodGroup.Count() == 1).SelectMany(prodGroup => prodGroup.Select(p => p.productName)),
                           ref products,
                           "GroupBy alternative Unique category (==) Lambda Expession",
                           "products.GroupBy(p => p.category).Where(prodGroup => prodGroup.Count() == 1).SelectMany(prodGroup => prodGroup.Select(p=> p.productName)"
                           );

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroupEager(() => products.ToLookup(p => p.category)).SelectMany(pLookup => products.Where(p => pLookup[p.category].Count() == 1).Select(p => p.productName)),
                           ref products,
                           "ToLookup alternative Unique unitPrice (==) Lambda Expession",
                           "OptimizerExtensions.AsGroup(() => products.ToLookup(p => p.category)).SelectMany(pLookup => products.Where(p => pLookup[p.category].Count() == 1).Select(p => p.productName))"
                           );
        }

        public static void uniqueCategoryVolatileIndex(IEnumerable<Product> products)
        {
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToVolatileIndex(p => p.category)).SelectMany(prodCatIdxThunk => products.Where(p => prodCatIdxThunk.Value.Lookup(() => p.category, false, true).Count() == 1).Select(p => p.productName)),
               ref products,
               "Volatile Index Unique category Lambda Expession",
               "OptimizerExtensions.AsGroup(() => products.ToVolatileIndex(p => p.category)).SelectMany(prodCatIdxThunk => products.Where(p => prodCatIdxThunk.Value.Lookup(() => p.category, false, true).Count() == 1).Select(p => p.productName))"
               );
            
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToVolatileIndex(p => p.category)).SelectMany(prodCatIdxThunk => products.Where(p => prodCatIdxThunk.Value.Lookup(() => p.category, false, false).Count() == 1).Select(p => p.productName)),
               ref products,
               "Volatile Index Unique category (==) Lambda Expession",
               "OptimizerExtensions.AsGroup(() => products.ToVolatileIndex(p => p.category)).SelectMany(prodCatIdxThunk => products.Where(p => prodCatIdxThunk.Value.Lookup(() => p.category, false, false).Count() == 1).Select(p => p.productName))"
               );

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToRelaxedVolatileIndex(p => p.category)).SelectMany(prodCatIdxThunk => products.Where(p => prodCatIdxThunk.Value.Lookup(() => p.category).Count() == 1).Select(p => p.productName)),
                           ref products,
                           "Relaxed Volatile Index Unique category (==) Lambda Expession",
                           "OptimizerExtensions.AsGroup(() => products.ToRelaxedVolatileIndex(p => p.category)).SelectMany(prodCatIdxThunk => products.Where(p => prodCatIdxThunk.Value.Lookup(() => p.category).Count() == 1).Select(p => p.productName))"
                           );
        }

        public static void uniqueCategoryPLINQ(IEnumerable<Product> products)
        {
            TestingEnvironment.BenchmarkQuery(() => products.AsParallel().Where(p => products.Where(p2 => p.category.Equals(p2.category)).Count() == 1).Select(p => p.productName),
                           ref products,
                           "Original Unique category Lambda Expession with PLINQ",
                           "products.AsParallel().Where(p => products.Where(p2 => p.category.Equals(p2.category)).Count() == 1).Select(p => p.productName)"
                           );

            TestingEnvironment.BenchmarkQuery(() => products.AsParallel().GroupBy(p => p.category).Where(prodGroup => prodGroup.Count() == 1).SelectMany(prodGroup => prodGroup.Select(p => p.productName)),
                           ref products,
                           "GroupBy alternative Unique category (==) Lambda Expession with PLINQ",
                           "products.AsParallel().GroupBy(p => p.category).Where(prodGroup => prodGroup.Count() == 1).SelectMany(prodGroup => prodGroup.Select(p=> p.productName)"
                           );

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroupSuspendedThreadSafe(() => products.ToVolatileIndex(p => p.category)).SelectMany(prodCatIdxThunk => products.AsParallel().Where(p => prodCatIdxThunk.Value.Lookup(() => p.category, false, true).Count() == 1).Select(p => p.productName)),
               ref products,
               "Volatile Index Unique category Lambda Expession with PLINQ",
               "OptimizerExtensions.AsGroup(() => products.ToVolatileIndex(p => p.category)).SelectMany(prodCatIdxThunk => products.AsParallel().Where(p => prodCatIdxThunk.Value.Lookup(() => p.category, false, true).Count() == 1).Select(p => p.productName))"
               );
        }

        public static void uniqueUnitPriceQueryOriginal(IEnumerable<ProductX> products)
        {
            TestingEnvironment.BenchmarkQuery(() => from p in products
                                                    where (from p2 in products where p.unitPrice.Value == p2.unitPrice.Value select p2).Count() == 1
                                                    select p.productName,
                           ref products,
                           "Original Unique unitPrice Query Expession",
                           "from Product p in products\n where (from Product p2 in products where p.unitPrice.Value == p2.unitPrice.Value select p2).Count() == 1\nselect p.productName"
                           );

            TestingEnvironment.BenchmarkQuery(() => products.Where(p => products.Where(p2 => p.unitPrice.Value == p2.unitPrice.Value).Count() == 1).Select(p => p.productName),
                           ref products,
                           "Original Unique unitPrice Lambda Expession",
                           "products.Where(p => products.Where(p2 => p.unitPrice.Value == p2.unitPrice.Value).Count() == 1).Select(p => p.productName)"
                           );
        }

        public static void uniqueUnitPriceAlternatives(IEnumerable<ProductX> products)
        {

            TestingEnvironment.BenchmarkQuery(() => () => products.OrderBy(p => p.unitPrice.Value).Aggregate(new { res = new List<ProductX>(100000), lastPrice = new List<double>(1) { -1.0 } },
                                (acc, p) => { if (acc.lastPrice[0] < p.unitPrice.Value) { acc.res.Add(p); acc.lastPrice[0] = p.unitPrice.Value; } else if (acc.res.Count() != 0 && acc.res.Last().unitPrice == p.unitPrice) { acc.res.RemoveAt(acc.res.Count() - 1); } return acc; },
                                (acc) => acc.res),
                           ref products,
                           "OrderBy alternative Unique unitPrice Lambda Expession",
                           "products.OrderBy(p => p.unitPrice).Aggregate(new { res = new List<Product>(), lastPrice = new List<double>(1) {-1.0 } },\n(acc, p) => { if (acc.lastPrice[0] < p.unitPrice.Value) { acc.res.Add(p); acc.lastPrice[0] = p.unitPrice.Value; } else if (acc.res.Count() != 0 && acc.res.Last().unitPrice == p.unitPrice) { acc.res.RemoveAt(acc.res.Count() - 1); } return acc; },\n(acc) => acc.res"
                           );

            TestingEnvironment.BenchmarkQuery(() => products.GroupBy(p => p.unitPrice.Value).Where(prodGroup => prodGroup.Count() == 1).SelectMany(prodGroup => prodGroup.Select(p => p.productName)),
                           ref products,
                           "GroupBy alternative Unique unitPrice Lambda Expession",
                           "products.GroupBy(p => p.unitPrice.Value).Where(prodGroup => prodGroup.Count() == 1).SelectMany(prodGroup => prodGroup.Select(p=> p.productName)"
                           );

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroupEager(() => products.ToLookup(p => p.unitPrice.Value)).SelectMany(pLookup => products.Where(p => pLookup[p.unitPrice.Value].Count() == 1).Select(p => p.productName)),
                           ref products, 
                           "ToLookup alternative Unique unitPrice Lambda Expession",
                           "OptimizerExtensions.AsGroup(() => products.ToLookup(p => p.unitPrice.Value)).SelectMany(pLookup => products.Where(p => pLookup[p.unitPrice.Value].Count() == 1).Select(p => p.productName))"
                           );
        }

        public static void uniqueUnitPriceVolatileIndex(IEnumerable<ProductX> products)
        {
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToVolatileIndex(p => p.unitPrice.Value)).SelectMany(prodPriceIdxThunk => products.Where(p => prodPriceIdxThunk.Value.Lookup(() => p.unitPrice.Value, false, false).Count() == 1).Select(p => p.productName)),
                           ref products,
                           "Volatile Index Unique unitPrice Lambda Expession",
                           "OptimizerExtensions.AsGroup(() => products.ToVolatileIndex(p => p.unitPrice.Value)).SelectMany(prodPriceIdxThunk => products.Where(p => prodPriceIdxThunk.Value.Lookup(() => p.unitPrice.Value, false, false).Count() == 1).Select(p => p.productName))"
                           );

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToRelaxedVolatileIndex(p => p.unitPrice.Value)).SelectMany(prodPriceIdxThunk => products.Where(p => prodPriceIdxThunk.Value.Lookup(() => p.unitPrice.Value).Count() == 1).Select(p => p.productName)),
                           ref products,
                           "Exception Ignoring Volatile Index Unique unitPrice Lambda Expession",
                           "OptimizerExtensions.AsGroup(() => products.ToRelaxedVolatileIndex(p => p.unitPrice.Value)).SelectMany(prodPriceIdxThunk => products.Where(p => prodPriceIdxThunk.Value.Lookup(() => p.unitPrice.Value).Count() == 1).Select(p => p.productName))"
                           );

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToPartlyRelaxedVolatileIndex(p => p.unitPrice.Value)).SelectMany(prodPriceIdxThunk => products.Where(p => prodPriceIdxThunk.Value.Lookup(() => p.unitPrice.Value, false).Count() == 1).Select(p => p.productName)),
                           ref products,
                           "Partly Relaxed Volatile Index Unique unitPrice Lambda Expession",
                           "OptimizerExtensions.AsGroup(() => products.ToPartlyRelaxedVolatileIndex(p => p.unitPrice.Value)).SelectMany(prodPriceIdxThunk => products.Where(p => prodPriceIdxThunk.Value.Lookup(() => p.unitPrice.Value, false).Count() == 1).Select(p => p.productName))"
                           );
        }

        public static void nullSomeUnitPrices(IEnumerable<Product> products, bool nullValidKeyValueCategories, bool nullValidIndexArgumentCategories)
        {
            if (products.Count() < 13)
                return; 

            IList<ProductX> productsX = (IList<ProductX>)products;
            productsX[10].unitPrice = null;
            productsX[11].unitPrice = null;

            if (nullValidKeyValueCategories) productsX[1].unitPrice = null; //beverages
            if (nullValidIndexArgumentCategories) productsX[13].unitPrice = null; //produce
        }

        public static void nullSomeCategories(IEnumerable<Product> products)
        {
            IList<Product> productsX = (IList<Product>)products;
            productsX[612].category = null;
            productsX[113].category = null;
        }

        static string[] categoriesToAudit = new string[] { "Beverages", "Seafood" };

        public static void sameUnitPriceQueryOriginal(IEnumerable<Product> products)
        {
            TestingEnvironment.BenchmarkQuery(() => products.Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => products.Where(p2 => !p2.category.Equals(p.category) && p2.unitPrice == p.unitPrice).Select(p2 => p.productName + " and " + p2.productName)),
                           ref products,
                           "Original Same unitPrice Lambda Expession",
                           "products.Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => products.Where(p2 => !p2.category.Equals(p.category) && p2.unitPrice == p.unitPrice).Select(p2 => p.productName + \" and \" + p2.productName)))"
                           );

            TestingEnvironment.BenchmarkQuery(() => products.Where(p => "Beverages".Equals(p.category) || "Seafood".Equals(p.category)).SelectMany(p => products.Where(p2 => !p2.category.Equals(p.category) && p2.unitPrice == p.unitPrice).Select(p2 => p.productName + " and " + p2.productName)),
                           ref products,
                           "Original Same unitPrice Lambda Expession (without categoriesToAudit)",
                           "products.Where(p => \"Beverages\".Equals(p.category) || \"Seafood\".Equals(p.category)).SelectMany(p => products.Where(p2 => !p2.category.Equals(p.category) && p2.unitPrice == p.unitPrice).Select(p2 => p.productName + \" and \" + p2.productName))"
                           );
        }

        public static void sameUnitPriceAlternatives(IEnumerable<Product> products)
        {
            TestingEnvironment.BenchmarkQuery(() => products.GroupBy(p => p.unitPrice).SelectMany(prodGroup => prodGroup.Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => prodGroup.Where(p2 => !p2.category.Equals(p.category)).Select(p2 => p.productName + " and " + p2.productName))),
                           ref products,
                           "GroupBy Same unitPrice Lambda Expession",
                           "products.GroupBy(p => p.unitPrice).SelectMany(prodGroup => prodGroup.Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => prodGroup.Where(p2 => !p2.category.Equals(p.category)).Select(p2 => p.productName + \" and \" + p2.productName)))"
                           );
        }

        public static void sameUnitPriceVolatileIndex(IEnumerable<Product> products)
        {
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToRelaxedVolatileIndex(p => p.unitPrice)).SelectMany(prodPriceIdxThunk => products.Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => prodPriceIdxThunk.Value.Lookup(() => p.unitPrice).Where(p2 => !p2.category.Equals(p.category)).Select(p2 => p.productName + " and " + p2.productName))),
                           ref products,
                           "Relaxed (Exception Ignoring) Volatile Index Same unitPrice Lambda Expession",
                           "OptimizerExtensions.AsGroup(() => products.ToRelaxedVolatileIndex(p => p.unitPrice)).SelectMany(prodPriceIdxThunk => products.Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => prodPriceIdxThunk.Value.Lookup(() => p.unitPrice).Where(p2 => !p2.category.Equals(p.category)).Select(p2 => p.productName + \" and \" + p2.productName)))"
                           );

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToPartlyRelaxedVolatileIndex(p => p.unitPrice)).SelectMany(prodPriceIdxThunk => products.Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => prodPriceIdxThunk.Value.Lookup(() => p.unitPrice, true, p2 => !p2.category.Equals(p.category)).Select(p2 => p.productName + " and " + p2.productName))),
                           ref products,
                           "Partly Relaxed Volatile Index Same unitPrice Lambda Expession",
                           "OptimizerExtensions.AsGroup(() => products.ToPartlyRelaxedVolatileIndex(p => p.unitPrice)).SelectMany(prodPriceIdxThunk => products.Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => prodPriceIdxThunk.Value.Lookup(() => p.unitPrice, true, p2 => !p2.category.Equals(p.category)).Select(p2 => p.productName + \" and \" + p2.productName)))"
                           );
        }

        public static void sameUnitPriceVolatileIndexExceptionsTest(IEnumerable<Product> products)
        {
            int indexOfFstCorrelatedProduct = 1486; 
            int indexOfSndCorrelatedProduct = 7318; // 7318, 1485, 41 (Seafood);

            string guardProductName = products.Take(indexOfFstCorrelatedProduct).Last().productName;
            string nulledProductName = products.Take(indexOfSndCorrelatedProduct).Last().productName;
            products.Take(indexOfSndCorrelatedProduct).Last().productName = null;

            Console.WriteLine(nulledProductName);

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.Where(p2 => !p2.productName.Equals(nulledProductName)).ToRelaxedVolatileIndex(p => p.unitPrice)).SelectMany(prodPriceIdxThunk => products.Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => prodPriceIdxThunk.Value.Lookup(() => p.unitPrice).Where(p2 => !p2.category.Equals(p.category)).TakeWhile(p2 => !p2.productName.Equals(nulledProductName)).Select(p2 => p.productName + " and " + p2.productName))),
                           ref products,
                           "Relaxed (Exception Ignoring) Volatile Index Same unitPrice Lambda Expession (true source exception)",
                           "OptimizerExtensions.AsGroup(() => products.Where(p2 => !p2.productName.Equals(nulledProductName)).ToRelaxedVolatileIndex(p => p.unitPrice)).SelectMany(prodPriceIdxThunk => products.Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => prodPriceIdxThunk.Value.Lookup(() => p.unitPrice).Where(p2 => !p2.category.Equals(p.category)).TakeWhile(p2 => !p2.productName.Equals(nulledProductName).Select(p2 => p.productName + \" and \" + p2.productName)))"
                           );

            TestingEnvironment.BenchmarkQuery(() => products.Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => products.Where(p2 => !p2.productName.Equals(nulledProductName)).Where(p2 => !p2.category.Equals(p.category) && p2.unitPrice == p.unitPrice).TakeWhile(p2 => !p2.productName.Equals(nulledProductName)).Select(p2 => p.productName + " and " + p2.productName)),
                           ref products,
                           "Original Same unitPrice Lambda Expession (true source exception)",
                           "products.Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => products.Where(p2 => !p2.productName.Equals(nulledProductName)).Where(p2 => !p2.category.Equals(p.category) && p2.unitPrice == p.unitPrice).TakeWhile(p2 => !p2.productName.Equals(nulledProductName)).Select(p2 => p.productName + \" and \" + p2.productName)))"
                           );

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToRelaxedVolatileIndex(p => p.unitPrice)).SelectMany(prodPriceIdxThunk => products.Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => prodPriceIdxThunk.Value.Lookup(() => p.unitPrice).Where(p2 => !p2.category.Equals(p.category)).TakeWhile(p2 => !p2.productName.Equals(nulledProductName)).Select(p2 => p.productName + " and " + p2.productName))),
                           ref products,
                           "Relaxed (Exception Ignoring) Volatile Index Same unitPrice Lambda Expession (true exception)",
                           "OptimizerExtensions.AsGroup(() => products.ToRelaxedVolatileIndex(p => p.unitPrice)).SelectMany(prodPriceIdxThunk => products.Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => prodPriceIdxThunk.Value.Lookup(() => p.unitPrice).Where(p2 => !p2.category.Equals(p.category)).TakeWhile(p2 => !p2.productName.Equals(nulledProductName).Select(p2 => p.productName + \" and \" + p2.productName)))"
                           );

            TestingEnvironment.BenchmarkQuery(() => products.Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => products.Where(p2 => !p2.category.Equals(p.category) && p2.unitPrice == p.unitPrice).TakeWhile(p2 => !p2.productName.Equals(nulledProductName)).Select(p2 => p.productName + " and " + p2.productName)),
                           ref products,
                           "Original Same unitPrice Lambda Expession (true exception)",
                           "products.Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => products.Where(p2 => !p2.category.Equals(p.category) && p2.unitPrice == p.unitPrice).TakeWhile(p2 => !p2.productName.Equals(nulledProductName)).Select(p2 => p.productName + \" and \" + p2.productName)))"
                           );

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToRelaxedVolatileIndex(p => p.unitPrice)).SelectMany(prodPriceIdxThunk => products.Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => prodPriceIdxThunk.Value.Lookup(() => p.unitPrice).Where(p2 => !p2.category.Equals(p.category)).TakeWhile(p2 => !p2.productName.Equals(guardProductName)).Select(p2 => p.productName + " and " + p2.productName))),
                           ref products,
                           "Relaxed (Exception Ignoring) Volatile Index Same unitPrice Lambda Expession (false exception)",
                           "OptimizerExtensions.AsGroup(() => products.ToRelaxedVolatileIndex(p => p.unitPrice)).SelectMany(prodPriceIdxThunk => products.Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => prodPriceIdxThunk.Value.Lookup(() => p.unitPrice).Where(p2 => !p2.category.Equals(p.category)).TakeWhile(p2 => !p2.productName.Equals(guardProductName)).Select(p2 => p.productName + \" and \" + p2.productName)))"
                           );

            TestingEnvironment.BenchmarkQuery(() => products.Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => products.Where(p2 => !p2.category.Equals(p.category) && p2.unitPrice == p.unitPrice).TakeWhile(p2 => !p2.productName.Equals(guardProductName)).Select(p2 => p.productName + " and " + p2.productName)),
                           ref products,
                           "Original Same unitPrice Lambda Expession (false exception)",
                           "products.Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => products.Where(p2 => !p2.category.Equals(p.category) && p2.unitPrice == p.unitPrice).TakeWhile(p2 => !p2.productName.Equals(guardProductName)).Select(p2 => p.productName + \" and \" + p2.productName)))"
                           );

            products.Take(indexOfSndCorrelatedProduct).Last().productName = nulledProductName;
        }



        public static void sameUnitPricePLINQ(IEnumerable<Product> products)
        {
            TestingEnvironment.BenchmarkQuery(() => products.AsParallel().Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => products.Where(p2 => !p2.category.Equals(p.category) && p2.unitPrice == p.unitPrice).Select(p2 => p.productName + " and " + p2.productName)),
                       ref products,
                       "Original Same unitPrice Lambda Expession with PLINQ",
                       "products.AsParallel().Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => products.Where(p2 => !p2.category.Equals(p.category) && p2.unitPrice == p.unitPrice).Select(p2 => p.productName + \" and \" + p2.productName))"
                       );

            TestingEnvironment.BenchmarkQuery(() => products.AsParallel().GroupBy(p => p.unitPrice).SelectMany(prodGroup => prodGroup.Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => prodGroup.Where(p2 => !p2.category.Equals(p.category)).Select(p2 => p.productName + " and " + p2.productName))),
                           ref products,
                           "Groupby Same unitPrice Lambda Expession with PLINQ",
                           "products.AsParallel().GroupBy(p => p.unitPrice).SelectMany(prodGroup => prodGroup.Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => prodGroup.Where(p2 => !p2.category.Equals(p.category)).Select(p2 => p.productName + \" and \" + p2.productName)))"
                           );

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroupSuspendedThreadSafe(() => products.ToRelaxedVolatileIndex(p => p.unitPrice)).SelectMany(prodPriceIdxThunk => products.AsParallel().Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => prodPriceIdxThunk.Value.Lookup(() => p.unitPrice).Where(p2 => !p2.category.Equals(p.category)).Select(p2 => p.productName + " and " + p2.productName))),
                           ref products,
                           "Relaxed (Exception Ignoring) Volatile Index Same unitPrice Lambda Expession with PLINQ",
                           "OptimizerExtensions.AsGroupSuspendedThreadSafe(() => products.ToRelaxedVolatileIndex(p => p.unitPrice)).SelectMany(prodPriceIdxThunk => products.AsParallel().Where(p => categoriesToAudit.Contains(p.category)).SelectMany(p => prodPriceIdxThunk.Value.lookup(() => p.unitPrice).Where(p2 => !p2.category.Equals(p.category)).Select(p2 => p.productName + \" and \" + p2.productName)))"
                           );
        }

        public static void singleExpressionVolatileIndex(IEnumerable<Product> products)
        {

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToVolatileIndex(p2 => p2.unitPrice)).SelectMany(prodPriceIdxThunk => products.Where(p => prodPriceIdxThunk.Value.Lookup(() => Math.Round(p.unitPrice / 1.2, 2), true, false).Any()).Select(p => p.productName)),
                            ref products,
                            "Volatile Index single Lambda Expession",
                            "OptimizerExtensions.AsGroup(() => products.ToVolatileIndex(p2 => p2.unitPrice)).SelectMany(prodPriceIdxThunk => products.Where(p => prodPriceIdxThunk.Value.Lookup(() => Math.Round(p.unitPrice / 1.2, 2), true, false).Any()).Select(p => p.productName))"
                            );


            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToVolatileIndex(p2 => p2.unitPrice)).SelectMany(prodPriceIdxThunk => products.Where(p => OptimizerExtensions.AsGroup(() => Math.Round(p.unitPrice / 1.2, 2)).Select(pup => prodPriceIdxThunk.Value.Lookup(() => pup.Value, true, false).Any()).First()).Select(p => p.productName)),
                               ref products,
                               "Volatile Index and Factoring out operator",
                               "() => OptimizerExtensions.AsGroup(() => products.ToVolatileIndex(p2 => p2.unitPrice)).SelectMany(prodPriceIdxThunk => products.Where(p => OptimizerExtensions.AsGroup(() => Math.Round(p.unitPrice / 1.2, 2)).Select(pup => prodPriceIdxThunk.Value.Lookup(() => pup.Value, true, false).Any()).First()).Select(p => p.productName))"
                               );

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroupSuspendedThreadSafe(() => products.ToVolatileIndex(p2 => p2.unitPrice)).SelectMany(prodPriceIdxThunk => products.AsParallel().Where(p => prodPriceIdxThunk.Value.Lookup(() => Math.Round(p.unitPrice / 1.2, 2), true, false).Any()).Select(p => p.productName)),
                            ref products,
                            "Volatile Index single Lambda Expession with PLINQ",
                            "OptimizerExtensions.AsGroupSuspendedThreadSafe(() => products.ToVolatileIndex(p2 => p2.unitPrice)).SelectMany(prodPriceIdxThunk => products.AsParallel().Where(p => prodPriceIdxThunk.Value.Lookup(() => Math.Round(p.unitPrice / 1.2, 2), true, false).Any()).Select(p => p.productName))"
                            );
        }

        /// <summary>
        /// Same as in <see cref="singleExpressionVolatileIndex"/> but with Math.Round expression on key instead of criterion
        /// </summary>
        /// <param name="products"></param>
        public static void singleKeyExpressionVolatileIndex(IEnumerable<Product> products)
        {
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToVolatileIndex(p2 => Math.Round(p2.unitPrice / 1.2, 2))).SelectMany(prodPriceIdxThunk => products.Where(p => prodPriceIdxThunk.Value.Lookup(() => p.unitPrice, false, false).Any()).Select(p => p.productName)),
                            ref products,
                            "Volatile Index single (key Math.Round) Lambda Expession",
                            "OptimizerExtensions.AsGroup(() => products.ToVolatileIndex(p2 => Math.Round(p2.unitPrice / 1.2, 2)).SelectMany(prodPriceIdxThunk => products.Where(p => prodPriceIdxThunk.Value.Lookup(() => p.unitPrice, false, false).Any()).Select(p => p.productName))"
                            );
        }
    }
}
