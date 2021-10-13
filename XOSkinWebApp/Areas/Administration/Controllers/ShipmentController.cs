using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ServiceStack;
using ShopifySharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using XOSkinWebApp.Areas.Administration.Models;
using XOSkinWebApp.ConfigurationHelper;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Areas.Administration.Controllers
{
  [Area("Administration")]
  [Authorize]
  public class ShipmentController : Controller
  {
    private readonly XOSkinContext _context;
    private readonly IOptions<Option> _option;

    public ShipmentController(XOSkinContext context, IOptions<Option> option)
    {
      _context = context;
      _option = option;
    }

    // GET: ShipmentController
    public async Task<ActionResult> Index()
    {
      List<ShipmentViewModel> model = new List<ShipmentViewModel>();
      int numberOfItems;
      ProductOrder order = null;
      OrderBillTo billing = null;

      foreach (OrderShipTo shipment in _context.OrderShipTos.ToList())
      {
        order = await _context.ProductOrders.FindAsync(shipment.Order);
        numberOfItems = 0;

        billing = _context.OrderBillTos.Where(x => x.Order == order.Id).FirstOrDefault();
        
        foreach (ProductOrderLineItem item in _context.ProductOrderLineItems.Where(
          x => x.ProductOrder == shipment.Order))
        {
          numberOfItems += item.Quantity;
        }

        model.Add(new ShipmentViewModel()
        {
          ShipmentStatus = billing.Refunded != null ? (bool)billing.Refunded ? "REFUNDED" : 
            (order.Cancelled == null ? (shipment.Shipped == true ? "SHIPPED" : "PENDING") : 
            (bool)order.Cancelled ? "CANCELLED" : (shipment.Shipped == true ? "SHIPPED" : "PENDING")) :
            (order.Cancelled == null ? (shipment.Shipped == true ? "SHIPPED" : "PENDING") :
            (bool)order.Cancelled ? "CANCELLED" : (shipment.Shipped == true ? "SHIPPED" : "PENDING")),
          FulfillmentStatus = billing.Refunded != null ? (bool)billing.Refunded ? "REFUNDED" :
            (order.Cancelled == null ? (shipment.Shipped == true ? "SHIPPED" : "PENDING") :
            (bool)order.Cancelled ? "CANCELLED" : (shipment.Shipped == true ? "SHIPPED" : "PENDING")) :
            (order.Cancelled == null ? (shipment.Shipped == true ? "SHIPPED" : "PENDING") :
            (bool)order.Cancelled ? "CANCELLED" : (shipment.Shipped == true ? "SHIPPED" : "PENDING")),
          DatePlaced = _context.ProductOrders.Where(
            x => x.Id == shipment.Order).Select(x => x.DatePlaced).FirstOrDefault(),
          DateShipped = shipment.ShipDate,
          ActualDateShipped = shipment.ActualShipDate,
          Arrives = shipment.Arrives,
          ActualArrives = shipment.ActualArrives,
          Recipient = shipment.RecipientName,
          NumberOfItems = numberOfItems,
          TrackingNumber = shipment.TrackingNumber,
          ShippingLabelURL = shipment.ShippingLabelUrl,
          StateName = shipment.StateName,
          OrderId = shipment.Order,
          OrderCancelled = order.Cancelled == null ? false : (bool)order.Cancelled
        });
      }

      return View(model);
    }

    // GET: ShipmentController/Edit/5
    public IActionResult Edit(long Id)
    {
      ProductOrder order = null;
      List<ProductOrderLineItem> lineItem = null;
      OrderBillTo billing = null;
      OrderShipTo shipping = null;
      ShipOutViewModel checkout = null;
      String geoLocationUrl = null;
      String geoLocationJson = null;

      try
      {
        order = _context.ProductOrders.Where(x => x.Id == Id).FirstOrDefault();
        lineItem = _context.ProductOrderLineItems.Where(x => x.ProductOrder == Id).ToList<ProductOrderLineItem>();
        billing = _context.OrderBillTos.Where(x => x.Order == Id).FirstOrDefault();
        shipping = _context.OrderShipTos.Where(x => x.Order == Id).FirstOrDefault();

        checkout = new ShipOutViewModel()
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
          ExpectedToShipOn = shipping.ShipDate == null ? new DateTime(1754, 1, 1) : shipping.ShipDate.Value,
          ShippingName = shipping.RecipientName,
          ShippingCountry = shipping.CountryName,
          ShippingAddress1 = shipping.AddressLine1,
          ShippingAddress2 = shipping.AddressLine2,
          ShippingAddressSame = ShippingAddressSame(billing, shipping),
          ExpectedToArrive = shipping.Arrives == null ? new DateTime(1754, 1, 1) : shipping.Arrives.Value,
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
          TrackingNumber = shipping.TrackingNumber,
          ShipEngineLabelUrl = shipping.ShippingLabelUrl,
          ShipmentStatus = billing.Refunded != null ? (bool)billing.Refunded ? "REFUNDED" :
            (order.Cancelled == null ? (shipping.Shipped == true ? "SHIPPED" : "PENDING") :
            (bool)order.Cancelled ? "CANCELLED" : (shipping.Shipped == true ? "SHIPPED" : "PENDING")) :
            (order.Cancelled == null ? (shipping.Shipped == true ? "SHIPPED" : "PENDING") :
            (bool)order.Cancelled ? "CANCELLED" : (shipping.Shipped == true ? "SHIPPED" : "PENDING")),
          ShippedOn = shipping.ActualShipDate,
          ShippedBy = _context.AspNetUsers.Where(
            x => x.Id.Equals(shipping.ShippedBy)).Select(x => x.Email).FirstOrDefault()
        };

        checkout.LineItem = new List<ShippingLineItemViewModel>();

        foreach (ProductOrderLineItem li in lineItem)
        {
          checkout.LineItem.Add(new ShippingLineItemViewModel()
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTrackingNumbers(long OrderId, ShipOutViewModel Model)
    {
      ProductOrder order = null;
      List<ProductOrderLineItem> lineItem = null;
      OrderBillTo billing = null;
      OrderShipTo shipping = null;
      ShipOutViewModel checkout = null;
      String geoLocationUrl = null;
      String geoLocationJson = null;

      shipping = _context.OrderShipTos.Where(x => x.Order == OrderId).FirstOrDefault();
      shipping.TrackingNumber = Model.TrackingNumber;
      _context.OrderShipTos.Update(shipping);
      await _context.SaveChangesAsync();

      try
      {
        order = _context.ProductOrders.Where(x => x.Id == OrderId).FirstOrDefault();
        lineItem = _context.ProductOrderLineItems.Where(x => x.ProductOrder == OrderId).ToList<ProductOrderLineItem>();
        billing = _context.OrderBillTos.Where(x => x.Order == OrderId).FirstOrDefault();

        checkout = new ShipOutViewModel()
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
          ExpectedToShipOn = shipping.ShipDate == null ? new DateTime(1754, 1, 1) : shipping.ShipDate.Value,
          ShippingName = shipping.RecipientName,
          ShippingCountry = shipping.CountryName,
          ShippingAddress1 = shipping.AddressLine1,
          ShippingAddress2 = shipping.AddressLine2,
          ShippingAddressSame = ShippingAddressSame(billing, shipping),
          ExpectedToArrive = shipping.Arrives == null ? new DateTime(1754, 1, 1) : shipping.Arrives.Value,
          IsGift = (bool)order.GiftOrder,
          OrderId = OrderId,
          CarrierName = shipping.CarrierName,
          ShippingCharges = order.ShippingCost,
          ShippingCity = shipping.CityName,
          ShippingPostalCode = shipping.PostalCode,
          ShippingState = shipping.StateName,
          ShopifyId = order.ShopifyId == null ? long.MinValue : order.ShopifyId.Value,
          SubTotal = order.Subtotal,
          Taxes = order.ApplicableTaxes,
          Total = order.Total,
          TrackingNumber = shipping.TrackingNumber,
          ShipEngineLabelUrl = shipping.ShippingLabelUrl,
          ShipmentStatus = billing.Refunded != null ? (bool)billing.Refunded ? "REFUNDED" :
            (order.Cancelled == null ? (shipping.Shipped == true ? "SHIPPED" : "PENDING") :
            (bool)order.Cancelled ? "CANCELLED" : (shipping.Shipped == true ? "SHIPPED" : "PENDING")) :
            (order.Cancelled == null ? (shipping.Shipped == true ? "SHIPPED" : "PENDING") :
            (bool)order.Cancelled ? "CANCELLED" : (shipping.Shipped == true ? "SHIPPED" : "PENDING")),
          ShippedOn = shipping.ActualShipDate,
          ShippedBy = _context.AspNetUsers.Where(
            x => x.Id.Equals(shipping.ShippedBy)).Select(x => x.Email).FirstOrDefault()
        };

        checkout.LineItem = new List<ShippingLineItemViewModel>();

        foreach (ProductOrderLineItem li in lineItem)
        {
          checkout.LineItem.Add(new ShippingLineItemViewModel()
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

      return View("Edit", checkout);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // POST: ShipmentController/Edit/5
    public async Task<IActionResult> Edit(long OrderId, ShipOutViewModel Model)
    {
      OrderShipTo order = null;
      LocationService shLocationService = null;
      FulfillmentService shFulfillmentService = null;
      Fulfillment shFulfillment = null;
      Location shLocation = null;

      shLocationService = new LocationService(
        _option.Value.ShopifyUrl, _option.Value.ShopifyStoreFrontAccessToken);

      shLocation = shLocationService.ListAsync().Result.First();

      shFulfillmentService = new FulfillmentService(
        _option.Value.ShopifyUrl, _option.Value.ShopifyStoreFrontAccessToken);

      shFulfillment = new Fulfillment()
      {
        LocationId = shLocation.Id,
        TrackingCompany = _option.Value.TrackingCompany,
        TrackingUrl = _option.Value.StampsDotComPackageTrackingUrl + _context.OrderShipTos.Where(
          x => x.Order == OrderId).Select(x => x.TrackingNumber).FirstOrDefault(),
        TrackingNumber = _context.OrderShipTos.Where(
          x => x.Order == OrderId).Select(x => x.TrackingNumber).FirstOrDefault()
      };

      shFulfillment = await shFulfillmentService.CreateAsync((long)_context.ProductOrders.Where(
        x => x.Id == OrderId).Select(x => x.ShopifyId).FirstOrDefault(), shFulfillment);

      order = _context.OrderShipTos.Where(
        x => x.Order == OrderId).FirstOrDefault();
      order.Shipped = true;
      order.ActualShipDate = DateTime.UtcNow;
      order.ActualArrives = DateTime.UtcNow.AddDays(2);
      order.ShippedBy = _context.AspNetUsers.Where(
        x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault();

      _context.OrderShipTos.Update(order);
      _context.SaveChanges();
     
      return RedirectToAction("Index");
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
