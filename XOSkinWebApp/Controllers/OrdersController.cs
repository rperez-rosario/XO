using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XOSkinWebApp.ORM;
using XOSkinWebApp.ConfigurationHelper;
using Microsoft.Extensions.Options;
using System.Web;
using System.Text.Json;
using ServiceStack;
using XOSkinWebApp.Models;

namespace XOSkinWebApp.Controllers
{
  [Authorize]
  public class OrdersController : Controller
  {
    private readonly XOSkinContext _context;
    private readonly IOptions<Option> _option;

    public OrdersController(XOSkinContext context, IOptions<Option> option)
    {
      _context = context;
      _option = option;
    }

    public IActionResult Index()
    {
      List<OrderViewModel> order = new List<OrderViewModel>();
      OrderShipTo shipping = null;

      foreach (ProductOrder o in _context.ProductOrders.Where(
        x => x.User.Equals(_context.AspNetUsers.Where(
        x => x.Email.Equals(User.Identity.Name)).Select(
        x => x.Id).FirstOrDefault())).Where(
        x => x.Completed != null).Where(
        x => x.Completed == true).OrderByDescending(x => x.DatePlaced).ToList())
      {
        shipping = _context.OrderShipTos.Where(x => x.Order == o.Id).FirstOrDefault();
        order.Add(new OrderViewModel()
        {
          OrderId = o.Id,
          Arrives = (shipping.Shipped == null || (bool)!shipping.Shipped) ? 
            _context.OrderShipTos.Where(x => x.Order == o.Id).Select(x => x.Arrives).FirstOrDefault() : 
            ((DateTime)shipping.ActualArrives),
          Recipient = _context.OrderShipTos.Where(x => x.Order == o.Id).Select(x => x.RecipientName).FirstOrDefault(),
          DatePlaced = o.DatePlaced,
          TrackingNumber = _context.OrderShipTos.Where(x => x.Order == o.Id).Select(x => x.TrackingNumber).FirstOrDefault(),
          Status = o.Cancelled == null ? ((shipping.Shipped == null || (bool)!shipping.Shipped) ?
            "Shipping Soon" : "Shipped: " + ((DateTime)shipping.ActualShipDate).ToShortDateString()) :
            (bool)o.Cancelled ? "Cancelled" : ((shipping.Shipped == null || (bool)!shipping.Shipped) ? "Shipping Soon" : 
            "Shipped: " + ((DateTime)shipping.ActualShipDate).ToShortDateString())
        });
      }

      ViewData.Add("Orders.WelcomeText", _context.LocalizedTexts.Where(
        x => x.PlacementPointCode.Equals("Orders.WelcomeText")).Select(x => x.Text).FirstOrDefault());

      return View(order);
    }

