using System;
using System.Collections.Generic;

namespace XOSkinWebApp.ORM
{
    public partial class ShoppingCart
    {
        public ShoppingCart()
        {
            ShoppingCartDiscountCodes = new HashSet<ShoppingCartDiscountCode>();
            ShoppingCartHistories = new HashSet<ShoppingCartHistory>();
            ShoppingCartLineItems = new HashSet<ShoppingCartLineItem>();
        }

        public long Id { get; set; }
        public string User { get; set; }

        public virtual AspNetUser UserNavigation { get; set; }
        public virtual ShoppingCartDiscountCoupon ShoppingCartDiscountCoupon { get; set; }
        public virtual ICollection<ShoppingCartDiscountCode> ShoppingCartDiscountCodes { get; set; }
        public virtual ICollection<ShoppingCartHistory> ShoppingCartHistories { get; set; }
        public virtual ICollection<ShoppingCartLineItem> ShoppingCartLineItems { get; set; }
    }
}
