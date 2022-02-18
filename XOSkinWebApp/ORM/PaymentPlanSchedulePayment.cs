using System;
using System.Collections.Generic;

namespace XOSkinWebApp.ORM
{
    public partial class PaymentPlanSchedulePayment
    {
        public long Id { get; set; }
        public long PlanSchedule { get; set; }
        public DateTime DueDate { get; set; }
        public decimal DueAmount { get; set; }
        public DateTime? ActualDate { get; set; }
        public decimal? ActualAmount { get; set; }
        public long LedgerTransaction { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual AspNetUser CreatedByNavigation { get; set; }
        public virtual AspNetUser LastUpdatedByNavigation { get; set; }
        public virtual UserLedgerTransaction LedgerTransactionNavigation { get; set; }
        public virtual PaymentPlanSchedule PlanScheduleNavigation { get; set; }
    }
}
