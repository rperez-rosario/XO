using System;
using System.Collections.Generic;

namespace XOSkinWebApp.ORM
{
    public partial class ProductOrderLineItem
    {
        public long Id { get; set; }
        public string ImageSource { get; set; }
        public long ProductOrder { get; set; }
        public long Product { get; set; }
        public bool? Sample { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public short? ProductType { get; set; }
        public short? KitType { get; set; }
        public decimal? VolumeInFluidOunces { get; set; }
        public decimal? PhBalance { get; set; }
        public decimal? ShippingWeightLb { get; set; }
        public long Price { get; set; }
        public long Cost { get; set; }
        public int Quantity { get; set; }
        public decimal? Total { get; set; }

        public virtual Product ProductNavigation { get; set; }
        public virtual ProductOrder ProductOrderNavigation { get; set; }
    }
}
