using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class DiscountCode
    {
        public DiscountCode()
        {
            DiscountCodeProducts = new HashSet<DiscountCodeProduct>();
            ProductOrderDiscountCodes = new HashSet<ProductOrderDiscountCode>();
            ShoppingCartDiscountCodes = new HashSet<ShoppingCartDiscountCode>();
        }

        public long Id { get; set; }
        public long? Code { get; set; }
        public bool DiscountAsInNproductPercentage { get; set; }
        public decimal? DiscountNproductPercentage { get; set; }
        public bool DiscountAsInNproductDollars { get; set; }
        public decimal? DiscountInNproductDollars { get; set; }
        public short? DiscountProductN { get; set; }
        public bool DiscountAsInGlobalOrderPercentage { get; set; }
        public decimal? DiscountGlobalOrderPercentage { get; set; }
        public bool DiscountAsInOrderDollars { get; set; }
        public decimal? DiscountGlobalOrderDollars { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual AspNetUser CreatedByNavigation { get; set; }
        public virtual AspNetUser LastUpdatedByNavigation { get; set; }
        public virtual ICollection<DiscountCodeProduct> DiscountCodeProducts { get; set; }
        public virtual ICollection<ProductOrderDiscountCode> ProductOrderDiscountCodes { get; set; }
        public virtual ICollection<ShoppingCartDiscountCode> ShoppingCartDiscountCodes { get; set; }
    }
}
