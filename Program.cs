using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace LINQSBQLExtensions
{

    public static class Extensions
    {
        public static TResult SelectEnum<TSource, TResult>(this IEnumerable<TSource> source, Func<IEnumerable<TSource>, TResult> selector)
        {
            return selector(source);
        }

        public static IEnumerable<TResult> GroupSelect<TSource, TResult>(this IEnumerable<TSource> source, Func<IEnumerable<TSource>, IEnumerable<TResult>> selector)
        {
            var groupas = selector(source.ToList());
            Console.WriteLine("lazy");
            foreach (var result in groupas)
            {
                yield return result;
            }
        } 
    }
}

namespace LINQPerfTest
{
    using LINQSBQLExtensions;
    using LINQConverters;
    using TestData;
    using Optimizer;
    using System.IO;
    class Program
    {
      
        static void Main(string[] args)
        {
            string path = @"c:\temp\MyTest.txt";
            StreamWriter sw;
            if (!File.Exists(path))
            {
                sw = File.CreateText(path);
            }
            else
            {
                sw = new StreamWriter(File.OpenWrite(path));
            }

            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Trace.Listeners.Add(new TextWriterTraceListener(sw));
            Trace.AutoFlush = true;
            Trace.Indent();
            //Console.WriteLine("Czy zegar oferuje wysoka dokladnosc: {0}", Stopwatch.IsHighResolution);
          
          //  tests();
            //zaladowanie danych

//            var res = products.Select(p => p.productName).ToList();



       Samples.testExprTree();
    //   SbqlTests.TestSBQL();
            
   //      EnumerableExtensions.test();
       Trace.Unindent();
            
            Console.ReadLine();
        }

        private static void tests()
        {
            DateTime startTime = DateTime.Now;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            ICollection<Product> products = new List<Product>();
            Data.fillProducts(ref products);
            watch.Stop();
            Console.WriteLine("Czas ladowania stopwatch: {0} ({1}), zegar: {2}", watch.ElapsedMilliseconds, watch.Elapsed, (DateTime.Now - startTime).TotalMilliseconds);




            //Console.WriteLine(Environment.NewLine + "Wyszukiwanie standardowe");
            //startTime = DateTime.Now;
            //watch.Restart();
            //Dictionary<Product, int> tmp2 = new Dictionary<Product, int>(new ProductComparer());
            //foreach (Product p in products)
            //{
            //    if (tmp2.ContainsKey(p))
            //        tmp2[p]++;
            //    else
            //        tmp2[p] = 1;
            //}
            //IList<string> result4 = new List<string>();
            //foreach (KeyValuePair<Product, int> p in tmp2)
            //{
            //    if (p.Value == 1)
            //        result4.Add(p.Key.productName);
            //}
            //int res = result4.Count();
            //watch.Stop();
            //Console.WriteLine("Liczba uzyskanych produktow (res): {0}", res);

            //Console.WriteLine("Czas wyszukiwania stopwatch: {0} ({1}), zegar: {2}", watch.ElapsedMilliseconds, watch.Elapsed, (DateTime.Now - startTime).TotalMilliseconds);
            // products.Where(p => products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).Contains(p.unitPrice)).Select(p => p.productName);
            // products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).Select(u => products.Where(p => u == p.unitPrice).Select(p => p.productName));
            Console.WriteLine(Environment.NewLine + "Wyszukiwanie Linq #{ Product where unitPrice in ((Product where productName ==\"Ikura\").unitPrice)) }");
            startTime = DateTime.Now;
            watch.Restart();
            //#{ Product where unitPrice in ((Product where productName ==\"Ikura\").unitPrice)) }
            var result12 = from Product p in products
                           where (from Product p2 in products where p2.productName == "Ikura" select p2.unitPrice).Contains(p.unitPrice)
                           select p.productName;
            //wlasciwe pobranie przy odwolaniu (lazy)
            var res = result12.Count();
            watch.Stop();
            Console.WriteLine("Liczba uzyskanych produktow (res): {0}", res);
            watch.Stop();
            Console.WriteLine("Czas wyszukiwania stopwatch: {0} ({1}), zegar: {2}", watch.ElapsedMilliseconds, watch.Elapsed, (DateTime.Now - startTime).TotalMilliseconds);




            Console.WriteLine(Environment.NewLine + "Wyszukiwanie Linq zoptymalizowane wersja z groupby #{ (((Product as p2 where p2.productName ==\"Ikura\").p2.unitPrice) groupas u).(Product as p where u contains p.unitPrice) }");
            startTime = DateTime.Now;
            watch.Restart();
            //#{ Product where unitPrice in ((Product where productName ==\"Ikura\").unitPrice)) }
            var result13 = products.Where(p => p.productName == "Ikura").Select(p => p.unitPrice).GroupBy(g => 0).SelectMany(uenum => products.Where(p => uenum.Contains(p.unitPrice)).Select(p => p.productName));
            //wlasciwe pobranie przy odwolaniu (lazy)
            res = result13.Count();
            watch.Stop();
            Console.WriteLine("Liczba uzyskanych produktow (res): {0}", res);
            //            watch.Stop();
            Console.WriteLine("Czas wyszukiwania stopwatch: {0} ({1}), zegar: {2}", watch.ElapsedMilliseconds, watch.Elapsed, (DateTime.Now - startTime).TotalMilliseconds);

            Console.WriteLine(Environment.NewLine + "Wyszukiwanie Linq zoptymalizowane wersja z selectEnumLazy#{ (((Product as p2 where p2.productName ==\"Ikura\").p2.unitPrice) groupas u).(Product as p where u contains p.unitPrice) }");
            startTime = DateTime.Now;
            watch.Restart();
            var result14 = products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).GroupSelect(uenum => products.Where(p => uenum.Contains(p.unitPrice)).Select(p => p.productName));
            //wlasciwe pobranie przy odwolaniu (lazy)
            res = result14.Count();
            watch.Stop();
            Console.WriteLine("Liczba uzyskanych produktow (res): {0}", res);
            //            watch.Stop();
            Console.WriteLine("Czas wyszukiwania stopwatch: {0} ({1}), zegar: {2}", watch.ElapsedMilliseconds, watch.Elapsed, (DateTime.Now - startTime).TotalMilliseconds);

