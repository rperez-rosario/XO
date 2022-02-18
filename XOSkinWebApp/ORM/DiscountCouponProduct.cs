using System;
using System.Collections.Generic;

namespace XOSkinWebApp.ORM
{
    public partial class DiscountCouponProduct
    {
        public long Id { get; set; }
        public long Coupon { get; set; }
        public long Product { get; set; }

        public virtual DiscountCoupon CouponNavigation { get; set; }
        public virtual Product ProductNavigation { get; set; }
    }
}
