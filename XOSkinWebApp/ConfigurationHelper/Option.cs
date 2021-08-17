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
	}
}
