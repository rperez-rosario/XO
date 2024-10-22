﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServiceStack;
using XOSkinWebApp.Areas.Administration.Models;
using XOSkinWebApp.ConfigurationHelper;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Areas.Administration.Controllers
{
  [Area("Administration")]
  [Authorize(Roles = "Administrator")]
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
        if (shipment != null && billing != null)
        {
          order.Add(new OrderViewModel()
          {
            Arrives = shipment.Arrives,
            Carrier = shipment.CarrierName,
            DatePlaced = po.DatePlaced,
            NumberOfItems = TotalQuantityOfItems(po.Id),
            OrderId = po.Id,
            Recipient = shipment.RecipientName,
            TrackingNumber = shipment.TrackingNumber,
            Status = billing.Refunded != null ? ((bool)billing.Refunded ? "REFUNDED" :
            (shipment.Shipped == null ? "SHIPPING SOON" : (bool)shipment.Shipped ?
            "SHIPPED: " + ((DateTime)shipment.ActualShipDate).ToShortDateString() : "SHIPPING SOON")) :
            (shipment.Shipped == null ? "SHIPPING SOON" : (bool)shipment.Shipped ?
            "SHIPPED: " + ((DateTime)shipment.ActualShipDate).ToShortDateString() : "SHIPPING SOON"),
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

      try
      {
        order = _context.ProductOrders.Where(x => x.Id == Id).FirstOrDefault();

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
          ShippedOn = (shipping.Shipped == null || (bool)!shipping.Shipped) ? 
            (DateTime)shipping.ShipDate : (DateTime)shipping.ActualShipDate,
          FulfillmentStatus = billing.Refunded != null ? ((bool)billing.Refunded ? "REFUNDED" :
            (shipping.Shipped == null ? "SHIPPING SOON" : (bool)shipping.Shipped ?
            "SHIPPED: " + ((DateTime)shipping.ActualShipDate).ToShortDateString() : "SHIPPING SOON")) :
            (shipping.Shipped == null ? "SHIPPING SOON" : (bool)shipping.Shipped ?
            "SHIPPED: " + ((DateTime)shipping.ActualShipDate).ToShortDateString() : "SHIPPING SOON"),
          ShippingName = shipping.RecipientName,
          ShippingCountry = shipping.CountryName,
          ShippingAddress1 = shipping.AddressLine1,
          ShippingAddress2 = shipping.AddressLine2,
          ShippingAddressSame = ShippingAddressSame(billing, shipping),
          ExpectedToArrive = (shipping.Shipped == null || (bool)!shipping.Shipped) ? 
            (DateTime)shipping.Arrives : (DateTime)shipping.ActualArrives,
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
          CancellationStatus = (order.Cancelled == null || (bool)!order.Cancelled) ? "NOT REQUESTED" : "CANCELLED",
          CancellationDate = order.CancelledOn,
          CancelReason = order.CancelReason,
          CancelledBy = order.CancelledBy == null ? String.Empty : 
            _context.AspNetUsers.FindAsync(order.CancelledBy).Result.Email,
          RefundStatus = (billing.RefundRequested == null || (bool)!billing.RefundRequested) ? "NOT REQUESTED" :
            (billing.Refunded == null || (bool)!billing.Refunded) ? "REQUESTED" : "REFUNDED",
          RefundDate = billing.RefundedOn,
          RefundAmount = billing.RefundAmount,
          RefundReason = billing.RefundReason,
          RefundedBy = billing.RefundedBy == null ? String.Empty : 
            _context.AspNetUsers.FindAsync(billing.RefundedBy).Result.Email,
          Shipped = shipping.Shipped == null ? false : (bool)shipping.Shipped,
          Cancelled = order.Cancelled == null ? false : (bool)order.Cancelled,
          Refunded = billing.Refunded == null ? false : (bool)billing.Refunded
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

      ViewData.Add("OrderDetails.WelcomeText", _context.LocalizedTexts.Where(
        x => x.PlacementPointCode.Equals("OrderDetails.WelcomeText")).Select(x => x.Text).FirstOrDefault());

      return View(checkout);
    }

    public async Task<IActionResult> CancelOrder([Bind("OrderId, CancelReason")] CheckoutViewModel Model)
    {
      ProductOrder order = _context.ProductOrders.FindAsync(Model.OrderId).Result;
      OrderShipTo shipping = _context.OrderShipTos.Where(x => x.Order == Model.OrderId).FirstOrDefault();
      OrderBillTo billing = _context.OrderBillTos.Where(x => x.Order == Model.OrderId).FirstOrDefault();
      List<ProductOrderLineItem> orderProduct = await _context.ProductOrderLineItems.Where(
        x => x.ProductOrder == Model.OrderId).ToListAsync();
      Product product = null;
      ShopifySharp.ProductService shProductService = null;
      ShopifySharp.ProductVariantService shProductVariantService = null;
      ShopifySharp.InventoryItemService shInventoryItemService = null;
      ShopifySharp.LocationService shLocationService = null;
      ShopifySharp.InventoryLevelService shInventoryLevelService = null;
      ShopifySharp.Product shProduct = null;
      ShopifySharp.ProductVariant shProductVariant = null;
      ShopifySharp.InventoryItem shInventoryItem = null;
      List<ShopifySharp.Location> shLocation = null;
      ShopifySharp.OrderService shOrderService = null;
      ShopifySharp.Order shOrder = null;
      long lowestStock = long.MaxValue;
      long originalKitStock = long.MaxValue;
      Stripe.RefundService stRefundService = null;
      Stripe.Refund stRefund = null;
      Stripe.RefundCreateOptions stRefundCreateOptions = null;
      UserLedgerTransaction userLedgerTransaction = null;
      decimal balanceBeforeTransaction = 0.0M;

      // Cancel order on app and Shopify.
      order.Cancelled = true;
      order.CancelledOn = DateTime.UtcNow;
      order.CancelReason = Model.CancelReason;
      order.CancelledBy = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault();
      _context.ProductOrders.Update(order);
      await _context.SaveChangesAsync();
      shOrderService = new ShopifySharp.OrderService(_option.Value.ShopifyUrl, _option.Value.ShopifyStoreFrontAccessToken);
      await shOrderService.CancelAsync((long)order.ShopifyId);
      shOrder = await shOrderService.GetAsync((long)order.ShopifyId);
      shOrder.CancelledAt = order.CancelledOn;
      shOrder.CancelReason = order.CancelReason;
      shOrder = await shOrderService.UpdateAsync((long)shOrder.Id, shOrder);
      // Increment inventory on app and Shopify.
      shProductService = new ShopifySharp.ProductService(_option.Value.ShopifyUrl, _option.Value.ShopifyStoreFrontAccessToken);
      shProductVariantService = new ShopifySharp.ProductVariantService(
        _option.Value.ShopifyUrl, _option.Value.ShopifyStoreFrontAccessToken);
      shInventoryItemService = new ShopifySharp.InventoryItemService(
        _option.Value.ShopifyUrl, _option.Value.ShopifyStoreFrontAccessToken);
      shLocationService = new ShopifySharp.LocationService(_option.Value.ShopifyUrl, _option.Value.ShopifyStoreFrontAccessToken);
      shInventoryLevelService = new ShopifySharp.InventoryLevelService(
        _option.Value.ShopifyUrl, _option.Value.ShopifyStoreFrontAccessToken);
      foreach (ProductOrderLineItem pli in orderProduct)
      {
        product = await _context.Products.FindAsync(pli.Product);
        if (pli.KitType != null)
        {
          lowestStock = long.MaxValue;
          originalKitStock = long.MaxValue;
          foreach (KitProduct kp in await _context.KitProducts.Where(x => x.Kit == pli.Product).ToListAsync())
          {
            product = await _context.Products.FindAsync(kp.Product);
            product.Stock += pli.Quantity;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            shProduct = await shProductService.GetAsync((long)product.ShopifyProductId);
            shProductVariant = await shProductVariantService.GetAsync((long)shProduct.Variants.First().Id);
            shInventoryItem = await shInventoryItemService.GetAsync((long)shProductVariant.InventoryItemId);
            shLocation = (List<ShopifySharp.Location>)await shLocationService.ListAsync();
            await shInventoryLevelService.AdjustAsync(new ShopifySharp.InventoryLevelAdjust()
            {
              AvailableAdjustment = (int?)(pli.Quantity),
              InventoryItemId = shInventoryItem.Id,
              LocationId = shLocation.First().Id // Change this when we get multiple locations.
            });
            if (product.Stock < lowestStock)
            {
              lowestStock = (long)product.Stock;
            }
          }
          product = await _context.Products.FindAsync(pli.Product);
          originalKitStock = (long)product.Stock;
          product.Stock = lowestStock;
          _context.Products.Update(product);
          await _context.SaveChangesAsync();
          shProduct = await shProductService.GetAsync((long)product.ShopifyProductId);
          shProductVariant = await shProductVariantService.GetAsync((long)shProduct.Variants.First().Id);
          shInventoryItem = await shInventoryItemService.GetAsync((long)shProductVariant.InventoryItemId);
          shLocation = (List<ShopifySharp.Location>)await shLocationService.ListAsync();
          await shInventoryLevelService.AdjustAsync(new ShopifySharp.InventoryLevelAdjust()
          {
            AvailableAdjustment = (int?)(product.Stock - originalKitStock),
            InventoryItemId = shInventoryItem.Id,
            LocationId = shLocation.First().Id // Change this when we get multiple locations.
          });
        }
        else
        {
          product.Stock += pli.Quantity;
          _context.Products.Update(product);
          await _context.SaveChangesAsync();
          shProduct = await shProductService.GetAsync((long)product.ShopifyProductId);
          shProductVariant = await shProductVariantService.GetAsync((long)shProduct.Variants.First().Id);
          shInventoryItem = await shInventoryItemService.GetAsync((long)shProductVariant.InventoryItemId);
          shLocation = (List<ShopifySharp.Location>)await shLocationService.ListAsync();
          await shInventoryLevelService.AdjustAsync(new ShopifySharp.InventoryLevelAdjust()
          {
            AvailableAdjustment = (int?)(pli.Quantity),
            InventoryItemId = shInventoryItem.Id,
            LocationId = shLocation.First().Id // Change this when we get multiple locations.
          });
        }
      }
      // Issue refund on app and Stripe.
      Stripe.StripeConfiguration.ApiKey = _option.Value.StripeSecretKey;
      stRefundService = new Stripe.RefundService();
      stRefundCreateOptions = new Stripe.RefundCreateOptions()
      {
        Amount = (long)((order.Subtotal + order.ApplicableTaxes) * 100),
        Reason = Stripe.RefundReasons.RequestedByCustomer,
        Charge = order.StripeChargeId
      };
      stRefund = await stRefundService.CreateAsync(stRefundCreateOptions);
      order.StripeRefundId = stRefund.Id;
      _context.ProductOrders.Update(order);
      await _context.SaveChangesAsync();

      billing.RefundAmount = order.Subtotal + order.ApplicableTaxes;
      billing.Refunded = true;
      billing.RefundedBy = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault();
      billing.RefundedOn = DateTime.UtcNow;
      billing.RefundReason = Model.CancelReason;
      billing.RefundRequested = true;
      _context.OrderBillTos.Update(billing);
      await _context.SaveChangesAsync();

      // Affect app ledger with cancel information.
      if (_context.UserLedgerTransactions.Where(
          x => x.User == order.User).OrderBy(x => x.Id).LastOrDefault() != null)
      {
        balanceBeforeTransaction = _context.UserLedgerTransactions.Where(
          x => x.User == order.User).OrderBy(x => x.Id).LastOrDefault().BalanceAfterTransaction;
      }
      userLedgerTransaction = new UserLedgerTransaction()
      {
        Amount = (stRefund.Amount / 100),
        BalanceBeforeTransaction = balanceBeforeTransaction,
        BalanceAfterTransaction = balanceBeforeTransaction + ((decimal)order.Subtotal + (decimal)order.ApplicableTaxes),
        Concept = 5, // Refund.
        ProductOrder = order.Id,
        Created = DateTime.UtcNow,
        CreatedBy = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
        Description = "Order #" + order.Id + ". Cancelled order refund credit.",
        TransactionType = 1, // Credit.
        User = order.User
      };
      _context.UserLedgerTransactions.Add(userLedgerTransaction);
      _context.SaveChanges();

      balanceBeforeTransaction += (stRefund.Amount / 100);

      userLedgerTransaction = new UserLedgerTransaction()
      {
        Amount = stRefund.Amount / 100,
        BalanceBeforeTransaction = balanceBeforeTransaction,
        BalanceAfterTransaction = balanceBeforeTransaction - ((decimal)order.Subtotal + (decimal)order.ApplicableTaxes),
        Concept = 5, // Refund.
        ProductOrder = order.Id,
        Created = DateTime.UtcNow,
        CreatedBy = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
        Description = "Order #" + order.Id + ". Cancelled order refund debit. Stripe refund identification: " + stRefund.Id + ".",
        TransactionType = 2, // Debit.
        User = order.User
      };
      _context.UserLedgerTransactions.Add(userLedgerTransaction);
      _context.SaveChanges();

      return RedirectToAction("Index");
    }

    public async Task<IActionResult> RefundOrder([Bind("OrderId", "RefundReason")] CheckoutViewModel Model)
    {
      ProductOrder order = _context.ProductOrders.FindAsync(Model.OrderId).Result;
      OrderShipTo shipping = _context.OrderShipTos.Where(x => x.Order == Model.OrderId).FirstOrDefault();
      OrderBillTo billing = _context.OrderBillTos.Where(x => x.Order == Model.OrderId).FirstOrDefault();
      Stripe.RefundService stRefundService = null;
      Stripe.Refund stRefund = null;
      Stripe.RefundCreateOptions stRefundCreateOptions = null;
      UserLedgerTransaction userLedgerTransaction = null;
      decimal balanceBeforeTransaction = 0.0M;

      // Issue refund on app and Stripe.
      Stripe.StripeConfiguration.ApiKey = _option.Value.StripeSecretKey;
      stRefundService = new Stripe.RefundService();
      stRefundCreateOptions = new Stripe.RefundCreateOptions()
      {
        Amount = (long)((order.Subtotal + order.ApplicableTaxes) * 100),
        Reason = Stripe.RefundReasons.RequestedByCustomer,
        Charge = order.StripeChargeId
      };
      stRefund = await stRefundService.CreateAsync(stRefundCreateOptions);
      order.StripeRefundId = stRefund.Id;
      _context.ProductOrders.Update(order);
      await _context.SaveChangesAsync();
      billing.RefundAmount = ((decimal)order.Subtotal + (decimal)order.ApplicableTaxes);
      billing.Refunded = true;
      billing.RefundedBy = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault();
      billing.RefundedOn = DateTime.UtcNow;
      billing.RefundReason = Model.RefundReason;
      billing.RefundRequested = true;

      _context.OrderBillTos.Update(billing);
      await _context.SaveChangesAsync();

      // Affect app ledger with cancel information.
      if (_context.UserLedgerTransactions.Where(
          x => x.User == order.User).OrderBy(x => x.Id).LastOrDefault() != null)
      {
        balanceBeforeTransaction = _context.UserLedgerTransactions.Where(
          x => x.User == order.User).OrderBy(x => x.Id).LastOrDefault().BalanceAfterTransaction;
      }
      userLedgerTransaction = new UserLedgerTransaction()
      {
        Amount = ((decimal)order.Subtotal + (decimal)order.ApplicableTaxes),
        BalanceBeforeTransaction = balanceBeforeTransaction,
        BalanceAfterTransaction = balanceBeforeTransaction + ((decimal)order.Subtotal + (decimal)order.ApplicableTaxes),
        Concept = 5, // Refund.
        ProductOrder = order.Id,
        Created = DateTime.UtcNow,
        CreatedBy = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
        Description = "Order #" + order.Id + ". Refund request credit.",
        TransactionType = 1, // Credit.
        User = order.User
      };
      _context.UserLedgerTransactions.Add(userLedgerTransaction);
      _context.SaveChanges();

      balanceBeforeTransaction += ((decimal)order.Subtotal + (decimal)order.ApplicableTaxes);

      userLedgerTransaction = new UserLedgerTransaction()
      {
        Amount = stRefund.Amount / 100,
        BalanceBeforeTransaction = balanceBeforeTransaction,
        BalanceAfterTransaction = balanceBeforeTransaction - ((decimal)order.Subtotal + (decimal)order.ApplicableTaxes),
        Concept = 5, // Refund.
        ProductOrder = order.Id,
        Created = DateTime.UtcNow,
        CreatedBy = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
        Description = "Order #" + order.Id + ". Refund request debit. Stripe refund identification: " + stRefund.Id + ".",
        TransactionType = 2, // Debit.
        User = order.User
      };
      _context.UserLedgerTransactions.Add(userLedgerTransaction);
      _context.SaveChanges();

      return RedirectToAction("Index");
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
