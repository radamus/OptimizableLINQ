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

            TestingEnvironment.BenchmarkQuery(() => products.OrderBy(p => p.unitPrice.Value).Aggregate(new { res = new List<ProductX>(100000), lastPrice = new List<double>(1) { -1.0 } },
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

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToLookup(p => p.unitPrice.Value)).SelectMany(pLookup => products.Where(p => pLookup[p.unitPrice.Value].Count() == 1).Select(p => p.productName)),
                           ref products, 
                           "ToLookup alternative Unique unitPrice Lambda Expession",
                           "OptimizerExtensions.AsGroup(() => products.ToLookup(p => p.unitPrice.Value)).SelectMany(pLookup => products.Where(p => pLookup[p.unitPrice.Value].Count() == 1).Select(p => p.productName))"
                           );
        }

        public static void volatileIndexCreation(IEnumerable<ProductX> products)
        {

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroupSuspended(() => products.ToMinimalVolatileIndex(p => p.unitPrice.Value)).SelectMany(pVolIdx => pVolIdx.Value.ToString()),
                           ref products,
                           "Minimal Volatile Index Creation",
                           ""
                           );

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroupSuspended(() => products.ToVolatileIndex(p => p.unitPrice.Value)).SelectMany(pVolIdx => pVolIdx.Value.ToString()),
                           ref products,
                           "Volatile Index Creation",
                           ""
                           );

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroupSuspended(() => products.ToSlowVolatileIndex(p => p.unitPrice.Value)).SelectMany(pVolIdx => pVolIdx.Value.ToString()),
                           ref products,
                           "Slow Volatile Index Creation",
                           ""
                           );
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroupSuspended(() => products.ToAlmostVolatileIndex(p => p.unitPrice.Value)).SelectMany(pVolIdx => pVolIdx.Value.ToString()),
                           ref products,
                           "Almost Volatile Index Creation",
                           ""
                           );
        }
        
        public static void uniqueUnitPriceVolatileIndex(IEnumerable<ProductX> products)
        {
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroupSuspended(() => products.ToMinimalVolatileIndex(p => p.unitPrice.Value)).SelectMany(pVolIdx => products.Where(p => pVolIdx.Value.lookup(() => p.unitPrice.Value).Count() == 1).Select(p => p.productName)),
                           ref products,
                           "Minimal Volatile Index Unique unitPrice Lambda Expession",
                           "OptimizerExtensions.AsGroupSuspended(() => products.ToMinimalVolatileIndex(p => p.unitPrice.Value)).SelectMany(pVolIdx => products.Where(p => pVolIdx.Value.lookup(() => p.unitPrice.Value).Count() == 1).Select(p => p.productName))"
                           );

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroupSuspended(() => products.ToVolatileIndex(p => p.unitPrice.Value)).SelectMany(pVolIdx => products.Where(p => pVolIdx.Value.lookup(() => p.unitPrice.Value, false).Count() == 1).Select(p => p.productName)),
                           ref products,
                           "Volatile Index Unique unitPrice Lambda Expession",
                           "OptimizerExtensions.AsGroupSuspended(() => products.ToVolatileIndex(p => p.unitPrice.Value)).SelectMany(pVolIdx => products.Where(p => pVolIdx.Value.lookup(() => p.unitPrice.Value, false).Count() == 1).Select(p => p.productName))"
                           );

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroupSuspended(() => products.ToSlowVolatileIndex(p => p.unitPrice.Value)).SelectMany(pVolIdx => products.Where(p => pVolIdx.Value[() => p.unitPrice.Value].Where(p2VI => p2VI.IsValid).Count() == 1).Select(p => p.productName)),
                           ref products,
                           "Slow Volatile Index Unique unitPrice Lambda Expession",
                           "OptimizerExtensions.AsGroupSuspended(() => products.ToSlowVolatileIndex(p => p.unitPrice.Value)).SelectMany(pVolIdx => products.Where(p => pVolIdx.Value[() => p.unitPrice.Value].Where(p2VI => p2VI.IsValid).Count() == 1).Select(p => p.productName))"
                           );

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroupSuspended(() => products.ToAlmostVolatileIndex(p => p.unitPrice.Value)).SelectMany(pVolIdx => products.Where(p => pVolIdx.Value[() => p.unitPrice.Value].Where(p2VI => p2VI.IsValid).Count() == 1).Select(p => p.productName)),
                           ref products,
                           "Almost Volatile Index Unique unitPrice Lambda Expession",
                           "OptimizerExtensions.AsGroupSuspended(() => products.ToVolatileIndex(p => p.unitPrice.Value)).SelectMany(pVolIdx => products.Where(p => pVolIdx.Value[() => p.unitPrice.Value].Count() == 1).Select(p => p.productName))"
                           );
        }

        public static void nullSomeUnitPrices(IEnumerable<ProductX> products, bool nullValidKeyValueCategories, bool nullValidIndexArgumentCategories)
        {
            if (products.Count() < 13)
                return; 

            IList<ProductX> productsX = (IList<ProductX>)products;
            productsX[10].unitPrice = null;
            productsX[11].unitPrice = null;

            if (nullValidKeyValueCategories) productsX[1].unitPrice = null; //beverages
            if (nullValidIndexArgumentCategories) productsX[13].unitPrice = null; //produce
        }
        
        public static void sameUnitPriceQueryOriginal(IEnumerable<ProductX> products)
        {
            TestingEnvironment.BenchmarkQuery(() => products.SelectMany(p => products.Where(p2 => p.category.Equals("Produce") && p2.category.Equals("Beverages") && p2.unitPrice.Value == p.unitPrice.Value).Select(p2 => p.productName + " and " + p2.productName)),
                           ref products,
                           "Original Same unitPrice Lambda Expession",
                           "products.SelectMany(p => products.Where(p2 => p.category.Equals(\"Produce\") && p2.category.Equals(\"Beverages\") && p2.unitPrice.Value == p.unitPrice.Value).Select(p2 => p.productName + \" and \" + p2.productName))"
                           );
        }

        public static void sameUnitPriceAlternatives(IEnumerable<ProductX> products)
        {
            TestingEnvironment.BenchmarkQuery(() => products.Where(p => p.category.Equals("Produce")).SelectMany(p => products.Where(p2 => p2.category.Equals("Beverages") && p2.unitPrice.Value == p.unitPrice.Value).Select(p2 => p.productName + " and " + p2.productName)),
                           ref products,
                           "Predicate pushed Same unitPrice Lambda Expession",
                           "products.SelectMany(p => products.Where(p2 => p.category.Equals(\"Produce\") && p2.category.Equals(\"Beverages\") && p2.unitPrice.Value == p.unitPrice.Value).Select(p2 => p.productName + \" and \" + p2.productName))"
                           );
        }

        public static void sameUnitPriceVolatileIndex(IEnumerable<ProductX> products)
        {
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroupSuspended(() => products.ToMinimalVolatileIndex(p => p.unitPrice.Value)).SelectMany(pVolIdx => products.SelectMany(p => pVolIdx.Value.lookup(() => p.unitPrice.Value, p2 => p.category.Equals("Produce") && p2.category.Equals("Beverages")).Select(p2 => p.productName + " and " + p2.productName))),
                           ref products,
                           "Minimal Volatile Index Same unitPrice Lambda Expession",
                           "OptimizerExtensions.AsGroupSuspended(() => products.ToMinimalVolatileIndex(p => p.unitPrice.Value)).SelectMany(pVolIdx => products.SelectMany(p => pVolIdx.Value.lookup(() => p.unitPrice.Value, p2 => p.category.Equals(\"Produce\") && p2.category.Equals(\"Beverages\")).Select(p2 => p.productName + \" and \" + p2.productName)))"
                           );
            
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroupSuspended(() => products.ToVolatileIndex(p => p.unitPrice.Value)).SelectMany(pVolIdx => products.SelectMany(p => pVolIdx.Value.lookup(() => p.unitPrice.Value, true, p2 => p.category.Equals("Produce") && p2.category.Equals("Beverages")).Select(p2 => p.productName + " and " + p2.productName))),
                           ref products,
                           "Volatile Index Same unitPrice Lambda Expession",
                           "OptimizerExtensions.AsGroupSuspended(() => products.ToVolatileIndex(p => p.unitPrice.Value)).SelectMany(pVolIdx => products.SelectMany(p => pVolIdx.Value.lookup(() => p.unitPrice.Value, true, p2 => p.category.Equals(\"Produce\") && p2.category.Equals(\"Beverages\")).Select(p2 => p.productName + \" and \" + p2.productName)))"
                           );
            
            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroupSuspended(() => products.ToSlowVolatileIndex(p => p.unitPrice.Value)).SelectMany(pVolIdx => products.SelectMany(p => pVolIdx.Value[() => p.unitPrice.Value].Where(p2VI => p.category.Equals("Produce") && p2VI.Value.category.Equals("Beverages") && p2VI.IsValid).Select(p2VI => p2VI.Value).Select(p2 => p.productName + " and " + p2.productName))),
                           ref products,
                           "Slow Volatile Index Same unitPrice Lambda Expession",
                           "OptimizerExtensions.AsGroupSuspended(() => products.ToSlowVolatileIndex(p => p.unitPrice.Value)).SelectMany(pVolIdx => products.SelectMany(p => pVolIdx.Value[() => p.unitPrice.Value].Where(p2VI => p.category.Equals(\"Produce\") && p2VI.Value.category.Equals(\"Beverages\") && p2VI.IsValid).Select(p2VI => p2VI.Value).Select(p2 => p.productName + \" and \" + p2.productName)))"
                           );
        }

    }
}
