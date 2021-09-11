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
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceStack;
using Newtonsoft.Json;
using System.Text.Json;

namespace XOSkinWebApp.Controllers
{
  [Authorize]
  public class CheckoutController : Controller
  {
    private readonly XOSkinContext _context;
    private readonly IOptions<Option> _option;

    public CheckoutController(XOSkinContext context, IOptions<Option> option)
    {
      _context = context;
      _option = option;
    }

    public async Task<IActionResult> Index()
    {
      CheckoutViewModel checkoutViewModel = new CheckoutViewModel();
      List<ShoppingCartLineItemViewModel> lineItemViewModel = new List<ShoppingCartLineItemViewModel>();
      List<ShoppingCartLineItem> lineItem = _context.ShoppingCartLineItems.Where(
        x => x.ShoppingCart == _context.ShoppingCarts.Where(x => x.User.Equals(_context.AspNetUsers.Where(
        x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault()))
        .Select(x => x.Id).FirstOrDefault()).ToList();
      ORM.Address billingAddress = null;
      ORM.Address shippingAddress = null;
      List<Models.Carrier> seCarrierList = null;
      String seCarrierJson = null;
      decimal totalOrderShippingWeightInPounds = 0.0M;

      checkoutViewModel.SubTotal = 0.0M;
      totalOrderShippingWeightInPounds = 0.0M;

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
          Total = _context.Prices.Where(
            x => x.Id == _context.Products.Where(
            x => x.Id == li.Product).Select(x => x.CurrentPrice).FirstOrDefault()).Select(
            x => x.Amount).FirstOrDefault() * li.Quantity
        });
        checkoutViewModel.SubTotal += _context.Prices.Where(
          x => x.Id == _context.Products.Where(
          x => x.Id == li.Product).Select(x => x.CurrentPrice).FirstOrDefault()).Select(
          x => x.Amount).FirstOrDefault() * li.Quantity;
        totalOrderShippingWeightInPounds += (decimal)(_context.Products.Where(
          x => x.Id == li.Product).Select(x => x.ShippingWeightLb).FirstOrDefault() * li.Quantity);
      }

      checkoutViewModel.LineItem = lineItemViewModel;
      checkoutViewModel.CreditCardExpirationDate = DateTime.Now;

      try
      {
        // TODO: Integrate ShipEngine API calls here.
        seCarrierJson = _option.Value.ShipEngineCarriersUrl.GetJsonFromUrl(requestFilter: webReq =>
         {
           webReq.Headers["API-Key"] = _option.Value.ShipEngineApiKey;
         });

        seCarrierList = new List<Models.Carrier>();

        using (JsonDocument seCarrier = JsonDocument.Parse(seCarrierJson))
        {
          JsonElement root = seCarrier.RootElement;
          JsonElement carriersElement = root.GetProperty("carriers");
          foreach (JsonElement carrier in carriersElement.EnumerateArray())
          {
            if (carrier.TryGetProperty("carrier_id", out JsonElement idElement))
            {

              seCarrierList.Add(new Models.Carrier()
              {
                Id = carrier.GetProperty("carrier_id").ToString(),
                Name = carrier.GetProperty("friendly_name").ToString()
              });
            }
          }
        }
        
        ViewData["Carrier"] = new SelectList(seCarrierList, "Id", "Name", null);
      }
      catch (Exception ex)
      {
        throw new Exception("An error was encountered while setting shipping information.", ex);
      }

      checkoutViewModel.Taxes = 0.0M; // TODO: IMPORTANT, GET FROM AvaTax API.
      checkoutViewModel.ShippingCharges = 0.0M; // TODO: IMPORTANT, GET FROM ShipEngine API.
      checkoutViewModel.CodeDiscount = 0.0M; // TODO: CALCULATE.
      checkoutViewModel.CouponDiscount = 0.0M; // TODO: CALCULATE.
      checkoutViewModel.IsGift = false; // TODO: Map this.
      checkoutViewModel.ShippingCarrier = "UPS"; // TODO: IMPORTANT, GET FROM ShipEngine API.
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

