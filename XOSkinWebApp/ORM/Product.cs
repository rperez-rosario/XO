using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class Product
    {
        public Product()
        {
            DiscountCodeProducts = new HashSet<DiscountCodeProduct>();
            DiscountCouponProducts = new HashSet<DiscountCouponProduct>();
            KitProductKitNavigations = new HashSet<KitProduct>();
            KitProductProductNavigations = new HashSet<KitProduct>();
            ProductIngredients = new HashSet<ProductIngredient>();
            ProductOrderLineItems = new HashSet<ProductOrderLineItem>();
            ShoppingCartHistories = new HashSet<ShoppingCartHistory>();
            ShoppingCartLineItems = new HashSet<ShoppingCartLineItem>();
            SubscriptionProducts = new HashSet<SubscriptionProduct>();
            Subscriptions = new HashSet<Subscription>();
            UserProductListProducts = new HashSet<UserProductListProduct>();
        }

        public long Id { get; set; }
        public long? ShopifyProductId { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public short ProductType { get; set; }
        public short ProductCategory { get; set; }
        public short? KitType { get; set; }
        public short? SubscriptionType { get; set; }
        public decimal? VolumeInFluidOunces { get; set; }
        public decimal? Ph { get; set; }
        public decimal? ShippingWeightLb { get; set; }
        public long? Stock { get; set; }
        public long CurrentPrice { get; set; }
        public long? Cost { get; set; }
        public string ImagePathSmall { get; set; }
        public string ImagePathMedium { get; set; }
        public string ImagePathLarge { get; set; }
        public bool Active { get; set; }
        public bool? Sample { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual Cost CostNavigation { get; set; }
        public virtual AspNetUser CreatedByNavigation { get; set; }
        public virtual Price CurrentPriceNavigation { get; set; }
        public virtual KitType KitTypeNavigation { get; set; }
        public virtual AspNetUser LastUpdatedByNavigation { get; set; }
        public virtual ProductCategory ProductCategoryNavigation { get; set; }
        public virtual ProductType ProductTypeNavigation { get; set; }
        public virtual SubscriptionType SubscriptionTypeNavigation { get; set; }
        public virtual ICollection<DiscountCodeProduct> DiscountCodeProducts { get; set; }
        public virtual ICollection<DiscountCouponProduct> DiscountCouponProducts { get; set; }
        public virtual ICollection<KitProduct> KitProductKitNavigations { get; set; }
        public virtual ICollection<KitProduct> KitProductProductNavigations { get; set; }
        public virtual ICollection<ProductIngredient> ProductIngredients { get; set; }
        public virtual ICollection<ProductOrderLineItem> ProductOrderLineItems { get; set; }
        public virtual ICollection<ShoppingCartHistory> ShoppingCartHistories { get; set; }
        public virtual ICollection<ShoppingCartLineItem> ShoppingCartLineItems { get; set; }
        public virtual ICollection<SubscriptionProduct> SubscriptionProducts { get; set; }
        public virtual ICollection<Subscription> Subscriptions { get; set; }
        public virtual ICollection<UserProductListProduct> UserProductListProducts { get; set; }
    }
}
