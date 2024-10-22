﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XOSkinWebApp.Models
{
  public class OrderViewModel
  {
    public long OrderId { get; set; }
    public DateTime? DatePlaced { get; set; }
    public String Recipient { get; set; }
    public int NumberOfItems { get; set; }
    public String Status { get; set; }
    public DateTime? Arrives { get; set; }
    public String Carrier { get; set; }
    public String TrackingNumber { get; set; }
  }
}
