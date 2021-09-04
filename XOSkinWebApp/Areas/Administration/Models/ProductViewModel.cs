using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Areas.Administration.Models
{
  public class ProductViewModel
  {
    [Key]
    public long Id { get; set; }

    public long? ShopifyProductId { get; set; }

    [Required(ErrorMessage ="An SKU is required.")]
    public String Sku { get; set; }

    [Remote("ProductNameAvailable", "Product", ErrorMessage = "Product name already registered.", AdditionalFields = "ActionCreate, OriginalProductName")]
    [Required(ErrorMessage ="A name is required.")]
    public String Name { get; set; }

    [Required(ErrorMessage = "A description is required.")]
    public String Description { get; set; }

    public short ProductType { get; set; }

    public short ProductCategory { get; set; }

    public bool Kit { get; set; }

    public short? KitType { get; set; }

    public String KitTypeName { get; set; }

    public ICollection<KitProduct> KitProduct { get; set; }

    public decimal? VolumeInFluidOunces { get; set; }

    public decimal? Ph { get; set; }

    public decimal? ShippingWeightLb { get; set; }

    public ICollection<ProductIngredient> Ingredient { get; set; }

    public long? Stock { get; set; }

    public long CurrentPriceId { get; set; }

    public decimal CurrentPrice { get; set; }

    public String ImagePathSmall { get; set; }

    public String ImagePathMedium { get; set; }

    public String ImagePathLarge { get; set; }

    public byte[] ImageLarge { get; set; }

    public bool Active { get; set; }

    public String CreatedBy { get; set; }

    public DateTime Created { get; set; }

    public String LastUpdatedBy { get; set; }

    public DateTime? LastUpdated { get; set; }
  }
}