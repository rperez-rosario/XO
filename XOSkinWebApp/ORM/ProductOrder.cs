using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class ProductOrder
    {
        public ProductOrder()
        {
            OrderBillTos = new HashSet<OrderBillTo>();
            OrderShipTos = new HashSet<OrderShipTo>();
            PaymentPlanProductOrders = new HashSet<PaymentPlanProductOrder>();
            ProductOrderDiscountCodes = new HashSet<ProductOrderDiscountCode>();
            ProductOrderDiscountCoupons = new HashSet<ProductOrderDiscountCoupon>();
            ProductOrderLineItems = new HashSet<ProductOrderLineItem>();
            UserLedgerTransactions = new HashSet<UserLedgerTransaction>();
        }

        public long Id { get; set; }
        public long? ShopifyId { get; set; }
        public string StripeChargeId { get; set; }
        public string StripeChargeStatus { get; set; }
        public bool? Completed { get; set; }
        public string User { get; set; }
        public DateTime DatePlaced { get; set; }
        public decimal Subtotal { get; set; }
        public decimal CouponDiscount { get; set; }
        public decimal CodeDiscount { get; set; }
        public decimal? ShippingCost { get; set; }
        public decimal ApplicableTaxes { get; set; }
        public decimal Total { get; set; }
        public bool GiftOrder { get; set; }
        public bool? Cancelled { get; set; }
        public DateTime? CancelledOn { get; set; }
        public string CancelReason { get; set; }
        public string CancelledBy { get; set; }

        public virtual AspNetUser UserNavigation { get; set; }
        public virtual ICollection<OrderBillTo> OrderBillTos { get; set; }
        public virtual ICollection<OrderShipTo> OrderShipTos { get; set; }
        public virtual ICollection<PaymentPlanProductOrder> PaymentPlanProductOrders { get; set; }
        public virtual ICollection<ProductOrderDiscountCode> ProductOrderDiscountCodes { get; set; }
        public virtual ICollection<ProductOrderDiscountCoupon> ProductOrderDiscountCoupons { get; set; }
        public virtual ICollection<ProductOrderLineItem> ProductOrderLineItems { get; set; }
        public virtual ICollection<UserLedgerTransaction> UserLedgerTransactions { get; set; }
    }
}
