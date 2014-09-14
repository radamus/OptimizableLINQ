using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestData
{
    class samples
    {
        void foo()
        {
            ICollection<Product> lproducts = new List<Product>();
            Data.fillProducts(ref lproducts);
            IQueryable<Product> products = lproducts.AsQueryable();
            var maxIkuraUnitPrice = calculateMaxUnitPrice(products, "IKura");
            var minTofuUnitPrice = calculateMinUnitPrice(products, "Tofu");

            var result = calculateResult(products, maxIkuraUnitPrice, minTofuUnitPrice);




            var result1 = (from Product p in products
                          where p.unitPrice == (from Product pm in products
                                                where pm.productName == "Ikura"
                                                select pm.unitPrice).Max() +
                                                (from Product pm in products
                                                 where pm.productName == "Tofu"
                                                 select pm.unitPrice).Min()
                          select p).Count();
        }

        private static object calculateResult(IQueryable<Product> products, object maxIkuraUnitPrice, object minTofuUnitPrice)
        {
            throw new NotImplementedException();
        }

        private static object calculateMinUnitPrice(IQueryable<Product> products, string p)
        {
            throw new NotImplementedException();
        }

        private static object calculateMaxUnitPrice(IQueryable<Product> products, string p)
        {
            throw new NotImplementedException();
        }

    }
}
