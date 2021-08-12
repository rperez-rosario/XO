using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class PaymentPlanSchedule
    {
        public PaymentPlanSchedule()
        {
            PaymentPlanSchedulePayments = new HashSet<PaymentPlanSchedulePayment>();
        }

        public long Id { get; set; }
        public long PaymentPlan { get; set; }
        public DateTime ScheduleStart { get; set; }
        public DateTime ScheduleProjectedEnd { get; set; }
        public DateTime? ScheduleEnd { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual AspNetUser CreatedByNavigation { get; set; }
        public virtual AspNetUser LastUpdatedByNavigation { get; set; }
        public virtual PaymentPlan PaymentPlanNavigation { get; set; }
        public virtual ICollection<PaymentPlanSchedulePayment> PaymentPlanSchedulePayments { get; set; }
    }
}
