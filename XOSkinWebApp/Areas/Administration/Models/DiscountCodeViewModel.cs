﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Areas.Administration.Models
{
  public class DiscountCodeViewModel
  {
    [Key]
    public long Id { get; set; }
    public bool Active { get; set; }
    [Required]
    public String Code { get; set; }
    public bool DiscountAsInNProductPercentage { get; set; }
    public decimal? DiscountNProductPercentage { get; set; }
    public bool DiscountAsInNProductDollars { get; set; }
    public decimal? DiscountInNProductDollars { get; set; }
    public short? DiscountProductN { get; set; }
    public bool DiscountAsInGlobalOrderPercentage { get; set; }
    public decimal? DiscountGlobalOrderPercentage { get; set; }
    public bool DiscountAsInGlobalOrderDollars { get; set; }
    public decimal? DiscountGlobalOrderDollars { get; set; }
    [Required]
    public DateTime ValidFrom { get; set; }
    [Required]
    public DateTime ValidTo { get; set; }
    public String CreatedBy { get; set; }
    public DateTime Created { get; set; }
    public String LastUpdatedBy { get; set; }
    public DateTime? LastUpdated { get; set; }
    public long[] Product { get; set; }
  }
}