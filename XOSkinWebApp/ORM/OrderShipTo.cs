using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class OrderShipTo
    {
        public long Id { get; set; }
        public long Order { get; set; }
        public string RecipientName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string CityName { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
        public string PostalCode { get; set; }
        public bool? Shipped { get; set; }
        public string CarrierName { get; set; }
        public string ShipEngineId { get; set; }
        public string ShipEngineRateId { get; set; }
        public string ShippingLabelUrl { get; set; }
        public string TrackingNumber { get; set; }
        public DateTime? ShipDate { get; set; }
        public DateTime? ActualShipDate { get; set; }
        public DateTime? Arrives { get; set; }
        public DateTime? ActualArrives { get; set; }

        public virtual ProductOrder OrderNavigation { get; set; }
    }
}
