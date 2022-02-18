using System;
using System.Collections.Generic;

namespace XOSkinWebApp.ORM
{
    public partial class UserLedgerTransaction
    {
        public UserLedgerTransaction()
        {
            PaymentPlanSchedulePayments = new HashSet<PaymentPlanSchedulePayment>();
        }

        public long Id { get; set; }
        public string User { get; set; }
        public long? ProductOrder { get; set; }
        public short TransactionType { get; set; }
        public int Concept { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceBeforeTransaction { get; set; }
        public decimal BalanceAfterTransaction { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }

        public virtual TransactionConcept ConceptNavigation { get; set; }
        public virtual AspNetUser CreatedByNavigation { get; set; }
        public virtual ProductOrder ProductOrderNavigation { get; set; }
        public virtual UserLedgerTransactionType TransactionTypeNavigation { get; set; }
        public virtual AspNetUser UserNavigation { get; set; }
        public virtual ICollection<PaymentPlanSchedulePayment> PaymentPlanSchedulePayments { get; set; }
    }
}
