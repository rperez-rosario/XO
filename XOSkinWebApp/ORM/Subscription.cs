using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class Subscription
    {
        public Subscription()
        {
            SubscriptionProducts = new HashSet<SubscriptionProduct>();
        }

        public long Id { get; set; }
        public string Customer { get; set; }
        public short Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int ShipmentFrequencyInDays { get; set; }
        public long Price { get; set; }

        public virtual SubscriptionType TypeNavigation { get; set; }
        public virtual SubscriptionShipmentSchedule SubscriptionShipmentSchedule { get; set; }
        public virtual ICollection<SubscriptionProduct> SubscriptionProducts { get; set; }
    }
}
