using System;
using System.Collections.Generic;

namespace XOSkinWebApp.ORM
{
    public partial class SubscriptionType
    {
        public SubscriptionType()
        {
            Products = new HashSet<Product>();
            Subscriptions = new HashSet<Subscription>();
        }

        public short Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Product> Products { get; set; }
        public virtual ICollection<Subscription> Subscriptions { get; set; }
    }
}
