using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestData
{
    public class Product
    {
        public Product(int productID, String productName, String category,
                double unitPrice, int unitsInStock)
        {
            this.productID = productID;
            this.productName = productName;
            this.category = category;
            this.unitPrice = unitPrice;
            this.unitsInStock = unitsInStock;
        }

        public int productID;
        public String productName;
        public String category;
        public double unitPrice;
        public int unitsInStock;
        public List<Product> related = new List<Product>();

        public override String ToString()
        {
            return "Product[productID=" + productID + ", productName=" + productName + ", category=" + category + ", unitPrice=" + unitPrice + ", unitsInStock=" + unitsInStock + "]";
        }

        public override int GetHashCode()
        {
            const int prime = 31;
            int result = 1;
            result = prime * result
                    + ((category == null) ? 0 : category.GetHashCode());
            result = prime * result + productID;
            result = prime * result
                    + ((productName == null) ? 0 : productName.GetHashCode());
            long temp;
            temp = BitConverter.DoubleToInt64Bits(unitPrice);
            result = prime * result + (int)(temp ^ (int)((uint)temp >> 32));
            result = prime * result + unitsInStock;
            return result;
        }

        public override bool Equals(Object obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if (GetType() != obj.GetType())
                return false;
            Product other = (Product)obj;
            if (category == null)
            {
                if (other.category != null)
                    return false;
            }
            else if (!category.Equals(other.category))
                return false;
            if (productID != other.productID)
                return false;
            if (productName == null)
            {
                if (other.productName != null)
                    return false;
            }
            else if (!productName.Equals(other.productName))
                return false;
            if (BitConverter.DoubleToInt64Bits(unitPrice) != BitConverter.DoubleToInt64Bits(other.unitPrice))
                return false;
            if (unitsInStock != other.unitsInStock)
                return false;
            return true;
        }
    }

}
