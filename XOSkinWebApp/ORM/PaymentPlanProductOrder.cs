using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class PaymentPlanProductOrder
    {
        public long Id { get; set; }
        public long PaymentPlan { get; set; }
        public long ProductOrder { get; set; }
        public long CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public long LastUpdatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual User CreatedByNavigation { get; set; }
        public virtual User LastUpdatedByNavigation { get; set; }
        public virtual PaymentPlan PaymentPlanNavigation { get; set; }
        public virtual ProductOrder ProductOrderNavigation { get; set; }
    }
}
