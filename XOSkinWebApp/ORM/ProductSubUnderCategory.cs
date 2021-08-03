using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class ProductSubUnderCategory
    {
        public int Id { get; set; }
        public int SubCategory { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ProductSubCategory SubCategoryNavigation { get; set; }
    }
}
