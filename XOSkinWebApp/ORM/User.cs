using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class User
    {
        public User()
        {
            Addresses = new HashSet<Address>();
            DiscountCodeCreatedByNavigations = new HashSet<DiscountCode>();
            DiscountCodeLastUpdatedByNavigations = new HashSet<DiscountCode>();
            DiscountCouponCreatedByNavigations = new HashSet<DiscountCoupon>();
            DiscountCouponLastUpdatedByNavigations = new HashSet<DiscountCoupon>();
            PaymentPlanCreatedByNavigations = new HashSet<PaymentPlan>();
            PaymentPlanProductOrderCreatedByNavigations = new HashSet<PaymentPlanProductOrder>();
            PaymentPlanProductOrderLastUpdatedByNavigations = new HashSet<PaymentPlanProductOrder>();
            PaymentPlanScheduleCreatedByNavigations = new HashSet<PaymentPlanSchedule>();
            PaymentPlanScheduleLastUpdatedByNavigations = new HashSet<PaymentPlanSchedule>();
            PaymentPlanSchedulePaymentCreatedByNavigations = new HashSet<PaymentPlanSchedulePayment>();
            PaymentPlanSchedulePaymentLastUpdatedByNavigations = new HashSet<PaymentPlanSchedulePayment>();
            PaymentPlanUserNavigations = new HashSet<PaymentPlan>();
            Prices = new HashSet<Price>();
            ProductCreatedByNavigations = new HashSet<Product>();
            ProductLastUpdatedByNavigations = new HashSet<Product>();
            ProductOrders = new HashSet<ProductOrder>();
            ShoppingCarts = new HashSet<ShoppingCart>();
            UserLedgerTransactionCreatedByNavigations = new HashSet<UserLedgerTransaction>();
            UserLedgerTransactionUserNavigations = new HashSet<UserLedgerTransaction>();
            UserProductLists = new HashSet<UserProductList>();
        }

        public long Id { get; set; }
        public string EmailAddress { get; set; }
        public string HomePhoneNumber { get; set; }
        public string WorkPhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? PreferredLanguage { get; set; }
        public bool IsLocked { get; set; }
        public short UserGroup { get; set; }

        public virtual Language PreferredLanguageNavigation { get; set; }
        public virtual UserGroup UserGroupNavigation { get; set; }
        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<DiscountCode> DiscountCodeCreatedByNavigations { get; set; }
        public virtual ICollection<DiscountCode> DiscountCodeLastUpdatedByNavigations { get; set; }
        public virtual ICollection<DiscountCoupon> DiscountCouponCreatedByNavigations { get; set; }
        public virtual ICollection<DiscountCoupon> DiscountCouponLastUpdatedByNavigations { get; set; }
        public virtual ICollection<PaymentPlan> PaymentPlanCreatedByNavigations { get; set; }
        public virtual ICollection<PaymentPlanProductOrder> PaymentPlanProductOrderCreatedByNavigations { get; set; }
        public virtual ICollection<PaymentPlanProductOrder> PaymentPlanProductOrderLastUpdatedByNavigations { get; set; }
        public virtual ICollection<PaymentPlanSchedule> PaymentPlanScheduleCreatedByNavigations { get; set; }
        public virtual ICollection<PaymentPlanSchedule> PaymentPlanScheduleLastUpdatedByNavigations { get; set; }
        public virtual ICollection<PaymentPlanSchedulePayment> PaymentPlanSchedulePaymentCreatedByNavigations { get; set; }
        public virtual ICollection<PaymentPlanSchedulePayment> PaymentPlanSchedulePaymentLastUpdatedByNavigations { get; set; }
        public virtual ICollection<PaymentPlan> PaymentPlanUserNavigations { get; set; }
        public virtual ICollection<Price> Prices { get; set; }
        public virtual ICollection<Product> ProductCreatedByNavigations { get; set; }
        public virtual ICollection<Product> ProductLastUpdatedByNavigations { get; set; }
        public virtual ICollection<ProductOrder> ProductOrders { get; set; }
        public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; }
        public virtual ICollection<UserLedgerTransaction> UserLedgerTransactionCreatedByNavigations { get; set; }
        public virtual ICollection<UserLedgerTransaction> UserLedgerTransactionUserNavigations { get; set; }
        public virtual ICollection<UserProductList> UserProductLists { get; set; }
    }
}
