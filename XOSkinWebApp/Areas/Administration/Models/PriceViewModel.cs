using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace XOSkinWebApp.Areas.Administration.Models
{
  public class PriceViewModel
  {
    [Key]
    long Id { get; set; }
    
    [Required(ErrorMessage = "An amount is required.")]
    [Remote("PriceAvailable", "Prices", ErrorMessage = "An active price with that amount already exists.", 
      AdditionalFields = "ActionCreate, OriginalPriceAmount")]
    decimal Amount { get; set; }

    bool Active { get; set; }
    String CreatedBy { get; set; }
    DateTime CreatedOn { get; set; }
  }
}
