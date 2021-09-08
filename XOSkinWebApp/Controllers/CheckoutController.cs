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

      checkoutViewModel.Taxes = 0.0M; // TODO: IMPORTANT, GET FROM SHOPIFY API.
      checkoutViewModel.ShippingCharges = 0.0M; // TODO: IMPORTANT, GET FROM SHOPIFY API.
      checkoutViewModel.CodeDiscount = 0.0M; // TODO: CALCULATE.
      checkoutViewModel.CouponDiscount = 0.0M; // TODO: CALCULATE.
      checkoutViewModel.IsGift = false; // TODO: Map this.
      checkoutViewModel.ShippingCarrier = "UPS"; // TODO: IMPORTANT, GET FROM SHOPIFY API.
      checkoutViewModel.ExpectedToArrive = DateTime.UtcNow > DateTime.UtcNow.AddHours(10) ?
        DateTime.UtcNow.AddDays(2) : DateTime.UtcNow.AddDays(3); // TODO: Map this.
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
      ORM.Product product = null;
      decimal balanceBeforeTransaction = 0.00M;
      decimal subTotal = 0.0M;
      decimal applicableTaxes = 0.0M;
      decimal codeDiscount = 0.0M;
      decimal couponDiscount = 0.0M;
      decimal shippingCost = 0.0M;
      decimal total = 0.0M;
      ORM.Product kit = null;
      List<KitProduct> kitProduct = null;
      long stock = long.MaxValue;

      Model.ShippingCarrier = Model.ShippingCarrier;
      Model.TrackingNumber = "1000000000000000000001"; // TODO: IMPORTANT, GET FROM SHOPIFY.
      Model.ShippedOn = DateTime.UtcNow > DateTime.UtcNow.AddHours(10) ? 
        DateTime.UtcNow : DateTime.UtcNow.AddDays(1); // 5:00 PM PTDT.
      Model.ExpectedToArrive = new DateTime(9999, 12, 31); // TODO: IMPORTANT, GET FROM SHOPIFY.
      Model.BilledOn = DateTime.UtcNow;

      Model.LineItem = new List<ShoppingCartLineItemViewModel>();

      try
      {
        order = new ProductOrder();

        foreach (ShoppingCartLineItem cli in _context.ShoppingCartLineItems.Where(
          x => x.ShoppingCart == _context.ShoppingCarts.Where(
          x => x.User.Equals(_context.AspNetUsers.Where(
          x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault())).Select(x => x.Id).FirstOrDefault()).ToList())
        {
          subTotal += _context.Prices.Where(
            x => x.Id == _context.Products.Where(
              x => x.Id == cli.Product).Select(x => x.CurrentPrice).FirstOrDefault()).Select(x => x.Amount).FirstOrDefault() *
              cli.Quantity;
        }

        shippingCost = 0.0M; // TODO: IMPORTANT, GET FROM SHOPIFY API.
        codeDiscount = 0.0M; // TODO: CALCULATE.
        couponDiscount = 0.0M; // TODO : CALCULATE.
        applicableTaxes = 0.0M; // TODO: IMPORTANT, GET FROM SHOPIFY API.
        
        total = subTotal + shippingCost - codeDiscount - couponDiscount  + applicableTaxes;

        order.User = _context.AspNetUsers.Where(
          x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault();
        order.DatePlaced = DateTime.UtcNow;
        order.Subtotal = subTotal;
        order.ApplicableTaxes = applicableTaxes;
        order.CodeDiscount = codeDiscount;
        order.CouponDiscount = couponDiscount;
        order.GiftOrder = Model.IsGift;
        order.ShippingCost = shippingCost;
        order.Total = total;

        _context.ProductOrders.Add(order);

        _context.SaveChanges();
      }
      catch (Exception ex)
      {
        throw new Exception("An error was encontered while writing the product order to the database.", ex);
      }

      Model.OrderId = order.Id;
      Model.ShopifyId = 123456789; // TODO: IMPORTANT, GET FROM SHOPIFY API.
      Model.LineItem = new List<ShoppingCartLineItemViewModel>();

      try
      {
        foreach (ShoppingCartLineItem item in _context.ShoppingCartLineItems.Where(
          x => x.ShoppingCart.Equals(_context.ShoppingCarts.Where(
          x => x.User.Equals(_context.AspNetUsers.Where(
          x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault())).Select(
          x => x.Id).FirstOrDefault())).ToList())
        {
          _context.ProductOrderLineItems.Add(new ProductOrderLineItem()
          {
            ImageSource = _context.Products.Where(
              x => x.Id.Equals(item.Product)).Select(x => x.ImagePathLarge).FirstOrDefault(),
            Product = item.Product,
            Sample = _context.Products.Where(x => x.Id == item.Product).Select(x => x.Sample).FirstOrDefault(),
            Sku = _context.Products.Where(x => x.Id == item.Product).Select(x => x.Sku).FirstOrDefault(),
            Name = _context.Products.Where(x => x.Id == item.Product).Select(x => x.Name).FirstOrDefault(),
            Description = _context.Products.Where(x => x.Id == item.Product).Select(x => x.Description).FirstOrDefault(),
            ProductType = _context.Products.Where(x => x.Id == item.Product).Select(x => x.ProductType).FirstOrDefault(),
            KitType = _context.Products.Where(x => x.Id == item.Product).Select(x => x.KitType).FirstOrDefault(),
            VolumeInFluidOunces = _context.Products.Where(
              x => x.Id == item.Product).Select(x => x.VolumeInFluidOunces).FirstOrDefault(),
            PhBalance = _context.Products.Where(x => x.Id == item.Product).Select(x => x.Ph).FirstOrDefault(),
            ShippingWeightLb = _context.Products.Where(
              x => x.Id == item.Product).Select(x => x.ShippingWeightLb).FirstOrDefault(),
            Price = _context.Products.Where(x => x.Id == item.Product).Select(x => x.CurrentPrice).FirstOrDefault(),
            Cost = (long)_context.Products.Where(x => x.Id == item.Product).Select(x => x.Cost).FirstOrDefault(),
            ProductOrder = order.Id,
            Quantity = item.Quantity,
            Total = item.Total
          });

          _context.SaveChanges();

          if (_context.Products.Where(x => x.Id == item.Product).Select(x => x.KitType).FirstOrDefault() == null)
          {
            product = _context.Products.Where(x => x.Id == item.Product).FirstOrDefault();
            product.Stock -= item.Quantity;
            _context.Products.Update(product);
            _context.SaveChanges();

            foreach (KitProduct kp in _context.KitProducts.ToList())
            {
              if (kp.Product == product.Id)
              {
                kit = _context.Products.Where(x => x.Id == kp.Kit).FirstOrDefault();
                kitProduct = _context.KitProducts.Where(x => x.Kit == kit.Id).ToList();
                stock = long.MaxValue;

                foreach (KitProduct kpB in kitProduct)
                {
                  if (_context.Products.Where(
                    x => x.Id == kpB.Product).Select(x => x.Stock).FirstOrDefault() < stock)
                  {
                    stock = (long)_context.Products.Where(
                      x => x.Id == kpB.Product).Select(x => x.Stock).FirstOrDefault();
                  }
                }

                kit.Stock = stock;

                _context.Products.Update(kit);
                _context.SaveChanges();
              }
            }
          }
          else
          {
            kit = _context.Products.Where(x => x.Id == item.Product).FirstOrDefault();
            kitProduct = _context.KitProducts.Where(x => x.Kit == kit.Id).ToList();

            foreach (KitProduct kp in kitProduct)
            {
              product = _context.Products.Where(x => x.Id == kp.Product).FirstOrDefault();
              product.Stock -= 1;
              _context.Products.Update(product);
              _context.SaveChanges();
            }

            foreach (KitProduct kp in kitProduct)
            {
              foreach (KitProduct kpB in _context.KitProducts.ToList())
              {
                if (kpB.Product == kp.Product)
                {
                  kit = _context.Products.Where(x => x.Id == kpB.Kit).FirstOrDefault();
                  kitProduct = _context.KitProducts.Where(x => x.Kit == kit.Id).ToList();
                  stock = long.MaxValue;

                  foreach (KitProduct kpC in kitProduct)
                  {
                    if (_context.Products.Where(
                      x => x.Id == kpC.Product).Select(x => x.Stock).FirstOrDefault() < stock)
                    {
                      stock = (long)_context.Products.Where(
                        x => x.Id == kpC.Product).Select(x => x.Stock).FirstOrDefault();
                    }
                  }

                  kit.Stock = stock;

                  _context.Products.Update(kit);
                  _context.SaveChanges();
                }
              }
            }
          }
          
          Model.LineItem.Add(new ShoppingCartLineItemViewModel()
          {
            Id = item.Id,
            ProductId = item.Product,
            ImageSource = _context.Products.Where(
              x => x.Id.Equals(item.Product)).Select(x => x.ImagePathLarge).FirstOrDefault(),
            ProductName = _context.ProductOrderLineItems.Where(
              x => x.ProductOrder == order.Id).Where(
              x => x.Product == item.Product).Select(x => x.Name).FirstOrDefault(),
            ProductDescription = _context.ProductOrderLineItems.Where(
              x => x.ProductOrder == order.Id).Where(
              x => x.Product == item.Product).Select(x => x.Description).FirstOrDefault(),
            Quantity = item.Quantity,
            Total = item.Total
          });

          _context.SaveChanges();

          _context.ShoppingCartLineItems.Remove(item);
          _context.SaveChanges();
        }

        _context.UserLedgerTransactions.Add(new UserLedgerTransaction()
        {
          User = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
          ProductOrder = order.Id,
          TransactionType = 2, // Debit.
          Description = "Order #" + order.Id + ". Taxes.",
          Concept = 3, // Taxation.
          Amount = applicableTaxes,
          BalanceBeforeTransaction = balanceBeforeTransaction,
          BalanceAfterTransaction = balanceBeforeTransaction - applicableTaxes,
          CreatedBy = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
          Created = DateTime.UtcNow
        });

        _context.SaveChanges();

        balanceBeforeTransaction -= applicableTaxes;

        _context.UserLedgerTransactions.Add(new UserLedgerTransaction()
        {
          User = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
          ProductOrder = order.Id,
          TransactionType = 2, // Debit.
          Description = "Order #" + order.Id + ". Shipping.",
          Concept = 2, // Shipping.
          Amount = shippingCost,
          BalanceBeforeTransaction = balanceBeforeTransaction,
          BalanceAfterTransaction = balanceBeforeTransaction - shippingCost,
          CreatedBy = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
          Created = DateTime.UtcNow
        });

        _context.SaveChanges();

        balanceBeforeTransaction -= shippingCost;

        _context.UserLedgerTransactions.Add(new UserLedgerTransaction()
        {
          User = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
          ProductOrder = order.Id,
          TransactionType = 2, // Debit.
          Description = "Order #" + order.Id + ". Product.",
          Concept = 1, // Product.
          Amount = subTotal,
          BalanceBeforeTransaction = balanceBeforeTransaction,
          BalanceAfterTransaction = balanceBeforeTransaction - subTotal,
          CreatedBy = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
          Created = DateTime.UtcNow
        });

        _context.SaveChanges();

        balanceBeforeTransaction -= subTotal;

        _context.UserLedgerTransactions.Add(new UserLedgerTransaction()
        {
          User = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
          ProductOrder = order.Id,
          TransactionType = 1, // Credit.
          Description = "Order #" + order.Id + ". Payment.",
          Concept = 4, // Total.
          Amount = total,
          BalanceBeforeTransaction = balanceBeforeTransaction,
          BalanceAfterTransaction = balanceBeforeTransaction + total,
          CreatedBy = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
          Created = DateTime.UtcNow
        });

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
      if (!Object.Equals(Billing.Name, Shipping.Name))
        return false;
      if (!Object.Equals(Billing.Line1, Shipping.Line1))
        return false;
      if (!Object.Equals(Billing.Line2, Shipping.Line2))
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
  }
}