    public async Task<IActionResult> PlaceOrder(CheckoutViewModel Model)
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
      ProductService sProductService = null;
      ProductVariantService sProductVariantService = null;
      InventoryLevelService sInventoryLevelService = null;
      InventoryItemService sInventoryItemService = null;
      LocationService sLocationService = null;
      CustomerService sCustomerService = null;
      OrderService sOrderService = null;
      ShopifySharp.Product sProduct = null;
      ProductVariant sProductVariant = null;
      List<Location> sLocation = null;
      InventoryItem sInventoryItem = null;
      long originalProductStock = 0L;
      long originalKitStock = 0L;
      List<long> updatedKit = null;
      Order sOrder = null;
      List<LineItem> sLineItemList = null;

      try
      {
        sProductService = new ProductService(_option.Value.ShopifyUrl,
            _option.Value.ShopifyStoreFrontAccessToken);
        sProductVariantService = new ProductVariantService(_option.Value.ShopifyUrl,
          _option.Value.ShopifyStoreFrontAccessToken);
        sInventoryLevelService = new InventoryLevelService(_option.Value.ShopifyUrl,
          _option.Value.ShopifyStoreFrontAccessToken);
        sLocationService = new LocationService(_option.Value.ShopifyUrl,
          _option.Value.ShopifyStoreFrontAccessToken);
        sInventoryItemService = new InventoryItemService(_option.Value.ShopifyUrl,
          _option.Value.ShopifyStoreFrontAccessToken);
        sOrderService = new OrderService(_option.Value.ShopifyUrl,
          _option.Value.ShopifyStoreFrontAccessToken);
        sCustomerService = new CustomerService(_option.Value.ShopifyUrl,
          _option.Value.ShopifyStoreFrontAccessToken);
      }
      catch (Exception ex)
      {
        throw new Exception("An error was encountered while initializing Shopify services.", ex);
      }

      Model.LineItem = new List<ShoppingCartLineItemViewModel>();
      order = new ProductOrder()
      {
        User = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
        DatePlaced = DateTime.MaxValue,
        Subtotal = 0.0M,
        CouponDiscount = 0.0M,
        CodeDiscount = 0.0M,
        ApplicableTaxes = 0.0M,
        Total = 0.0M,
        GiftOrder = false
      };

      _context.ProductOrders.Add(order);
      _context.SaveChanges();

