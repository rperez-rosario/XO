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
using System.Text.Json;
using System.IO;
using System.Text;
using Stripe;
using System.Web;

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

    public IActionResult Index(CheckoutViewModel Model = null)
    {
      CheckoutViewModel checkoutViewModel = new CheckoutViewModel();
      List<ShoppingCartLineItemViewModel> lineItemViewModel = new List<ShoppingCartLineItemViewModel>();
      List<ShoppingCartLineItem> lineItem = _context.ShoppingCartLineItems.Where(
        x => x.ShoppingCart == _context.ShoppingCarts.Where(x => x.User.Equals(_context.AspNetUsers.Where(
        x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault()))
        .Select(x => x.Id).FirstOrDefault()).ToList();
      ORM.Address billingAddress = null;
      ORM.Address shippingAddress = null;
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
      checkoutViewModel.TotalWeightInPounds = totalOrderShippingWeightInPounds;
      ViewData["Country"] = new SelectList(new List<String> { "US" });
      ViewData["State"] = new SelectList(_context.StateUs.ToList(), "StateAbbreviation", "StateName");

      checkoutViewModel.Taxes = 0.0M; // TODO: IMPORTANT, GET FROM AvaTax API.
      checkoutViewModel.CodeDiscount = 0.0M; // TODO: CALCULATE.
      checkoutViewModel.CouponDiscount = 0.0M; // TODO: CALCULATE.
      checkoutViewModel.IsGift = false; // TODO: Map this.
      checkoutViewModel.ShippingCarrier = _option.Value.ShipEngineDefaultCarrier;
      checkoutViewModel.CarrierName = _option.Value.ShipEngineDefaultCarrierName;
      checkoutViewModel.ShippedOn = DateTime.UtcNow.TimeOfDay > new TimeSpan(10, 0, 0) ? // 5:00 PM PTDT.
         DateTime.UtcNow.AddDays(2) : DateTime.UtcNow.AddDays(1); // If after 4:00 PM PDT, two-days (2), else one (1). 
      checkoutViewModel.ExpectedToArrive = checkoutViewModel.ShippedOn.AddDays(3);
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
          checkoutViewModel.ShippingName = billingAddress.Name;
          checkoutViewModel.ShippingAddress1 = billingAddress.Line1;
          checkoutViewModel.ShippingAddress2 = billingAddress.Line2;
          checkoutViewModel.ShippingCity = billingAddress.CityName;
          checkoutViewModel.ShippingState = billingAddress.StateName;
          checkoutViewModel.ShippingCountry = billingAddress.CountryName;
          checkoutViewModel.ShippingPostalCode = billingAddress.PostalCode;
        }
      }

      ViewData.Add("Checkout.WelcomeText", _context.LocalizedTexts.Where(
       x => x.PlacementPointCode.Equals("Checkout.WelcomeText"))
       .Select(x => x.Text).FirstOrDefault());

      if (Model != null && Model.ShippingAddressDeclined)
        checkoutViewModel.ShippingAddressDeclined = true;

      checkoutViewModel.ShippingAddressSame = Model == null ? checkoutViewModel.ShippingAddressSame : Model.ShippingAddressSame;

      return View(checkoutViewModel);
    }

    public IActionResult CalculateShippingCostAndTaxes(CheckoutViewModel Model)
    {
      String seShipmentDetailsJson = null;
      String seShipmentCostJson = null;
      List<ShoppingCartLineItemViewModel> lineItemViewModel = new List<ShoppingCartLineItemViewModel>();
      List<ShoppingCartLineItem> lineItem = _context.ShoppingCartLineItems.Where(
        x => x.ShoppingCart == _context.ShoppingCarts.Where(x => x.User.Equals(_context.AspNetUsers.Where(
        x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault()))
        .Select(x => x.Id).FirstOrDefault()).ToList();
      decimal totalOrderShippingWeightInPounds = 0.0M;
      ViewData["Country"] = new SelectList(new List<String> { "US" });
      ViewData["State"] = new SelectList(_context.StateUs.ToList(), "StateAbbreviation", "StateName");

      Model.SubTotal = 0.0M;
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
        Model.SubTotal += _context.Prices.Where(
          x => x.Id == _context.Products.Where(
          x => x.Id == li.Product).Select(x => x.CurrentPrice).FirstOrDefault()).Select(
          x => x.Amount).FirstOrDefault() * li.Quantity;
        totalOrderShippingWeightInPounds += (decimal)(_context.Products.Where(
          x => x.Id == li.Product).Select(x => x.ShippingWeightLb).FirstOrDefault() * li.Quantity);
      }

      Model.LineItem = lineItemViewModel;
      
      ViewData.Add("Checkout.WelcomeText", _context.LocalizedTexts.Where(
       x => x.PlacementPointCode.Equals("Checkout.WelcomeText"))
       .Select(x => x.Text).FirstOrDefault());

      Model.ShippingCarrier = _option.Value.ShipEngineDefaultCarrier;
      
      if (!Model.CalculatedShippingAndTaxes)
      {
        try
        {
          var options = new JsonWriterOptions
          {
            Indented = true
          };

          using var stream = new MemoryStream();
          using var writer = new Utf8JsonWriter(stream, options);

          writer.WriteStartObject();
          writer.WriteStartObject("rate_options");
          writer.WriteStartArray("carrier_ids");
          writer.WriteStringValue(Model.ShippingCarrier);
          writer.WriteEndArray(); // carrier_ids.
          writer.WriteEndObject(); // rate_options.
          writer.WriteStartObject("shipment");
          writer.WritePropertyName("validate_address");
          writer.WriteStringValue("validate_and_clean");
          writer.WriteStartObject("ship_to");
          writer.WritePropertyName("name");
          writer.WriteStringValue(Model.ShippingAddressSame ? Model.BillingName : Model.ShippingName);
          writer.WritePropertyName("phone");
          writer.WriteStringValue(_context.AspNetUsers.Where(
            x => x.Email.Equals(User.Identity.Name)).Select(x => x.PhoneNumber).FirstOrDefault());
          writer.WritePropertyName("address_line1");
          writer.WriteStringValue(Model.ShippingAddressSame ? Model.BillingAddress1 : Model.ShippingAddress1);
          writer.WritePropertyName("address_line2");
          writer.WriteStringValue(Model.ShippingAddressSame ? Model.BillingAddress2 : Model.ShippingAddress2);
          writer.WritePropertyName("city_locality");
          writer.WriteStringValue(Model.ShippingAddressSame ? Model.BillingCity : Model.ShippingCity);
          writer.WritePropertyName("state_province");
          writer.WriteStringValue(Model.ShippingAddressSame ? Model.BillingState : Model.ShippingState);
          writer.WritePropertyName("postal_code");
          writer.WriteStringValue(Model.ShippingAddressSame ? Model.BillingPostalCode : Model.ShippingPostalCode);
          writer.WritePropertyName("country_code");
          writer.WriteStringValue(Model.ShippingAddressSame ? Model.BillingCountry : Model.ShippingCountry);
          writer.WriteEndObject(); // ship_to.
          writer.WriteStartObject("ship_from");
          writer.WritePropertyName("company_name");
          writer.WriteStringValue(_option.Value.ShipFromCompanyName);
          writer.WritePropertyName("name");
          writer.WriteStringValue(_option.Value.ShipFromName);
          writer.WritePropertyName("phone");
          writer.WriteStringValue(_option.Value.ShipFromPhone);
          writer.WritePropertyName("address_line1");
          writer.WriteStringValue(_option.Value.ShipFromAddressLine1);
          writer.WritePropertyName("city_locality");
          writer.WriteStringValue(_option.Value.ShipFromCity);
          writer.WritePropertyName("state_province");
          writer.WriteStringValue(_option.Value.ShipFromState);
          writer.WritePropertyName("postal_code");
          writer.WriteStringValue(_option.Value.ShipFromPostalCode);
          writer.WritePropertyName("country_code");
          writer.WriteStringValue(_option.Value.ShipFromCountryCode);
          writer.WriteEndObject(); // ship_from.
          writer.WriteStartArray("packages");
          writer.WriteStartObject();
          writer.WriteStartObject("weight");
          writer.WritePropertyName("value");
          writer.WriteNumberValue(Model.TotalWeightInPounds);
          writer.WritePropertyName("unit");
          writer.WriteStringValue("pound");
          writer.WriteEndObject(); // weight.
          writer.WriteEndObject(); // packages.
          writer.WriteEndArray(); // packages.
          writer.WriteEndObject(); // shipment.
          writer.WriteEndObject(); // root.

          writer.Flush();

          seShipmentDetailsJson = Encoding.UTF8.GetString(stream.ToArray());

          seShipmentCostJson = _option.Value.ShipEngineShippingCostUrl.PostJsonToUrlAsync(seShipmentDetailsJson,
            requestFilter: webReq =>
            {
              webReq.Headers["API-Key"] = _option.Value.ShipEngineApiKey;
            }).Result;

          using (JsonDocument document = JsonDocument.Parse(seShipmentCostJson))
          {
            JsonElement root = document.RootElement;
            JsonElement ratesElement = root.GetProperty("rate_response").GetProperty("rates");
            foreach (JsonElement rate in ratesElement.EnumerateArray())
            {
              if (rate.GetProperty("carrier_id").ValueEquals(_option.Value.ShipEngineDefaultCarrier) &&
                rate.GetProperty("rate_type").ValueEquals(_option.Value.ShipEngineDefaultRateType) &&
                rate.GetProperty("package_type").ValueEquals(_option.Value.ShipEngineDefaultPackageType) &&
                rate.GetProperty("service_code").ValueEquals(_option.Value.ShipEngineDefaultServiceCode))
              {
                Model.ShippingCharges = decimal.Parse(rate.GetProperty("shipping_amount").GetProperty("amount").ToString());
                Model.ShipEngineShipmentId = root.GetProperty("shipment_id").ToString();
                break;
              }
            }
          }
        }
        catch
        {
          Model.ShippingAddressDeclined = true;
          return RedirectToAction("Index", Model);
        }
      }

      Model.Total = Model.SubTotal + Model.Taxes + Model.ShippingCharges -
        Model.CodeDiscount - Model.CouponDiscount;

      Model.CalculatedShippingAndTaxes = true;
      
      return View("Index", Model);
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
      ShopifySharp.ProductService sProductService = null;
      ProductVariantService shProductVariantService = null;
      InventoryLevelService shInventoryLevelService = null;
      InventoryItemService shInventoryItemService = null;
      LocationService shLocationService = null;
      ShopifySharp.CustomerService shCustomerService = null;
      ShopifySharp.OrderService shOrderService = null;
      ShopifySharp.Product sProduct = null;
      ProductVariant shProductVariant = null;
      List<Location> shLocation = null;
      InventoryItem shInventoryItem = null;
      long originalProductStock = 0L;
      long originalKitStock = 0L;
      List<long> updatedKit = null;
      ShopifySharp.Order shOrder = null;
      List<ShopifySharp.LineItem> shLineItemList = null;
      String seShipmentRateJson = null;
      Stripe.ChargeService stChargeService = null;
      Stripe.CustomerService stCustomerService = null;
      SourceService stSourceService = null;
      TokenService stTokenService = null;
      ChargeCreateOptions stChargeCreateOptions = null;
      Dictionary<String, String> stCreditTransactionMetaValue = null;
      Stripe.CustomerCreateOptions stCustomerCreateOptions = null;
      Stripe.Customer stCustomer = null;
      CardCreateNestedOptions stCardCreateNestedOptions = null;
      Source stSource = null;
      SourceCreateOptions stSourceCreateOptions = null;
      TokenCreateOptions stTokenCreateOptions = null;
      Token stToken = null;
      TokenCardOptions stTokenCardOptions = null;
      AspNetUser xoUser = null;
      Stripe.Charge stCharge = null;
      String geoLocationUrl = null;
      String geoLocationJson = null;
      

      if (_context.ShoppingCartLineItems.Where(
          x => x.ShoppingCart == _context.ShoppingCarts.Where(
          x => x.User.Equals(_context.AspNetUsers.Where(
          x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault())).Select(
            x => x.Id).FirstOrDefault()).Count<ShoppingCartLineItem>() == 0)
      {
        return RedirectToAction("Index", "Home");
      }

      try
      {
        sProductService = new ShopifySharp.ProductService(_option.Value.ShopifyUrl,
            _option.Value.ShopifyStoreFrontAccessToken);
        shProductVariantService = new ProductVariantService(_option.Value.ShopifyUrl,
          _option.Value.ShopifyStoreFrontAccessToken);
        shInventoryLevelService = new InventoryLevelService(_option.Value.ShopifyUrl,
          _option.Value.ShopifyStoreFrontAccessToken);
        shLocationService = new LocationService(_option.Value.ShopifyUrl,
          _option.Value.ShopifyStoreFrontAccessToken);
        shInventoryItemService = new InventoryItemService(_option.Value.ShopifyUrl,
          _option.Value.ShopifyStoreFrontAccessToken);
        shOrderService = new ShopifySharp.OrderService(_option.Value.ShopifyUrl,
          _option.Value.ShopifyStoreFrontAccessToken);
        shCustomerService = new ShopifySharp.CustomerService(_option.Value.ShopifyUrl,
          _option.Value.ShopifyStoreFrontAccessToken);
      }
      catch (Exception ex)
      {
        throw new Exception("An error was encountered while initializing Shopify services.", ex);
      }

      if (Model.OrderId == 0)
      {
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

        Model.OrderId = order.Id;
      }
      else
      {
        order = _context.ProductOrders.Where(x => x.Id == Model.OrderId).FirstOrDefault();
      }

      try
      {
        shLineItemList = new List<ShopifySharp.LineItem>();

        foreach (ShoppingCartLineItem cli in _context.ShoppingCartLineItems.Where(
          x => x.ShoppingCart == _context.ShoppingCarts.Where(
          x => x.User.Equals(_context.AspNetUsers.Where(
          x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault())).Select(x => x.Id).FirstOrDefault()).ToList())
        {
          subTotal += _context.Prices.Where(
            x => x.Id == _context.Products.Where(
              x => x.Id == cli.Product).Select(x => x.CurrentPrice).FirstOrDefault()).Select(x => x.Amount).FirstOrDefault() *
              cli.Quantity;
          
          shLineItemList.Add(new ShopifySharp.LineItem()
          {
            ProductId = _context.Products.Where(x => x.Id == cli.Product).Select(x => x.ShopifyProductId).FirstOrDefault(),
            VariantId = sProductService.GetAsync((long)_context.Products.Where(
              x => x.Id == cli.Product).Select(x => x.ShopifyProductId).FirstOrDefault()).Result.Variants.First().Id,
            Quantity = cli.Quantity,
            Taxable = true,
            RequiresShipping = true
          });
        }

        try
        {
          shOrder = new ShopifySharp.Order()
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

          seShipmentRateJson = (_option.Value.ShipEngineGetShipmentCostFromIdPrefixUrl + 
            Model.ShipEngineShipmentId + _option.Value.ShipEngineGetShipmentCostFromIdPostfixUrl).GetJsonFromUrl(
            requestFilter: webReq =>
            {
              webReq.Headers["API-Key"] = _option.Value.ShipEngineApiKey;
            }, responseFilter: null);

          using (JsonDocument document = JsonDocument.Parse(seShipmentRateJson))
          {
            JsonElement root = document.RootElement;
            JsonElement ratesElement = root.EnumerateArray().ElementAt(0).GetProperty("rates");
            foreach (JsonElement rate in ratesElement.EnumerateArray())
            {
              if (rate.GetProperty("carrier_id").ValueEquals(_option.Value.ShipEngineDefaultCarrier) &&
                rate.GetProperty("rate_type").ValueEquals(_option.Value.ShipEngineDefaultRateType) &&
                rate.GetProperty("package_type").ValueEquals(_option.Value.ShipEngineDefaultPackageType) &&
                rate.GetProperty("service_code").ValueEquals(_option.Value.ShipEngineDefaultServiceCode))
              {
                Model.ShippingCharges = decimal.Parse(rate.GetProperty("shipping_amount").GetProperty("amount").ToString());
                break;
              }
            }
          }

          Model.ShippingCarrier = _option.Value.ShipEngineDefaultCarrierName;
          Model.BilledOn = DateTime.UtcNow;

          shippingCost = (decimal)Model.ShippingCharges;
          codeDiscount = 0.0M; // TODO: CALCULATE.
          couponDiscount = 0.0M; // TODO : CALCULATE.
          applicableTaxes = 0.0M; // TODO: IMPORTANT, GET FROM AvaTax.

          total = subTotal + shippingCost - codeDiscount - couponDiscount + applicableTaxes;

          try
          {
            // Credit card processing.
            Stripe.StripeConfiguration.ApiKey = _option.Value.StripeSecretKey;

            if (_context.AspNetUsers.Where(
              x => x.Email.Equals(User.Identity.Name)).Select(
              x => x.StripeCustomerId).FirstOrDefault() == null)
            {
              try
              {
                stCardCreateNestedOptions = new CardCreateNestedOptions()
                {
                  AddressCity = Model.BillingCity,
                  AddressCountry = Model.BillingCountry,
                  AddressLine1 = Model.BillingAddress1,
                  AddressLine2 = Model.BillingAddress2,
                  AddressState = Model.BillingState,
                  AddressZip = Model.BillingPostalCode,
                  Cvc = Model.CreditCardCVC,
                  ExpMonth = Model.CreditCardExpirationDate.Month,
                  ExpYear = Model.CreditCardExpirationDate.Year,
                  Name = Model.BillingName,
                  Number = Model.CreditCardNumber
                };
                stCustomerCreateOptions = new Stripe.CustomerCreateOptions();
                stCustomerCreateOptions.Email = User.Identity.Name;
                stCustomerCreateOptions.Source = stCardCreateNestedOptions;
                stCustomerCreateOptions.Description = "XO Skin Customer.";
                stCustomerService = new Stripe.CustomerService();
                stCustomer = stCustomerService.Create(stCustomerCreateOptions);

                stCustomerCreateOptions = null;
                Model.CreditCardNumber = null;
                Model.CreditCardCVC = null;
                Model.CreditCardExpirationDate = DateTime.MinValue;
                
                xoUser = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).FirstOrDefault();
                xoUser.StripeCustomerId = stCustomer.Id;
                _context.AspNetUsers.Update(xoUser);
                _context.SaveChanges();
              }
              catch
              {
                Model.CardDeclined = true;
                Model.CalculatedShippingAndTaxes = true;
                return RedirectToAction("CalculateShippingCostAndTaxes", Model);
              }
            }
            else
            {
              stCustomerService = new Stripe.CustomerService();
              stCustomer = stCustomerService.Get(_context.AspNetUsers.Where(
                x => x.Email.Equals(User.Identity.Name)).Select(x => x.StripeCustomerId).FirstOrDefault());

              stTokenCardOptions = new TokenCardOptions()
              {
                AddressCity = Model.BillingCity,
                AddressCountry = Model.BillingCountry,
                AddressLine1 = Model.BillingAddress1,
                AddressLine2 = Model.BillingAddress2,
                AddressState = Model.BillingState,
                AddressZip = Model.BillingPostalCode,
                Cvc = Model.CreditCardCVC,
                ExpMonth = Model.CreditCardExpirationDate.Month,
                ExpYear = Model.CreditCardExpirationDate.Year,
                Name = Model.BillingName,
                Number = Model.CreditCardNumber
              };
              stTokenCreateOptions = new TokenCreateOptions()
              {
                Card = stTokenCardOptions
              };
              stTokenService = new TokenService();
              stToken = stTokenService.Create(stTokenCreateOptions);
              
              stTokenCreateOptions = null;
              Model.CreditCardNumber = null;
              Model.CreditCardCVC = null;
              Model.CreditCardExpirationDate = DateTime.MinValue;
              
              stSourceCreateOptions = new SourceCreateOptions()
              {
                Token = stToken.Id,
                Type = SourceType.Card
              };

              stToken = null;
              
              stSourceService = new SourceService();
              stSource = await stSourceService.CreateAsync(stSourceCreateOptions);
              try
              {
                stSourceService.Attach(stCustomer.Id, new SourceAttachOptions()
                {
                  Source = stSource.Id
                });
              }
              catch
              {
                Model.CardDeclined = true;
                Model.CalculatedShippingAndTaxes = true;
                return RedirectToAction("CalculateShippingCostAndTaxes", Model);
              }
            }

            stCreditTransactionMetaValue = new Dictionary<string, string>();
            stCreditTransactionMetaValue.Add("ProductOrderId", order.Id.ToString());
            stCreditTransactionMetaValue.Add("UserIdentityName", User.Identity.Name);

            stChargeCreateOptions = new ChargeCreateOptions()
            {
              Amount = (long?)total * 100,
              Currency = "usd",
              Description = "Total charges for an XO Skin customer order #XO" + (order.Id + 10000).ToString() +
                ". Customer: " + User.Identity.Name + ".",
              Metadata = stCreditTransactionMetaValue,
              ReceiptEmail = User.Identity.Name,
              Customer = stCustomer.Id
            };

            stChargeService = new Stripe.ChargeService();
            stCharge = stChargeService.Create(stChargeCreateOptions);

            if (stCharge.Status.Equals("succeeded"))
            {
              order.StripeChargeId = stCharge.Id;
              order.StripeChargeStatus = stCharge.Status;
            }
            else
            {
              Model.CardDeclined = true;
              Model.CalculatedShippingAndTaxes = true;
              return RedirectToAction("CalculateShippingCostAndTaxes", Model);
            }
            
            stCustomer = stCustomerService.Get(_context.AspNetUsers.Where(
              x => x.Email.Equals(User.Identity.Name)).Select(x => x.StripeCustomerId).FirstOrDefault());

            if (stCustomer.Sources != null)
            {
              stSourceService = new SourceService();
              foreach (Source s in stCustomer.Sources)
              {
                stSourceService.Detach(stCustomer.Id, s.Id);
              }
            }
          }
          catch
          {
            Model.CardDeclined = true;
            Model.CalculatedShippingAndTaxes = true;
            return RedirectToAction("CalculateShippingCostAndTaxes", Model);
          }
          

          // TODO: IMPORTANT GET TAXES FROM AvaTax.
          shOrder.ShippingLines = new List<ShippingLine>()
          {
            new ShippingLine()
            {
              PriceSet = new PriceSet()
              {
                PresentmentMoney = new ShopifySharp.Price()
                {
                  Amount = (decimal)Model.ShippingCharges,
                  CurrencyCode = "usd"
                },
                ShopMoney = new ShopifySharp.Price()
                {
                  Amount = (decimal)Model.ShippingCharges,
                  CurrencyCode = "usd"
                }
              },
              Title = _option.Value.ShopifyShippingLineTitle,
              Code = _option.Value.ShopifyShippingLineCode,
              Source = _option.Value.ShopifyShippingLineSource,
              Price = (decimal)Model.ShippingCharges
            }
          };
          shOrder.Name = "#XO" + (order.Id + 10000).ToString();
          shOrder.OrderStatusUrl = _option.Value.ShopifyOrderStatusUrl + order.Id.ToString();
          shOrder.CreatedAt = DateTime.UtcNow;
          shOrder.LineItems = shLineItemList;
          shOrder.Test = false;
          shOrder.TaxesIncluded = false;
          shOrder.Test = false;  // Switch to "true" for testing against a production store.
          shOrder.Customer = await shCustomerService.GetAsync((long)_context.AspNetUsers.Where(
            x => x.Email.Equals(User.Identity.Name)).Select(x => x.ShopifyCustomerId).FirstOrDefault());

          shOrder = await shOrderService.CreateAsync(shOrder);
          order.ShopifyId = shOrder.Id;
        }
        catch (Exception ex)
        {
          throw new Exception("An error was encountered while writing order to Shopify.", ex);
        }

        order.DatePlaced = DateTime.UtcNow;
        order.Subtotal = subTotal;
        order.ApplicableTaxes = applicableTaxes;
        order.CodeDiscount = codeDiscount;
        order.CouponDiscount = couponDiscount;
        order.GiftOrder = Model.IsGift;
        order.ShippingCost = shippingCost;
        order.Total = total;
        order.Completed = true;

        _context.ProductOrders.Update(order);

        _context.SaveChanges();
      }
      catch (Exception ex)
      {
        throw new Exception("An error was encontered while initializing the product order.", ex);
      }

      Model.OrderId = order.Id;
      Model.ShopifyId = (long)shOrder.Id;
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
              shProductVariant = await shProductVariantService.GetAsync((long)sProduct.Variants.First().Id);
              shInventoryItem = await shInventoryItemService.GetAsync((long)shProductVariant.InventoryItemId);
              shLocation = (List<Location>)await shLocationService.ListAsync();
              
              await shInventoryLevelService.AdjustAsync(new InventoryLevelAdjust()
              {
                AvailableAdjustment = (int?)(-1 * item.Quantity),
                InventoryItemId = shInventoryItem.Id,
                LocationId = shLocation.First().Id
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
                    shProductVariant = await shProductVariantService.GetAsync((long)sProduct.Variants.First().Id);
                    shInventoryItem = await shInventoryItemService.GetAsync((long)shProductVariant.InventoryItemId);
                    shLocation = (List<Location>)await shLocationService.ListAsync();

                    await shInventoryLevelService.AdjustAsync(new InventoryLevelAdjust()
                    {
                      AvailableAdjustment = (int?)(kit.Stock - originalKitStock),
                      InventoryItemId = shInventoryItem.Id,
                      LocationId = shLocation.First().Id
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
              product.Stock -= 1 * item.Quantity;
              _context.Products.Update(product);
              _context.SaveChanges();

              try
              {
                sProduct = await sProductService.GetAsync((long)product.ShopifyProductId);
                shProductVariant = await shProductVariantService.GetAsync((long)sProduct.Variants.First().Id);
                shInventoryItem = await shInventoryItemService.GetAsync((long)shProductVariant.InventoryItemId);
                shLocation = (List<Location>)await shLocationService.ListAsync();
                
                await shInventoryLevelService.AdjustAsync(new InventoryLevelAdjust()
                {
                  AvailableAdjustment = -1 * item.Quantity,
                  InventoryItemId = shInventoryItem.Id,
                  LocationId = shLocation.First().Id
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
                      shProductVariant = await shProductVariantService.GetAsync((long)sProduct.Variants.First().Id);
                      shInventoryItem = await shInventoryItemService.GetAsync((long)shProductVariant.InventoryItemId);
                      shLocation = (List<Location>)await shLocationService.ListAsync();

                      await shInventoryLevelService.AdjustAsync(new InventoryLevelAdjust()
                      {
                        AvailableAdjustment = (int?)(kit.Stock - originalKitStock),
                        InventoryItemId = shInventoryItem.Id,
                        LocationId = shLocation.First().Id
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

          _context.SaveChanges();

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
            CarrierName = Model.CarrierName,
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
            CarrierName = Model.CarrierName,
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

      try
      {
        geoLocationUrl = new string(_option.Value.BingMapsGeolocationUrl)
        .Replace("{adminDistrict}", Model.ShippingAddressSame ? Model.BillingState : Model.ShippingState)
        .Replace("{postalCode}", Model.ShippingAddressSame ? Model.BillingPostalCode.Trim() : Model.ShippingPostalCode.Trim())
        .Replace("{locality}", Model.ShippingAddressSame ? Model.BillingCity.Trim() : Model.ShippingCity.Trim())
        .Replace("{addressLine}", Model.ShippingAddressSame ? Model.BillingAddress1.Trim() +
          (Model.BillingAddress2 == null ? String.Empty : (" " + Model.BillingAddress2.Trim())) :
          Model.ShippingAddress1.Trim() + (Model.ShippingAddress2 == null ? String.Empty : (" " + Model.ShippingAddress2.Trim())))
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
          Model.ShippingLatitude = Decimal.Parse(resourcePointCoordinates[0].ToString());
          Model.ShippingLongitude = Decimal.Parse(resourcePointCoordinates[1].ToString());
        }
      }
      catch
      {
        // Address was not found. Fail silently and continue. Geolocation will not be displayed for this address.
      }

      Model.GoogleMapsUrl = _option.Value.GoogleMapsUrl;

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
