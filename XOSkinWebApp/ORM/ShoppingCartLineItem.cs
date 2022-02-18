using System;
using System.Collections.Generic;

namespace XOSkinWebApp.ORM
{
    public partial class ShoppingCartLineItem
    {
        public long Id { get; set; }
        public long ShoppingCart { get; set; }
        public long Product { get; set; }
        public int Quantity { get; set; }
        public decimal? Total { get; set; }

        public virtual Product ProductNavigation { get; set; }
        public virtual ShoppingCart ShoppingCartNavigation { get; set; }
    }
}
