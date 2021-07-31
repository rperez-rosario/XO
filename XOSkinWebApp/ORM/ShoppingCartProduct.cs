using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class ShoppingCartProduct
    {
        public long ShoppingCart { get; set; }
        public long Product { get; set; }

        public virtual Product ProductNavigation { get; set; }
        public virtual ShoppingCart ShoppingCartNavigation { get; set; }
    }
}
