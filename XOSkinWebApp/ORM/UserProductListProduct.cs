using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class UserProductListProduct
    {
        public long Id { get; set; }
        public long UserProductList { get; set; }
        public long Product { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateRemoved { get; set; }
        public bool PromotedToShoppingCart { get; set; }

        public virtual UserProductList UserProductList1 { get; set; }
        public virtual Product UserProductListNavigation { get; set; }
    }
}