      try
      {

        sLineItemList = new List<LineItem>();

        foreach (ShoppingCartLineItem cli in _context.ShoppingCartLineItems.Where(
          x => x.ShoppingCart == _context.ShoppingCarts.Where(
          x => x.User.Equals(_context.AspNetUsers.Where(
          x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault())).Select(x => x.Id).FirstOrDefault()).ToList())
        {
          subTotal += _context.Prices.Where(
            x => x.Id == _context.Products.Where(
              x => x.Id == cli.Product).Select(x => x.CurrentPrice).FirstOrDefault()).Select(x => x.Amount).FirstOrDefault() *
              cli.Quantity;
          
          if (_context.Products.Where(x => x.Id == cli.Product).Select(x => x.KitType).FirstOrDefault() == null)
          {
            sLineItemList.Add(new LineItem()
            {
              ProductId = _context.Products.Where(x => x.Id == cli.Product).Select(x => x.ShopifyProductId).FirstOrDefault(),
              VariantId = sProductService.GetAsync((long)_context.Products.Where(
                x => x.Id == cli.Product).Select(x => x.ShopifyProductId).FirstOrDefault()).Result.Variants.First().Id,
              Quantity = cli.Quantity,
              Taxable = true,
              RequiresShipping = true
            });
          }
          else
          {
            // TODO: Add Kit products.
          }
        }

        try
        {
          sOrder = new Order()
          {
            BillingAddress = new ShopifySharp.Address()
            {
              Address1 = Model.BillingAddress1,
              Address2 = Model.BillingAddress2,
              City = Model.BillingCity,
              Province = Model.BillingState,
              Zip = Model.BillingPostalCode,
              Phone = _context.AspNetUsers.Where(
                x => x.Email.Equals(User.Identity.Name)).Select(x => x.PhoneNumber).FirstOrDefault(),
              Name = Model.BillingName,
              Country = Model.BillingCountry
            },
            ShippingAddress = new ShopifySharp.Address()
            {
              Address1 = Model.ShippingAddressSame ? Model.BillingAddress1 : Model.ShippingAddress1,
              Address2 = Model.ShippingAddressSame ? Model.BillingAddress2 : Model.ShippingAddress2,
              City = Model.ShippingAddressSame ? Model.BillingCity : Model.ShippingCity,
              Province = Model.ShippingAddressSame ? Model.BillingState : Model.ShippingState,
              Zip = Model.ShippingAddressSame ? Model.BillingPostalCode : Model.ShippingPostalCode,
              Phone = _context.AspNetUsers.Where(
                x => x.Email.Equals(User.Identity.Name)).Select(x => x.PhoneNumber).FirstOrDefault(),
              Name = Model.ShippingAddressSame ? Model.BillingName : Model.ShippingName,
              Country = Model.ShippingAddressSame ? Model.BillingCountry : Model.ShippingCountry
            }
          };

          // TODO: Set taxes From Model.
          sOrder.Name = "#XO-10" + order.Id.ToString();
          sOrder.OrderStatusUrl = "https://xoskinqatest.azurewebsites.net/Orders/Detail/" + order.Id.ToString();
          sOrder.CreatedAt = DateTime.UtcNow;
          sOrder.LineItems = sLineItemList;
          sOrder.Test = false;
          sOrder.TaxesIncluded = false;
          sOrder.Test = false;  // Switch to "true" for testing against a production store.
          sOrder.Customer = await sCustomerService.GetAsync((long)_context.AspNetUsers.Where(
            x => x.Email.Equals(User.Identity.Name)).Select(x => x.ShopifyCustomerId).FirstOrDefault());

          sOrder = await sOrderService.CreateAsync(sOrder);
        }
        catch (Exception ex)
        {
          throw new Exception("An error was encountered while writing order to Shopify.", ex);
        }

        Model.ShippingCarrier = Model.ShippingCarrier;
        Model.TrackingNumber = "1000000000000000000001"; // TODO: IMPORTANT, GET FROM ShipEngine.
        Model.ShippedOn = DateTime.UtcNow > DateTime.UtcNow.AddHours(10) ?
          DateTime.UtcNow : DateTime.UtcNow.AddDays(1); // 5:00 PM PTDT.
        Model.ExpectedToArrive = new DateTime(9999, 12, 31); // TODO: IMPORTANT, GET FROM ShipEngine.
        Model.BilledOn = DateTime.UtcNow;

        shippingCost = 0.0M; // TODO: IMPORTANT, GET FROM ShipEngine.
        codeDiscount = 0.0M; // TODO: CALCULATE.
        couponDiscount = 0.0M; // TODO : CALCULATE.
        applicableTaxes = 0.0M; // TODO: IMPORTANT, GET FROM AvaTax.
        
        total = subTotal + shippingCost - codeDiscount - couponDiscount  + applicableTaxes;

        order.DatePlaced = DateTime.UtcNow;
        order.Subtotal = subTotal;
        order.ApplicableTaxes = applicableTaxes;
        order.CodeDiscount = codeDiscount;
        order.CouponDiscount = couponDiscount;
        order.GiftOrder = Model.IsGift;
        order.ShippingCost = shippingCost;
        order.Total = total;

        _context.ProductOrders.Update(order);

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
            Total = _context.Prices.Where(
              x => x.Id == _context.Products.Where(
              x => x.Id == item.Product).Select(x => x.CurrentPrice).FirstOrDefault()).Select(
              x => x.Amount).FirstOrDefault() * item.Quantity
          });

          _context.SaveChanges();

