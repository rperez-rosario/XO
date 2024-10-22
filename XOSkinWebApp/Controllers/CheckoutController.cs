﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using System.IO;
using System.Text;
using System.Web;
using ShopifySharp;
using Stripe;
using Taxjar;
using ServiceStack;
using XOSkinWebApp.ConfigurationHelper;
using XOSkinWebApp.Models;
using XOSkinWebApp.ORM;
using XOSkinWebApp.Areas.Administration.Models;
using EllipticCurve.Utils;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;
using Stripe.Issuing;

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

    public async Task<IActionResult> Index(Models.CheckoutViewModel Model = null)
    {
      Models.CheckoutViewModel checkoutViewModel = new Models.CheckoutViewModel();
      List<Models.ShoppingCartLineItemViewModel> lineItemViewModel = new List<Models.ShoppingCartLineItemViewModel>();
      List<ShoppingCartLineItem> lineItem = _context.ShoppingCartLineItems.Where(
        x => x.ShoppingCart == _context.ShoppingCarts.Where(x => x.User.Equals(_context.AspNetUsers.Where(
        x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault()))
        .Select(x => x.Id).FirstOrDefault()).ToList();
      ORM.Address billingAddress = null;
      ORM.Address shippingAddress = null;
      decimal totalOrderShippingWeightInPounds = 0.0M;
      List<SelectListItem> discountCoupon = null;
      List<SelectListItem> discountCouponToRemove = null;
      DiscountCoupon coupon = null;
      List<DiscountCouponProduct> couponProduct = null;
      decimal percentage = decimal.MinValue;
      short minimumNumberOfProducts = short.MinValue;
      decimal minimumPurchase = decimal.MinValue;
      decimal selectedProductSubTotal = 0.0M;
      int totalNumberOfProducts = 0;

      checkoutViewModel.SubTotal = 0.0M;
      totalOrderShippingWeightInPounds = 0.0M;

      foreach (ShoppingCartLineItem li in lineItem)
      {
        lineItemViewModel.Add(new Models.ShoppingCartLineItemViewModel()
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
        totalNumberOfProducts += li.Quantity;
      }

      discountCoupon = new SelectList(_context.DiscountCoupons.Where(
        x => x.Active).Where(
        x => x.ValidFrom <= DateTime.UtcNow).Where(
        x => x.ValidTo >= DateTime.UtcNow), "Id", "Name").ToList();

      discountCouponToRemove = new List<SelectListItem>();

      foreach (SelectListItem item in discountCoupon)
      {
        coupon = await _context.DiscountCoupons.FindAsync(long.Parse(item.Value));
        couponProduct = _context.DiscountCouponProducts.Where(x => x.Coupon == coupon.Id).ToList();

        percentage = coupon.DiscountNproductPercentage == null ? 0.0M : (decimal)coupon.DiscountNproductPercentage;
        minimumNumberOfProducts = coupon.DiscountProductN == null ? (short)0 : (short)coupon.DiscountProductN;
        minimumPurchase = coupon.MinimumPurchase == null ? 0.0M : (decimal)coupon.MinimumPurchase;
        if (checkoutViewModel.SubTotal < minimumPurchase)
        {
          discountCouponToRemove.Add(item);
        }
        else
        {
          if (couponProduct != null && couponProduct.Count > 0)
          {
            foreach (DiscountCouponProduct product in couponProduct)
            {
              if (!lineItem.Any(x => x.Product == product.Product))
              {
                discountCouponToRemove.Add(item);
                break;
              }
              else if (lineItem.Where(
                x => x.Product == product.Product).FirstOrDefault().Quantity <= minimumNumberOfProducts)
              {
                discountCouponToRemove.Add(item);
                break;
              }
            }
          }
          if (coupon.DiscountAsInNproductPercentage || coupon.DiscountAsInNproductDollars)
          {
            selectedProductSubTotal = 0.0M;
            foreach (DiscountCouponProduct product in couponProduct)
            {
              selectedProductSubTotal += lineItem.Find(x => x.Product == product.Product) == null ?
                0.0M : (decimal)lineItem.Find(x => x.Product == product.Product).Total;
            }
            if (selectedProductSubTotal < minimumPurchase)
            {
              discountCouponToRemove.Add(item);
            }
          }
          if (coupon.DiscountProductN != null && coupon.DiscountProductN > 0)
          {
            if (totalNumberOfProducts < coupon.DiscountProductN)
            {
              discountCouponToRemove.Add(item);
            }
          }
        }
      }

      foreach (SelectListItem remove in discountCouponToRemove)
      {
        discountCoupon.Remove(remove);
      }

      discountCoupon.Insert(0, new SelectListItem()
      {
        Text = "--- Please Select a Coupon ---",
        Value = long.MinValue.ToString()
      });
      ViewData["DiscountCoupon"] = discountCoupon;

      checkoutViewModel.LineItem = lineItemViewModel;
      checkoutViewModel.CreditCardExpirationDate = DateTime.Now;
      checkoutViewModel.TotalWeightInPounds = totalOrderShippingWeightInPounds;
      ViewData["Country"] = new SelectList(new List<string> { "US" });
      ViewData["State"] = new SelectList(_context.StateUs.ToList(), "StateAbbreviation", "StateName");

      checkoutViewModel.Taxes = 0.0M;
      checkoutViewModel.CodeDiscount = 0.0M; // TODO: CALCULATE.
      checkoutViewModel.CouponDiscount = 0.0M; // TODO: CALCULATE.
      checkoutViewModel.IsGift = false; // TODO: Map this.
      checkoutViewModel.ShippingCarrier = _option.Value.ShipEngineDefaultCarrier;
      checkoutViewModel.CarrierName = _option.Value.ShipEngineDefaultCarrierName;
      checkoutViewModel.ShippedOn = DateTime.UtcNow.TimeOfDay > new TimeSpan(10, 0, 0) ? // 5:00 PM PTDT.
         DateTime.UtcNow.AddDays(2) : DateTime.UtcNow.AddDays(1); // If after 4:00 PM PDT, two-days (2), else one (1). 
      checkoutViewModel.ExpectedToArrive = checkoutViewModel.ShippedOn.AddDays(3);
      checkoutViewModel.Total = checkoutViewModel.SubTotal + checkoutViewModel.Taxes +
        checkoutViewModel.ShippingCharges - checkoutViewModel.CodeDiscount - checkoutViewModel.CouponDiscount;

      ViewData.Add("Checkout.WelcomeText", _context.LocalizedTexts.Where(
       x => x.PlacementPointCode.Equals("Checkout.WelcomeText"))
       .Select(x => x.Text).FirstOrDefault());

      if (Model == null || (Model.BillingName == null || Model.BillingName.Trim().Length == 0))
      {
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
      }
      else
      {
        if (Model.ShippingAddressDeclined)
        {
          checkoutViewModel.ShippingAddressDeclined = true;
        }

        if (Model.TaxCalculationServiceOffline)
        {
          checkoutViewModel.TaxCalculationServiceOffline = true;
        }

        checkoutViewModel.BillingName = Model.BillingName;
        checkoutViewModel.BillingAddress1 = Model.BillingAddress1;
        checkoutViewModel.BillingAddress2 = Model.BillingAddress2;
        checkoutViewModel.BillingCity = Model.BillingCity;
        checkoutViewModel.BillingState = Model.BillingState;
        checkoutViewModel.BillingCountry = Model.BillingCountry;
        checkoutViewModel.BillingPostalCode = Model.BillingPostalCode;

        checkoutViewModel.ShippingAddressSame = Model.ShippingAddressSame;

        checkoutViewModel.ShippingName = Model.ShippingName;
        checkoutViewModel.ShippingAddress1 = Model.ShippingAddress1;
        checkoutViewModel.ShippingAddress2 = Model.ShippingAddress2;
        checkoutViewModel.ShippingCity = Model.ShippingCity;
        checkoutViewModel.ShippingState = Model.ShippingState;
        checkoutViewModel.ShippingCountry = Model.ShippingCountry;
        checkoutViewModel.ShippingPostalCode = Model.ShippingPostalCode;
      }

      checkoutViewModel.ShippingAddressSame = Model == null ?
        checkoutViewModel.ShippingAddressSame : Model.ShippingAddressSame;

      return View(checkoutViewModel);
    }

    public async Task<IActionResult> CalculateShippingCostAndTaxes(Models.CheckoutViewModel Model)
    {
      string seShipmentDetailsJson = null;
      string seShipmentCostJson = null;
      List<Models.ShoppingCartLineItemViewModel> lineItemViewModel = new List<Models.ShoppingCartLineItemViewModel>();
      List<ShoppingCartLineItem> lineItem = _context.ShoppingCartLineItems.Where(
        x => x.ShoppingCart == _context.ShoppingCarts.Where(x => x.User.Equals(_context.AspNetUsers.Where(
        x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault()))
        .Select(x => x.Id).FirstOrDefault()).ToList();
      object[] tjLineItem = null;
      decimal totalOrderShippingWeightInPounds = 0.0M;
      TaxjarApi tjService = null;
      TaxResponseAttributes tjTaxRate = null;
      int i = 0;
      List<SelectListItem> discountCoupon = null;
      List<SelectListItem> discountCouponToRemove = null;
      DiscountCoupon coupon = null;
      List<DiscountCouponProduct> couponProduct = null;
      decimal percentage = decimal.MinValue;
      short minimumNumberOfProducts = short.MinValue;
      decimal minimumPurchase = decimal.MinValue;
      decimal selectedProductSubTotal = 0.0M;
      bool minimumNumberOfSelectedProductsMet = true;
      ORM.DiscountCode code = null;
      List<DiscountCodeProduct> codeProduct = null;
      long totalNumberOfProducts = 0L;

      ViewData["Country"] = new SelectList(new List<string> { "US" });
      ViewData["State"] = new SelectList(_context.StateUs.ToList(), "StateAbbreviation", "StateName");

      Model.SubTotal = 0.0M;
      totalOrderShippingWeightInPounds = 0.0M;

      tjLineItem = new object[lineItem.Count];

      foreach (ShoppingCartLineItem li in lineItem)
      {
        lineItemViewModel.Add(new Models.ShoppingCartLineItemViewModel()
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

        tjLineItem[i] = new
        {
          id = _context.ShoppingCartLineItems.Where(
            x => x.Product == li.Product).Select(x => x.Id).FirstOrDefault().ToString(),
          quantity = li.Quantity,
          product_tax_code = _option.Value.TaxJarSkinCareProductTaxCode,
          unit_price = _context.Prices.Where(
            x => x.Id == _context.Products.Where(
            x => x.Id == li.Product).Select(x => x.CurrentPrice).FirstOrDefault()).Select(x => x.Amount).FirstOrDefault(),
          discount = 0
        };
        i++;
      }

      Model.LineItem = lineItemViewModel;

      discountCoupon = new SelectList(_context.DiscountCoupons.Where(
        x => x.Active).Where(
        x => x.ValidFrom <= DateTime.UtcNow).Where(
        x => x.ValidTo >= DateTime.UtcNow), "Id", "Name").ToList();
      discountCouponToRemove = new List<SelectListItem>();

      foreach (SelectListItem dc in discountCoupon)
      {
        coupon = await _context.DiscountCoupons.FindAsync(long.Parse(dc.Value));

        if (coupon != null)
        {
          couponProduct = _context.DiscountCouponProducts.Where(x => x.Coupon == coupon.Id).ToList();
          percentage = coupon.DiscountAsInNproductPercentage ?
            coupon.DiscountNproductPercentage == null ?
            0.0M : (decimal)coupon.DiscountNproductPercentage : coupon.DiscountAsInGlobalOrderPercentage ?
            coupon.DiscountGlobalOrderPercentage == null ? 0.0M : (decimal)coupon.DiscountGlobalOrderPercentage :
            0.0M;
          minimumNumberOfProducts = coupon.DiscountProductN == null ? (short)0 : (short)coupon.DiscountProductN;
          minimumPurchase = coupon.MinimumPurchase == null ? 0.0M : (decimal)coupon.MinimumPurchase;
          selectedProductSubTotal = 0.0M;
          minimumNumberOfSelectedProductsMet = true;
          totalNumberOfProducts = 0L;

          if (coupon.DiscountAsInGlobalOrderPercentage || coupon.DiscountAsInGlobalOrderPercentage)
          {
            foreach (Models.ShoppingCartLineItemViewModel item in Model.LineItem)
            {
              totalNumberOfProducts += item.Quantity;
            }

            if (totalNumberOfProducts < coupon.DiscountProductN &&
              Model.SubTotal < minimumPurchase)
            {
              discountCouponToRemove.Add(dc);
            }
          }
          else if (coupon.DiscountAsInNproductPercentage || coupon.DiscountAsInNproductDollars)
          {
            if (couponProduct != null && couponProduct.Count > 0)
            {
              if (minimumPurchase > 0)
              {
                foreach (DiscountCouponProduct product in couponProduct)
                {
                  selectedProductSubTotal += lineItem.Find(x => x.Product == product.Product) == null ?
                    0.0M : (decimal)lineItem.Find(x => x.Product == product.Product).Total;
                }
              }

              foreach (DiscountCouponProduct product in couponProduct)
              {
                if (!lineItem.Any(x => x.Product == product.Product) || lineItem.Where(
                  x => x.Product == product.Product).FirstOrDefault().Quantity <= minimumNumberOfProducts)
                {
                  minimumNumberOfSelectedProductsMet = false;
                  break;
                }
              }

              if (!minimumNumberOfSelectedProductsMet || selectedProductSubTotal < minimumPurchase)
              {
                discountCouponToRemove.Add(dc);
              }
            }
            else
            {
              discountCouponToRemove.Add(dc);
            }
          }
        }
      }

      foreach (SelectListItem remove in discountCouponToRemove)
      {
        discountCoupon.Remove(remove);
      }

      discountCoupon.Insert(0, new SelectListItem()
      {
        Text = "--- Please Select a Coupon ---",
        Value = long.MinValue.ToString()
      });
      ViewData["DiscountCoupon"] = discountCoupon;

      if (discountCoupon.Any(x => long.Parse(x.Value) == Model.DiscountCouponId) && Model.DiscountCouponId > 0)
      {
        coupon = await _context.DiscountCoupons.FindAsync(Model.DiscountCouponId);

        if (coupon != null)
        {
          couponProduct = _context.DiscountCouponProducts.Where(x => x.Coupon == coupon.Id).ToList();
          percentage = coupon.DiscountAsInNproductPercentage ?
            coupon.DiscountNproductPercentage == null ?
            0.0M : (decimal)coupon.DiscountNproductPercentage : coupon.DiscountAsInGlobalOrderPercentage ?
            coupon.DiscountGlobalOrderPercentage == null ? 0.0M : (decimal)coupon.DiscountGlobalOrderPercentage :
            0.0M;
          minimumNumberOfProducts = coupon.DiscountProductN == null ? (short)0 : (short)coupon.DiscountProductN;
          minimumPurchase = coupon.MinimumPurchase == null ? 0.0M : (decimal)coupon.MinimumPurchase;
          selectedProductSubTotal = 0.0M;
          minimumNumberOfSelectedProductsMet = true;
          totalNumberOfProducts = 0L;

          if (coupon.DiscountAsInGlobalOrderPercentage || coupon.DiscountAsInGlobalOrderDollars)
          {
            foreach (Models.ShoppingCartLineItemViewModel item in Model.LineItem)
            {
              totalNumberOfProducts += item.Quantity;
            }

            if (totalNumberOfProducts >= minimumNumberOfProducts &&
              Model.SubTotal >= minimumPurchase)
            {
              if (coupon.DiscountAsInGlobalOrderPercentage)
              {
                Model.CouponDiscount = (Model.SubTotal * (coupon.DiscountGlobalOrderPercentage / 100));
              }
              else if (coupon.DiscountAsInGlobalOrderDollars)
              {
                Model.CouponDiscount = coupon.DiscountGlobalOrderDollars;
              }
            }
          }
          else if (coupon.DiscountAsInNproductPercentage || coupon.DiscountAsInNproductDollars)
          {
            if (couponProduct != null && couponProduct.Count > 0)
            {
              if (minimumPurchase > 0)
              {
                foreach (DiscountCouponProduct product in couponProduct)
                {
                  selectedProductSubTotal += lineItem.Find(x => x.Product == product.Product) == null ?
                    0.0M : (decimal)lineItem.Find(x => x.Product == product.Product).Total;
                }
              }

              foreach (DiscountCouponProduct product in couponProduct)
              {
                if (!lineItem.Any(x => x.Product == product.Product) || lineItem.Where(
                  x => x.Product == product.Product).FirstOrDefault().Quantity <= minimumNumberOfProducts)
                {
                  minimumNumberOfSelectedProductsMet = false;
                  break;
                }
              }

              if (minimumNumberOfSelectedProductsMet && selectedProductSubTotal >= minimumPurchase)
              {
                if (coupon.DiscountAsInNproductPercentage)
                {
                  Model.CouponDiscount = _context.Prices.Where(
                    x => x.Id == (_context.Products.FindAsync(
                    couponProduct.Last().Product).Result.CurrentPrice)).Select(x => x.Amount).FirstOrDefault() *
                    (coupon.DiscountNproductPercentage / 100);
                }
                else if (coupon.DiscountAsInNproductDollars)
                {
                  Model.CouponDiscount = coupon.DiscountInNproductDollars;
                }
              }
            }
          }
        }
      }
      else if (Model.DiscountCode != null && Model.DiscountCode.Trim().Length > 0)
      {
        code = _context.DiscountCodes.Where(
          x => x.Code.Trim().ToUpper().Equals(Model.DiscountCode.Trim().ToUpper())).FirstOrDefault();

        if (code != null)
        {
          codeProduct = _context.DiscountCodeProducts.Where(x => x.Code == code.Id).ToList();
          percentage = code.DiscountAsInNproductPercentage ?
            code.DiscountNproductPercentage == null ?
            0.0M : (decimal)code.DiscountNproductPercentage : code.DiscountAsInGlobalOrderPercentage ?
            code.DiscountGlobalOrderPercentage == null ? 0.0M : (decimal)code.DiscountGlobalOrderPercentage :
            0.0M;
          minimumNumberOfProducts = code.DiscountProductN == null ? (short)0 : (short)code.DiscountProductN;
          minimumPurchase = code.MinimumPurchase == null ? 0.0M : (decimal)code.MinimumPurchase;
          selectedProductSubTotal = 0.0M;
          minimumNumberOfSelectedProductsMet = true;
          totalNumberOfProducts = 0L;

          if (code.DiscountAsInGlobalOrderPercentage || code.DiscountAsInGlobalOrderDollars)
          {
            foreach (Models.ShoppingCartLineItemViewModel item in Model.LineItem)
            {
              totalNumberOfProducts += item.Quantity;
            }

            if (totalNumberOfProducts >= minimumNumberOfProducts &&
              Model.SubTotal >= minimumPurchase)
            {
              if (code.DiscountAsInGlobalOrderPercentage)
              {
                Model.CodeDiscount = (Model.SubTotal * (code.DiscountGlobalOrderPercentage / 100));
              }
              else if (code.DiscountAsInGlobalOrderDollars)
              {
                Model.CodeDiscount = code.DiscountGlobalOrderDollars;
              }
            }
          }
          else if (code.DiscountAsInNproductPercentage || code.DiscountAsInNproductDollars)
          {
            if (codeProduct != null && codeProduct.Count > 0)
            {
              if (minimumPurchase > 0)
              {
                foreach (DiscountCodeProduct product in codeProduct)
                {
                  selectedProductSubTotal += lineItem.Find(x => x.Product == product.Product) == null ?
                    0.0M : (decimal)lineItem.Find(x => x.Product == product.Product).Total;
                }
              }

              foreach (DiscountCodeProduct product in codeProduct)
              {
                if (!lineItem.Any(x => x.Product == product.Product) || lineItem.Where(
                  x => x.Product == product.Product).FirstOrDefault().Quantity <= minimumNumberOfProducts)
                {
                  minimumNumberOfSelectedProductsMet = false;
                  break;
                }
              }

              if (minimumNumberOfSelectedProductsMet && selectedProductSubTotal > minimumPurchase)
              {
                if (code.DiscountAsInNproductPercentage)
                {
                  Model.CodeDiscount = _context.Prices.Where(
                    x => x.Id == (_context.Products.FindAsync(
                    codeProduct.Last().Product).Result.CurrentPrice)).Select(x => x.Amount).FirstOrDefault() *
                    (code.DiscountNproductPercentage / 100);
                }
                else if (code.DiscountAsInNproductDollars)
                {
                  Model.CodeDiscount = code.DiscountInNproductDollars;
                }
              }
            }
          }
        }
      }

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
                Model.ShipEngineRateId = rate.GetProperty("rate_id").ToString();
                break;
              }
            }
          }

          try
          {
            tjService = new TaxjarApi(_option.Value.TaxJarApiKey);
            tjTaxRate = tjService.TaxForOrder(new
            {
              amount = (decimal)Model.SubTotal,
              from_city = _option.Value.ShipFromCity,
              from_country = _option.Value.ShipFromCountryCode,
              from_state = _option.Value.ShipFromState,
              from_street = _option.Value.ShipFromAddressLine1,
              from_zip = _option.Value.ShipFromPostalCode,
              line_items = tjLineItem,
              shipping = (decimal)Model.ShippingCharges,
              to_city = Model.ShippingAddressSame ? Model.BillingCity.Trim() : Model.ShippingCity.Trim(),
              to_country = Model.ShippingAddressSame ? Model.BillingCountry : Model.ShippingCountry,
              to_state = Model.ShippingAddressSame ? Model.BillingState : Model.ShippingState,
              to_street = Model.ShippingAddressSame ?
                (Model.BillingAddress1.Trim() + (Model.BillingAddress2 == null ||
                (Model.BillingAddress2 != null && Model.BillingAddress2.Trim() == string.Empty) ? string.Empty :
                " " + Model.BillingAddress2.Trim())) :
                (Model.ShippingAddress1.Trim() + (Model.ShippingAddress2 == null ||
                (Model.ShippingAddress2 != null && Model.ShippingAddress2.Trim() == string.Empty) ? string.Empty :
                " " + Model.ShippingAddress2.Trim())),
              to_zip = Model.ShippingAddressSame ? Model.BillingPostalCode : Model.ShippingPostalCode,
              nexus_addresses = new[]
              {
              new
              {
                id = _option.Value.ShipFromName,
                country = _option.Value.ShipFromCountryCode,
                zip = _option.Value.ShipFromPostalCode,
                state = _option.Value.ShipFromState,
                city = _option.Value.ShipFromCity,
                street = _option.Value.ShipFromAddressLine1
              }
            }
            });
            Model.Taxes = tjTaxRate.AmountToCollect;
          }
          catch
          {
            if (_option.Value.TaxJarEnabled)
            {
              Model.ShippingAddressDeclined = false;
              Model.TaxCalculationServiceOffline = true;
              return RedirectToAction("Index", Model);
            }
          }
        }
        catch
        {
          if (_option.Value.ShipEngineEnabled)
          {
            Model.TaxCalculationServiceOffline = false;
            Model.ShippingAddressDeclined = true;
            return RedirectToAction("Index", Model);
          }
        }
      }

      Model.Total = Model.SubTotal + Model.Taxes + Model.ShippingCharges -
        Model.CodeDiscount - Model.CouponDiscount;

      Model.CalculatedShippingAndTaxes = true;

      return View("Index", Model);
    }

    public async Task<IActionResult> PlaceOrder(Models.CheckoutViewModel Model)
    {
      ShopifySharp.ProductService shProductService = null;
      ProductVariantService shProductVariantService = null;
      InventoryLevelService shInventoryLevelService = null;
      InventoryItemService shInventoryItemService = null;
      LocationService shLocationService = null;
      ShopifySharp.CustomerService shCustomerService = null;
      ShopifySharp.OrderService shOrderService = null;
      ShopifySharp.TransactionService shTransactionService = null;
      Stripe.ChargeService stChargeService = null;
      Stripe.CustomerService stCustomerService = null;
      SourceService stSourceService = null;
      TokenService stTokenService = null;
      TaxjarApi tjService = null;
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
      ShopifySharp.Transaction shTransaction = null;
      ShopifySharp.Product shProduct = null;
      ProductVariant shProductVariant = null;
      List<Location> shLocation = null;
      InventoryItem shInventoryItem = null;
      long originalProductStock = 0L;
      long originalKitStock = 0L;
      List<long> updatedKit = null;
      ShopifySharp.Order shOrder = null;
      List<ShopifySharp.LineItem> shLineItemList = null;
      string seShipmentRateJson = null;
      ChargeCreateOptions stChargeCreateOptions = null;
      Dictionary<string, string> stCreditTransactionMetaValue = null;
      Stripe.CustomerCreateOptions stCustomerCreateOptions = null;
      Stripe.Customer stCustomer = null;
      CardCreateNestedOptions stCardCreateNestedOptions = null;
      Source stSource = null;
      SourceCreateOptions stSourceCreateOptions = null;
      TokenCreateOptions stTokenCreateOptions = null;
      Token stToken = null;
      TokenCardOptions stTokenCardOptions = null;
      AspNetUser user = null;
      OrderBillTo previousOrderBillTo = null;
      Stripe.Charge stCharge = null;
      string geoLocationUrl = null;
      string geoLocationJson = null;
      TaxResponseAttributes tjTaxRate = null;
      OrderResponseAttributes tjOrder = null;
      object[] tjLineItem = null;
      List<ShoppingCartLineItem> lineItem = null;
      int i = 0;
      List<ShopifySharp.Transaction> shOrderTransactions = null;
      string clientIpAddress = null;
      string seShippingLabelRequestJson = null;
      string seShippingLabelResponseJson = null;
      DiscountCoupon coupon = null;
      List<DiscountCouponProduct> couponProduct = null;
      decimal percentage = decimal.MinValue;
      short minimumNumberOfProducts = short.MinValue;
      decimal minimumPurchase = decimal.MinValue;
      decimal selectedProductSubTotal = 0.0M;
      bool minimumNumberOfSelectedProductsMet = true;
      ORM.DiscountCode code = null;
      List<DiscountCodeProduct> codeProduct = null;
      long totalNumberOfProducts = 0L;
      List<ShopifySharp.DiscountCode> discountCode = null;

      // Shopping cart empty?
      if (_context.ShoppingCartLineItems.Where(
          x => x.ShoppingCart == _context.ShoppingCarts.Where(
          x => x.User.Equals(_context.AspNetUsers.Where(
          x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault())).Select(
            x => x.Id).FirstOrDefault()).Count<ShoppingCartLineItem>() == 0)
      {
        return RedirectToAction("Index", "Home");
      }

      // Capture client IP address for this order.
      clientIpAddress = HttpContext.Connection.RemoteIpAddress == null ?
        string.Empty : HttpContext.Connection.RemoteIpAddress.ToString();

      // Initialize Shopify services.
      try
      {
        shProductService = new ShopifySharp.ProductService(_option.Value.ShopifyUrl,
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
        shTransactionService = new ShopifySharp.TransactionService(_option.Value.ShopifyUrl,
          _option.Value.ShopifyStoreFrontAccessToken);
      }
      catch (Exception ex)
      {
        throw new Exception("An error was encountered while initializing Shopify services.", ex);
      }

      // Order has not been initialized? Initialize order.
      if (Model.OrderId == 0)
      {
        order = new ProductOrder()
        {
          User = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(
            x => x.Id).FirstOrDefault(),
          DatePlaced = DateTime.MaxValue,
          Subtotal = 0.0M,
          CouponDiscount = 0.0M,
          CodeDiscount = 0.0M,
          ApplicableTaxes = 0.0M,
          Total = 0.0M,
          GiftOrder = false,
          ClientIpAddress = clientIpAddress
        };

        // Save order to database.
        _context.ProductOrders.Add(order);
        _context.SaveChanges();

        Model.OrderId = order.Id;
      }
      else
      {
        // Retrieve order based on given id.
        order = _context.ProductOrders.Where(x => x.Id == Model.OrderId).FirstOrDefault();
      }

      // Retrieve line items for order.
      lineItem = _context.ShoppingCartLineItems.Where(
          x => x.ShoppingCart == _context.ShoppingCarts.Where(
          x => x.User.Equals(_context.AspNetUsers.Where(
          x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault())).Select(
            x => x.Id).FirstOrDefault()).ToList();

      // Initialize TaxJar line item object array.
      tjLineItem = new object[lineItem.Count];

      try
      {
        // Create Shopify line item list.
        shLineItemList = new List<ShopifySharp.LineItem>();

        // Iterate over line items in order.
        foreach (ShoppingCartLineItem cli in lineItem)
        {
          // Update subtotal calculation based on current line item.
          subTotal += _context.Prices.Where(
            x => x.Id == _context.Products.Where(
              x => x.Id == cli.Product).Select(
              x => x.CurrentPrice).FirstOrDefault()).Select(x => x.Amount).FirstOrDefault() *
              cli.Quantity;

          // Add line item to Shopify line item list.
          shLineItemList.Add(new ShopifySharp.LineItem()
          {
            ProductId = _context.Products.Where(x => x.Id == cli.Product).Select(
              x => x.ShopifyProductId).FirstOrDefault(),
            VariantId = shProductService.GetAsync((long)_context.Products.Where(
              x => x.Id == cli.Product).Select(
              x => x.ShopifyProductId).FirstOrDefault()).Result.Variants.First().Id,
            Quantity = cli.Quantity,
            Taxable = true,
            RequiresShipping = true
          });

          // Add line item to TaxJar line item array.
          tjLineItem[i] = new
          {
            quantity = cli.Quantity,
            product_identifier = _context.Products.Where(
              x => x.Id == cli.Product).Select(x => x.Id).FirstOrDefault().ToString(),
            description = _context.Products.Where(
              x => x.Id == cli.Product).Select(x => x.Name).FirstOrDefault(),
            unit_price = _context.Prices.Where(
              x => x.Id == _context.Products.Where(
              x => x.Id == cli.Product).Select(
                x => x.CurrentPrice).FirstOrDefault()).Select(x => x.Amount).FirstOrDefault()
          };
          // Increment TaxJar line item array index.
          i++;
        }

        try
        {
          // Create Shopify order.
          shOrder = new ShopifySharp.Order()
          {
            // Add billing address.
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
            // Add shipping address.
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

          // Get ShipEngine shipment rate Json document using ShipEngine shipment id obtained 
          // from CalculateShippingCostAndTaxes previously.
          seShipmentRateJson = (_option.Value.ShipEngineGetShipmentCostFromIdPrefixUrl +
            Model.ShipEngineShipmentId +
            _option.Value.ShipEngineGetShipmentCostFromIdPostfixUrl).GetJsonFromUrl(
            requestFilter: webReq =>
            {
              webReq.Headers["API-Key"] = _option.Value.ShipEngineApiKey;
            }, responseFilter: null);

          // Parse the Json document to obtain shipping cost using configured defaults for carrier,
          // rate type, package type and service code.
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
                shippingCost = decimal.Parse(
                  rate.GetProperty("shipping_amount").GetProperty("amount").ToString());
                break;
              }
            }
          }

          // Assign shipping carrier and shipping charges to model.
          Model.ShippingCarrier = _option.Value.ShipEngineDefaultCarrierName;
          Model.ShippingCharges = shippingCost;

          try
          {
            // Initialize Stripe api key.
            Stripe.StripeConfiguration.ApiKey = _option.Value.StripeSecretKey;
            previousOrderBillTo = _context.OrderBillTos.OrderByDescending(x => x.Order).FirstOrDefault();

            // If we need to create a Stripe user for the order's customer, create it.
            if ((_context.AspNetUsers.Where(
              x => x.Email.Equals(User.Identity.Name)).Select(
              x => x.StripeCustomerId).FirstOrDefault() == null &&
              previousOrderBillTo != null) &&
              ((previousOrderBillTo.NameOnCreditCard == null ? (Model.BillingName == null ||
              Model.BillingName.Trim().Equals(string.Empty)) :
              !previousOrderBillTo.NameOnCreditCard.Equals(Model.BillingName == null ?
                string.Empty : Model.BillingName.Trim())) ||
              (previousOrderBillTo.AddressLine1 == null ? (Model.BillingAddress1 == null ||
              Model.BillingAddress1.Trim().Equals(string.Empty)) :
              !previousOrderBillTo.AddressLine1.Equals(Model.BillingAddress1 == null ?
                string.Empty : Model.BillingAddress1.Trim())) ||
              (previousOrderBillTo.AddressLine2 == null ? (Model.BillingAddress2 == null ||
              Model.BillingAddress2.Trim().Equals(string.Empty)) :
              !previousOrderBillTo.AddressLine2.Equals(Model.BillingAddress2 == null ?
                string.Empty : Model.BillingAddress2.Trim())) ||
              (previousOrderBillTo.CityName == null ? (Model.BillingCity == null ||
              Model.BillingCity.Trim().Equals(string.Empty)) :
              !previousOrderBillTo.CityName.Equals(Model.BillingCity == null ?
                string.Empty : Model.BillingCity.Trim())) ||
              (previousOrderBillTo.StateName == null ? (Model.BillingState == null ||
              Model.BillingState.Trim().Equals(string.Empty)) :
              !previousOrderBillTo.StateName.Equals(Model.BillingState == null ?
                string.Empty : Model.BillingState.Trim())) ||
              (previousOrderBillTo.CountryName == null ? (Model.BillingCountry == null ||
              Model.BillingCountry.Trim().Equals(string.Empty)) :
              !previousOrderBillTo.CountryName.Equals(Model.BillingCountry == null ?
                string.Empty : Model.BillingCountry.Trim())) ||
              (previousOrderBillTo.PostalCode == null ? (Model.BillingPostalCode == null ||
              Model.BillingPostalCode.Trim().Equals(string.Empty)) :
              !previousOrderBillTo.PostalCode.Equals(Model.BillingPostalCode == null ?
              string.Empty : Model.BillingPostalCode.Trim()))))
            {
              try
              {
                // Create and initialize Stripe nested options object using Model data.
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

                // Create and initialize Stripe customer create options object.
                stCustomerCreateOptions = new Stripe.CustomerCreateOptions();
                stCustomerCreateOptions.Email = User.Identity.Name;
                stCustomerCreateOptions.Source = stCardCreateNestedOptions;
                stCustomerCreateOptions.Description = "XO Skin Customer.";

                // Instantiate a Stripe customer service object.
                stCustomerService = new Stripe.CustomerService();
                // Create a Stripe customer.
                stCustomer = stCustomerService.Create(stCustomerCreateOptions);

                // Null credit card information.
                stCustomerCreateOptions = null;
                Model.CreditCardNumber = null;
                Model.CreditCardCVC = null;
                Model.CreditCardExpirationDate = DateTime.MinValue;

                // Save Stripe customer id to user in database.
                user = _context.AspNetUsers.Where(
                  x => x.Email.Equals(User.Identity.Name)).FirstOrDefault();
                user.StripeCustomerId = stCustomer.Id;
                _context.AspNetUsers.Update(user);
                _context.SaveChanges();
              }
              catch
              {
                // In case of error, pass a CardDeclined flag to the ui.
                Model.CardDeclined = true;
                Model.CalculatedShippingAndTaxes = true;
                return RedirectToAction("CalculateShippingCostAndTaxes", Model);
              }
            }
            // If the order's customer already has a Stripe customer id.
            else
            {
              // Instantiate a Stripe customer service object.
              stCustomerService = new Stripe.CustomerService();
              // Retrieve Stripe customer using id from database.
              stCustomer = stCustomerService.Get(_context.AspNetUsers.Where(
                x => x.Email.Equals(User.Identity.Name)).Select(
                x => x.StripeCustomerId).FirstOrDefault());

              // Create Stripe token card options object using data from the model
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
              // Create Stripe token create options object using token card options.
              stTokenCreateOptions = new TokenCreateOptions()
              {
                Card = stTokenCardOptions
              };

              // Instantiate Stripe token service.
              stTokenService = new TokenService();
              // Create Stripe token using token create options.
              stToken = stTokenService.Create(stTokenCreateOptions);

              // Null credit card information.
              stTokenCreateOptions = null;
              Model.CreditCardNumber = null;
              Model.CreditCardCVC = null;
              Model.CreditCardExpirationDate = DateTime.MinValue;

              // Instantiate a Stripe source create options object.
              stSourceCreateOptions = new SourceCreateOptions()
              {
                Token = stToken.Id,
                Type = SourceType.Card
              };

              // Once we instantiate the source create options object using the token id,
              // we null the token.
              stToken = null;

              // Instantiate Stripe source service client.
              stSourceService = new SourceService();
              // Instantiate Stripe source object using Stripe source create options.
              stSource = await stSourceService.CreateAsync(stSourceCreateOptions);

              // We attempt to attach our source service client to Stripe.
              try
              {
                stSourceService.Attach(stCustomer.Id, new SourceAttachOptions()
                {
                  Source = stSource.Id
                });
              }
              catch
              {
                // In case of error, pass a CardDeclined flag to the ui.
                Model.CardDeclined = true;
                Model.CalculatedShippingAndTaxes = true;
                return RedirectToAction("CalculateShippingCostAndTaxes", Model);
              }
            }

            try
            {
              // Instantiate TaxJar (web api used to calculate taxes applicable to the sale) using key in the options configuration.
              tjService = new TaxjarApi(_option.Value.TaxJarApiKey);
              // Call TaxJar service to get tax rate information using our order's information.
              tjTaxRate = tjService.TaxForOrder(new
              {
                // This is self-explanatory.
                amount = (decimal)Model.SubTotal,
                from_city = _option.Value.ShipFromCity,
                from_country = _option.Value.ShipFromCountryCode,
                from_state = _option.Value.ShipFromState,
                from_street = _option.Value.ShipFromAddressLine1,
                from_zip = _option.Value.ShipFromPostalCode,
                line_items = tjLineItem,
                shipping = shippingCost,
                to_city = Model.ShippingAddressSame ? Model.BillingCity.Trim() : Model.ShippingCity.Trim(),
                to_country = Model.ShippingAddressSame ? Model.BillingCountry : Model.ShippingCountry,
                to_state = Model.ShippingAddressSame ? Model.BillingState : Model.ShippingState,
                // There is some boolean logic applied to calculating the right street address.
                to_street = Model.ShippingAddressSame ?
                  (Model.BillingAddress1.Trim() + (Model.BillingAddress2 == null ||
                  (Model.BillingAddress2 != null && Model.BillingAddress2.Trim() == string.Empty) ? string.Empty :
                  " " + Model.BillingAddress2.Trim())) :
                  (Model.ShippingAddress1.Trim() + (Model.ShippingAddress2 == null ||
                  (Model.ShippingAddress2 != null && Model.ShippingAddress2.Trim() == string.Empty) ? string.Empty :
                  " " + Model.ShippingAddress2.Trim())),
                to_zip = Model.ShippingAddressSame ? Model.BillingPostalCode : Model.ShippingPostalCode,
                // Handles the concept of nexus address(es). See https://quickbooks.intuit.com/r/taxes/nexus-guide/ (Retrieved 12/13/2022.)
                nexus_addresses = new[]
                {
                new
                {
                  id = _option.Value.ShipFromName,
                  country = _option.Value.ShipFromCountryCode,
                  zip = _option.Value.ShipFromPostalCode,
                  state = _option.Value.ShipFromState,
                  city = _option.Value.ShipFromCity,
                  street = _option.Value.ShipFromAddressLine1
                }
              }
              });
              // We create a TaxJar order object including information obtained from the previous service call and the actual line items.
              tjOrder = tjService.CreateOrder(new
              {
                transaction_id = Model.OrderId.ToString(),
                transaction_date = DateTime.UtcNow.ToString(),
                to_country = Model.ShippingAddressSame ? Model.BillingCountry : Model.ShippingCountry,
                to_state = Model.ShippingAddressSame ? Model.BillingState : Model.ShippingState,
                to_zip = Model.ShippingAddressSame ? Model.BillingPostalCode : Model.ShippingPostalCode,
                to_city = Model.ShippingAddressSame ? Model.BillingCity.Trim() : Model.ShippingCity.Trim(),
                // Same concept applied to the street address calculation for the order.
                to_street = Model.ShippingAddressSame ?
                  (Model.BillingAddress1.Trim() + (Model.BillingAddress2 == null ||
                  (Model.BillingAddress2 != null && Model.BillingAddress2.Trim() == string.Empty) ? string.Empty :
                  " " + Model.BillingAddress2.Trim())) :
                  (Model.ShippingAddress1.Trim() + (Model.ShippingAddress2 == null ||
                  (Model.ShippingAddress2 != null && Model.ShippingAddress2.Trim() == string.Empty) ? string.Empty :
                  " " + Model.ShippingAddress2.Trim())),
                amount = subTotal,
                lineItems = tjLineItem,
                // This is the only item we grab from the previous service call.
                sales_tax = tjTaxRate.AmountToCollect,
                shipping = shippingCost
              });
            }
            catch
            {
              // If the TaxJar-enabling switch on?
              if (_option.Value.TaxJarEnabled)
              {
                // Send a flag to the UI using the view model to indicate that the tax calculation service in unavailable.
                // A message will be displayed prompting the user to try again in a few minutes or to contact us.
                Model.TaxCalculationServiceOffline = true;
                Model.CalculatedShippingAndTaxes = true;
                return RedirectToAction("CalculateShippingCostAndTaxes", Model);
              }
            }

            // If the service is enabled in the configuration file.
            // This flag is normally set to true, allows us to switch it off from the configuration file in order to 
            // bypass the service for any given reason.
            if (_option.Value.TaxJarEnabled)
            {
              applicableTaxes = tjOrder.SalesTax;
            }
            else
            {
              // Sets the tax to a bogus value to remind us that the service is disabled.
              applicableTaxes = 0.00M;
            }

            // Pass the applicable taxes to the UI model.
            Model.Taxes = applicableTaxes;

            Model.CouponDiscount = 0.0M;
            Model.CodeDiscount = 0.0M;

            // Try to load any specified coupons.
            coupon = await _context.DiscountCoupons.FindAsync(Model.DiscountCouponId);
            // If there is a coupon.
            if (coupon != null)
            {
              // Process coupon data according to rules set on the database.
              couponProduct = _context.DiscountCouponProducts.Where(x => x.Coupon == coupon.Id).ToList();
              percentage = coupon.DiscountAsInNproductPercentage ?
                coupon.DiscountNproductPercentage == null ?
                0.0M : (decimal)coupon.DiscountNproductPercentage : coupon.DiscountAsInGlobalOrderPercentage ?
                coupon.DiscountGlobalOrderPercentage == null ? 0.0M : (decimal)coupon.DiscountGlobalOrderPercentage :
                0.0M;
              minimumNumberOfProducts = coupon.DiscountProductN == null ? (short)0 : (short)coupon.DiscountProductN;
              minimumPurchase = coupon.MinimumPurchase == null ? 0.0M : (decimal)coupon.MinimumPurchase;
              selectedProductSubTotal = 0.0M;
              minimumNumberOfSelectedProductsMet = true;
              totalNumberOfProducts = 0L;

              // If it's a global order percentage or global order in dollars coupon.
              if (coupon.DiscountAsInGlobalOrderPercentage || coupon.DiscountAsInGlobalOrderDollars)
              {
                // Calculate the total number of products.
                foreach (ShoppingCartLineItem item in lineItem)
                {
                  totalNumberOfProducts += item.Quantity;
                }
                // If coupon rules are met.
                if (totalNumberOfProducts >= minimumNumberOfProducts &&
                  Model.SubTotal >= minimumPurchase)
                {
                  // Apply dollar or percentage discount.
                  if (coupon.DiscountAsInGlobalOrderPercentage)
                  {
                    Model.CouponDiscount = (Model.SubTotal * (coupon.DiscountGlobalOrderPercentage / 100));
                  }
                  else if (coupon.DiscountAsInGlobalOrderDollars)
                  {
                    Model.CouponDiscount = coupon.DiscountGlobalOrderDollars;
                  }
                }
              }
              // If it's a discount as in n product percentage or dollars coupon.
              else if (coupon.DiscountAsInNproductPercentage || coupon.DiscountAsInNproductDollars)
              {
                // If a coupon product exists and its count is larger than zero.
                if (couponProduct != null && couponProduct.Count > 0)
                {
                  // If there's a minimum purchase required.
                  if (minimumPurchase > 0)
                  {
                    foreach (DiscountCouponProduct p in couponProduct)
                    {
                      // Calculate the subtotal for the selected product.
                      selectedProductSubTotal += lineItem.Find(x => x.Product == p.Product) == null ?
                        0.0M : (decimal)lineItem.Find(x => x.Product == p.Product).Total;
                    }
                  }
                  // For each product related to a coupon.
                  foreach (DiscountCouponProduct p in couponProduct)
                  {
                    // Determine whether the product quantity is below the minimum number of products for the coupon.
                    if (!lineItem.Any(x => x.Product == p.Product) || lineItem.Where(
                      x => x.Product == p.Product).FirstOrDefault().Quantity <= minimumNumberOfProducts)
                    {
                      // If so, set a flag and exit foreach.
                      minimumNumberOfSelectedProductsMet = false;
                      break;
                    }
                  }
                  // If the minimum number of selected products is met and the selected product subtotal is more or
                  // equal to the minimum purchase specified by the coupon rules.
                  if (minimumNumberOfSelectedProductsMet && selectedProductSubTotal >= minimumPurchase)
                  {
                    // If it's a N product percentage discount.
                    if (coupon.DiscountAsInNproductPercentage)
                    {
                      // Apply discount.
                      Model.CouponDiscount = _context.Prices.Where(
                        x => x.Id == (_context.Products.FindAsync(
                        couponProduct.Last().Product).Result.CurrentPrice)).Select(x => x.Amount).FirstOrDefault() *
                        (coupon.DiscountNproductPercentage / 100);
                    }
                    // N product dollars discount.
                    else if (coupon.DiscountAsInNproductDollars)
                    {
                      // Apply coupon discount.
                      Model.CouponDiscount = coupon.DiscountInNproductDollars;
                    }
                  }
                }
              }
            }
            // If there is a discount code.
            else if (Model.DiscountCode != null && Model.DiscountCode.Trim().Length > 0)
            {
              // Fetch the discount code information from the database.
              code = _context.DiscountCodes.Where(
                x => x.Code.Trim().ToUpper().Equals(Model.DiscountCode.Trim().ToUpper())).FirstOrDefault();
              // If there is discount code information on the database.
              if (code != null)
              {
                // Initialize discount calculation variables.
                codeProduct = _context.DiscountCodeProducts.Where(x => x.Code == code.Id).ToList();
                percentage = code.DiscountAsInNproductPercentage ?
                  code.DiscountNproductPercentage == null ?
                  0.0M : (decimal)code.DiscountNproductPercentage : code.DiscountAsInGlobalOrderPercentage ?
                  code.DiscountGlobalOrderPercentage == null ? 0.0M : (decimal)code.DiscountGlobalOrderPercentage :
                  0.0M;
                minimumNumberOfProducts = code.DiscountProductN == null ? (short)0 : (short)code.DiscountProductN;
                minimumPurchase = code.MinimumPurchase == null ? 0.0M : (decimal)code.MinimumPurchase;
                selectedProductSubTotal = 0.0M;
                minimumNumberOfSelectedProductsMet = true;
                totalNumberOfProducts = 0L;

                // If the discount code is for a global order discount.
                if (code.DiscountAsInGlobalOrderPercentage || code.DiscountAsInGlobalOrderDollars)
                {
                  // Add the items and quantities to obtain the total number of products in the order.
                  foreach (ShoppingCartLineItem item in lineItem)
                  {
                    totalNumberOfProducts += item.Quantity;
                  }

                  // If the total number of products exceeds the minimum set by the discount code, and the 
                  // order subtotal is greater or equal than the minimum purchase set by the same code.
                  if (totalNumberOfProducts >= minimumNumberOfProducts &&
                    Model.SubTotal >= minimumPurchase)
                  {
                    // Calculate discounts for globals, percentage or dollars.
                    if (code.DiscountAsInGlobalOrderPercentage)
                    {
                      Model.CodeDiscount = (Model.SubTotal * (code.DiscountGlobalOrderPercentage / 100));
                    }
                    else if (code.DiscountAsInGlobalOrderDollars)
                    {
                      Model.CodeDiscount = code.DiscountGlobalOrderDollars;
                    }
                  }
                }
                // If the discount code is a n product type.
                else if (code.DiscountAsInNproductPercentage || code.DiscountAsInNproductDollars)
                {
                  // Determine whether the discount applies, and if so, then apply it.
                  if (codeProduct != null && codeProduct.Count > 0)
                  {
                    // If the minimum purchase for this discount code is higher than zero.
                    if (minimumPurchase > 0)
                    {
                      // Add the selected product sub total.
                      foreach (DiscountCodeProduct p in codeProduct)
                      {
                        selectedProductSubTotal += lineItem.Find(x => x.Product == p.Product) == null ?
                          0.0M : (decimal)lineItem.Find(x => x.Product == p.Product).Total;
                      }
                    }
                    // Check if the cart has the product featured in the discount code and
                    // if the minumum number of products is met.
                    foreach (DiscountCodeProduct p in codeProduct)
                    {
                      if (!lineItem.Any(x => x.Product == p.Product) || lineItem.Where(
                        x => x.Product == p.Product).FirstOrDefault().Quantity <= minimumNumberOfProducts)
                      {
                        // If not, set a flag.
                        minimumNumberOfSelectedProductsMet = false;
                        break;
                      }
                    }
                    // If the correct flags have been set and the subtotal of selected products is 
                    // above the minimum purchase.
                    if (minimumNumberOfSelectedProductsMet && selectedProductSubTotal >= minimumPurchase)
                    {
                      // Apply the correct discount according to code discount rules in the db.
                      // Product percentage discount.
                      if (code.DiscountAsInNproductPercentage)
                      {
                        Model.CodeDiscount = _context.Prices.Where(
                          x => x.Id == (_context.Products.FindAsync(
                          codeProduct.Last().Product).Result.CurrentPrice)).Select(x => x.Amount).FirstOrDefault() *
                          (code.DiscountNproductPercentage / 100);
                      }
                      // Dollar discount.
                      else if (code.DiscountAsInNproductDollars)
                      {
                        Model.CodeDiscount = code.DiscountInNproductDollars;
                      }
                    }
                  }
                }
              }
            }

            // Set discount(s).
            codeDiscount = Model.CodeDiscount == null ? 0.0M : (decimal)Model.CodeDiscount;
            couponDiscount = Model.CouponDiscount == null ? 0.0M : (decimal)Model.CouponDiscount;
            // Apply to total.
            total = subTotal - codeDiscount - couponDiscount + shippingCost + applicableTaxes;
            // Set meta data for Stripe transaction.
            stCreditTransactionMetaValue = new Dictionary<string, string>();
            stCreditTransactionMetaValue.Add("ProductOrderId", order.Id.ToString());
            stCreditTransactionMetaValue.Add("UserIdentityName", User.Identity.Name);
            // Create and initialize a Stripe charge creation options object.
            stChargeCreateOptions = new ChargeCreateOptions()
            {
              Amount = (long?)total * 100, // Stripe encodes dollar amounts as a nullable long.
              Currency = "usd",
              Description = "Total charges for an XO Skin customer order #XO" +
                (order.Id + 10000).ToString() +
                ". Customer: " + User.Identity.Name + ".",
              Metadata = stCreditTransactionMetaValue,
              ReceiptEmail = User.Identity.Name,
              Customer = stCustomer.Id
            };
            // Create and initialize Stripe charge service.
            stChargeService = new Stripe.ChargeService();
            stCharge = stChargeService.Create(stChargeCreateOptions);
            // If charge to cc succeeded.
            if (stCharge.Status.Equals("succeeded"))
            {
              // Set values to persist to db.
              order.StripeChargeId = stCharge.Id;
              order.StripeChargeStatus = stCharge.Status;
              Model.BilledOn = stCharge.Created;
            }
            else
            {
              // Pass the appropriate flags to the UI.
              Model.CardDeclined = true;
              Model.CalculatedShippingAndTaxes = true;
              return RedirectToAction("CalculateShippingCostAndTaxes", Model);
            }

            // Populate Stripe customer object using data stored in the db.
            stCustomer = stCustomerService.Get(_context.AspNetUsers.Where(
              x => x.Email.Equals(User.Identity.Name)).Select(x => x.StripeCustomerId).FirstOrDefault());
            // If the object has been correctly initialized.
            if (stCustomer.Sources != null)
            {
              // Create and initiaze Stripe "source" (financial instrument) service.
              stSourceService = new SourceService();
              foreach (Source s in stCustomer.Sources)
              {
                // Done with all sources so we detach them.
                stSourceService.Detach(stCustomer.Id, s.Id);
              }
            }
          }
          catch
          {
            // Any errors related to the transaction return the following flags to the UI.
            Model.CardDeclined = true;
            Model.CalculatedShippingAndTaxes = true;
            return RedirectToAction("CalculateShippingCostAndTaxes", Model);
          }

          // At this point the order has been charged to the customer-specified instrument.
          // we now proceed to record all the information related to the transaction
          // to both our db and Shopify.

          // Create and add shipping line (shipping data) to Shopify order object.
          shOrder.ShippingLines = new List<ShippingLine>()
          {
            new ShippingLine()
            {
              PriceSet = new PriceSet()
              {
                PresentmentMoney = new ShopifySharp.Price()
                {
                  Amount = shippingCost,
                  CurrencyCode = stCharge.Currency
                },
                ShopMoney = new ShopifySharp.Price()
                {
                  Amount = shippingCost,
                  CurrencyCode = stCharge.Currency
                }
              },
              Title = _option.Value.ShopifyShippingLineTitle,
              Code = _option.Value.ShopifyShippingLineCode,
              Source = _option.Value.ShopifyShippingLineSource,
              Price = shippingCost
            }
          };
          // Set tax information to the Shopify object.
          shOrder.EstimatedTaxes = false;
          shOrder.TotalTax = applicableTaxes;
          shOrder.TotalTaxSet = new PriceSet()
          {
            PresentmentMoney = new ShopifySharp.Price()
            {
              Amount = applicableTaxes,
              CurrencyCode = stCharge.Currency
            },
            ShopMoney = new ShopifySharp.Price()
            {
              Amount = applicableTaxes,
              CurrencyCode = stCharge.Currency
            }
          };
          // Create and add tax line (shipping data) to Shopify order object.
          shOrder.TaxLines = new List<TaxLine>()
          {
            new TaxLine()
            {
              Price = applicableTaxes,
              PriceSet = new PriceSet()
              {
                PresentmentMoney = new ShopifySharp.Price()
                {
                  Amount = applicableTaxes,
                  CurrencyCode = stCharge.Currency
                },
                ShopMoney = new ShopifySharp.Price()
                {
                  Amount = applicableTaxes,
                  CurrencyCode = stCharge.Currency
                }
              },
              Rate = applicableTaxes,
              Title = "Sales tax calculation performed by Taxjar."
            }
          };
          // Add payment processor information to Shopify order object.
          shOrder.PaymentGatewayNames = new List<string>()
          {
            "Stripe"
          };
          shOrder.ProcessingMethod = "Stripe payment gateway.";
          shOrder.SubtotalPrice = subTotal;
          shOrder.SubtotalPriceSet = new PriceSet()
          {
            PresentmentMoney = new ShopifySharp.Price()
            {
              Amount = subTotal,
              CurrencyCode = stCharge.Currency
            },
            ShopMoney = new ShopifySharp.Price()
            {
              Amount = subTotal,
              CurrencyCode = stCharge.Currency
            }
          };
          // Add line item totals to Shopify order object.
          shOrder.TotalLineItemsPrice = subTotal;
          shOrder.TotalLineItemsPriceSet = new PriceSet()
          {
            PresentmentMoney = new ShopifySharp.Price()
            {
              Amount = subTotal,
              CurrencyCode = stCharge.Currency
            },
            ShopMoney = new ShopifySharp.Price()
            {
              Amount = subTotal,
              CurrencyCode = stCharge.Currency
            }
          };
          // Do the same for total price.
          shOrder.TotalPrice = total;
          shOrder.TotalPriceSet = new PriceSet()
          {
            PresentmentMoney = new ShopifySharp.Price()
            {
              Amount = total,
              CurrencyCode = stCharge.Currency
            },
            ShopMoney = new ShopifySharp.Price()
            {
              Amount = total,
              CurrencyCode = stCharge.Currency
            }
          };
          // And shipping price.
          shOrder.TotalShippingPriceSet = new PriceSet()
          {
            PresentmentMoney = new ShopifySharp.Price
            {
              Amount = shippingCost,
              CurrencyCode = stCharge.Currency
            },
            ShopMoney = new ShopifySharp.Price()
            {
              Amount = shippingCost,
              CurrencyCode = stCharge.Currency
            }
          };

          // Add coupon discount information to Shopify order object.
          if (Model.CouponDiscount != null && Model.CouponDiscount > 0)
          {
            discountCode = new List<ShopifySharp.DiscountCode>();
            discountCode.Add(new ShopifySharp.DiscountCode()
            {
              Amount = ((decimal)Model.CouponDiscount).ToString(),
              Code = Model.DiscountCouponId.ToString(),
              Type = "Discount Coupon Id"
            });
            shOrder.DiscountCodes = discountCode;
            shOrder.TotalDiscounts = Model.CouponDiscount;
            shOrder.TotalDiscountsSet = new PriceSet()
            {
              PresentmentMoney = new ShopifySharp.Price()
              {
                Amount = Model.CouponDiscount,
                CurrencyCode = stCharge.Currency
              },
              ShopMoney = new ShopifySharp.Price()
              {
                Amount = Model.CouponDiscount,
                CurrencyCode = stCharge.Currency
              }
            };
          }
          // Add code discount information to Shopify order object.
          else if (Model.CodeDiscount != null && Model.CodeDiscount > 0)
          {
            discountCode = new List<ShopifySharp.DiscountCode>();
            discountCode.Add(new ShopifySharp.DiscountCode()
            {
              Amount = ((decimal)Model.CodeDiscount).ToString(),
              Code = Model.DiscountCode,
              Type = "Discount Code"
            });
            shOrder.DiscountCodes = discountCode;
            shOrder.TotalDiscounts = Model.CodeDiscount;
            shOrder.TotalDiscountsSet = new PriceSet()
            {
              PresentmentMoney = new ShopifySharp.Price()
              {
                Amount = Model.CodeDiscount,
                CurrencyCode = stCharge.Currency
              },
              ShopMoney = new ShopifySharp.Price()
              {
                Amount = Model.CodeDiscount,
                CurrencyCode = stCharge.Currency
              }
            };
          }

          // Set remaining properties of the Shopify order object prior to submitting data to 
          // Shopify.
          shOrder.PresentmentCurrency = stCharge.Currency.ToUpper();
          shOrder.Currency = stCharge.Currency.ToUpper();
          shOrder.Name = "#XO" + (order.Id + 10000).ToString();
          shOrder.OrderStatusUrl = _option.Value.ShopifyOrderStatusUrl + order.Id.ToString();
          shOrder.CreatedAt = DateTime.UtcNow;
          shOrder.LineItems = shLineItemList;
          shOrder.Test = false;
          shOrder.TaxesIncluded = false;
          shOrder.Test = false;  // Switch to "true" for testing against a production store.
          shOrder.Customer = await shCustomerService.GetAsync((long)_context.AspNetUsers.Where(
            x => x.Email.Equals(User.Identity.Name)).Select(x => x.ShopifyCustomerId).FirstOrDefault());
          shOrder.FinancialStatus = "pending";
          shOrder.Transactions = new List<Transaction>()
          {
            new Transaction()
            {
              Amount = total,
              Kind = "authorization",
              Status = "success",
              CreatedAt = stCharge.Created,
              Currency = stCharge.Currency.ToUpper(),
              DeviceId = shOrder.DeviceId.ToString(),
              Gateway = shOrder.PaymentGatewayNames.FirstOrDefault(),
              LocationId = shOrder.LocationId,
              MaximumRefundable = subTotal + applicableTaxes,
              Message = "Transaction authorized by Stripe on behalf of XO Skin.",
              Source = shOrder.SourceName,
              Authorization = stCharge.AuthorizationCode
            }
          };
          // Store the customer's ip address for future reference.
          shOrder.BrowserIp = clientIpAddress;
          // Set Shopify order billing address for the order.
          shOrder.BillingAddress = new ShopifySharp.Address()
          {
            Address1 = stCharge.BillingDetails.Address.Line1 == null ? string.Empty :
              stCharge.BillingDetails.Address.Line1,
            Address2 = stCharge.BillingDetails.Address.Line2 == null ? string.Empty :
              stCharge.BillingDetails.Address.Line2,
            City = stCharge.BillingDetails.Address.City == null ? string.Empty :
              stCharge.BillingDetails.Address.City,
            Country = stCharge.BillingDetails.Address.Country == null ? string.Empty :
              stCharge.BillingDetails.Address.Country,
            Name = stCharge.BillingDetails.Name == null ? string.Empty : stCharge.BillingDetails.Name,
            Phone = stCharge.BillingDetails.Phone == null ? string.Empty : stCharge.BillingDetails.Phone,
            Province = stCharge.BillingDetails.Address.State == null ? string.Empty :
              stCharge.BillingDetails.Address.State,
            Zip = stCharge.BillingDetails.Address.PostalCode == null ? string.Empty :
              stCharge.BillingDetails.Address.PostalCode
          };
          // Set Shopify order shipping address for the order.
          shOrder.ShippingAddress = new ShopifySharp.Address()
          {
            Address1 = Model.ShippingAddressSame ? Model.BillingAddress1 : Model.ShippingAddress1,
            Address2 = Model.ShippingAddressSame ? Model.BillingAddress2 : Model.ShippingAddress2,
            City = Model.ShippingAddressSame ? Model.BillingCity : Model.ShippingCity,
            Country = Model.ShippingAddressSame ? Model.BillingCountry : Model.ShippingCountry,
            Name = Model.ShippingAddressSame ? Model.BillingName : Model.ShippingName,
            Phone = _context.AspNetUsers.Where(
              x => x.Email.Equals(
                User.Identity.Name)).Select(x => x.PhoneNumber).FirstOrDefault() == null ?
              string.Empty : _context.AspNetUsers.Where(
              x => x.Email.Equals(User.Identity.Name)).Select(x => x.PhoneNumber).FirstOrDefault(),
            Province = Model.ShippingAddressSame ? Model.BillingState : Model.ShippingState,
            Zip = Model.ShippingAddressSame ? Model.BillingPostalCode : Model.ShippingPostalCode
          };
          // Submit order to Shopify.
          shOrder = await shOrderService.CreateAsync(shOrder);

          // Get the Shopify order id from the Shopify order object and save it to persist it to the db.
          order.ShopifyId = shOrder.Id;
          // Get the Shopify order transactions from the Shopify transaction service.
          shOrderTransactions = shTransactionService.ListAsync((long)shOrder.Id).Result.ToList();
          // Create a new Shopify transaction object using information from the Shopify order transaction object,
          // and other Shopify objects.
          shTransaction = new ShopifySharp.Transaction()
          {
            Kind = "capture",
            Gateway = "manual",
            Amount = total,
            ParentId = shOrderTransactions.FirstOrDefault().Id,
            Status = "success",
            Currency = stCharge.Currency.ToUpper(),
            Authorization = stCharge.AuthorizationCode,
            CreatedAt = stCharge.Created,
            DeviceId = shOrderTransactions.FirstOrDefault().DeviceId,
            LocationId = shOrder.LocationId,
            MaximumRefundable = subTotal + applicableTaxes,
            Message = "Transaction captured by Stripe on behalf of XO Skin.",
            OrderId = shOrder.Id,
            Source = shOrderTransactions.FirstOrDefault().Source,
          };
          // Populate Shopify transaction object with data obtained from the Shopify transaction service.
          shTransaction = await shTransactionService.CreateAsync((long)shOrder.Id, shTransaction);
        }
        catch (Exception ex)
        {
          // This will be caught by the parent catch.
          throw new Exception("An error was encountered while writing order to Shopify.", ex);
        }
        // Populated order db object fields.
        order.DatePlaced = DateTime.UtcNow;
        order.Subtotal = subTotal;
        order.ApplicableTaxes = applicableTaxes;
        order.CodeDiscount = codeDiscount;
        order.CouponDiscount = couponDiscount;
        order.GiftOrder = Model.IsGift;
        order.ShippingCost = shippingCost;
        order.Total = total;
        order.Completed = true;
        // Flag order data to be written the db.
        _context.ProductOrders.Update(order);
        // Write flagged order data to the db.
        _context.SaveChanges();
      }
      catch (Exception ex)
      {
        throw new Exception("An error was encontered while initializing the product order.", ex);
      }
      
      /** At this point we have already charged the customer. **/

      PopulateUIModelWithShopifyDataAndInitializeLineItems(ref Model, ref order, ref shOrder);

      // Add product order line items to the database, populate UI model to reflect this.
      Model = PersistOrderLineItemsToDatabase(order, product, originalProductStock, shProduct,
        shProductVariant, shProductService, shProductVariantService, shInventoryItem,
        shInventoryItemService, shLocation, shLocationService, shInventoryLevelService,
        updatedKit, kit, kitProduct, stock, originalKitStock, Model).Result;

      UpdateLedger(order, ref balanceBeforeTransaction, ref applicableTaxes, ref shippingCost,
        ref subTotal, ref couponDiscount, ref codeDiscount, ref stCharge, ref total);

      // Commence shipping label related code.
      try
      {
        // Initialize a JSON writer options object and configure it.
        var options = new JsonWriterOptions
        {
          Indented = true
        };
        using var stream = ConstructShippingLabelRequestJson(ref options);
        // Assign the resulting stream to a string variable now containing the 
        // fully-formed JSON object.
        RequestShippingLabelResponseJson(ref Model, ref seShippingLabelRequestJson, stream,
          ref seShippingLabelResponseJson);
        // Using a JsonDocument object which we obtain by parsing the previous request response.
        ParseJsonDocumentAndObtainTrackingNumberAndLabelUrl(ref Model, ref seShippingLabelResponseJson);
      }
      catch
      {
        // Fail silently and continue processing order. Shipping label will be created manually.
      }
      IfNoTrackingNumberYetAddMessageToModel(ref Model);
      // Adding bill-to address to order in database.
      try
      {
        AddBillToAddressToOrder(ref Model, ref order);
        IfThereIsACustomerBillingAddressRemoveIt();
        AddOrderBillingAddressToCustomerAsLatest(ref Model);
        // If the shipping address is the same as the billing address.
        if (Model.ShippingAddressSame)
        {
          AddOrderShippingAddressUsingBillingAddress(ref Model, ref order);
          IfCustomerHasShippingAddressRemoveIt();
          AddLatestBillToAddressAsLatestShipTo(ref Model);
        }
        // Else means the billing and shipping addresses are different.
        else
        {
          AddShipToAddressToOrder(ref Model, ref order);
          IfThereIsAPreviousShipToAddressForCustomerRemoveIt();
          AddNewShipToAddressToCustomerInDatabase(ref Model);
        }
      }
      catch (Exception ex)
      {
        throw new Exception("An error was encountered while saving the order's address(es.)", ex);
      }
      // Commence geolocation code, feeds map in UI.
      try
      {
        CreateGeoLocationUrl(ref geoLocationUrl, ref Model);
        ObtainParseGeoLocationJsonAddCoordinatesToModel(ref geoLocationUrl, ref geoLocationJson, ref Model);
      }
      catch
      {
        // Address was not found. Fail silently and continue. Geolocation will not be displayed for this address.
      }

      Model.GoogleMapsUrl = _option.Value.GoogleMapsUrl;

      AddLocalizedOrderConfirmationTextToViewData();

      return View("OrderConfirmation", Model);
    }

    private void InitializeProductOrder(ref List<ShopifySharp.LineItem> ShLineItemList,
      ref List<ShoppingCartLineItem> LineItem, ref decimal SubTotal, 
      ref ShopifySharp.ProductService ShProductService, ref object[] TjLineItem, 
      ref int I, ref ShopifySharp.Order ShOrder, ref Models.CheckoutViewModel Model,
      ref string SeShipmentRateJson, ref decimal ShippingCost, 
      ref OrderBillTo PreviousOrderBillTo, 
      ref CardCreateNestedOptions StCardCreateNestedOptions, 
      ref Stripe.CustomerCreateOptions StCustomerCreateOptions,
      ref Stripe.CustomerService StCustomerService, ref Stripe.Customer StCustomer)
    {
      // Create Shopify line item list.
      ShLineItemList = new List<ShopifySharp.LineItem>();

      // Iterate over line items in order.
      foreach (ShoppingCartLineItem cli in LineItem)
      {
        // Update subtotal calculation based on current line item.
        SubTotal += _context.Prices.Where(
          x => x.Id == _context.Products.Where(
            x => x.Id == cli.Product).Select(
            x => x.CurrentPrice).FirstOrDefault()).Select(x => x.Amount).FirstOrDefault() *
            cli.Quantity;

        // Add line item to Shopify line item list.
        ShLineItemList.Add(new ShopifySharp.LineItem()
        {
          ProductId = _context.Products.Where(x => x.Id == cli.Product).Select(
            x => x.ShopifyProductId).FirstOrDefault(),
          VariantId = ShProductService.GetAsync((long)_context.Products.Where(
            x => x.Id == cli.Product).Select(
            x => x.ShopifyProductId).FirstOrDefault()).Result.Variants.First().Id,
          Quantity = cli.Quantity,
          Taxable = true,
          RequiresShipping = true
        });

        // Add line item to TaxJar line item array.
        TjLineItem[I] = new
        {
          quantity = cli.Quantity,
          product_identifier = _context.Products.Where(
            x => x.Id == cli.Product).Select(x => x.Id).FirstOrDefault().ToString(),
          description = _context.Products.Where(
            x => x.Id == cli.Product).Select(x => x.Name).FirstOrDefault(),
          unit_price = _context.Prices.Where(
            x => x.Id == _context.Products.Where(
            x => x.Id == cli.Product).Select(
              x => x.CurrentPrice).FirstOrDefault()).Select(x => x.Amount).FirstOrDefault()
        };
        // Increment TaxJar line item array index.
        I++;
      }

      try
      {
        // Create Shopify order.
        ShOrder = new ShopifySharp.Order()
        {
          // Add billing address.
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
          // Add shipping address.
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

        // Get ShipEngine shipment rate Json document using ShipEngine shipment id obtained 
        // from CalculateShippingCostAndTaxes previously.
        SeShipmentRateJson = (_option.Value.ShipEngineGetShipmentCostFromIdPrefixUrl +
          Model.ShipEngineShipmentId +
          _option.Value.ShipEngineGetShipmentCostFromIdPostfixUrl).GetJsonFromUrl(
          requestFilter: webReq =>
          {
            webReq.Headers["API-Key"] = _option.Value.ShipEngineApiKey;
          }, responseFilter: null);

        // Parse the Json document to obtain shipping cost using configured defaults for carrier,
        // rate type, package type and service code.
        using (JsonDocument document = JsonDocument.Parse(SeShipmentRateJson))
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
              ShippingCost = decimal.Parse(
                rate.GetProperty("shipping_amount").GetProperty("amount").ToString());
              break;
            }
          }
        }

        // Assign shipping carrier and shipping charges to model.
        Model.ShippingCarrier = _option.Value.ShipEngineDefaultCarrierName;
        Model.ShippingCharges = ShippingCost;

        try
        {
          // Initialize Stripe api key.
          Stripe.StripeConfiguration.ApiKey = _option.Value.StripeSecretKey;
          PreviousOrderBillTo = _context.OrderBillTos.OrderByDescending(x => x.Order).FirstOrDefault();

          // If we need to create a Stripe user for the order's customer, create it.
          if ((_context.AspNetUsers.Where(
            x => x.Email.Equals(User.Identity.Name)).Select(
            x => x.StripeCustomerId).FirstOrDefault() == null &&
            PreviousOrderBillTo != null) &&
            ((PreviousOrderBillTo.NameOnCreditCard == null ? (Model.BillingName == null ||
            Model.BillingName.Trim().Equals(string.Empty)) :
            !PreviousOrderBillTo.NameOnCreditCard.Equals(Model.BillingName == null ?
              string.Empty : Model.BillingName.Trim())) ||
            (PreviousOrderBillTo.AddressLine1 == null ? (Model.BillingAddress1 == null ||
            Model.BillingAddress1.Trim().Equals(string.Empty)) :
            !PreviousOrderBillTo.AddressLine1.Equals(Model.BillingAddress1 == null ?
              string.Empty : Model.BillingAddress1.Trim())) ||
            (PreviousOrderBillTo.AddressLine2 == null ? (Model.BillingAddress2 == null ||
            Model.BillingAddress2.Trim().Equals(string.Empty)) :
            !PreviousOrderBillTo.AddressLine2.Equals(Model.BillingAddress2 == null ?
              string.Empty : Model.BillingAddress2.Trim())) ||
            (PreviousOrderBillTo.CityName == null ? (Model.BillingCity == null ||
            Model.BillingCity.Trim().Equals(string.Empty)) :
            !PreviousOrderBillTo.CityName.Equals(Model.BillingCity == null ?
              string.Empty : Model.BillingCity.Trim())) ||
            (PreviousOrderBillTo.StateName == null ? (Model.BillingState == null ||
            Model.BillingState.Trim().Equals(string.Empty)) :
            !PreviousOrderBillTo.StateName.Equals(Model.BillingState == null ?
              string.Empty : Model.BillingState.Trim())) ||
            (PreviousOrderBillTo.CountryName == null ? (Model.BillingCountry == null ||
            Model.BillingCountry.Trim().Equals(string.Empty)) :
            !PreviousOrderBillTo.CountryName.Equals(Model.BillingCountry == null ?
              string.Empty : Model.BillingCountry.Trim())) ||
            (PreviousOrderBillTo.PostalCode == null ? (Model.BillingPostalCode == null ||
            Model.BillingPostalCode.Trim().Equals(string.Empty)) :
            !PreviousOrderBillTo.PostalCode.Equals(Model.BillingPostalCode == null ?
            string.Empty : Model.BillingPostalCode.Trim()))))
          {
            try
            {
              // Create and initialize Stripe nested options object using Model data.
              StCardCreateNestedOptions = new CardCreateNestedOptions()
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

              // Create and initialize Stripe customer create options object.
              StCustomerCreateOptions = new Stripe.CustomerCreateOptions();
              StCustomerCreateOptions.Email = User.Identity.Name;
              StCustomerCreateOptions.Source = StCardCreateNestedOptions;
              StCustomerCreateOptions.Description = "XO Skin Customer.";

              // Instantiate a Stripe customer service object.
              StCustomerService = new Stripe.CustomerService();
              // Create a Stripe customer.
              StCustomer = StCustomerService.Create(StCustomerCreateOptions);

              // Null credit card information.
              StCustomerCreateOptions = null;
              Model.CreditCardNumber = null;
              Model.CreditCardCVC = null;
              Model.CreditCardExpirationDate = DateTime.MinValue;

              // TODO: Continue here.

              // Save Stripe customer id to user in database.
              user = _context.AspNetUsers.Where(
                x => x.Email.Equals(User.Identity.Name)).FirstOrDefault();
              user.StripeCustomerId = StCustomer.Id;
              _context.AspNetUsers.Update(user);
              _context.SaveChanges();
            }
            catch
            {
              // In case of error, pass a CardDeclined flag to the ui.
              Model.CardDeclined = true;
              Model.CalculatedShippingAndTaxes = true;
              return RedirectToAction("CalculateShippingCostAndTaxes", Model);
            }
          }
          // If the order's customer already has a Stripe customer id.
          else
          {
            // Instantiate a Stripe customer service object.
            StCustomerService = new Stripe.CustomerService();
            // Retrieve Stripe customer using id from database.
            stCustomer = StCustomerService.Get(_context.AspNetUsers.Where(
              x => x.Email.Equals(User.Identity.Name)).Select(
              x => x.StripeCustomerId).FirstOrDefault());

            // Create Stripe token card options object using data from the model
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
            // Create Stripe token create options object using token card options.
            stTokenCreateOptions = new TokenCreateOptions()
            {
              Card = stTokenCardOptions
            };

            // Instantiate Stripe token service.
            stTokenService = new TokenService();
            // Create Stripe token using token create options.
            stToken = stTokenService.Create(stTokenCreateOptions);

            // Null credit card information.
            stTokenCreateOptions = null;
            Model.CreditCardNumber = null;
            Model.CreditCardCVC = null;
            Model.CreditCardExpirationDate = DateTime.MinValue;

            // Instantiate a Stripe source create options object.
            stSourceCreateOptions = new SourceCreateOptions()
            {
              Token = stToken.Id,
              Type = SourceType.Card
            };

            // Once we instantiate the source create options object using the token id,
            // we null the token.
            stToken = null;

            // Instantiate Stripe source service client.
            stSourceService = new SourceService();
            // Instantiate Stripe source object using Stripe source create options.
            stSource = await stSourceService.CreateAsync(stSourceCreateOptions);

            // We attempt to attach our source service client to Stripe.
            try
            {
              stSourceService.Attach(stCustomer.Id, new SourceAttachOptions()
              {
                Source = stSource.Id
              });
            }
            catch
            {
              // In case of error, pass a CardDeclined flag to the ui.
              Model.CardDeclined = true;
              Model.CalculatedShippingAndTaxes = true;
              return RedirectToAction("CalculateShippingCostAndTaxes", Model);
            }
          }

          try
          {
            // Instantiate TaxJar (web api used to calculate taxes applicable to the sale) using key in the options configuration.
            tjService = new TaxjarApi(_option.Value.TaxJarApiKey);
            // Call TaxJar service to get tax rate information using our order's information.
            tjTaxRate = tjService.TaxForOrder(new
            {
              // This is self-explanatory.
              amount = (decimal)Model.SubTotal,
              from_city = _option.Value.ShipFromCity,
              from_country = _option.Value.ShipFromCountryCode,
              from_state = _option.Value.ShipFromState,
              from_street = _option.Value.ShipFromAddressLine1,
              from_zip = _option.Value.ShipFromPostalCode,
              line_items = tjLineItem,
              shipping = shippingCost,
              to_city = Model.ShippingAddressSame ? Model.BillingCity.Trim() : Model.ShippingCity.Trim(),
              to_country = Model.ShippingAddressSame ? Model.BillingCountry : Model.ShippingCountry,
              to_state = Model.ShippingAddressSame ? Model.BillingState : Model.ShippingState,
              // There is some boolean logic applied to calculating the right street address.
              to_street = Model.ShippingAddressSame ?
                (Model.BillingAddress1.Trim() + (Model.BillingAddress2 == null ||
                (Model.BillingAddress2 != null && Model.BillingAddress2.Trim() == string.Empty) ? string.Empty :
                " " + Model.BillingAddress2.Trim())) :
                (Model.ShippingAddress1.Trim() + (Model.ShippingAddress2 == null ||
                (Model.ShippingAddress2 != null && Model.ShippingAddress2.Trim() == string.Empty) ? string.Empty :
                " " + Model.ShippingAddress2.Trim())),
              to_zip = Model.ShippingAddressSame ? Model.BillingPostalCode : Model.ShippingPostalCode,
              // Handles the concept of nexus address(es). See https://quickbooks.intuit.com/r/taxes/nexus-guide/ (Retrieved 12/13/2022.)
              nexus_addresses = new[]
              {
                new
                {
                  id = _option.Value.ShipFromName,
                  country = _option.Value.ShipFromCountryCode,
                  zip = _option.Value.ShipFromPostalCode,
                  state = _option.Value.ShipFromState,
                  city = _option.Value.ShipFromCity,
                  street = _option.Value.ShipFromAddressLine1
                }
              }
            });
            // We create a TaxJar order object including information obtained from the previous service call and the actual line items.
            tjOrder = tjService.CreateOrder(new
            {
              transaction_id = Model.OrderId.ToString(),
              transaction_date = DateTime.UtcNow.ToString(),
              to_country = Model.ShippingAddressSame ? Model.BillingCountry : Model.ShippingCountry,
              to_state = Model.ShippingAddressSame ? Model.BillingState : Model.ShippingState,
              to_zip = Model.ShippingAddressSame ? Model.BillingPostalCode : Model.ShippingPostalCode,
              to_city = Model.ShippingAddressSame ? Model.BillingCity.Trim() : Model.ShippingCity.Trim(),
              // Same concept applied to the street address calculation for the order.
              to_street = Model.ShippingAddressSame ?
                (Model.BillingAddress1.Trim() + (Model.BillingAddress2 == null ||
                (Model.BillingAddress2 != null && Model.BillingAddress2.Trim() == string.Empty) ? string.Empty :
                " " + Model.BillingAddress2.Trim())) :
                (Model.ShippingAddress1.Trim() + (Model.ShippingAddress2 == null ||
                (Model.ShippingAddress2 != null && Model.ShippingAddress2.Trim() == string.Empty) ? string.Empty :
                " " + Model.ShippingAddress2.Trim())),
              amount = subTotal,
              lineItems = tjLineItem,
              // This is the only item we grab from the previous service call.
              sales_tax = tjTaxRate.AmountToCollect,
              shipping = shippingCost
            });
          }
          catch
          {
            // If the TaxJar-enabling switch on?
            if (_option.Value.TaxJarEnabled)
            {
              // Send a flag to the UI using the view model to indicate that the tax calculation service in unavailable.
              // A message will be displayed prompting the user to try again in a few minutes or to contact us.
              Model.TaxCalculationServiceOffline = true;
              Model.CalculatedShippingAndTaxes = true;
              return RedirectToAction("CalculateShippingCostAndTaxes", Model);
            }
          }

          // If the service is enabled in the configuration file.
          // This flag is normally set to true, allows us to switch it off from the configuration file in order to 
          // bypass the service for any given reason.
          if (_option.Value.TaxJarEnabled)
          {
            applicableTaxes = tjOrder.SalesTax;
          }
          else
          {
            // Sets the tax to a bogus value to remind us that the service is disabled.
            applicableTaxes = 0.00M;
          }

          // Pass the applicable taxes to the UI model.
          Model.Taxes = applicableTaxes;

          Model.CouponDiscount = 0.0M;
          Model.CodeDiscount = 0.0M;

          // Try to load any specified coupons.
          coupon = await _context.DiscountCoupons.FindAsync(Model.DiscountCouponId);
          // If there is a coupon.
          if (coupon != null)
          {
            // Process coupon data according to rules set on the database.
            couponProduct = _context.DiscountCouponProducts.Where(x => x.Coupon == coupon.Id).ToList();
            percentage = coupon.DiscountAsInNproductPercentage ?
            coupon.DiscountNproductPercentage == null ?
            0.0M : (decimal)coupon.DiscountNproductPercentage : coupon.DiscountAsInGlobalOrderPercentage ?
            coupon.DiscountGlobalOrderPercentage == null ? 0.0M : (decimal)coupon.DiscountGlobalOrderPercentage :
              0.0M;
            minimumNumberOfProducts = coupon.DiscountProductN == null ? (short)0 : (short)coupon.DiscountProductN;
            minimumPurchase = coupon.MinimumPurchase == null ? 0.0M : (decimal)coupon.MinimumPurchase;
            selectedProductSubTotal = 0.0M;
            minimumNumberOfSelectedProductsMet = true;
            totalNumberOfProducts = 0L;

            // If it's a global order percentage or global order in dollars coupon.
            if (coupon.DiscountAsInGlobalOrderPercentage || coupon.DiscountAsInGlobalOrderDollars)
            {
              // Calculate the total number of products.
              foreach (ShoppingCartLineItem item in lineItem)
              {
                totalNumberOfProducts += item.Quantity;
              }
              // If coupon rules are met.
              if (totalNumberOfProducts >= minimumNumberOfProducts &&
                Model.SubTotal >= minimumPurchase)
              {
                // Apply dollar or percentage discount.
                if (coupon.DiscountAsInGlobalOrderPercentage)
                {
                  Model.CouponDiscount = (Model.SubTotal * (coupon.DiscountGlobalOrderPercentage / 100));
                }
                else if (coupon.DiscountAsInGlobalOrderDollars)
                {
                  Model.CouponDiscount = coupon.DiscountGlobalOrderDollars;
                }
              }
            }
            // If it's a discount as in n product percentage or dollars coupon.
            else if (coupon.DiscountAsInNproductPercentage || coupon.DiscountAsInNproductDollars)
            {
              // If a coupon product exists and its count is larger than zero.
              if (couponProduct != null && couponProduct.Count > 0)
              {
                // If there's a minimum purchase required.
                if (minimumPurchase > 0)
                {
                  foreach (DiscountCouponProduct p in couponProduct)
                  {
                    // Calculate the subtotal for the selected product.
                    selectedProductSubTotal += lineItem.Find(x => x.Product == p.Product) == null ?
                      0.0M : (decimal)lineItem.Find(x => x.Product == p.Product).Total;
                  }
                }
                // For each product related to a coupon.
                foreach (DiscountCouponProduct p in couponProduct)
                {
                  // Determine whether the product quantity is below the minimum number of products for the coupon.
                  if (!lineItem.Any(x => x.Product == p.Product) || lineItem.Where(
                    x => x.Product == p.Product).FirstOrDefault().Quantity <= minimumNumberOfProducts)
                  {
                    // If so, set a flag and exit foreach.
                    minimumNumberOfSelectedProductsMet = false;
                    break;
                  }
                }
                // If the minimum number of selected products is met and the selected product subtotal is more or
                // equal to the minimum purchase specified by the coupon rules.
                if (minimumNumberOfSelectedProductsMet && selectedProductSubTotal >= minimumPurchase)
                {
                  // If it's a N product percentage discount.
                  if (coupon.DiscountAsInNproductPercentage)
                  {
                    // Apply discount.
                    Model.CouponDiscount = _context.Prices.Where(
                      x => x.Id == (_context.Products.FindAsync(
                      couponProduct.Last().Product).Result.CurrentPrice)).Select(x => x.Amount).FirstOrDefault() *
                      (coupon.DiscountNproductPercentage / 100);
                  }
                  // N product dollars discount.
                  else if (coupon.DiscountAsInNproductDollars)
                  {
                    // Apply coupon discount.
                    Model.CouponDiscount = coupon.DiscountInNproductDollars;
                  }
                }
              }
            }
          }
          // If there is a discount code.
          else if (Model.DiscountCode != null && Model.DiscountCode.Trim().Length > 0)
          {
            // Fetch the discount code information from the database.
            code = _context.DiscountCodes.Where(
              x => x.Code.Trim().ToUpper().Equals(Model.DiscountCode.Trim().ToUpper())).FirstOrDefault();
            // If there is discount code information on the database.
            if (code != null)
            {
              // Initialize discount calculation variables.
              codeProduct = _context.DiscountCodeProducts.Where(x => x.Code == code.Id).ToList();
              percentage = code.DiscountAsInNproductPercentage ?
                code.DiscountNproductPercentage == null ?
                0.0M : (decimal)code.DiscountNproductPercentage : code.DiscountAsInGlobalOrderPercentage ?
                code.DiscountGlobalOrderPercentage == null ? 0.0M : (decimal)code.DiscountGlobalOrderPercentage :
                0.0M;
              minimumNumberOfProducts = code.DiscountProductN == null ? (short)0 : (short)code.DiscountProductN;
              minimumPurchase = code.MinimumPurchase == null ? 0.0M : (decimal)code.MinimumPurchase;
              selectedProductSubTotal = 0.0M;
              minimumNumberOfSelectedProductsMet = true;
              totalNumberOfProducts = 0L;

              // If the discount code is for a global order discount.
              if (code.DiscountAsInGlobalOrderPercentage || code.DiscountAsInGlobalOrderDollars)
              {
                // Add the items and quantities to obtain the total number of products in the order.
                foreach (ShoppingCartLineItem item in lineItem)
                {
                  totalNumberOfProducts += item.Quantity;
                }

                // If the total number of products exceeds the minimum set by the discount code, and the 
                // order subtotal is greater or equal than the minimum purchase set by the same code.
                if (totalNumberOfProducts >= minimumNumberOfProducts &&
                  Model.SubTotal >= minimumPurchase)
                {
                  // Calculate discounts for globals, percentage or dollars.
                  if (code.DiscountAsInGlobalOrderPercentage)
                  {
                    Model.CodeDiscount = (Model.SubTotal * (code.DiscountGlobalOrderPercentage / 100));
                  }
                  else if (code.DiscountAsInGlobalOrderDollars)
                  {
                    Model.CodeDiscount = code.DiscountGlobalOrderDollars;
                  }
                }
              }
              // If the discount code is a n product type.
              else if (code.DiscountAsInNproductPercentage || code.DiscountAsInNproductDollars)
              {
                // Determine whether the discount applies, and if so, then apply it.
                if (codeProduct != null && codeProduct.Count > 0)
                {
                  // If the minimum purchase for this discount code is higher than zero.
                  if (minimumPurchase > 0)
                  {
                    // Add the selected product sub total.
                    foreach (DiscountCodeProduct p in codeProduct)
                    {
                      selectedProductSubTotal += lineItem.Find(x => x.Product == p.Product) == null ?
                        0.0M : (decimal)lineItem.Find(x => x.Product == p.Product).Total;
                    }
                  }
                  // Check if the cart has the product featured in the discount code and
                  // if the minumum number of products is met.
                  foreach (DiscountCodeProduct p in codeProduct)
                  {
                    if (!lineItem.Any(x => x.Product == p.Product) || lineItem.Where(
                      x => x.Product == p.Product).FirstOrDefault().Quantity <= minimumNumberOfProducts)
                    {
                      // If not, set a flag.
                      minimumNumberOfSelectedProductsMet = false;
                      break;
                    }
                  }
                  // If the correct flags have been set and the subtotal of selected products is 
                  // above the minimum purchase.
                  if (minimumNumberOfSelectedProductsMet && selectedProductSubTotal >= minimumPurchase)
                  {
                    // Apply the correct discount according to code discount rules in the db.
                    // Product percentage discount.
                    if (code.DiscountAsInNproductPercentage)
                    {
                      Model.CodeDiscount = _context.Prices.Where(
                        x => x.Id == (_context.Products.FindAsync(
                        codeProduct.Last().Product).Result.CurrentPrice)).Select(x => x.Amount).FirstOrDefault() *
                        (code.DiscountNproductPercentage / 100);
                    }
                    // Dollar discount.
                    else if (code.DiscountAsInNproductDollars)
                    {
                      Model.CodeDiscount = code.DiscountInNproductDollars;
                    }
                  }
                }
              }
            }
          }

          // Set discount(s).
          codeDiscount = Model.CodeDiscount == null ? 0.0M : (decimal)Model.CodeDiscount;
          couponDiscount = Model.CouponDiscount == null ? 0.0M : (decimal)Model.CouponDiscount;
          // Apply to total.
          total = subTotal - codeDiscount - couponDiscount + shippingCost + applicableTaxes;
          // Set meta data for Stripe transaction.
          stCreditTransactionMetaValue = new Dictionary<string, string>();
          stCreditTransactionMetaValue.Add("ProductOrderId", order.Id.ToString());
          stCreditTransactionMetaValue.Add("UserIdentityName", User.Identity.Name);
          // Create and initialize a Stripe charge creation options object.
          stChargeCreateOptions = new ChargeCreateOptions()
          {
            Amount = (long?)total * 100, // Stripe encodes dollar amounts as a nullable long.
            Currency = "usd",
            Description = "Total charges for an XO Skin customer order #XO" +
              (order.Id + 10000).ToString() +
              ". Customer: " + User.Identity.Name + ".",
            Metadata = stCreditTransactionMetaValue,
            ReceiptEmail = User.Identity.Name,
            Customer = stCustomer.Id
          };
          // Create and initialize Stripe charge service.
          stChargeService = new Stripe.ChargeService();
          stCharge = stChargeService.Create(stChargeCreateOptions);
          // If charge to cc succeeded.
          if (stCharge.Status.Equals("succeeded"))
          {
            // Set values to persist to db.
            order.StripeChargeId = stCharge.Id;
            order.StripeChargeStatus = stCharge.Status;
            Model.BilledOn = stCharge.Created;
          }
          else
          {
            // Pass the appropriate flags to the UI.
            Model.CardDeclined = true;
            Model.CalculatedShippingAndTaxes = true;
            return RedirectToAction("CalculateShippingCostAndTaxes", Model);
          }

          // Populate Stripe customer object using data stored in the db.
          stCustomer = stCustomerService.Get(_context.AspNetUsers.Where(
            x => x.Email.Equals(User.Identity.Name)).Select(x => x.StripeCustomerId).FirstOrDefault());
          // If the object has been correctly initialized.
          if (stCustomer.Sources != null)
          {
            // Create and initiaze Stripe "source" (financial instrument) service.
            stSourceService = new SourceService();
            foreach (Source s in stCustomer.Sources)
            {
              // Done with all sources so we detach them.
              stSourceService.Detach(stCustomer.Id, s.Id);
            }
          }
        }
        catch
        {
          // Any errors related to the transaction return the following flags to the UI.
          Model.CardDeclined = true;
          Model.CalculatedShippingAndTaxes = true;
          return RedirectToAction("CalculateShippingCostAndTaxes", Model);
        }

        // At this point the order has been charged to the customer-specified instrument.
        // we now proceed to record all the information related to the transaction
        // to both our db and Shopify.

        // Create and add shipping line (shipping data) to Shopify order object.
        shOrder.ShippingLines = new List<ShippingLine>()
          {
            new ShippingLine()
            {
              PriceSet = new PriceSet()
              {
                PresentmentMoney = new ShopifySharp.Price()
                {
                  Amount = shippingCost,
                  CurrencyCode = stCharge.Currency
                },
                ShopMoney = new ShopifySharp.Price()
                {
                  Amount = shippingCost,
                  CurrencyCode = stCharge.Currency
                }
              },
              Title = _option.Value.ShopifyShippingLineTitle,
              Code = _option.Value.ShopifyShippingLineCode,
              Source = _option.Value.ShopifyShippingLineSource,
              Price = shippingCost
            }
          };
        // Set tax information to the Shopify object.
        shOrder.EstimatedTaxes = false;
        shOrder.TotalTax = applicableTaxes;
        shOrder.TotalTaxSet = new PriceSet()
        {
          PresentmentMoney = new ShopifySharp.Price()
          {
            Amount = applicableTaxes,
            CurrencyCode = stCharge.Currency
          },
          ShopMoney = new ShopifySharp.Price()
          {
            Amount = applicableTaxes,
            CurrencyCode = stCharge.Currency
          }
        };
        // Create and add tax line (shipping data) to Shopify order object.
        shOrder.TaxLines = new List<TaxLine>()
          {
            new TaxLine()
            {
              Price = applicableTaxes,
              PriceSet = new PriceSet()
              {
                PresentmentMoney = new ShopifySharp.Price()
                {
                  Amount = applicableTaxes,
                  CurrencyCode = stCharge.Currency
                },
                ShopMoney = new ShopifySharp.Price()
                {
                  Amount = applicableTaxes,
                  CurrencyCode = stCharge.Currency
                }
              },
              Rate = applicableTaxes,
              Title = "Sales tax calculation performed by Taxjar."
            }
          };
        // Add payment processor information to Shopify order object.
        shOrder.PaymentGatewayNames = new List<string>()
          {
            "Stripe"
          };
        shOrder.ProcessingMethod = "Stripe payment gateway.";
        shOrder.SubtotalPrice = subTotal;
        shOrder.SubtotalPriceSet = new PriceSet()
        {
          PresentmentMoney = new ShopifySharp.Price()
          {
            Amount = subTotal,
            CurrencyCode = stCharge.Currency
          },
          ShopMoney = new ShopifySharp.Price()
          {
            Amount = subTotal,
            CurrencyCode = stCharge.Currency
          }
        };
        // Add line item totals to Shopify order object.
        shOrder.TotalLineItemsPrice = subTotal;
        shOrder.TotalLineItemsPriceSet = new PriceSet()
        {
          PresentmentMoney = new ShopifySharp.Price()
          {
            Amount = subTotal,
            CurrencyCode = stCharge.Currency
          },
          ShopMoney = new ShopifySharp.Price()
          {
            Amount = subTotal,
            CurrencyCode = stCharge.Currency
          }
        };
        // Do the same for total price.
        shOrder.TotalPrice = total;
        shOrder.TotalPriceSet = new PriceSet()
        {
          PresentmentMoney = new ShopifySharp.Price()
          {
            Amount = total,
            CurrencyCode = stCharge.Currency
          },
          ShopMoney = new ShopifySharp.Price()
          {
            Amount = total,
            CurrencyCode = stCharge.Currency
          }
        };
        // And shipping price.
        shOrder.TotalShippingPriceSet = new PriceSet()
        {
          PresentmentMoney = new ShopifySharp.Price
          {
            Amount = shippingCost,
            CurrencyCode = stCharge.Currency
          },
          ShopMoney = new ShopifySharp.Price()
          {
            Amount = shippingCost,
            CurrencyCode = stCharge.Currency
          }
        };

        // Add coupon discount information to Shopify order object.
        if (Model.CouponDiscount != null && Model.CouponDiscount > 0)
        {
          discountCode = new List<ShopifySharp.DiscountCode>();
          discountCode.Add(new ShopifySharp.DiscountCode()
          {
            Amount = ((decimal)Model.CouponDiscount).ToString(),
            Code = Model.DiscountCouponId.ToString(),
            Type = "Discount Coupon Id"
          });
          shOrder.DiscountCodes = discountCode;
          shOrder.TotalDiscounts = Model.CouponDiscount;
          shOrder.TotalDiscountsSet = new PriceSet()
          {
            PresentmentMoney = new ShopifySharp.Price()
            {
              Amount = Model.CouponDiscount,
              CurrencyCode = stCharge.Currency
            },
            ShopMoney = new ShopifySharp.Price()
            {
              Amount = Model.CouponDiscount,
              CurrencyCode = stCharge.Currency
            }
          };
        }
        // Add code discount information to Shopify order object.
        else if (Model.CodeDiscount != null && Model.CodeDiscount > 0)
        {
          discountCode = new List<ShopifySharp.DiscountCode>();
          discountCode.Add(new ShopifySharp.DiscountCode()
          {
            Amount = ((decimal)Model.CodeDiscount).ToString(),
            Code = Model.DiscountCode,
            Type = "Discount Code"
          });
          shOrder.DiscountCodes = discountCode;
          shOrder.TotalDiscounts = Model.CodeDiscount;
          shOrder.TotalDiscountsSet = new PriceSet()
          {
            PresentmentMoney = new ShopifySharp.Price()
            {
              Amount = Model.CodeDiscount,
              CurrencyCode = stCharge.Currency
            },
            ShopMoney = new ShopifySharp.Price()
            {
              Amount = Model.CodeDiscount,
              CurrencyCode = stCharge.Currency
            }
          };
        }

        // Set remaining properties of the Shopify order object prior to submitting data to 
        // Shopify.
        shOrder.PresentmentCurrency = stCharge.Currency.ToUpper();
        shOrder.Currency = stCharge.Currency.ToUpper();
        shOrder.Name = "#XO" + (order.Id + 10000).ToString();
        shOrder.OrderStatusUrl = _option.Value.ShopifyOrderStatusUrl + order.Id.ToString();
        shOrder.CreatedAt = DateTime.UtcNow;
        shOrder.LineItems = ShLineItemList;
        shOrder.Test = false;
        shOrder.TaxesIncluded = false;
        shOrder.Test = false;  // Switch to "true" for testing against a production store.
        shOrder.Customer = await shCustomerService.GetAsync((long)_context.AspNetUsers.Where(
          x => x.Email.Equals(User.Identity.Name)).Select(x => x.ShopifyCustomerId).FirstOrDefault());
        shOrder.FinancialStatus = "pending";
        shOrder.Transactions = new List<Transaction>()
          {
            new Transaction()
            {
              Amount = total,
              Kind = "authorization",
              Status = "success",
              CreatedAt = stCharge.Created,
              Currency = stCharge.Currency.ToUpper(),
              DeviceId = shOrder.DeviceId.ToString(),
              Gateway = shOrder.PaymentGatewayNames.FirstOrDefault(),
              LocationId = shOrder.LocationId,
              MaximumRefundable = subTotal + applicableTaxes,
              Message = "Transaction authorized by Stripe on behalf of XO Skin.",
              Source = shOrder.SourceName,
              Authorization = stCharge.AuthorizationCode
            }
          };
        // Store the customer's ip address for future reference.
        shOrder.BrowserIp = clientIpAddress;
        // Set Shopify order billing address for the order.
        shOrder.BillingAddress = new ShopifySharp.Address()
        {
          Address1 = stCharge.BillingDetails.Address.Line1 == null ? string.Empty :
            stCharge.BillingDetails.Address.Line1,
          Address2 = stCharge.BillingDetails.Address.Line2 == null ? string.Empty :
            stCharge.BillingDetails.Address.Line2,
          City = stCharge.BillingDetails.Address.City == null ? string.Empty :
            stCharge.BillingDetails.Address.City,
          Country = stCharge.BillingDetails.Address.Country == null ? string.Empty :
            stCharge.BillingDetails.Address.Country,
          Name = stCharge.BillingDetails.Name == null ? string.Empty : stCharge.BillingDetails.Name,
          Phone = stCharge.BillingDetails.Phone == null ? string.Empty : stCharge.BillingDetails.Phone,
          Province = stCharge.BillingDetails.Address.State == null ? string.Empty :
            stCharge.BillingDetails.Address.State,
          Zip = stCharge.BillingDetails.Address.PostalCode == null ? string.Empty :
            stCharge.BillingDetails.Address.PostalCode
        };
        // Set Shopify order shipping address for the order.
        shOrder.ShippingAddress = new ShopifySharp.Address()
        {
          Address1 = Model.ShippingAddressSame ? Model.BillingAddress1 : Model.ShippingAddress1,
          Address2 = Model.ShippingAddressSame ? Model.BillingAddress2 : Model.ShippingAddress2,
          City = Model.ShippingAddressSame ? Model.BillingCity : Model.ShippingCity,
          Country = Model.ShippingAddressSame ? Model.BillingCountry : Model.ShippingCountry,
          Name = Model.ShippingAddressSame ? Model.BillingName : Model.ShippingName,
          Phone = _context.AspNetUsers.Where(
            x => x.Email.Equals(
              User.Identity.Name)).Select(x => x.PhoneNumber).FirstOrDefault() == null ?
            string.Empty : _context.AspNetUsers.Where(
            x => x.Email.Equals(User.Identity.Name)).Select(x => x.PhoneNumber).FirstOrDefault(),
          Province = Model.ShippingAddressSame ? Model.BillingState : Model.ShippingState,
          Zip = Model.ShippingAddressSame ? Model.BillingPostalCode : Model.ShippingPostalCode
        };
        // Submit order to Shopify.
        shOrder = await shOrderService.CreateAsync(shOrder);

        // Get the Shopify order id from the Shopify order object and save it to persist it to the db.
        order.ShopifyId = shOrder.Id;
        // Get the Shopify order transactions from the Shopify transaction service.
        shOrderTransactions = shTransactionService.ListAsync((long)shOrder.Id).Result.ToList();
        // Create a new Shopify transaction object using information from the Shopify order transaction object,
        // and other Shopify objects.
        shTransaction = new Transaction()
        {
          Kind = "capture",
          Gateway = "manual",
          Amount = total,
          ParentId = shOrderTransactions.FirstOrDefault().Id,
          Status = "success",
          Currency = stCharge.Currency.ToUpper(),
          Authorization = stCharge.AuthorizationCode,
          CreatedAt = stCharge.Created,
          DeviceId = shOrderTransactions.FirstOrDefault().DeviceId,
          LocationId = shOrder.LocationId,
          MaximumRefundable = subTotal + applicableTaxes,
          Message = "Transaction captured by Stripe on behalf of XO Skin.",
          OrderId = shOrder.Id,
          Source = shOrderTransactions.FirstOrDefault().Source,
        };
        // Populate Shopify transaction object with data obtained from the Shopify transaction service.
        shTransaction = await shTransactionService.CreateAsync((long)shOrder.Id, shTransaction);
      }
      catch (Exception ex)
      {
        // This will be caught by the parent catch.
        throw new Exception("An error was encountered while writing order to Shopify.", ex);
      }
      // Populated order db object fields.
      order.DatePlaced = DateTime.UtcNow;
      order.Subtotal = subTotal;
      order.ApplicableTaxes = applicableTaxes;
      order.CodeDiscount = codeDiscount;
      order.CouponDiscount = couponDiscount;
      order.GiftOrder = Model.IsGift;
      order.ShippingCost = shippingCost;
      order.Total = total;
      order.Completed = true;
      // Flag order data to be written the db.
      _context.ProductOrders.Update(order);
      // Write flagged order data to the db.
      _context.SaveChanges();
    }

    private void PopulateUIModelWithShopifyDataAndInitializeLineItems(ref Models.CheckoutViewModel Model, 
      ref ORM.ProductOrder Order, ref ShopifySharp.Order ShOrder)
    {
      // Populate the ui model with data obtained from Shopify. 
      Model.OrderId = Order.Id;
      Model.ShopifyId = (long)ShOrder.Id;
      // Initialize ui model line items.
      Model.LineItem = new List<XOSkinWebApp.Models.ShoppingCartLineItemViewModel>();
    }

    private async Task<XOSkinWebApp.Models.CheckoutViewModel> PersistOrderLineItemsToDatabase(
      ProductOrder Order, ORM.Product Product, 
      long OriginalProductStock, ShopifySharp.Product ShProduct, 
      ProductVariant ShProductVariant, ShopifySharp.ProductService ShProductService,
      ProductVariantService ShProductVariantService, InventoryItem ShInventoryItem,
      InventoryItemService ShInventoryItemService, List<Location> ShLocation, 
      LocationService ShLocationService, InventoryLevelService ShInventoryLevelService,
      List<long> UpdatedKit, ORM.Product Kit, List<KitProduct> KitProduct, long Stock,
      long OriginalKitStock, XOSkinWebApp.Models.CheckoutViewModel Model)
    {
      // Add product order line items to the database by iterating over 
      // the shopping cart line items.
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
            // Data to be saved to database product order line items table.
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
            ProductOrder = Order.Id,
            Quantity = item.Quantity,
            Total = _context.Prices.Where(
              x => x.Id == _context.Products.Where(
              x => x.Id == item.Product).Select(x => x.CurrentPrice).FirstOrDefault()).Select(
              x => x.Amount).FirstOrDefault() * item.Quantity
          });
          // Persist data to db.
          _context.SaveChanges();
          // Update the local db and Shopify inventory.
          // Start with regular products (non-kit-related.)
          if (_context.Products.Where(x => x.Id == item.Product).Select(x => x.KitType).FirstOrDefault() == null)
          {
            // Update local db with new inventory information.
            Product = _context.Products.Where(x => x.Id == item.Product).FirstOrDefault();
            OriginalProductStock = (long)Product.Stock;
            Product.Stock -= item.Quantity;
            _context.Products.Update(Product);
            _context.SaveChanges();

            try
            {
              // Update Shopify with new inventory information.
              ShProduct = await ShProductService.GetAsync((long)Product.ShopifyProductId);
              ShProductVariant = await ShProductVariantService.GetAsync((long)ShProduct.Variants.First().Id);
              ShInventoryItem = await ShInventoryItemService.GetAsync((long)ShProductVariant.InventoryItemId);
              ShLocation = (List<Location>)await ShLocationService.ListAsync();
              // Call inventory levels service to make the inventory adjustment.
              await ShInventoryLevelService.AdjustAsync(new InventoryLevelAdjust()
              {
                AvailableAdjustment = (int?)(-1 * item.Quantity),
                InventoryItemId = ShInventoryItem.Id,
                LocationId = ShLocation.First().Id // Change this when we get multiple locations.
              });
            }
            catch (Exception ex)
            {
              throw new Exception("An error was encountered while updating Shopify product inventory levels.", ex);
            }

            UpdatedKit = new List<long>();

            // Update local db for kit products.
            foreach (KitProduct kp in _context.KitProducts.ToList())
            {
              if (kp.Product == Product.Id)
              {
                Kit = _context.Products.Where(x => x.Id == kp.Kit).FirstOrDefault();

                if (!UpdatedKit.Any(x => x.Equals(Kit.Id)))
                {
                  KitProduct = _context.KitProducts.Where(x => x.Kit == Kit.Id).ToList();
                  Stock = long.MaxValue;

                  foreach (KitProduct kpB in KitProduct)
                  {
                    if (_context.Products.Where(
                      x => x.Id == kpB.Product).Select(x => x.Stock).FirstOrDefault() < Stock)
                    {
                      Stock = (long)_context.Products.Where(
                        x => x.Id == kpB.Product).Select(x => x.Stock).FirstOrDefault();
                    }
                  }

                  OriginalKitStock = (long)Kit.Stock;
                  Kit.Stock = Stock;
                  // Update kit stock in local db.
                  _context.Products.Update(Kit);
                  _context.SaveChanges();

                  try
                  {
                    // Update kit stock in Shopify.
                    ShProduct = await ShProductService.GetAsync((long)Kit.ShopifyProductId);
                    ShProductVariant = await ShProductVariantService.GetAsync((long)ShProduct.Variants.First().Id);
                    ShInventoryItem = await ShInventoryItemService.GetAsync((long)ShProductVariant.InventoryItemId);
                    ShLocation = (List<Location>)await ShLocationService.ListAsync();
                    // Call inventory levels service and make the actual adjustment.
                    await ShInventoryLevelService.AdjustAsync(new InventoryLevelAdjust()
                    {
                      AvailableAdjustment = (int?)(Kit.Stock - OriginalKitStock),
                      InventoryItemId = ShInventoryItem.Id,
                      LocationId = ShLocation.First().Id // Change this when we get multiple locations.
                    });

                    UpdatedKit.Add(Kit.Id);
                  }
                  catch (Exception ex)
                  {
                    throw new Exception("An error was encountered while updating Shopify kit inventory levels.", ex);
                  }
                }
              }
            }
          }
          // It's a kit product, process accordingly.
          else
          {
            Kit = _context.Products.Where(x => x.Id == item.Product).FirstOrDefault();
            KitProduct = _context.KitProducts.Where(x => x.Kit == Kit.Id).ToList();
            // Iterate over products in kit.
            foreach (KitProduct kp in KitProduct)
            {
              Product = _context.Products.Where(x => x.Id == kp.Product).FirstOrDefault();
              Product.Stock -= 1 * item.Quantity;
              // Save updated inventory levels to db.
              _context.Products.Update(Product);
              _context.SaveChanges();

              try
              {
                // Save updated inventory levels to Shopify.
                ShProduct = await ShProductService.GetAsync((long)Product.ShopifyProductId);
                ShProductVariant = await ShProductVariantService.GetAsync((long)ShProduct.Variants.First().Id);
                ShInventoryItem = await ShInventoryItemService.GetAsync((long)ShProductVariant.InventoryItemId);
                ShLocation = (List<Location>)await ShLocationService.ListAsync();
                // Call inventory levels service to make all necessary adjustments.
                await ShInventoryLevelService.AdjustAsync(new InventoryLevelAdjust()
                {
                  AvailableAdjustment = -1 * item.Quantity,
                  InventoryItemId = ShInventoryItem.Id,
                  LocationId = ShLocation.First().Id
                });
              }
              catch (Exception ex)
              {
                throw new Exception("An error was encountered while updating Shopify kit-product inventory levels.", ex);
              }
            }

            UpdatedKit = new List<long>();

            // Update kit stock in local database.
            foreach (KitProduct kp in KitProduct)
            {
              foreach (KitProduct kpB in _context.KitProducts.ToList())
              {
                if (kpB.Product == kp.Product)
                {
                  Kit = _context.Products.Where(x => x.Id == kpB.Kit).FirstOrDefault();

                  if (!UpdatedKit.Any(x => x.Equals(Kit.Id)))
                  {
                    KitProduct = _context.KitProducts.Where(x => x.Kit == Kit.Id).ToList();
                    Stock = long.MaxValue;

                    foreach (KitProduct kpC in KitProduct)
                    {
                      if (_context.Products.Where(
                        x => x.Id == kpC.Product).Select(x => x.Stock).FirstOrDefault() < Stock)
                      {
                        Stock = (long)_context.Products.Where(
                          x => x.Id == kpC.Product).Select(x => x.Stock).FirstOrDefault();
                      }
                    }

                    OriginalKitStock = (long)Kit.Stock;
                    Kit.Stock = Stock;

                    try
                    {
                      // Update Shopify with appropriate kit inventory adjustment.
                      ShProduct = await ShProductService.GetAsync((long)Kit.ShopifyProductId);
                      ShProductVariant = await ShProductVariantService.GetAsync((long)ShProduct.Variants.First().Id);
                      ShInventoryItem = await ShInventoryItemService.GetAsync((long)ShProductVariant.InventoryItemId);
                      ShLocation = (List<Location>)await ShLocationService.ListAsync();
                      // Call inventory levels service to make the adjustment.
                      await ShInventoryLevelService.AdjustAsync(new InventoryLevelAdjust()
                      {
                        AvailableAdjustment = (int?)(Kit.Stock - OriginalKitStock),
                        InventoryItemId = ShInventoryItem.Id,
                        LocationId = ShLocation.First().Id
                      });
                    }
                    catch (Exception ex)
                    {
                      throw new Exception("An error was encountered while updating Shopify kit inventory levels.", ex);
                    }

                    UpdatedKit.Add(Kit.Id);
                    // Save updated kit information to the local database.
                    _context.Products.Update(Kit);
                    _context.SaveChanges();
                  }
                }
              }
            }
          }
          // Save any pending changes to the local db.
          _context.SaveChanges();
          // Add current item to the UI's model for line items.
          Model.LineItem.Add(new XOSkinWebApp.Models.ShoppingCartLineItemViewModel()
          {
            Id = item.Id,
            ProductId = item.Product,
            ImageSource = _context.Products.Where(
              x => x.Id.Equals(item.Product)).Select(x => x.ImagePathLarge).FirstOrDefault(),
            ProductName = _context.ProductOrderLineItems.Where(
              x => x.ProductOrder == Order.Id).Where(
              x => x.Product == item.Product).Select(x => x.Name).FirstOrDefault(),
            ProductDescription = _context.ProductOrderLineItems.Where(
              x => x.ProductOrder == Order.Id).Where(
              x => x.Product == item.Product).Select(x => x.Description).FirstOrDefault(),
            Quantity = item.Quantity,
            Total = _context.Prices.Where(
              x => x.Id == _context.Products.Where(
              x => x.Id == item.Product).Select(x => x.CurrentPrice).FirstOrDefault()).Select(
              x => x.Amount).FirstOrDefault() * item.Quantity
          });
          // Remove item from current shopping cart as it's been processed already.
          _context.ShoppingCartLineItems.Remove(item);
          // Save this change to the database.
          _context.SaveChanges();
        }
        return Model;
      }
      catch (Exception ex)
      {
        throw (new Exception("An error was encountered while adding order line items to the database.", ex));
      }
    }

    private void UpdateLedger(ProductOrder Order, ref decimal BalanceBeforeTransaction, ref decimal ApplicableTaxes, 
      ref decimal ShippingCost, ref decimal SubTotal, ref decimal CouponDiscount, ref decimal CodeDiscount, 
      ref Stripe.Charge StCharge, ref decimal Total)
    {
      try
      {
        // If the user has an existing ledger associated with it, grab the current balance.
        if (_context.UserLedgerTransactions.Where(
          x => x.User == Order.User).OrderBy(x => x.Id).LastOrDefault() != null)
        {
          // and assign it to our running balance variable.
          BalanceBeforeTransaction = _context.UserLedgerTransactions.Where(
            x => x.User == Order.User).OrderBy(x => x.Id).LastOrDefault().BalanceAfterTransaction;
        }
        // Add a new transaction representing the current purchase, this time for taxation.
        _context.UserLedgerTransactions.Add(new UserLedgerTransaction()
        {
          User = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
          ProductOrder = Order.Id,
          TransactionType = 2, // Debit.
          Description = "Order #" + Order.Id + ". Taxes.",
          Concept = 3, // Taxation.
          Amount = ApplicableTaxes,
          BalanceBeforeTransaction = BalanceBeforeTransaction,
          BalanceAfterTransaction = BalanceBeforeTransaction - ApplicableTaxes,
          CreatedBy = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
          Created = DateTime.UtcNow
        });
        // Save new transaction to database.
        _context.SaveChanges();
        // Affect balance negatively using previously calculated applicable taxes.
        BalanceBeforeTransaction -= ApplicableTaxes;
        // Add another transaction to the ledger, this time covering shipping.
        _context.UserLedgerTransactions.Add(new UserLedgerTransaction()
        {
          User = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
          ProductOrder = Order.Id,
          TransactionType = 2, // Debit.
          Description = "Order #" + Order.Id + ". Shipping.",
          Concept = 2, // Shipping.
          Amount = ShippingCost,
          BalanceBeforeTransaction = BalanceBeforeTransaction,
          BalanceAfterTransaction = BalanceBeforeTransaction - ShippingCost,
          CreatedBy = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
          Created = DateTime.UtcNow
        });
        // Save new transaction to database.
        _context.SaveChanges();
        // Deduct shipping costs from balance.
        BalanceBeforeTransaction -= ShippingCost;
        // Add a new ledger transaction for the product subtotal.
        _context.UserLedgerTransactions.Add(new UserLedgerTransaction()
        {
          User = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
          ProductOrder = Order.Id,
          TransactionType = 2, // Debit.
          Description = "Order #" + Order.Id + ". Product.",
          Concept = 1, // Product.
          Amount = SubTotal,
          BalanceBeforeTransaction = BalanceBeforeTransaction,
          BalanceAfterTransaction = BalanceBeforeTransaction - SubTotal,
          CreatedBy = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
          Created = DateTime.UtcNow
        });
        // Deduct product subtotal from balance.
        BalanceBeforeTransaction -= SubTotal;
        // If there's either a coupon or code discount.
        if (CouponDiscount + CodeDiscount > 0)
        {
          // Create a new ledger transaction recording the discount as such.
          _context.UserLedgerTransactions.Add(new UserLedgerTransaction()
          {
            User = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
            ProductOrder = Order.Id,
            TransactionType = 1, // Credit.
            Description = "Order #" + Order.Id + ". Discount.",
            Concept = 6, // Discount.
            Amount = CouponDiscount + CodeDiscount,
            BalanceBeforeTransaction = BalanceBeforeTransaction,
            BalanceAfterTransaction = BalanceBeforeTransaction + CouponDiscount + CodeDiscount,
            CreatedBy = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
            Created = DateTime.UtcNow
          });
          // Save ledger transaction to database.
          _context.SaveChanges();
          // Add discount to balance.
          BalanceBeforeTransaction += CouponDiscount + CodeDiscount;
        }
        // Add a new ledger transaction for the total amount credited to this account.
        _context.UserLedgerTransactions.Add(new UserLedgerTransaction()
        {
          User = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
          ProductOrder = Order.Id,
          TransactionType = 1, // Credit.
          Description = "Order #" + Order.Id + ". Payment. Stripe charge identification: " + StCharge.Id + ".",
          Concept = 4, // Total.
          Amount = Total,
          BalanceBeforeTransaction = BalanceBeforeTransaction,
          BalanceAfterTransaction = BalanceBeforeTransaction + Total,
          CreatedBy = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
          Created = DateTime.UtcNow
        });
        // Save this final transaction to finalize the ledger entries for this order.
        _context.SaveChanges();
      }
      catch(Exception ex)
      {
        throw new Exception("An error was encountered while saving the order's ledger.", ex);
      }
    } 

    private MemoryStream ConstructShippingLabelRequestJson(ref JsonWriterOptions Options)
    {
      // Initialize a memory buffer.
      using var stream = new MemoryStream();
      // Initialize a JSON writer object (UTF8) with said resources and configuration.
      using var writer = new Utf8JsonWriter(stream, Options);
      // Write mailing label related information to a JSON memory stream.
      writer.WriteStartObject();
      writer.WritePropertyName("label_format");
      writer.WriteStringValue("pdf");
      writer.WritePropertyName("label_layout");
      writer.WriteStringValue("4x6");
      writer.WritePropertyName("label_download_type");
      writer.WriteStringValue("url");
      writer.WriteEndObject();

      writer.Flush();

      return stream;
    }

    private void RequestShippingLabelResponseJson(ref Models.CheckoutViewModel Model, ref string SeShippingLabelRequestJson,
      MemoryStream Stream, ref string SeShippingLabelResponseJson)
    {
      // Assign the resulting stream to a string variable now containing the 
      // fully-formed JSON object.
      SeShippingLabelRequestJson = Encoding.UTF8.GetString(Stream.ToArray());
      // Assign the result of a http request to the ShipEngine API with the
      // shipping label parameters to a string variable.
      SeShippingLabelResponseJson =
        (_option.Value.ShipEngineGetLabelUrlFromIdUrl + Model.ShipEngineRateId).PostJsonToUrlAsync(
          SeShippingLabelRequestJson,
        requestFilter: webReq =>
        {
          webReq.Headers["API-Key"] = _option.Value.ShipEngineApiKey;
        }).Result;
    }

    private void ParseJsonDocumentAndObtainTrackingNumberAndLabelUrl(ref Models.CheckoutViewModel Model, 
      ref string SeShippingLabelResponseJson)
    {
      using (JsonDocument document = JsonDocument.Parse(SeShippingLabelResponseJson))
      {
        // Grab the root element.
        JsonElement root = document.RootElement;
        // Transverse the tree to obtain the the tracking number and url 
        // for the pdf generated by ShipEngine for us to pick up later.
        Model.TrackingNumber = root.GetProperty("tracking_number").ToString();
        Model.ShipEngineLabelUrl = root.GetProperty("label_download").GetProperty("pdf").ToString();
      }
    }

    private void IfNoTrackingNumberYetAddMessageToModel(ref Models.CheckoutViewModel Model)
    {
      // If we weren't able to retrieve the tracking number, assign a text
      // explaining it will be assigned soon, for UI consumption.
      if (Model.TrackingNumber == null || Model.TrackingNumber.Trim().Count() == 0)
      {
        Model.TrackingNumber = "Will be Assigned Soon.";
      }
    }

    private void AddBillToAddressToOrder(ref Models.CheckoutViewModel Model, ref ProductOrder Order)
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
        Order = Order.Id
      });
      // Save information to database.
      _context.SaveChanges();
    }

    private void IfThereIsACustomerBillingAddressRemoveIt()
    {
      // If there's a previous billing address on the customer's address book
      if (_context.Addresses.Where(
        x => x.User.Equals(_context.AspNetUsers.Where(
        x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault())).Where(
        x => x.AddressType == 1).FirstOrDefault() != null)
      {
        // remove it.
        _context.Addresses.Remove(_context.Addresses.Where(
          x => x.User.Equals(_context.AspNetUsers.Where(
          x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault())).Where(
          x => x.AddressType == 1).FirstOrDefault());
        // Persist this change to database.
        _context.SaveChanges();
      }
    }

    private void AddOrderBillingAddressToCustomerAsLatest(ref Models.CheckoutViewModel Model)
    {
      // Add billing address used in this purchase as the latest
      // (to be suggested on next purchase.)
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
      // Save latest billing address.
      _context.SaveChanges();
    }

    private void AddOrderShippingAddressUsingBillingAddress(ref Models.CheckoutViewModel Model, ref ProductOrder Order)
    {
      // Add the database's order shipping address
      // using the supplied billing address.
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
        Order = Order.Id,
        Arrives = Model.ExpectedToArrive,
        ShipEngineId = Model.ShipEngineShipmentId,
        ShipEngineRateId = Model.ShipEngineRateId,
        ShippingLabelUrl = Model.ShipEngineLabelUrl
      });
      // Save this information to the database.
      _context.SaveChanges();
    }

    private void IfCustomerHasShippingAddressRemoveIt()
    {
      // If there's a shipping address saved for this customer.
      if (_context.Addresses.Where(
        x => x.User.Equals(_context.AspNetUsers.Where(
        x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault())).Where(
        x => x.AddressType == 2).FirstOrDefault() != null)
      {
        // Remove it to make way for the new default shipping address.
        _context.Addresses.Remove(_context.Addresses.Where(
          x => x.User.Equals(_context.AspNetUsers.Where(
          x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault())).Where(
          x => x.AddressType == 2).FirstOrDefault());
        // Persist change to database.
        _context.SaveChanges();
      }
    }

    private void AddLatestBillToAddressAsLatestShipTo(ref Models.CheckoutViewModel Model)
    {
      // Add latest bill to address as latest ship to address to be suggested in next
      // purchase.
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
      // Save new ship address to database.
      _context.SaveChanges();
    }

    private void AddShipToAddressToOrder(ref Models.CheckoutViewModel Model, ref ProductOrder Order)
    {
      // Add ship to address to be attached to order.
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
        Order = Order.Id,
        Arrives = Model.ExpectedToArrive,
        ShipEngineId = Model.ShipEngineShipmentId,
        ShipEngineRateId = Model.ShipEngineRateId,
        ShippingLabelUrl = Model.ShipEngineLabelUrl
      });
      // Save it to the databse.
      _context.SaveChanges();
    }

    private void IfThereIsAPreviousShipToAddressForCustomerRemoveIt()
    {
      // If there's a previous ship to address for this customer
      if (_context.Addresses.Where(
        x => x.User.Equals(_context.AspNetUsers.Where(
        x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault())).Where(
        x => x.AddressType == 2).FirstOrDefault() != null)
      {
        // Remove it.
        _context.Addresses.Remove(_context.Addresses.Where(
          x => x.User.Equals(_context.AspNetUsers.Where(
          x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault())).Where(
          x => x.AddressType == 2).FirstOrDefault());
        // Persist change to database.
        _context.SaveChanges();
      }
    }

    private void AddNewShipToAddressToCustomerInDatabase(ref Models.CheckoutViewModel Model)
    {
      // Add new shipping address to customer in database.
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
      // Save changes to database.
      _context.SaveChanges();
    }

    private void ObtainParseGeoLocationJsonAddCoordinatesToModel(ref string GeoLocationUrl, ref string GeoLocationJson, 
      ref Models.CheckoutViewModel Model)
    {
      GeoLocationJson = (GeoLocationUrl).GetJsonFromUrl();
      // Create JSON document to obtain data from Bing.
      // We obtain coordinates from address using Bing Maps API.
      // Then we feed these coordinates to the Google Maps API to render the 
      // appropriate map in the UI by copying them to the page Model.
      using (JsonDocument document = JsonDocument.Parse(GeoLocationJson))
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

    private void CreateGeoLocationUrl(ref string GeoLocationUrl, ref Models.CheckoutViewModel Model)
    {
      GeoLocationUrl = new string(_option.Value.BingMapsGeolocationUrl)
          .Replace("{adminDistrict}", Model.ShippingAddressSame ? Model.BillingState : Model.ShippingState)
          .Replace("{postalCode}", Model.ShippingAddressSame ?
            Model.BillingPostalCode.Trim() : Model.ShippingPostalCode.Trim())
          .Replace("{locality}", Model.ShippingAddressSame ? Model.BillingCity.Trim() : Model.ShippingCity.Trim())
          .Replace("{addressLine}", Model.ShippingAddressSame ?
          Model.BillingAddress1.Trim() + (Model.BillingAddress2 == null ||
          (Model.BillingAddress2 != null && Model.BillingAddress2.Trim() == string.Empty) ? string.Empty :
          " " + Model.BillingAddress2.Trim()) :
          Model.ShippingAddress1.Trim() + (Model.ShippingAddress2 == null ||
          (Model.ShippingAddress2 != null && Model.ShippingAddress2.Trim() == string.Empty) ? string.Empty :
          " " + Model.ShippingAddress2.Trim()))
          .Replace("{includeNeighborhood}", "0")
          .Replace("{includeValue}", string.Empty)
          .Replace("{maxResults}", "1")
          .Replace("{BingMapsAPIKey}", _option.Value.BingMapsKey);
      GeoLocationUrl = HttpUtility.UrlPathEncode(GeoLocationUrl);
    }

    private void AddLocalizedOrderConfirmationTextToViewData()
    {
      ViewData.Add("OrderConfirmation.WelcomeText", _context.LocalizedTexts.Where(
       x => x.PlacementPointCode.Equals("OrderConfirmation.WelcomeText"))
       .Select(x => x.Text).FirstOrDefault());
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
