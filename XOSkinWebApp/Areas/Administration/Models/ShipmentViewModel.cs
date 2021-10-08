using System;
using System.ComponentModel.DataAnnotations;

namespace XOSkinWebApp.Areas.Administration.Models
{
  public class ShipmentViewModel
  {
    [Key]
    public long ShipmentId { get; set; }
    
    public long OrderId { get; set; }
    public bool Shipped { get; set; }
    public String ShipmentStatus { get; set; }
    public DateTime? DatePlaced { get; set; }
    public String Recipient { get; set; }
    public String AddressLine1 { get; set; }
    public String AddressLine2 { get; set; }
    public String CityName { get; set; }
    public String StateName { get; set; }
    public String CountryName { get; set; }
    public String PostalCode { get; set; }
    public int NumberOfItems { get; set; }
    public DateTime? DateShipped { get; set; }
    public DateTime? ActualDateShipped { get; set; }
    public DateTime? Arrives { get; set; }
    public DateTime? ActualArrives { get; set; }
    public String CarrierName { get; set; }
    public String TrackingNumber { get; set; }
    public String ShipEngineId { get; set; }
    public String ShippingLabelURL { get; set; }
    public bool OrderCancelled { get; set; }
    public String FulfillmentStatus { get; set; }
  }
}
