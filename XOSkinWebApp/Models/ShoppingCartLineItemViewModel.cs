using System;

namespace XOSkinWebApp.Models
{
  public class ShoppingCartLineItemViewModel
  {
    public long ProductId { get; set; }
    public String ProductName { get; set; }
    public String ProductDescription { get; set; }
    public int Quantity { get; set; }
    public decimal? Total { get; set; }
  }
}
