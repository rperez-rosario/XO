using System;
using System.Collections.Generic;

namespace XOSkinWebApp.ORM
{
    public partial class PaymentPlanProductOrder
    {
        public long Id { get; set; }
        public long PaymentPlan { get; set; }
        public long ProductOrder { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual AspNetUser CreatedByNavigation { get; set; }
        public virtual AspNetUser LastUpdatedByNavigation { get; set; }
        public virtual PaymentPlan PaymentPlanNavigation { get; set; }
        public virtual ProductOrder ProductOrderNavigation { get; set; }
    }
}