          if (_context.Products.Where(x => x.Id == item.Product).Select(x => x.KitType).FirstOrDefault() == null)
          {
            product = _context.Products.Where(x => x.Id == item.Product).FirstOrDefault();
            originalProductStock = (long)product.Stock;
            product.Stock -= item.Quantity;
            _context.Products.Update(product);
            _context.SaveChanges();

            try
            {
              sProduct = await sProductService.GetAsync((long)product.ShopifyProductId);
              sProductVariant = await sProductVariantService.GetAsync((long)sProduct.Variants.First().Id);
              sInventoryItem = await sInventoryItemService.GetAsync((long)sProductVariant.InventoryItemId);
              sLocation = (List<Location>)await sLocationService.ListAsync();
              
              await sInventoryLevelService.AdjustAsync(new InventoryLevelAdjust()
              {
                AvailableAdjustment = (int?)(-1 * item.Quantity),
                InventoryItemId = sInventoryItem.Id,
                LocationId = sLocation.First().Id
              });
            }
            catch (Exception ex)
            {
              throw new Exception("An error was encountered while updating Shopify product inventory levels.", ex);
            }

            updatedKit = new List<long>();

            foreach (KitProduct kp in _context.KitProducts.ToList())
            {
              if (kp.Product == product.Id)
              {
                kit = _context.Products.Where(x => x.Id == kp.Kit).FirstOrDefault();

                if (!updatedKit.Any(x => x.Equals(kit.Id)))
                {
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

                  originalKitStock = (long)kit.Stock;
                  kit.Stock = stock;

                  _context.Products.Update(kit);
                  _context.SaveChanges();

                  try
                  {
                    sProduct = await sProductService.GetAsync((long)kit.ShopifyProductId);
                    sProductVariant = await sProductVariantService.GetAsync((long)sProduct.Variants.First().Id);
                    sInventoryItem = await sInventoryItemService.GetAsync((long)sProductVariant.InventoryItemId);
                    sLocation = (List<Location>)await sLocationService.ListAsync();

                    await sInventoryLevelService.AdjustAsync(new InventoryLevelAdjust()
                    {
                      AvailableAdjustment = (int?)(kit.Stock - originalKitStock),
                      InventoryItemId = sInventoryItem.Id,
                      LocationId = sLocation.First().Id
                    });

                    updatedKit.Add(kit.Id);
                  }
                  catch (Exception ex)
                  {
                    throw new Exception("An error was encountered while updating Shopify kit inventory levels.", ex);
                  }
                }
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

              try
              {
                sProduct = await sProductService.GetAsync((long)product.ShopifyProductId);
                sProductVariant = await sProductVariantService.GetAsync((long)sProduct.Variants.First().Id);
                sInventoryItem = await sInventoryItemService.GetAsync((long)sProductVariant.InventoryItemId);
                sLocation = (List<Location>)await sLocationService.ListAsync();
                
                await sInventoryLevelService.AdjustAsync(new InventoryLevelAdjust()
                {
                  AvailableAdjustment = -1,
                  InventoryItemId = sInventoryItem.Id,
                  LocationId = sLocation.First().Id
                });
              }
              catch (Exception ex)
              {
                throw new Exception("An error was encountered while updating Shopify kit-product inventory levels.", ex);
              }
            }

            updatedKit = new List<long>();

            foreach (KitProduct kp in kitProduct)
            {
              foreach (KitProduct kpB in _context.KitProducts.ToList())
              {
                if (kpB.Product == kp.Product)
                {
                  kit = _context.Products.Where(x => x.Id == kpB.Kit).FirstOrDefault();

                  if (!updatedKit.Any(x => x.Equals(kit.Id)))
                  {
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

                    originalKitStock = (long)kit.Stock;
                    kit.Stock = stock;

                    try
                    {
                      sProduct = await sProductService.GetAsync((long)kit.ShopifyProductId);
                      sProductVariant = await sProductVariantService.GetAsync((long)sProduct.Variants.First().Id);
                      sInventoryItem = await sInventoryItemService.GetAsync((long)sProductVariant.InventoryItemId);
                      sLocation = (List<Location>)await sLocationService.ListAsync();

                      await sInventoryLevelService.AdjustAsync(new InventoryLevelAdjust()
                      {
                        AvailableAdjustment = (int?)(kit.Stock - originalKitStock),
                        InventoryItemId = sInventoryItem.Id,
                        LocationId = sLocation.First().Id
                      });
                    }
                    catch (Exception ex)
                    {
                      throw new Exception("An error was encountered while updating Shopify kit inventory levels.", ex);
                    }

                    updatedKit.Add(kit.Id);

                    _context.Products.Update(kit);
                    _context.SaveChanges();
                  }
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
            Total = _context.Prices.Where(
              x => x.Id == _context.Products.Where(
              x => x.Id == item.Product).Select(x => x.CurrentPrice).FirstOrDefault()).Select(
              x => x.Amount).FirstOrDefault() * item.Quantity
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
