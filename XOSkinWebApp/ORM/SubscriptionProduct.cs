using System;
using System.Collections.Generic;

namespace XOSkinWebApp.ORM
{
    public partial class SubscriptionProduct
    {
        public long Id { get; set; }
        public long Subscription { get; set; }
        public long Product { get; set; }
        public int Quantity { get; set; }

        public virtual Product ProductNavigation { get; set; }
        public virtual Subscription SubscriptionNavigation { get; set; }
    }
}
