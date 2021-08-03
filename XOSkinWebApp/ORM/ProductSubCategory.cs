using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class ProductSubCategory
    {
        public ProductSubCategory()
        {
            ProductSubUnderCategories = new HashSet<ProductSubUnderCategory>();
        }

        public int Id { get; set; }
        public short Category { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ProductCategory CategoryNavigation { get; set; }
        public virtual ICollection<ProductSubUnderCategory> ProductSubUnderCategories { get; set; }
    }
}
