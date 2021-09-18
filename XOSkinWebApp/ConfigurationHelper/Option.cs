using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XOSkinWebApp.ConfigurationHelper
{
	public class Option
	{
		public const String SectionName = "Settings";

		public String ShopifyUrl { get; set; }
		public String ShopifyStoreFrontAccessToken { get; set; }
		public String ShopifyYourFaceCollectionId { get; set; }
		public String ShopifyYourEyesCollectionId { get; set; }
		public String ShopifyYourLipsAndSmileCollectionId { get; set; }
		public String ShipEngineApiKey { get; set; }
		public String ShipEngineCarriersUrl { get; set; }
		public String ShipEngineShippingCostUrl { get; set; }
		public String ShipEngineGetShipmentCostFromIdPrefixUrl { get; set; }
		public String ShipEngineGetShipmentCostFromIdPostfixUrl { get; set; }
		public String ShipEngineDefaultCarrier { get; set; }
		public String ShipEngineDefaultCarrierName { get; set; }
		public String ShipEngineDefaultRateType { get; set; }
		public String ShipEngineDefaultPackageType { get; set; }
		public String ShipEngineDefaultServiceCode { get; set; }
		public String ShopifyShippingLineTitle { get; set; }
		public String ShopifyShippingLineCode { get; set; }
		public String ShopifyShippingLineSource { get; set; }
		public String ShopifyOrderStatusUrl { get; set; }
		public String StripeSecretKey { get; set; }
		public String StripePublishableKey { get; set; }
		public String ShipFromCompanyName { get; set; }
		public String ShipFromName { get; set; }
		public String ShipFromPhone { get; set; }
		public String ShipFromAddressLine1 { get; set; }
		public String ShipFromCity { get; set; }
		public String ShipFromState { get; set; }
		public String ShipFromPostalCode { get; set; }
		public String ShipFromCountryCode { get; set; }
		public String BingMapsKey { get; set; }
		public String BingMapsGeolocationUrl { get; set; }
		public String GoogleMapsKey { get; set; }
		public String GoogleMapsUrl { get; set; }
	}
}
