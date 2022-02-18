using System;
using System.Collections.Generic;

namespace XOSkinWebApp.ORM
{
    public partial class ShoppingCartDiscountCoupon
    {
        public long Id { get; set; }
        public long Coupon { get; set; }
        public long ShoppingCart { get; set; }

        public virtual DiscountCoupon CouponNavigation { get; set; }
        public virtual ShoppingCart IdNavigation { get; set; }
    }
}
