using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class PaymentPlan
    {
        public PaymentPlan()
        {
            PaymentPlanProductOrders = new HashSet<PaymentPlanProductOrder>();
            PaymentPlanSchedules = new HashSet<PaymentPlanSchedule>();
        }

        public long Id { get; set; }
        public long User { get; set; }
        public string Description { get; set; }
        public long CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public long LastUpdatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual User CreatedByNavigation { get; set; }
        public virtual User UserNavigation { get; set; }
        public virtual ICollection<PaymentPlanProductOrder> PaymentPlanProductOrders { get; set; }
        public virtual ICollection<PaymentPlanSchedule> PaymentPlanSchedules { get; set; }
    }
}
