using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class ProductOrderLineItem
    {
        public long Id { get; set; }
        public string ImageSource { get; set; }
        public long ProductOrder { get; set; }
        public long Product { get; set; }
        public int Quantity { get; set; }
        public decimal? Total { get; set; }

        public virtual Product ProductNavigation { get; set; }
        public virtual ProductOrder ProductOrderNavigation { get; set; }
    }
}