    public IActionResult Detail(long Id)
    {
      ProductOrder order = null;
      List<ProductOrderLineItem> lineItem = null;
      OrderBillTo billing = null;
      OrderShipTo shipping = null;
      CheckoutViewModel checkout = null;
      String geoLocationUrl = null;
      String geoLocationJson = null;

      try
      {
        order = _context.ProductOrders.Where(x => x.Id == Id).FirstOrDefault();

        // Check if order belongs to currently logged-in user.
        if (!order.User.Equals(_context.AspNetUsers.Where(
          x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault()))
          return new RedirectToActionResult("Index", "Orders", null);

        lineItem = _context.ProductOrderLineItems.Where(x => x.ProductOrder == Id).ToList<ProductOrderLineItem>();
        billing = _context.OrderBillTos.Where(x => x.Order == Id).FirstOrDefault();
        shipping = _context.OrderShipTos.Where(x => x.Order == Id).FirstOrDefault();

        checkout = new CheckoutViewModel()
        {
          BilledOn = billing.BillingDate == null ? new DateTime(1754, 1, 1) : billing.BillingDate.Value,
          BillingAddress1 = billing.AddressLine1,
          BillingAddress2 = billing.AddressLine2,
          BillingCity = billing.CityName,
          BillingCountry = billing.CountryName,
          BillingName = billing.NameOnCreditCard,
          BillingPostalCode = billing.PostalCode,
          BillingState = billing.StateName,
          CodeDiscount = order.CodeDiscount,
          CouponDiscount = order.CouponDiscount,
          ShippedOn = (shipping.Shipped == null || (bool)!shipping.Shipped) ? (DateTime)shipping.ShipDate : (DateTime)shipping.ActualShipDate,
          FulfillmentStatus = order.Cancelled == null ? ((shipping.Shipped == null || (bool)!shipping.Shipped) ?
            "Shipping Soon" : "Shipped: " + ((DateTime)shipping.ActualShipDate).ToShortDateString()) :
            (bool)order.Cancelled ? "Cancelled" : ((shipping.Shipped == null || (bool)!shipping.Shipped) ? "Shipping Soon" :
            "Shipped: " + ((DateTime)shipping.ActualShipDate).ToShortDateString()),
          ShippingName = shipping.RecipientName,
          ShippingCountry = shipping.CountryName,
          ShippingAddress1 = shipping.AddressLine1,
          ShippingAddress2 = shipping.AddressLine2,
          ShippingAddressSame = ShippingAddressSame(billing, shipping),
          CreditCardCVC = null,
          CreditCardExpirationDate = new DateTime(1754, 1, 1),
          CreditCardNumber = null,
          ExpectedToArrive = (shipping.Shipped == null || (bool)!shipping.Shipped) ? (DateTime)shipping.Arrives : (DateTime)shipping.ActualArrives,
          IsGift = (bool)order.GiftOrder,
          OrderId = Id,
          CarrierName = shipping.CarrierName,
          ShippingCharges = order.ShippingCost,
          ShippingCity = shipping.CityName,
          ShippingPostalCode = shipping.PostalCode,
          ShippingState = shipping.StateName,
          ShopifyId = order.ShopifyId == null ? long.MinValue : order.ShopifyId.Value,
          SubTotal = order.Subtotal,
          Taxes = order.ApplicableTaxes,
          Total = order.Total,
          TrackingNumber = shipping.TrackingNumber
        };

        checkout.LineItem = new List<ShoppingCartLineItemViewModel>();

        foreach (ProductOrderLineItem li in lineItem)
        {
          checkout.LineItem.Add(new ShoppingCartLineItemViewModel()
          {
            Id = li.Id,
            ImageSource = li.ImageSource,
            ProductDescription = _context.Products.Where(x => x.Id == li.Product).Select(x => x.Description).FirstOrDefault(),
            ProductId = li.Product,
            ProductName = _context.Products.Where(x => x.Id == li.Product).Select(x => x.Name).FirstOrDefault(),
            Quantity = li.Quantity,
            Total = li.Total
          });
        }
      }
      catch (Exception ex)
      {
        throw new Exception("An error was encountered while retrieving order details.", ex);
      }

      try
      {
        geoLocationUrl = new string(_option.Value.BingMapsGeolocationUrl)
        .Replace("{adminDistrict}", checkout.ShippingState)
        .Replace("{postalCode}", checkout.ShippingPostalCode.Trim())
        .Replace("{locality}", checkout.ShippingCity.Trim())
        .Replace("{addressLine}", (checkout.ShippingAddress1.Trim() + (checkout.ShippingAddress2 == null || 
          (checkout.ShippingAddress2 != null && checkout.ShippingAddress2.Trim() == String.Empty) ? String.Empty : 
          " " + checkout.ShippingAddress2.Trim())))
        .Replace("{includeNeighborhood}", "0")
        .Replace("{includeValue}", String.Empty)
        .Replace("{maxResults}", "1")
        .Replace("{BingMapsAPIKey}", _option.Value.BingMapsKey);

        geoLocationUrl = HttpUtility.UrlPathEncode(geoLocationUrl);
        geoLocationJson = (geoLocationUrl).GetJsonFromUrl();

        using (JsonDocument document = JsonDocument.Parse(geoLocationJson))
        {
          JsonElement root = document.RootElement;
          JsonElement resourceSetElement = root.GetProperty("resourceSets");
          JsonElement resource = resourceSetElement[0].GetProperty("resources")[0];
          JsonElement resourcePoint = resource.GetProperty("point");
          JsonElement resourcePointCoordinates = resourcePoint.GetProperty("coordinates");
          checkout.ShippingLatitude = Decimal.Parse(resourcePointCoordinates[0].ToString());
          checkout.ShippingLongitude = Decimal.Parse(resourcePointCoordinates[1].ToString());
        }
      }
      catch
      {
        // Address was not found. Fail silently and continue. Geolocation will not be displayed for this address.
      }

      checkout.GoogleMapsUrl = _option.Value.GoogleMapsUrl;

      ViewData.Add("OrderDetails.WelcomeText", _context.LocalizedTexts.Where(
        x => x.PlacementPointCode.Equals("OrderDetails.WelcomeText")).Select(x => x.Text).FirstOrDefault());

      return View(checkout);
    }

    private bool ShippingAddressSame(OrderBillTo Billing, OrderShipTo Shipping)
    {
      if (!Object.Equals(Billing.NameOnCreditCard, Shipping.RecipientName))
        return false;
      if (!Object.Equals(Billing.AddressLine1, Shipping.AddressLine1))
        return false;
      if (!Object.Equals(Billing.AddressLine2, Shipping.AddressLine2))
        return false;
      if (!Object.Equals(Billing.CityName, Shipping.CityName))
        return false;
      if (!Object.Equals(Billing.StateName, Shipping.StateName))
        return false;
      if (!Object.Equals(Billing.CountryName, Shipping.CountryName))
        return false;
      if (!Object.Equals(Billing.PostalCode, Shipping.PostalCode))
        return false;
      return true;
    }

    private int TotalQuantityOfItems(long OrderId)
    {
      int total = 0;

      foreach (ProductOrderLineItem lineItem in _context.ProductOrderLineItems.Where(
        x => x.ProductOrder == OrderId).ToList())
        total += lineItem.Quantity;

      return total;
    }
  }
}
