using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class ShoppingCartLineItem
    {
        public long Id { get; set; }
        public long ShoppingCart { get; set; }
        public long Product { get; set; }
        public int Quantity { get; set; }

        public virtual Product ProductNavigation { get; set; }
        public virtual ShoppingCartHistory ShoppingCartNavigation { get; set; }
    }
}
