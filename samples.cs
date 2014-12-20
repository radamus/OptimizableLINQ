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
    using LINQ_SBQLConverters;
    
    using SBQL.Expressions;
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
            Trace.WriteLine("query");
            watch.Restart();
            var finalQuery = expr(source.AsQueryable());
            watch.Stop();
            Trace.WriteLine("Optimisation time: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + ")");
            Trace.WriteLine(finalQuery.ToString());
            startTime = DateTime.Now;
            watch.Restart();
            var q = evalOriginal(source);
            watch.Stop();
            Trace.WriteLine("Zapytanie niezoptymalizowane stopwatch: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + "), zegar: " + (DateTime.Now - startTime).TotalMilliseconds);
            Trace.WriteLine(q);
            Trace.WriteLine("optimized query");
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
            //foreach (LocalVariableInfo li in MethodBase.GetCurrentMethod().GetMethodBody().LocalVariables)
            //{
            //    Console.WriteLine(li.LocalIndex + ": " + li.LocalType.Name);
            //}
            //Expression<Func<Product, bool>> lambda = p2 => p2.productName == "Ikura";
            //Expression<Func<IEnumerable<Product>, IEnumerable<Product>>> exe = Expression.Lambda<Func<IEnumerable<Product>, IEnumerable<Product>>>(genWhereTree(products.AsQueryable(), lambda), Expression.Parameter(typeof(
            //IEnumerable<Product>)));
            //var result = exe.Compile()(products).Select(p => p.productName);
            //foreach (var r in result)
            //    Console.WriteLine(r);
     //      var tst = products.GroupBy(p => p.productName).Select(p => p.AsQueryable<Product>());
       //     var q1 = products.Where(p2 => (p2.productName != "Ikura")).Select(p2 => p2.unitPrice).GroupBy(key => 0).Select( aux0 => aux0.Max() ).SelectMany(aux1 => products.Where(p => aux1 == (p.unitPrice))).Count();
        //    var q2 = products.Where(p2 => (p2.productName != "Ikura")).Select(p2 => p2.unitPrice).GroupBy(key => 0).SelectMany(aux1 => products.Where(p => aux1.Max() == (p.unitPrice))).Count();
            var q = products.AsQueryable().GroupBy(key => key.productID).Select(g => g.First().related.Select(p => p.productName)).Expression;
            //sproducts.AsQueryable().Where(p => sproducts.Contains(sproducts.Where(p1 => p1.productName == "IKura").FirstOrDefault())).Select(p => products.Select(p1 => p1.productID)).Expression;
                
           // var q =  products.Where(p => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).Contains(p.unitPrice)).Select(p => p.productName).Count();
    //        var q = products.Where(p2 => (p2.productName == "Ikura")).Select(p2 => p2.unitPrice).GroupBy(key => 0).SelectMany(aux0 => products.Where(p => aux0.Contains(p.unitPrice))).Expression;

  //          var test = products.Where(p1 => (p1.productName == "Ikura")).Select(p1 => p1.unitPrice).GroupBy(key => 0).Select(aux2 => aux2).SelectMany(aux3 => products.Where(p1 => (p1.productName == "Tofu")).Select(p1 => p1.unitPrice).GroupBy(key => 0).Select(aux0 => aux0).SelectMany(aux1 => products.Join(products, p =>  aux3.Contains(p.unitPrice)? p.unitPrice: 0, p => aux1.Contains(p.unitPrice)? p.unitPrice: 0, (p, s) => p.productName)));

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
