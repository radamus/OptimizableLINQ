using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace SampleData
{
    public class SimpleExtendedGenerator
    {
        private static String PRODUCTS_FILE = "products.txt";

        public static void fillProducts(ref ICollection<Product> list, int limit = -1)
        {
            int counter = 0;

            string line;

            NumberFormatInfo provider = new NumberFormatInfo();
            provider.NumberDecimalSeparator = ".";
            
            System.IO.TextReader file;
            if (System.IO.File.Exists(PRODUCTS_FILE))
                file = new System.IO.StreamReader(PRODUCTS_FILE);
            else 
                file = new System.IO.StringReader(TestData.Properties.Resources.products);
            while ((line = file.ReadLine()) != null && limit > counter++)
            {
                string[] fields = line.Split(',');
                list.Add(new Product(Convert.ToInt32(fields[0]), fields[1], fields[2], Convert.ToDouble(fields[3], provider), Convert.ToInt32(fields[4])));
            }

            file.Close();
            addRelated(ref list);
        }

        private static void addRelated(ref ICollection<Product> list)
        {
            Random r = new Random(27);
            foreach (Product p in list)
            {
                if (p.related.Count == 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Product related;
                        do
                        {
                            related = list.ElementAt(r.Next(list.Count));
                        } while (p.Equals(related));
                        p.related.Add(related);
                    }
                }
            }
        }
    }
}