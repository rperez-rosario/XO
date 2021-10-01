using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace XOSkinWebApp.Areas.Administration.Models
{
  public class OrderViewModel
  {
    [Key]
    public long OrderId { get; set; }
    public DateTime? DatePlaced { get; set; }
    public String Recipient { get; set; }
    public int NumberOfItems { get; set; }
    public DateTime? Arrives { get; set; }
    public String Carrier { get; set; }
    public String TrackingNumber { get; set; }
    public String Status { get; set; }
    public String CancellationStatus { get; set; }
    public DateTime? CancellationDate { get; set; }
    public String CancelReason { get; set; }
    public String RefundStatus { get; set; }
    public DateTime? RefundDate { get; set; }
    public String RefundReason { get; set; }
    public decimal? RefundAmount { get; set; }
  }
}
