using System;
using System.Collections.Generic;

#nullable disable

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
        public short TransactionType { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountBeforeTransaction { get; set; }
        public decimal AmountAfterTransaction { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }

        public virtual AspNetUser CreatedByNavigation { get; set; }
        public virtual UserLedgerTransactionType TransactionTypeNavigation { get; set; }
        public virtual AspNetUser UserNavigation { get; set; }
        public virtual ICollection<PaymentPlanSchedulePayment> PaymentPlanSchedulePayments { get; set; }
    }
}
