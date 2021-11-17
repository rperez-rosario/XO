using System;
using System.Collections.Generic;

namespace XOSkinWebApp.Areas.Administration.Models
{
  public class SubscriptionViewModel
  {
    public long Id { get; set; }
    public String ImagePathLarge { get; set; }
    public long Subscription { get; set; }
    public short Type { get; set; }
    public int ShipmentFrequencyInDays { get; set; }
    public List<ProductViewModel> Product { get; set; }
  }
}
