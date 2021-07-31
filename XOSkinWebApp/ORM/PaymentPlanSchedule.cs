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
        public long CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public long LastUpdatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual User CreatedByNavigation { get; set; }
        public virtual User LastUpdatedByNavigation { get; set; }
        public virtual PaymentPlan PaymentPlanNavigation { get; set; }
        public virtual ICollection<PaymentPlanSchedulePayment> PaymentPlanSchedulePayments { get; set; }
    }
}
