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
        public static void uniqueUnitPriceQueryOriginal(IEnumerable<Product> products)
        {
            TestingEnvironment.BenchmarkQuery(() => from Product p in products
                                                    where (from Product p2 in products where p2.unitPrice == p.unitPrice select p2).Count() == 1
                                                    select p.productName,
                           ref products,
                           "Original Unique unitPrice Query Expession",
                           "from Product p in products\n where (from Product p2 in products where p2.unitPrice == p.unitPrice select p2).Count() == 1\nselect p.productName"
                           );

            TestingEnvironment.BenchmarkQuery(() => products.Where(p => products.Where(p2 => p2.unitPrice == p.unitPrice).Count() == 1).Select(p => p.productName),
                           ref products,
                           "Original Unique unitPrice Lambda Expession",
                           "products.Where(p => products.Where(p2 => p2.unitPrice == p.unitPrice).Count() == 1).Select(p => p.productName)"
                           );
        }

        public static void uniqueUnitPriceAlternatives(IEnumerable<Product> products)
        {

            TestingEnvironment.BenchmarkQuery(() => products.OrderBy(p => p.unitPrice).Aggregate(new { res = new List<Product>(), lastPrice = new List<double>(1) { -1.0 } },
                                (acc, p) => { if (acc.lastPrice[0] < p.unitPrice) { acc.res.Add(p); acc.lastPrice[0] = p.unitPrice; } else if (acc.res.Count() != 0 && acc.res.Last().unitPrice == p.unitPrice) { acc.res.RemoveAt(acc.res.Count() - 1); } return acc; },
                                (acc) => acc.res),
                           ref products,
                           "OrderBy alternative Unique unitPrice Lambda Expession",
                           "products.OrderBy(p => p.unitPrice).Aggregate(new { res = new List<Product>(), lastPrice = new List<double>(1) {-1.0 } },\n(acc, p) => { if (acc.lastPrice[0] < p.unitPrice) { acc.res.Add(p); acc.lastPrice[0] = p.unitPrice; } else if (acc.res.Count() != 0 && acc.res.Last().unitPrice == p.unitPrice) { acc.res.RemoveAt(acc.res.Count() - 1); } return acc; },\n(acc) => acc.res"
                           );

            TestingEnvironment.BenchmarkQuery(() => products.GroupBy(p => p.unitPrice).Where(prodGroup => prodGroup.Count() == 1).SelectMany(prodGroup => prodGroup.Select(p=> p.productName)),
                           ref products,
                           "GroupBy alternative Unique unitPrice Lambda Expession",
                           "products.GroupBy(p => p.unitPrice).Where(prodGroup => prodGroup.Count() == 1).SelectMany(prodGroup => prodGroup.Select(p=> p.productName)"
                           );

            TestingEnvironment.BenchmarkQuery(() => OptimizerExtensions.AsGroup(() => products.ToLookup(p => p.unitPrice)).SelectMany(pLookup => products.Where(p => pLookup[p.unitPrice].Count() == 1).Select(p => p.productName)),
                           ref products, 
                           "ToLookup alternative Unique unitPrice Lambda Expession",
                           "OptimizerExtensions.AsGroup(() => products.ToLookup(p => p.unitPrice)).SelectMany(pLookup => products.Where(p => pLookup[p.unitPrice].Count() == 1).Select(p => p.productName))"
                           );

            
        }

    }
}
