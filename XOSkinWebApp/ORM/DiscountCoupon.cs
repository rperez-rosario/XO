using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class DiscountCoupon
    {
        public DiscountCoupon()
        {
            DiscountCouponProducts = new HashSet<DiscountCouponProduct>();
            ProductOrderDiscountCoupons = new HashSet<ProductOrderDiscountCoupon>();
            ShoppingCartDiscountCoupons = new HashSet<ShoppingCartDiscountCoupon>();
        }

        public long Id { get; set; }
        public bool Active { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool DiscountAsInNproductPercentage { get; set; }
        public decimal? DiscountNproductPercentage { get; set; }
        public bool DiscountAsInNproductDollars { get; set; }
        public decimal? DiscountInNproductDollars { get; set; }
        public short? DiscountProductN { get; set; }
        public decimal? MinimumPurchase { get; set; }
        public bool DiscountAsInGlobalOrderPercentage { get; set; }
        public decimal? DiscountGlobalOrderPercentage { get; set; }
        public bool DiscountAsInGlobalOrderDollars { get; set; }
        public decimal? DiscountGlobalOrderDollars { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual AspNetUser CreatedByNavigation { get; set; }
        public virtual AspNetUser LastUpdatedByNavigation { get; set; }
        public virtual ICollection<DiscountCouponProduct> DiscountCouponProducts { get; set; }
        public virtual ICollection<ProductOrderDiscountCoupon> ProductOrderDiscountCoupons { get; set; }
        public virtual ICollection<ShoppingCartDiscountCoupon> ShoppingCartDiscountCoupons { get; set; }
    }
}
