using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ShopifySharp;
using XOSkinWebApp.ConfigurationHelper;
using XOSkinWebApp.Models;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Controllers
{
  [Authorize]
  public class CheckoutController : Controller
  {
    private readonly XOSkinContext _context;
    private readonly IOptions<Option> _option;

    public CheckoutController(XOSkinContext context)
    {
      _context = context;
    }

    public IActionResult Index()
    {
      CheckoutViewModel checkoutViewModel = new CheckoutViewModel();
      List<ShoppingCartLineItemViewModel> lineItemViewModel = new List<ShoppingCartLineItemViewModel>();
      List<ShoppingCartLineItem> lineItem = _context.ShoppingCartLineItems.Where(
        x => x.ShoppingCart == _context.ShoppingCarts.Where(x => x.User.Equals(_context.AspNetUsers.Where(
        x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault()))
        .Select(x => x.Id).FirstOrDefault()).ToList();

      checkoutViewModel.SubTotal = 0.0M;

      foreach (ShoppingCartLineItem li in lineItem)
      {
        lineItemViewModel.Add(new ShoppingCartLineItemViewModel()
        {
          Id = li.Id,
          ProductId = li.Product,
          ImageSource = _context.Products.Where(
            x => x.Id.Equals(li.Product)).Select(x => x.ImagePathLarge).FirstOrDefault(),
          ProductName = _context.Products.Where(
            x => x.Id == li.Product)
            .Select(x => x.Name).FirstOrDefault(),
          ProductDescription = _context.Products.Where(
            x => x.Id == li.Product)
            .Select(x => x.Description).FirstOrDefault(),
          Quantity = li.Quantity,
          Total = li.Total
        });
        checkoutViewModel.SubTotal += li.Total;
      }

      checkoutViewModel.LineItem = lineItemViewModel;
      checkoutViewModel.CreditCardExpirationDate = DateTime.Now;

      checkoutViewModel.Taxes = 0.0M; // TODO: Map this.
      checkoutViewModel.ShippingCharges = 0.0M; // TODO: Map this.
      checkoutViewModel.CodeDiscount = 0.0M; // TODO: Map this.
      checkoutViewModel.CouponDiscount = 0.0M; // TODO: Map this.
      checkoutViewModel.IsGift = false; // TODO: Map this.
      checkoutViewModel.Total = checkoutViewModel.SubTotal + checkoutViewModel.Taxes + checkoutViewModel.ShippingCharges -
        checkoutViewModel.CodeDiscount - checkoutViewModel.CouponDiscount;

      ViewData.Add("Checkout.WelcomeText", _context.LocalizedTexts.Where(
       x => x.PlacementPointCode.Equals("Checkout.WelcomeText"))
       .Select(x => x.Text).FirstOrDefault());

      return View(checkoutViewModel);
    }

    public IActionResult PlaceOrder(CheckoutViewModel Model)
    {
      ProductOrder order = null;
      
      Model.ShippingCarrier = "UPS"; // TODO: Map this.
      Model.TrackingNumber = "0123456789"; // TODO: Map this.
      Model.ShippedOn = DateTime.UtcNow; // TODO: Map this.
      Model.ExpectedToArrive = DateTime.UtcNow.AddDays(2); // TODO: Map this.
      Model.BilledOn = DateTime.UtcNow; // TODO: Map this.

      Model.LineItem = new List<ShoppingCartLineItemViewModel>();      

      try
      {
        order = new ProductOrder();
        
        order.ShippingCost = 0.0M;
        order.Subtotal = 0.0M;
        order.ApplicableTaxes = 0.0M;
        order.CodeDiscount = 0.0M;
        order.CouponDiscount = 0.0M;
        order.ShippingCost = 0.0M;
        order.Total = 0.0M;

        order.User = _context.AspNetUsers.Where(
          x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault();
        order.DatePlaced = DateTime.Now;
        order.Subtotal = (decimal)Model.SubTotal;
        order.ApplicableTaxes = (decimal)Model.Taxes;
        order.CodeDiscount = (decimal)Model.CodeDiscount;
        order.CouponDiscount = (decimal)Model.CouponDiscount;
        order.GiftOrder = Model.IsGift;
        order.ShippingCost = (decimal)Model.ShippingCharges;
        order.Total = (decimal)Model.Total;

        _context.ProductOrders.Add(order);

        _context.SaveChanges();
      }
      catch (Exception ex)
      {
        throw new Exception("An error was encontered while writing the product order to the database.", ex);
      }

      Model.OrderId = order.Id;
      Model.ShopifyId = 123456789; // TODO: Map this.
      Model.LineItem = new List<ShoppingCartLineItemViewModel>();

      try
      {
        foreach (ShoppingCartLineItem item in _context.ShoppingCartLineItems.Where(
        x => x.ShoppingCart.Equals(_context.ShoppingCarts.Where(
          x => x.User.Equals(_context.AspNetUsers.Where(
            x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault())).Select(x => x.Id).FirstOrDefault())))
        {
          _context.ProductOrderLineItems.Add(new ProductOrderLineItem()
          {
            ImageSource = _context.Products.Where(
              x => x.Id.Equals(item.Product)).Select(x => x.ImagePathLarge).FirstOrDefault(),
            Product = item.Product,
            ProductOrder = order.Id,
            Quantity = item.Quantity,
            Total = item.Total
          });
          Model.LineItem.Add(new ShoppingCartLineItemViewModel()
          {
            Id = item.Id,
            ProductId = item.Product,
            ImageSource = _context.Products.Where(
              x => x.Id.Equals(item.Product)).Select(x => x.ImagePathLarge).FirstOrDefault(),
            ProductName = _context.Products.Where(
              x => x.Id == item.Product)
              .Select(x => x.Name).FirstOrDefault(),
            ProductDescription = _context.Products.Where(
              x => x.Id == item.Product)
              .Select(x => x.Description).FirstOrDefault(),
            Quantity = item.Quantity,
            Total = item.Total
          });
          _context.ShoppingCartLineItems.Remove(item);
        }

        _context.SaveChanges();
      }
      catch (Exception ex)
      {
        throw new Exception("An error was encountered while saving the order's line items.", ex);
      }

      try
      {
        _context.OrderBillTos.Add(new OrderBillTo()
        {
          NameOnCreditCard = Model.BillingName,
          AddressLine1 = Model.BillingAddress1,
          AddressLine2 = Model.BillingAddress2,
          CityName = Model.BillingCity,
          StateName = Model.BillingState,
          CountryName = Model.BillingCountry,
          PostalCode = Model.BillingPostalCode,
          BillingDate = Model.BilledOn,
          Order = order.Id
        });

        _context.SaveChanges();

        if (Model.ShippingAddressSame)
        {
          _context.OrderShipTos.Add(new OrderShipTo()
          {
            RecipientName = Model.BillingName,
            AddressLine1 = Model.BillingAddress1,
            AddressLine2 = Model.BillingAddress2,
            CityName = Model.BillingCity,
            StateName = Model.BillingState,
            CountryName = Model.BillingCountry,
            PostalCode = Model.BillingPostalCode,
            ShipDate = Model.ShippedOn,
            CarrierName = Model.ShippingCarrier,
            TrackingNumber = Model.TrackingNumber,
            Order = order.Id
          });
          _context.SaveChanges();
        }
        else
        {
          _context.OrderShipTos.Add(new OrderShipTo()
          {
            RecipientName = Model.ShippingName,
            AddressLine1 = Model.ShippingAddress1,
            AddressLine2 = Model.ShippingAddress2,
            CityName = Model.ShippingCity,
            StateName = Model.ShippingState,
            CountryName = Model.ShippingCountry,
            PostalCode = Model.ShippingPostalCode,
            ShipDate = Model.ShippedOn,
            CarrierName = Model.ShippingCarrier,
            TrackingNumber = Model.TrackingNumber,
            Order = order.Id
          });
          _context.SaveChanges();
        }
      }
      catch (Exception ex)
      {
        throw new Exception("An error was encountered while saving the order's address(es.)", ex);
      }

      ViewData.Add("OrderConfirmation.WelcomeText", _context.LocalizedTexts.Where(
       x => x.PlacementPointCode.Equals("OrderConfirmation.WelcomeText"))
       .Select(x => x.Text).FirstOrDefault());

      return View("OrderConfirmation", Model);
    }
  }
}
