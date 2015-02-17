using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace LINQPerfTest
{
    using LINQConverters;
    using SampleData;
    using OptimisableLINQ;
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
            

            Samples.testExprTree();
 //          SbqlTests.TestSBQL();
            
 //          EnumerableExtensions.test();
            Trace.Unindent();
            
            Console.ReadLine();
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