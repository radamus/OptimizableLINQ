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
    using Optimizer;
    using TestData;
    static class Samples
    {
        static ICollection<Product> sproducts = new List<Product>();
        
        static DateTime startTime;
        static Stopwatch watch = new Stopwatch();
        public static void Compare<TSource, TResult>(this IEnumerable<TSource> source, Func<IQueryable<TSource>, Expression> expr, Func<IEnumerable<TSource>, TResult> evalOriginal, Func<IEnumerable<TSource>, TResult> evalOptimized)
        {
            Trace.WriteLine("\nquery");
            watch.Restart();
            var finalQuery = expr(source.AsQueryable());
            watch.Stop();
            Trace.WriteLine("\nOptimisation time: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + ")\n");
            Trace.WriteLine(finalQuery.ToString());
            startTime = DateTime.Now;
            watch.Restart();
            var q = evalOriginal(source);
            watch.Stop();
            Trace.WriteLine("Zapytanie niezoptymalizowane stopwatch: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + "), zegar: " + (DateTime.Now - startTime).TotalMilliseconds);
            Trace.WriteLine(q);
            Trace.WriteLine("\noptimized query");
            Trace.WriteLine(expr(source.AsOptimizable()).ToString());
            startTime = DateTime.Now;
            watch.Restart();
            var q1 = evalOptimized(source);
            watch.Stop();
            Trace.WriteLine("Zapytanie zoptymalizowane  stopwatch: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + "), zegar: " + (DateTime.Now - startTime).TotalMilliseconds);

            
            Trace.WriteLine(q1);

        }
        public static void testExprTree()
        {
            
            watch.Start();
            ICollection<Product> lproducts = new List<Product>();
            Data.fillProducts(ref lproducts);

            ICollection<Product> rproducts = new List<Product>();
            Data.fillProducts(ref rproducts);

            IQueryable<Product> products = lproducts.AsQueryable();
            IQueryable<Product> products1 = rproducts.AsQueryable();

            var q = products.AsQueryable().GroupBy(key => key.productID).Select(g => g.First().related.Select(p => p.productName)).Expression;

            Compare<Product, int>(lproducts,
              ep => ep.Join(products1, p => products.Where(p1 => p1.productName == "Ikura").Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, p => products.Where(p1 => p1.productName == "Tofu").Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, (p, s) => p.productName).Expression,
              ep => ep.Join(lproducts, p => lproducts.Where(p1 => p1.productName == "Ikura").Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, p => lproducts.Where(p1 => p1.productName == "Tofu").Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, (p, s) => p.productName).Count(),
              ep => ep.AsOptimizable().Join(products1, p => products.Where(p1 => p1.productName == "Ikura").Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, p => products1.Where(p1 => p1.productName == "Tofu").Select(p1 => p1.unitPrice).Contains(p.unitPrice) ? p.unitPrice : 0, (p, s) => p.productName).Count());

            // -----------------------------------------------------------------------------------------------------------------------------------------------------------
            Compare<Product, int>(lproducts,
               ep => ep.GroupBy(key => key.category).Select(p => p.Where(p1 => p1.unitPrice == p.Select(p3 => p3.unitPrice).Max()).Count()).Expression,
               ep => ep.GroupBy(key => key.category).Select(p => p.Where(p1 => p1.unitPrice == p.Select(p3 => p3.unitPrice).Max()).Count()).Count(),
               ep => ep.AsOptimizable().GroupBy(key => key.category).Select(p => p.Where(p1 => p1.unitPrice == p.Select(p3 => p3.unitPrice).Max()).Count()).Count());
       
            
            //-----------------------------------------------------------------------------------------------------------------------------------------------------------
            Compare<Product, int>(lproducts,
                ep => ep.OrderBy(p => p.category).Where(p => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).Contains(p.unitPrice)).Select(p => p.productName).Expression,
                ep => ep.OrderBy(p => p.category).Where(p => lproducts.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).Contains(p.unitPrice)).Select(p => p.productName).Count(),
                ep => ep.AsOptimizable().Where(p => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).Contains(p.unitPrice)).Select(p => p.productName).Count());

            
            //-----------------------------------------------------------------------------------------------------------------------------------------------------------
            Compare<Product, int>(lproducts,
                ep => ep.Where(p => products.Where(p2 => (p2.productName == "Ikura")).Select(p2 => p2.unitPrice).Max() == (p.unitPrice)).Expression,
                ep => ep.Where(p => products.Where(p2 => (p2.productName == "Ikura")).Select(p2 => p2.unitPrice).Max() == (p.unitPrice)).Count(),
                ep => ep.AsOptimizable().Where(p => products.Where(p2 => (p2.productName == "Ikura")).Select(p2 => p2.unitPrice).Max() == (p.unitPrice)).Count());
            
            ////-----------------------------------------------------------------------------------------------------------------------------------------------------------

            Compare<Product, int>(lproducts,
                ep => ep.Where(p => products.Where(p2 => (p2.productName == "Ikura")).Select(p2 => p2.unitPrice).Max() + products.Where(p2 => (p2.productName == "Ikura")).Select(p2 => p2.unitPrice).Min() == (p.unitPrice)).Expression,
                ep => ep.Where(p => lproducts.Where(p2 => (p2.productName == "Ikura")).Select(p2 => p2.unitPrice).Max() + lproducts.Where(p2 => (p2.productName == "Ikura")).Select(p2 => p2.unitPrice).Min() == (p.unitPrice)).Count(),
                ep => ep.AsOptimizable().Where(p => products.Where(p2 => (p2.productName == "Ikura")).Select(p2 => p2.unitPrice).Max() + products.Where(p2 => (p2.productName == "Ikura")).Select(p2 => p2.unitPrice).Min() == (p.unitPrice)).Count());

            
            //-----------------------------------------------------------------------------------------------------------------------------------------------------------
            Compare<Product, int>(lproducts,
                ep => ep.GroupBy(key => key.productName).SelectMany(p => p.Where(p1 => p1.unitPrice == products.Where(p2 => p2.unitsInStock > 50).Select(p3 => p3.unitPrice).Max())).Expression,
                ep => ep.GroupBy(key => key.productName).SelectMany(p => p.Where(p1 => p1.unitPrice == products.Where(p2 => p2.unitsInStock > 50).Select(p3 => p3.unitPrice).Max())).Count(),
                ep => ep.AsOptimizable().GroupBy(key => key.productName).SelectMany(p => p.Where(p1 => p1.unitPrice == products.Where(p2 => p2.unitsInStock > 50).Select(p3 => p3.unitPrice).Max())).Count());
                    
        }

    }
}
