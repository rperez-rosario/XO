using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class ShoppingCart
    {
        public ShoppingCart()
        {
            ShoppingCartDiscountCodes = new HashSet<ShoppingCartDiscountCode>();
            ShoppingCartHistories = new HashSet<ShoppingCartHistory>();
        }

        public long Id { get; set; }
        public long User { get; set; }

        public virtual User UserNavigation { get; set; }
        public virtual ShoppingCartDiscountCoupon ShoppingCartDiscountCoupon { get; set; }
        public virtual ICollection<ShoppingCartDiscountCode> ShoppingCartDiscountCodes { get; set; }
        public virtual ICollection<ShoppingCartHistory> ShoppingCartHistories { get; set; }
    }
}
