using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class ProductCategory
    {
        public ProductCategory()
        {
            ProductSubCategories = new HashSet<ProductSubCategory>();
            Products = new HashSet<Product>();
        }

        public short Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<ProductSubCategory> ProductSubCategories { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
