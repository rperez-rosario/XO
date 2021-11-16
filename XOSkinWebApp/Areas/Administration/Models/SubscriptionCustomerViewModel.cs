using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace XOSkinWebApp.Areas.Administration.Models
{
  public class SubscriptionCustomerViewModel
  {
    [Key]
    public long Id { get; set; }
    public long Subscription { get; set; }
    public short Type { get; set; }
    public String Customer { get; set; }
    public int ShipmentFrequencyInDays { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime FrequencyEndDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<ProductViewModel> Product { get; set; }
  }
}
