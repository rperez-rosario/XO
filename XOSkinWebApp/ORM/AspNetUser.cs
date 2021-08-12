using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class AspNetUser
    {
        public AspNetUser()
        {
            Addresses = new HashSet<Address>();
            AspNetUserClaims = new HashSet<AspNetUserClaim>();
            AspNetUserLogins = new HashSet<AspNetUserLogin>();
            AspNetUserRoles = new HashSet<AspNetUserRole>();
            AspNetUserTokens = new HashSet<AspNetUserToken>();
            DiscountCodeCreatedByNavigations = new HashSet<DiscountCode>();
            DiscountCodeLastUpdatedByNavigations = new HashSet<DiscountCode>();
            DiscountCouponCreatedByNavigations = new HashSet<DiscountCoupon>();
            DiscountCouponLastUpdatedByNavigations = new HashSet<DiscountCoupon>();
            PaymentPlanCreatedByNavigations = new HashSet<PaymentPlan>();
            PaymentPlanLastUpdatedByNavigations = new HashSet<PaymentPlan>();
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
            Questionnaires = new HashSet<Questionnaire>();
            ShoppingCarts = new HashSet<ShoppingCart>();
            UserAnswers = new HashSet<UserAnswer>();
            UserCommonAllergens = new HashSet<UserCommonAllergen>();
            UserLedgerTransactionCreatedByNavigations = new HashSet<UserLedgerTransaction>();
            UserLedgerTransactionUserNavigations = new HashSet<UserLedgerTransaction>();
            UserProductLists = new HashSet<UserProductList>();
        }

        public string Id { get; set; }
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string HomePhoneNumber { get; set; }
        public string LastName { get; set; }
        public string WorkPhoneNumber { get; set; }
        public bool? Disabled { get; set; }

        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual ICollection<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual ICollection<AspNetUserRole> AspNetUserRoles { get; set; }
        public virtual ICollection<AspNetUserToken> AspNetUserTokens { get; set; }
        public virtual ICollection<DiscountCode> DiscountCodeCreatedByNavigations { get; set; }
        public virtual ICollection<DiscountCode> DiscountCodeLastUpdatedByNavigations { get; set; }
        public virtual ICollection<DiscountCoupon> DiscountCouponCreatedByNavigations { get; set; }
        public virtual ICollection<DiscountCoupon> DiscountCouponLastUpdatedByNavigations { get; set; }
        public virtual ICollection<PaymentPlan> PaymentPlanCreatedByNavigations { get; set; }
        public virtual ICollection<PaymentPlan> PaymentPlanLastUpdatedByNavigations { get; set; }
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
        public virtual ICollection<Questionnaire> Questionnaires { get; set; }
        public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; }
        public virtual ICollection<UserAnswer> UserAnswers { get; set; }
        public virtual ICollection<UserCommonAllergen> UserCommonAllergens { get; set; }
        public virtual ICollection<UserLedgerTransaction> UserLedgerTransactionCreatedByNavigations { get; set; }
        public virtual ICollection<UserLedgerTransaction> UserLedgerTransactionUserNavigations { get; set; }
        public virtual ICollection<UserProductList> UserProductLists { get; set; }
    }
}
