﻿using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class SubscriptionType
    {
        public SubscriptionType()
        {
            Subscriptions = new HashSet<Subscription>();
        }

        public short Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Subscription> Subscriptions { get; set; }
    }
}