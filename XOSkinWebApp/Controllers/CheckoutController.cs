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
      ORM.Address billingAddress = null;
      ORM.Address shippingAddress = null;

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
      checkoutViewModel.ShippingCarrier = "UPS"; // TODO: Map this.
      checkoutViewModel.ExpectedToArrive = DateTime.UtcNow.AddDays(2); // TODO: Map this.
      checkoutViewModel.Total = checkoutViewModel.SubTotal + checkoutViewModel.Taxes + checkoutViewModel.ShippingCharges -
        checkoutViewModel.CodeDiscount - checkoutViewModel.CouponDiscount;

      if (_context.Addresses.Where(
        x => x.User.Equals(_context.AspNetUsers.Where(
        x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault())).Count() == 2)
      {
        billingAddress = _context.Addresses.Where(
          x => x.User.Equals(_context.AspNetUsers.Where(
          x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault())).Where(
          x => x.AddressType == 1).FirstOrDefault();

        checkoutViewModel.BillingName = billingAddress.Name;
        checkoutViewModel.BillingAddress1 = billingAddress.Line1;
        checkoutViewModel.BillingAddress2 = billingAddress.Line2;
        checkoutViewModel.BillingCity = billingAddress.CityName;
        checkoutViewModel.BillingState = billingAddress.StateName;
        checkoutViewModel.BillingCountry = billingAddress.CountryName;
        checkoutViewModel.BillingPostalCode = billingAddress.PostalCode;

        shippingAddress = _context.Addresses.Where(
          x => x.User.Equals(_context.AspNetUsers.Where(
          x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault())).Where(
          x => x.AddressType == 2).FirstOrDefault();

        if (!ShippingAddressSame(billingAddress, shippingAddress))
        {
          checkoutViewModel.ShippingName = shippingAddress.Name;
          checkoutViewModel.ShippingAddress1 = shippingAddress.Line1;
          checkoutViewModel.ShippingAddress2 = shippingAddress.Line2;
          checkoutViewModel.ShippingCity = shippingAddress.CityName;
          checkoutViewModel.ShippingState = shippingAddress.StateName;
          checkoutViewModel.ShippingCountry = shippingAddress.CountryName;
          checkoutViewModel.ShippingPostalCode = shippingAddress.PostalCode;
        }
        else
        {
          checkoutViewModel.ShippingAddressSame = true;
        }
      }

      ViewData.Add("Checkout.WelcomeText", _context.LocalizedTexts.Where(
       x => x.PlacementPointCode.Equals("Checkout.WelcomeText"))
       .Select(x => x.Text).FirstOrDefault());

      return View(checkoutViewModel);
    }

    public IActionResult PlaceOrder(CheckoutViewModel Model)
    {
      ProductOrder order = null;

      Model.ShippingCarrier = Model.ShippingCarrier;
      Model.TrackingNumber = "0123456789"; // TODO: Map this.
      Model.ShippedOn = DateTime.UtcNow; // TODO: Map this.
      Model.ExpectedToArrive = Model.ExpectedToArrive;
      Model.BilledOn = DateTime.UtcNow;

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

        if (_context.Addresses.Where(
          x => x.User.Equals(_context.AspNetUsers.Where(
          x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault())).Where(
          x => x.AddressType == 1).FirstOrDefault() != null)
        {
          _context.Addresses.Remove(_context.Addresses.Where(
            x => x.User.Equals(_context.AspNetUsers.Where(
            x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault())).Where(
            x => x.AddressType == 1).FirstOrDefault());
          _context.SaveChanges();
        }

        _context.Addresses.Add(new ORM.Address()
        {
          Name = Model.BillingName,
          AddressType = 1, // Billing.
          CountryName = Model.BillingCountry,
          Line1 = Model.BillingAddress1,
          Line2 = Model.BillingAddress2,
          PostalCode = Model.BillingPostalCode,
          StateName = Model.BillingState,
          CityName = Model.BillingCity,
          User = _context.AspNetUsers.Where(
            x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault()
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
            Order = order.Id,
            Arrives = Model.ExpectedToArrive
          });

          _context.SaveChanges();

          if (_context.Addresses.Where(
            x => x.User.Equals(_context.AspNetUsers.Where(
            x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault())).Where(
            x => x.AddressType == 2).FirstOrDefault() != null)
          {
            _context.Addresses.Remove(_context.Addresses.Where(
              x => x.User.Equals(_context.AspNetUsers.Where(
              x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault())).Where(
              x => x.AddressType == 2).FirstOrDefault());
            _context.SaveChanges();
          }

          _context.Addresses.Add(new ORM.Address()
          {
            Name = Model.BillingName,
            AddressType = 2, // Shipping.
            CountryName = Model.BillingCountry,
            Line1 = Model.BillingAddress1,
            Line2 = Model.BillingAddress2,
            PostalCode = Model.BillingPostalCode,
            CityName = Model.BillingCity,
            StateName = Model.BillingState,
            User = _context.AspNetUsers.Where(
              x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault()
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
            Order = order.Id,
            Arrives = Model.ExpectedToArrive
          });

          _context.SaveChanges();

          if (_context.Addresses.Where(
            x => x.User.Equals(_context.AspNetUsers.Where(
            x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault())).Where(
            x => x.AddressType == 2).FirstOrDefault() != null)
          {
            _context.Addresses.Remove(_context.Addresses.Where(
              x => x.User.Equals(_context.AspNetUsers.Where(
              x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault())).Where(
              x => x.AddressType == 2).FirstOrDefault());
            _context.SaveChanges();
          }
          
          _context.Addresses.Add(new ORM.Address()
          {
            Name = Model.ShippingName,
            AddressType = 2, // Shipping.
            CountryName = Model.ShippingCountry,
            Line1 = Model.ShippingAddress1,
            Line2 = Model.ShippingAddress2,
            PostalCode = Model.ShippingPostalCode,
            CityName = Model.ShippingCity,
            StateName = Model.ShippingState,
            User = _context.AspNetUsers.Where(
              x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault()
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

    private bool ShippingAddressSame(ORM.Address Billing, ORM.Address Shipping)
    {
      if (!Billing.Name.Equals(Shipping.Name))
        return false;
      if (!Billing.Line1.Equals(Shipping.Line1))
        return false;
      if (!Billing.Line2.Equals(Shipping.Line2))
        return false;
      if (!Billing.CityName.Equals(Shipping.CityName))
        return false;
      if (!Billing.StateName.Equals(Shipping.StateName))
        return false;
      if (!Billing.CountryName.Equals(Shipping.CountryName))
        return false;
      if (!Billing.PostalCode.Equals(Shipping.PostalCode))
        return false;

      return true;
    }
  }
}
