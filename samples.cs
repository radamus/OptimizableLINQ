using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Diagnostics;

namespace LINQConverters
{
    using LINQPerfTest;

    using TypeHelpers;
    using OptimisableLINQ;
    using SampleData;

    static class Samples
    {
        static ICollection<Product> sproducts = new List<Product>();
        
        static Stopwatch watch = new Stopwatch();

        public static void Compare<TSource>(this IEnumerable<TSource> source, Func<IQueryable<TSource>, IQueryable> expr)
        {
            Trace.WriteLine("\n****** Q U E R Y *****");
            watch.Restart();
            var warmingup = expr(source.AsQueryable());
            watch.Stop();
//            Trace.WriteLine("\n***** Lambda call warming up time: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + ")\n");
            
            watch.Restart();
            var orgQuery = expr(source.AsQueryable());
            watch.Stop();
            Trace.WriteLine("\n**** Queryable query instruction time: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + ")\n");
            Trace.WriteLine(orgQuery.Expression.ToString());

            watch.Restart();
            var q = orgQuery.Count();
            watch.Stop();
            Trace.WriteLine("*** Unoptimised query stopwatch: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + ")");
            Trace.WriteLine(q);

            watch.Restart();
            q = orgQuery.Count();
            watch.Stop();
            Trace.WriteLine("*** 2nd run: Unoptimised query stopwatch: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + ")");
            Trace.WriteLine(q);

            var warmingup2 = expr(source.AsOptimizable()).GetEnumerator();

            watch.Restart();
            var optQuery = expr(source.AsOptimizable());
            optQuery.GetEnumerator();
            watch.Stop();
            Trace.WriteLine("\n** Query optimisation instruction time: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + ")\n");
            Trace.WriteLine("\nOptimized query:\n");
            Trace.WriteLine(new Rewriter().Optimize(orgQuery.Expression).ToString());

            watch.Restart();
            var q1 = optQuery.Count();
            watch.Stop();
            Trace.WriteLine("* Optimized query stopwatch: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + ")");
            Trace.WriteLine(q1);
            
            watch.Restart();
            q1 = optQuery.Count();
            watch.Stop();
            Trace.WriteLine("* 2nd run: Optimized query stopwatch: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + ")");
            Trace.WriteLine(q1);

        }

        private static void testQueryInstructionTimes()
        {
            ICollection<Product> lproducts = new List<Product>();
            ICollection<Product> rproducts = new List<Product>();

            watch.Restart();
            var xoptQuery = lproducts.AsOptimizable().Join(rproducts, p => lproducts.Where(p1 => p1.productName == "Ikura").
                  Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, p => lproducts.
                      Where(p1 => p1.productName == "Tofu").Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, (p, s) => p.productName);
            xoptQuery.GetEnumerator();
            watch.Stop();
            Trace.WriteLine("* Warmup query optimisation instruction time: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + ")");

            watch.Restart();
            var orgQuery = lproducts.Join(rproducts, p => lproducts.Where(p1 => p1.productName == "Ikura").
                  Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, p => lproducts.
                      Where(p1 => p1.productName == "Tofu").Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, (p, s) => p.productName);
            watch.Stop();
            Trace.WriteLine("* Original instruction time: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + ")");

            watch.Restart();
            orgQuery = lproducts.Join(rproducts, p => lproducts.Where(p1 => p1.productName == "Ikura").
                  Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, p => lproducts.
                      Where(p1 => p1.productName == "Tofu").Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, (p, s) => p.productName);
            watch.Stop();
            Trace.WriteLine("* 2nd run: Original instruction time: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + ")");

            watch.Restart();
            var queryable = lproducts.AsQueryable().Join(rproducts, p => lproducts.Where(p1 => p1.productName == "Ikura").
                  Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, p => lproducts.
                      Where(p1 => p1.productName == "Tofu").Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, (p, s) => p.productName);
            watch.Stop();
            Trace.WriteLine("* Queryable instruction time: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + ")");

            watch.Restart();
            queryable = lproducts.AsQueryable().Join(rproducts, p => lproducts.Where(p1 => p1.productName == "Ikura").
                  Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, p => lproducts.
                      Where(p1 => p1.productName == "Tofu").Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, (p, s) => p.productName);
            watch.Stop();
            Trace.WriteLine("* 2nd run: Queryable instruction time: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + ")");

            watch.Restart();
            queryable = lproducts.AsQueryable().Join(rproducts, p => lproducts.Where(p1 => p1.productName == "Ikura").
                  Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, p => lproducts.
                      Where(p1 => p1.productName == "Tofu").Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, (p, s) => p.productName);
            queryable.GetEnumerator();
            watch.Stop();
            Trace.WriteLine("* Queryable compilation time: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + ")");

            watch.Restart();
            queryable = lproducts.AsQueryable().Join(rproducts, p => lproducts.Where(p1 => p1.productName == "Ikura").
                  Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, p => lproducts.
                      Where(p1 => p1.productName == "Tofu").Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, (p, s) => p.productName);
            queryable.GetEnumerator();
            watch.Stop();
            Trace.WriteLine("* 2nd run: Queryable compilation time: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + ")");


            watch.Restart();
            var optQuery = lproducts.AsOptimizable().Join(rproducts, p => lproducts.Where(p1 => p1.productName == "Ikura").
                  Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, p => lproducts.
                      Where(p1 => p1.productName == "Tofu").Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, (p, s) => p.productName);
            watch.Stop();
            Trace.WriteLine("* Optimisable instruction time: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + ")");

            watch.Restart();
            optQuery = lproducts.AsOptimizable().Join(rproducts, p => lproducts.Where(p1 => p1.productName == "Ikura").
                  Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, p => lproducts.
                      Where(p1 => p1.productName == "Tofu").Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, (p, s) => p.productName);
            watch.Stop();
            Trace.WriteLine("* 2nd run: Optimisable instruction time: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + ")");

            watch.Restart();
            optQuery = lproducts.AsOptimizable().Join(rproducts, p => lproducts.Where(p1 => p1.productName == "Ikura").
                  Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, p => lproducts.
                      Where(p1 => p1.productName == "Tofu").Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, (p, s) => p.productName);
            optQuery.GetEnumerator();
            watch.Stop();
            Trace.WriteLine("* Query optimisation instruction time: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + ")");

            watch.Restart();
            optQuery = lproducts.AsOptimizable().Join(rproducts, p => lproducts.Where(p1 => p1.productName == "Ikura").
                  Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, p => lproducts.
                      Where(p1 => p1.productName == "Tofu").Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, (p, s) => p.productName);
            optQuery.GetEnumerator();
            watch.Stop();
            Trace.WriteLine("* 2nd run: Query optimisation instruction time: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + ")");

        }


