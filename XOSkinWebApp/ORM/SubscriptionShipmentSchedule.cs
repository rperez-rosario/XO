using System;
using System.Collections.Generic;

namespace XOSkinWebApp.ORM
{
    public partial class SubscriptionShipmentSchedule
    {
        public long Id { get; set; }
        public long Subscription { get; set; }
        public DateTime ShipOn { get; set; }

        public virtual Subscription IdNavigation { get; set; }
    }
}
