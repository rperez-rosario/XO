using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class SubscriptionCustomer
    {
        public long Id { get; set; }
        public long Subscription { get; set; }
        public string Customer { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public virtual AspNetUser CustomerNavigation { get; set; }
        public virtual Subscription SubscriptionNavigation { get; set; }
    }
}
