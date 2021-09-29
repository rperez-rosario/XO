using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace XOSkinWebApp.Areas.Administration.Models
{
  public class ShipOutViewModel
  {
    [Key]
    public long OrderId { get; set; }
    public long ShopifyId { get; set; }
    public List<ShippingLineItemViewModel> LineItem { get; set; }
    public String BillingName { get; set; }
    public String BillingAddress1 { get; set; }
    public String BillingAddress2 { get; set; }
    public String BillingCity { get; set; }
    public String BillingState { get; set; }
    public String BillingCountry { get; set; }
    public String BillingPostalCode { get; set; }
    public DateTime BilledOn { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? ShippingCharges { get; set; }
    public decimal? Taxes { get; set; }
    public decimal? CodeDiscount { get; set; }
    public decimal? CouponDiscount { get; set; }
    public decimal? Total { get; set; }
    public bool IsGift { get; set; }
    public bool ShippingAddressSame { get; set; }
    public String ShippingName { get; set; }    
    public String ShippingAddress1 { get; set; }
    public String ShippingAddress2 { get; set; }
    public String ShippingCity { get; set; }
    public String ShippingState { get; set; }
    public String ShippingCountry { get; set; }
    public String ShippingPostalCode { get; set; }
    public decimal ShippingLongitude { get; set; }
    public decimal ShippingLatitude { get; set; }
    public String GoogleMapsUrl { get; set; }
    public String ShippingCarrier { get; set; }
    public String TrackingNumber { get; set; }
    public DateTime ExpectedToShipOn { get; set; }
    public DateTime ExpectedToArrive { get; set; }
    public DateTime? ShippedOn { get; set; }
    public String SelectedCarrierId { get; set; }
    public String CarrierName { get; set; }
    public decimal TotalWeightInPounds { get; set; }
    public String ShipEngineShipmentId { get; set; }
    public String ShipEngineRateId { get; set; }
    public String ShipEngineLabelUrl { get; set; }
    public bool CalculatedShippingAndTaxes { get; set; }
    public bool CardDeclined { get; set; }
    public bool ShippingAddressDeclined { get; set; }
    public String clientIpAddress { get; set; }
    public String ShipmentStatus { get; set; }
    public String ShippedBy { get; set; }

    public ShipOutViewModel()
    {
      LineItem = new List<ShippingLineItemViewModel>();
      CalculatedShippingAndTaxes = false;
      ShippingAddressDeclined = false;
      CardDeclined = false;
    }
  }
}
