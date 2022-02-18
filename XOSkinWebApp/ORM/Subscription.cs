using System;
using System.Collections.Generic;

namespace XOSkinWebApp.ORM
{
    public partial class Subscription
    {
        public Subscription()
        {
            SubscriptionCustomers = new HashSet<SubscriptionCustomer>();
            SubscriptionProducts = new HashSet<SubscriptionProduct>();
        }

        public long Id { get; set; }
        public string ImagePathLarge { get; set; }
        public long Subscription1 { get; set; }
        public short Type { get; set; }
        public int ShipmentFrequencyInDays { get; set; }

        public virtual Product Subscription1Navigation { get; set; }
        public virtual SubscriptionType TypeNavigation { get; set; }
        public virtual SubscriptionShipmentSchedule SubscriptionShipmentSchedule { get; set; }
        public virtual ICollection<SubscriptionCustomer> SubscriptionCustomers { get; set; }
        public virtual ICollection<SubscriptionProduct> SubscriptionProducts { get; set; }
    }
}
