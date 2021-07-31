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
            PaymentPlanCreatedByNavigations = new HashSet<PaymentPlan>();
            PaymentPlanProductOrderCreatedByNavigations = new HashSet<PaymentPlanProductOrder>();
            PaymentPlanProductOrderLastUpdatedByNavigations = new HashSet<PaymentPlanProductOrder>();
            PaymentPlanScheduleCreatedByNavigations = new HashSet<PaymentPlanSchedule>();
            PaymentPlanScheduleLastUpdatedByNavigations = new HashSet<PaymentPlanSchedule>();
            PaymentPlanSchedulePaymentCreatedByNavigations = new HashSet<PaymentPlanSchedulePayment>();
            PaymentPlanSchedulePaymentLastUpdatedByNavigations = new HashSet<PaymentPlanSchedulePayment>();
            PaymentPlanUserNavigations = new HashSet<PaymentPlan>();
            PriceCreatedByNavigations = new HashSet<Price>();
            PriceLastEditedByNavigations = new HashSet<Price>();
            ProductOrders = new HashSet<ProductOrder>();
            ShoppingCarts = new HashSet<ShoppingCart>();
            UserLedgerTransactions = new HashSet<UserLedgerTransaction>();
            UserProductLists = new HashSet<UserProductList>();
        }

        public long Id { get; set; }
        public string EmailAddress { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? PreferredLanguage { get; set; }
        public bool IsLocked { get; set; }
        public short UserGroup { get; set; }

        public virtual Language PreferredLanguageNavigation { get; set; }
        public virtual UserGroup UserGroupNavigation { get; set; }
        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<PaymentPlan> PaymentPlanCreatedByNavigations { get; set; }
        public virtual ICollection<PaymentPlanProductOrder> PaymentPlanProductOrderCreatedByNavigations { get; set; }
        public virtual ICollection<PaymentPlanProductOrder> PaymentPlanProductOrderLastUpdatedByNavigations { get; set; }
        public virtual ICollection<PaymentPlanSchedule> PaymentPlanScheduleCreatedByNavigations { get; set; }
        public virtual ICollection<PaymentPlanSchedule> PaymentPlanScheduleLastUpdatedByNavigations { get; set; }
        public virtual ICollection<PaymentPlanSchedulePayment> PaymentPlanSchedulePaymentCreatedByNavigations { get; set; }
        public virtual ICollection<PaymentPlanSchedulePayment> PaymentPlanSchedulePaymentLastUpdatedByNavigations { get; set; }
        public virtual ICollection<PaymentPlan> PaymentPlanUserNavigations { get; set; }
        public virtual ICollection<Price> PriceCreatedByNavigations { get; set; }
        public virtual ICollection<Price> PriceLastEditedByNavigations { get; set; }
        public virtual ICollection<ProductOrder> ProductOrders { get; set; }
        public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; }
        public virtual ICollection<UserLedgerTransaction> UserLedgerTransactions { get; set; }
        public virtual ICollection<UserProductList> UserProductLists { get; set; }
    }
}
