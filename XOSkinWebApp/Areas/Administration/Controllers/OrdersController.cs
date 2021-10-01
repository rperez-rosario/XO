using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServiceStack;
using XOSkinWebApp.Areas.Administration.Models;
using XOSkinWebApp.ConfigurationHelper;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Areas.Administration.Controllers
{
  [Area("Administration")]
  public class OrdersController : Controller
  {
    private readonly XOSkinContext _context;
    private readonly IOptions<Option> _option;

    public OrdersController(XOSkinContext context, IOptions<Option> option)
    {
      _context = context;
      _option = option;
    }

    // GET: Administration/Orders
    public async Task<IActionResult> Index()
    {
      List<OrderViewModel> order = null;
      OrderShipTo shipment = null;
      OrderBillTo billing = null;

      order = new List<OrderViewModel>();

      foreach (ProductOrder po in await _context.ProductOrders.ToListAsync())
      {
        shipment = _context.OrderShipTos.Where(x => x.Order == po.Id).FirstOrDefault();
        billing = _context.OrderBillTos.Where(x => x.Order == po.Id).FirstOrDefault();
        order.Add(new OrderViewModel()
        {
          Arrives = shipment.Arrives,
          Carrier = shipment.CarrierName,
          DatePlaced = po.DatePlaced,
          NumberOfItems = TotalQuantityOfItems(po.Id),
          OrderId = po.Id,
          Recipient = shipment.RecipientName,
          TrackingNumber = shipment.TrackingNumber,
          Status = shipment.Shipped == null ? "Shipping Soon" : ((bool)shipment.Shipped ?
            "Shipped: " + ((DateTime)shipment.ActualShipDate).ToShortDateString() : "Shipping Soon"),
          RefundStatus = (billing.RefundRequested == null || (bool)!billing.RefundRequested) ? "NOT REQUESTED" :
            (bool)billing.RefundRequested && (billing.Refunded == null || (bool)!billing.Refunded) ? "REQUESTED" :
            ((billing.Refunded == null || (bool)!billing.Refunded) ? "NOT REQUESTED" : "REFUNDED"),
          RefundDate = billing.RefundedOn,
          RefundReason = billing.RefundReason,
          CancellationStatus = (po.Cancelled == null || (bool)!po.Cancelled) ? "NOT REQUESTED" : "CANCELLED",
          CancellationDate = po.CancelledOn,
          CancelReason = po.CancelReason
        });
      }

      return View(order);
    }

    // GET: Administration/Orders/Edit/5
    public IActionResult Edit(long Id)
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
          FulfillmentStatus = (shipping.Shipped == null || (bool)!shipping.Shipped) ? "Shipping Soon" : "Shipped",
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

    // POST: Administration/Orders/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, [Bind("OrderId,ShopifyId,BillingName,BillingAddress1,BillingAddress2," +
      "BillingCity,BillingState,BillingCountry,BillingPostalCode,BilledOn,SubTotal,ShippingCharges,Taxes,CodeDiscount," +
      "CouponDiscount,Total,IsGift,ShippingAddressSame,ShippingName,ShippingAddress1,ShippingAddress2,ShippingCity,ShippingState," +
      "ShippingCountry,ShippingPostalCode,ShippingLongitude,ShippingLatitude,GoogleMapsUrl,ShippingCarrier,TrackingNumber,ShippedOn," +
      "ExpectedToArrive,CreditCardNumber,CreditCardCVC,CreditCardExpirationDate,SelectedCarrierId,CarrierName,TotalWeightInPounds," +
      "ShipEngineShipmentId,ShipEngineRateId,ShipEngineLabelUrl,CalculatedShippingAndTaxes,CardDeclined,ShippingAddressDeclined," +
      "FulfillmentStatus,clientIpAddress")] CheckoutViewModel checkoutViewModel)
    {
      if (id != checkoutViewModel.OrderId)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
            _context.Update(checkoutViewModel);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CheckoutViewModelExists(checkoutViewModel.OrderId))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        return RedirectToAction(nameof(Index));
      }
      return View(checkoutViewModel);
    }

    private bool CheckoutViewModelExists(long id)
    {
      return _context.ProductOrders.Any(e => e.Id == id);
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
