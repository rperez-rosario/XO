using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class ProductOrderDiscountCoupon
    {
        public long Id { get; set; }
        public long Coupon { get; set; }
        public long ProductOrder { get; set; }

        public virtual DiscountCoupon CouponNavigation { get; set; }
        public virtual ProductOrder ProductOrderNavigation { get; set; }
    }
}
