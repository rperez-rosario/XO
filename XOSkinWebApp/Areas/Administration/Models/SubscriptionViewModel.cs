using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Areas.Administration.Models
{
  public class SubscriptionViewModel
  {
    [Key]
    public long Id { get; set; }
    public long Subscription { get; set; }
    public short Type { get; set; }
    public int ShipmentFrequencyInDays { get; set; }
    public List<ProductViewModel> Product { get; set; }
  }
}
