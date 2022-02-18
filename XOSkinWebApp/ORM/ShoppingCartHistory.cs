using System;
using System.Collections.Generic;

namespace XOSkinWebApp.ORM
{
    public partial class ShoppingCartHistory
    {
        public long Id { get; set; }
        public long ShoppingCart { get; set; }
        public long Product { get; set; }
        public DateTime? DateAddedToCart { get; set; }
        public DateTime? DateRemovedFromCart { get; set; }
        public bool? PromotedToOrder { get; set; }

        public virtual Product ProductNavigation { get; set; }
        public virtual ShoppingCart ShoppingCartNavigation { get; set; }
    }
}
