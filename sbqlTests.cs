using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace TestData
{
    using SBQL;
    public static class SbqlTests
    {
        static DateTime startTime;
        static Stopwatch watch = new Stopwatch();
        public static void TestSBQL()
        {
            int numOfSteps = 20;
            int numberOfTests = 1;
            ICollection<Product> lproducts = new List<Product>();
            
            Trace.WriteLine("filling data");
            for (int i = 0; i < numOfSteps; i++)
            {
                Data.fillProducts100t(ref lproducts, lproducts.Count);
            }
            Trace.WriteLine("data filled");


            startTime = DateTime.Now;
            watch.Restart();
            for (int i = 0; i < numberOfTests; i++)
            {
                var resbql = lproducts.SBQLWhere(p => p.productName == "Ikura").Navi(p => p.productID).Count();
                
            }
            watch.Stop();
            Trace.WriteLine("");
            Trace.WriteLine("Zapytanie sbql stopwatch: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + "), zegar: " + (DateTime.Now - startTime).TotalMilliseconds);

            startTime = DateTime.Now;
            watch.Restart();
            for (int i = 0; i < numberOfTests; i++)
            {
                var reslinq = lproducts.Where(p => p.productName == "Ikura").Select(p => p.productID).Count();                
            }
            watch.Stop();
            Trace.WriteLine("");
            Trace.WriteLine("Zapytanie linq stopwatch: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + "), zegar: " + (DateTime.Now - startTime).TotalMilliseconds);
            int[] empty = new int[0];
           var cbres = lproducts.Select(p => new ProductIndex() { product = p, index = 0 }).CloseBy(p => p.index < 2 ? p.product.related.Select(p1 => new ProductIndex() { product = p1, index = (++p.index) }) : new ProductIndex[0] ).Select( p => p.product).Distinct();

            var res = lproducts.CloseBy(p => p.related.SBQLWhere(p1 => p1.unitsInStock < 10)).Distinct().Count();
            
   //         TestSingle(lproducts, numOfSteps, c => c.Select(p => new ProductIndex() { product = p, index = 0 }).CloseBy(p => p.index < 2 ? p.product.related.Select(p1 => new ProductIndex() { product = p1, index = (p.index + 1) }) : new ProductIndex[0]).Select(p => p.product).Distinct().Count());


//            TestSingle(lproducts, numOfSteps, c => c.Where(p => p.productName == "Ikura").Select(p => p.productID).Count());
//            TestSingle(lproducts, numOfSteps, c => c.SBQLWhere(p => p.productName == "Ikura").Navi(p => p.productID).Count()); 
            //int[] empty = new int[0];
            //startTime = DateTime.Now;
            //watch.Restart();
            //var cbres = new int[] { 0 }.CloseBy(p => p < 1000000 ? new int[] { p + 1 } : empty).Count();
            //watch.Stop();
            //Trace.WriteLine(cbres);
            //Trace.WriteLine("Zapytanie closeby stopwatch: " + watch.ElapsedMilliseconds + " (" + watch.Elapsed + "), zegar: " + (DateTime.Now - startTime).TotalMilliseconds);
           TestSingle(lproducts, numOfSteps, q2 => q2.SelectMany(p => p.related.Select(r => new { product = p.productName, related = r.productName })).Count()); 
            TestSingle(lproducts, numOfSteps, q1 => q1.Join(p => p.related, (p, n) => new { product = p.productName, related = n.productName }).Count()); 
         //   TestMany(lproducts, numOfElems,q1 => q1.Join(p => p.related, (p, n) => new { product = p.productName, related = n.productName }).Count(), q2 => q2.SelectMany(p => p.related.Select(r => new { product = p.productName, related = r.productName })).Count()); 

        }

        static void TestMany<T, R>(ICollection<T> col, int numOfSteps, Func<IEnumerable<T>, R> q1, Func<IEnumerable<T>, R> q2)
        {
            Trace.WriteLine("i | numOfElems | numOfRes | q1 Time | numOfRes | q2 Time");
            int count = col.Count;
            if (count % numOfSteps != 0)
                throw new InvalidOperationException("col.Count % numOfSteps != 0");
            int step = count / numOfSteps;
            for (int i = 0; i < numOfSteps; i++)
            {
                StringBuilder builder = new StringBuilder();
                int elems = step * (i + 1);
                startTime = DateTime.Now;
                watch.Restart();
                var cbres = q1(col.Take(elems));
                watch.Stop();
                builder.Append(i);
                builder.Append(". | ");
                builder.Append(elems);
                builder.Append("    | ");
                builder.Append(cbres);
                builder.Append("  | ");
                builder.Append(watch.ElapsedMilliseconds);
                builder.Append("  | ");
                

                startTime = DateTime.Now;
                watch.Restart();
                
                cbres = q2(col.Take(elems));
                watch.Stop();

                builder.Append(cbres);
                builder.Append(" | ");
                builder.Append(watch.ElapsedMilliseconds);
                Trace.WriteLine(builder.ToString());
                
            }
        }

        static void TestSingle<T, R>(ICollection<T> col, int numOfSteps, Func<IEnumerable<T>, R> query)
        {
            Trace.WriteLine("i | numOfElems | numOfRes | q1 Time");
            int count = col.Count;
            if (count % numOfSteps != 0)
                throw new InvalidOperationException("col.Count % numOfSteps != 0");
            int step = count / numOfSteps;
            for (int i = 0; i < numOfSteps; i++)
            {
                StringBuilder builder = new StringBuilder();
                int elems = step * (i + 1);
                startTime = DateTime.Now;
                watch.Restart();
                var cbres = query(col.Take(elems));
                watch.Stop();
                builder.Append(i);
                builder.Append(". | ");
                builder.Append(elems);
                builder.Append("    | ");
                builder.Append(cbres);
                builder.Append("  | ");
                builder.Append(watch.ElapsedMilliseconds);
                Trace.WriteLine(builder.ToString());
            }
        }

        class ProductIndex
        {
           public  Product product;
           public  int index;
        }
    }

}