        public static void testExprTree()
        {

            testQueryInstructionTimes();

            ICollection<Product> lproducts = new List<Product>();
            SimpleGenerator.fillProducts(ref lproducts);

            ICollection<Product> rproducts = new List<Product>();
            SimpleGenerator.fillProducts(ref rproducts);

            Compare<Product>(lproducts,
              ep => ep.Join(rproducts, p => ep.Where(p1 => p1.productName == "Ikura").
                  Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, p => lproducts.
                      Where(p1 => p1.productName == "Tofu").Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, (p, s) => p.productName));

     
            Compare<Product>(lproducts,
               ep => ep.GroupBy(key => key.category).Select(p => p.Where(p1 => p1.unitPrice == p.Select(p3 => p3.unitPrice).Max()).Count()));

            Compare<Product>(lproducts,
                ep => ep.OrderBy(p => p.category).Where(p => lproducts.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).Contains(p.unitPrice)).Select(p => p.productName));

            Compare<Product>(lproducts,
                ep => ep.Where(p => lproducts.Where(p2 => (p2.productName == "Ikura")).Select(p2 => p2.unitPrice).Max() == (p.unitPrice)));

            Compare<Product>(lproducts,
                ep => ep.Where(p => lproducts.Where(p2 => (p2.productName == "Ikura")).
                    Select(p2 => p2.unitPrice).Max() + lproducts.Where(p2 => (p2.productName == "Ikura")).Select(p2 => p2.unitPrice).Min() == (p.unitPrice)));

            Compare<Product>(lproducts,
                ep => ep.GroupBy(key => key.productName).SelectMany(p => p.Where(p1 => p1.unitPrice == lproducts.Where(p2 => p2.unitsInStock > 50).Select(p3 => p3.unitPrice).Max())));

            Compare<Product>(lproducts,
                ep => from Product p in ep where (from Product p2 in lproducts where p2.productName == "Ikura" select p2.unitPrice).Contains(p.unitPrice) select p.productName);

            Compare<Product>(lproducts,
                ep => from Product p in ep where (from Product p2 in lproducts select p2.unitPrice).Max() == p.unitPrice select p.productName);
/*
            Compare<Product>(lproducts,
                ep => ep.Where(p => lproducts.Any(p2 => p2.unitPrice == Math.Round(p.unitPrice / 1.2, 2))).Select(p => p.productName));

            int max = 1000;

            Compare<int>(Enumerable.Range(1, max + 1),
                er => er.SelectMany(a => Enumerable.Range(a, max + 1 - a).SelectMany(b => Enumerable.Range(b, max + 1 - b).Where(c => a * a + b * b == c * c).Select(r => true))));
*/
        }

    }
}
