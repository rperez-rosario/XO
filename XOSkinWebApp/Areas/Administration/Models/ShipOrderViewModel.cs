using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XOSkinWebApp.Areas.Administration.Models
{
  public class ShipOrderViewModel
  {
    public long OrderId { get; set; }
    public DateTime? DatePlaced { get; set; }
    public String Recipient { get; set; }
    public int NumberOfItems { get; set; }
    public DateTime? Arrives { get; set; }
    public String Carrier { get; set; }
    public String TrackingNumber { get; set; }
  }
}
