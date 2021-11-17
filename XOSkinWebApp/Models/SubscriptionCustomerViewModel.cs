using System;
using System.Collections.Generic;

namespace XOSkinWebApp.Areas.Administration.Models
{
  public class SubscriptionCustomerViewModel
  {
    public long Id { get; set; }
    public long Subscription { get; set; }
    public short Type { get; set; }
    public String Customer { get; set; }
    public int ShipmentFrequencyInDays { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime FrequencyEndDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<Areas.Administration.Models.ProductViewModel> Product { get; set; }
  }
}
