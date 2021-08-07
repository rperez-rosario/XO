using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using CompareAttribute =
System.ComponentModel.DataAnnotations.CompareAttribute;
using Microsoft.AspNetCore.Mvc;

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
            Questionnaires = new HashSet<Questionnaire>();
            ShoppingCarts = new HashSet<ShoppingCart>();
            UserAnswers = new HashSet<UserAnswer>();
            UserCommonAllergens = new HashSet<UserCommonAllergen>();
            UserLedgerTransactionCreatedByNavigations = new HashSet<UserLedgerTransaction>();
            UserLedgerTransactionUserNavigations = new HashSet<UserLedgerTransaction>();
            UserProductLists = new HashSet<UserProductList>();
        }

        public long Id { get; set; }

        [StringLength(200, ErrorMessage = "Maximum field length is 200.")]
        [Required(ErrorMessage = "Email address required.")]
        [Remote("EmailAddressAvailable", "Users", ErrorMessage = "Email address already registered.", AdditionalFields = "ActionCreate, OriginalEmailAddress")]
        public string EmailAddress { get; set; }

        [StringLength(50, ErrorMessage = "Maximum field length is 50.")]
        public string HomePhoneNumber { get; set; }

        [StringLength(50, ErrorMessage = "Maximum field length is 50.")]
        public string WorkPhoneNumber { get; set; }
        
        [StringLength(50, ErrorMessage = "Maximum field length is 50.")]
        public string AdditionalPhoneNumber { get; set; }
        
        public string PasswordHash { get; set; }
        
        [Required(ErrorMessage ="First name required.")]
        [StringLength(50, ErrorMessage = "Maximum field length is 50.")]
        public string FirstName { get; set; }
        
        [Required(ErrorMessage = "Last name required.")]
        [StringLength(50, ErrorMessage = "Maximum field length is 50.")]
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
        public virtual ICollection<Questionnaire> Questionnaires { get; set; }
        public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; }
        public virtual ICollection<UserAnswer> UserAnswers { get; set; }
        public virtual ICollection<UserCommonAllergen> UserCommonAllergens { get; set; }
        public virtual ICollection<UserLedgerTransaction> UserLedgerTransactionCreatedByNavigations { get; set; }
        public virtual ICollection<UserLedgerTransaction> UserLedgerTransactionUserNavigations { get; set; }
        public virtual ICollection<UserProductList> UserProductLists { get; set; }
    }
}
