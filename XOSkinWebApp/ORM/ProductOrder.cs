using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class ProductOrder
    {
        public ProductOrder()
        {
            OrderProducts = new HashSet<OrderProduct>();
            OrderShipTos = new HashSet<OrderShipTo>();
            PaymentPlanProductOrders = new HashSet<PaymentPlanProductOrder>();
            ProductOrderDiscountCodes = new HashSet<ProductOrderDiscountCode>();
            ProductOrderDiscountCoupons = new HashSet<ProductOrderDiscountCoupon>();
        }

        public long Id { get; set; }
        public long User { get; set; }
        public DateTime DatePlaced { get; set; }
        public decimal Subtotal { get; set; }
        public decimal CouponDiscount { get; set; }
        public decimal CodeDiscount { get; set; }
        public decimal ApplicableTaxes { get; set; }
        public decimal Total { get; set; }
        public bool GiftOrder { get; set; }

        public virtual User UserNavigation { get; set; }
        public virtual ICollection<OrderProduct> OrderProducts { get; set; }
        public virtual ICollection<OrderShipTo> OrderShipTos { get; set; }
        public virtual ICollection<PaymentPlanProductOrder> PaymentPlanProductOrders { get; set; }
        public virtual ICollection<ProductOrderDiscountCode> ProductOrderDiscountCodes { get; set; }
        public virtual ICollection<ProductOrderDiscountCoupon> ProductOrderDiscountCoupons { get; set; }
    }
}
