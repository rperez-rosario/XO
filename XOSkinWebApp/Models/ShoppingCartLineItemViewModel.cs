﻿using System;

namespace XOSkinWebApp.Models
{
  public class ShoppingCartLineItemViewModel
  {
    public long Id { get; set; }
    public long ProductId { get; set; }
    public String ImageSource { get; set; }
    public String ProductName { get; set; }
    public String ProductDescription { get; set; }
    public int Quantity { get; set; }
    public decimal? Total { get; set; }
  }
}
