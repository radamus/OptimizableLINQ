using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace LINQBenchmark
{
    namespace LINQSampleData
    {
        public class SimpleExtendedGenerator
        {
            public static void fillProducts(ref ICollection<Product> list, int limit = -1)
            {
                int counter = 0;
                
                string line;

                NumberFormatInfo provider = new NumberFormatInfo();
                provider.NumberDecimalSeparator = ".";

                System.IO.StreamReader file = new System.IO.StreamReader("products.txt");
                while ((line = file.ReadLine()) != null || limit == counter++)
                {
                    string[] fields = line.Split(',');
                    list.Add(new Product(Convert.ToInt32(fields[0]), fields[1], fields[2], Convert.ToDouble(fields[3], provider), Convert.ToInt32(fields[4])));    
                }

                file.Close();
            }
        }
    }
}