            Console.WriteLine(Environment.NewLine + "Wyszukiwanie Linq zoptymalizowane wersja dwuzapytaniowa #{ (((Product as p2 where p2.productName ==\"Ikura\").p2.unitPrice) groupas u).(Product as p where u contains p.unitPrice) }");
            startTime = DateTime.Now;
            watch.Restart();
            //#{ Product where unitPrice in ((Product where productName ==\"Ikura\").unitPrice)) }

            IEnumerable<double> aux0 = products.Where(p2 => p2.productName == "Ikura").Select(p2 => p2.unitPrice).ToList();
            result14 = products.Where(p => aux0.Contains(p.unitPrice)).Select(p => p.productName);
            //wlasciwe pobranie przy odwolaniu (lazy)
            res = result14.Count();
            watch.Stop();
            Console.WriteLine("Liczba uzyskanych produktow (res): {0}", res);
            //            watch.Stop();
            Console.WriteLine("Czas wyszukiwania stopwatch: {0} ({1}), zegar: {2}", watch.ElapsedMilliseconds, watch.Elapsed, (DateTime.Now - startTime).TotalMilliseconds);

            // var result = products.Select(p => p.category).Distinct().Join(products, ucat => ucat, p => p.category, (ucat, p) => new {ucat = ucat, p = p}).GroupBy(a => a.u);


            //         Console.WriteLine(Environment.NewLine + "Wyszukiwanie Linq Object res = #{ (unique(product.category) as c.(c, ((product where unitPrice == max((product where category == c).unitPrice)).productName))) }; ");
            //startTime = DateTime.Now;
            //watch.Restart();
            //// Object res = #{ (unique(product.category) as c.(c, ((product where unitPrice == max((product where category == c).unitPrice)).productName))) }; 
            //var result = products.GroupBy(p => p.category).SelectMany(g => g.Where(p1 => p1.unitPrice == g.Max(g1 => g1.unitPrice))).ToDictionary(r => r.category, r => r.productName).ToList();
            //watch.Stop();
            //Console.WriteLine("Czas wyszukiwania stopwatch: {0} ({1}), zegar: {2}", watch.ElapsedMilliseconds, watch.Elapsed, (DateTime.Now - startTime).TotalMilliseconds);
            // Object res = #{ (unique(product.category) as c.(max((product where category == c).unitPrice) as mup.mup.(c, ((product where unitPrice == mup).productName)))) }; 
            // var resultopt = products.GroupBy(p => p.category).SelectMany(g => g.Where(p1 => p1.unitPrice == g.Max(g1 => g1.unitPrice))).ToDictionary(r => r.category, r => r.productName);



            //foreach(var r in result)
            //    Console.WriteLine(r);
        }

        class ProductComparer : IEqualityComparer<Product>
        {
            public bool Equals(Product x, Product y)
            {
                //Check whether the compared objects reference the same data.
                if (Object.ReferenceEquals(x, y)) return true;

                //Check whether any of the compared objects is null.
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;

                //Check whether the products' properties are equal.
                return x.unitPrice == y.unitPrice;
            }

            public int GetHashCode(Product product)
            {
                //Check whether the object is null
                if (Object.ReferenceEquals(product, null)) return 0;

                //Get hash code for the Code field.
                int hashUnitPrice = product.unitPrice.GetHashCode();

                //Calculate the hash code for the product.
                return hashUnitPrice;
            }
        }
    }
}