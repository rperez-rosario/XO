using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace XOSkinWebApp.Areas.Administration.Models
{
  public class CostViewModel
  {
    public long Id { get; set; }
    [Required(ErrorMessage = "Amount required.")]
    [Remote("ActiveAmountAvailable", "Costs", ErrorMessage = "An active cost with that amount exists.",
      AdditionalFields = "ActionCreate")]
    public decimal Amount { get; set; }
    public bool Active { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
  }
}
