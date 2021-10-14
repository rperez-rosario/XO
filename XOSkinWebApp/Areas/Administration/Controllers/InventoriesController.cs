using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using XOSkinWebApp.Areas.Administration.Models;
using XOSkinWebApp.ConfigurationHelper;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Areas.Administration.Controllers
{
  [Area("Administration")]
  [Authorize(Roles = "Administrator")]
  public class InventoriesController : Controller
  {
    private readonly XOSkinContext _context; 
    private readonly IOptions<Option> _option;

    public InventoriesController(XOSkinContext context, IOptions<Option> option)
    {
      _context = context;
      _option = option;
    }

    // GET: Administration/Inventories
    public async Task<IActionResult> Index()
    {
      List<InventoryViewModel> inventory = new List<InventoryViewModel>();

      foreach (Product product in await _context.Products.ToListAsync())
      {
        inventory.Add(new InventoryViewModel()
        {
          CurrentStock = (long)product.Stock,
          NewStock = (long)product.Stock,
          ProductId = product.Id,
          ProductName = product.Name,
          Sku = product.Sku,
          IsKit = product.KitType != null
        });
      }

      return View(inventory);
    }

    public async Task<IActionResult> UpdateInventory(long ProductId, int Quantity)
    {
      List<InventoryViewModel> inventory = new List<InventoryViewModel>();
      Product product = null;
      long lowestStock = long.MaxValue;
      long originalProductStock = 0L;
      List<long> kitList = new List<long>();
      ShopifySharp.ProductService shProductService = null;
      ShopifySharp.ProductVariantService shProductVariantService = null;
      ShopifySharp.InventoryItemService shInventoryItemService = null;
      ShopifySharp.LocationService shLocationService = null;
      ShopifySharp.InventoryLevelService shInventoryLevelService = null;
      ShopifySharp.Product shProduct = null;
      ShopifySharp.ProductVariant shProductVariant = null;
      ShopifySharp.InventoryItem shInventoryItem = null;
      List<ShopifySharp.Location> shLocation = null;

      try
      {
        product = await _context.Products.FindAsync(ProductId);
        originalProductStock = (long)product.Stock;
        product.Stock = Quantity;
        _context.Products.Update(product);
        await _context.SaveChangesAsync();

        shProductService = new ShopifySharp.ProductService(
          _option.Value.ShopifyUrl, _option.Value.ShopifyStoreFrontAccessToken);
        shProductVariantService = new ShopifySharp.ProductVariantService(
          _option.Value.ShopifyUrl, _option.Value.ShopifyStoreFrontAccessToken);
        shInventoryItemService = new ShopifySharp.InventoryItemService(
          _option.Value.ShopifyUrl, _option.Value.ShopifyStoreFrontAccessToken);
        shLocationService = new ShopifySharp.LocationService(
          _option.Value.ShopifyUrl, _option.Value.ShopifyStoreFrontAccessToken);
        shInventoryLevelService = new ShopifySharp.InventoryLevelService(
          _option.Value.ShopifyUrl, _option.Value.ShopifyStoreFrontAccessToken);

        shProduct = await shProductService.GetAsync((long)product.ShopifyProductId);
        shProductVariant = await shProductVariantService.GetAsync((long)shProduct.Variants.First().Id);
        shInventoryItem = await shInventoryItemService.GetAsync((long)shProductVariant.InventoryItemId);
        shLocation = (List<ShopifySharp.Location>)await shLocationService.ListAsync();
        await shInventoryLevelService.AdjustAsync(new ShopifySharp.InventoryLevelAdjust()
        {
          AvailableAdjustment = (int?)(product.Stock - originalProductStock),
          InventoryItemId = shInventoryItem.Id,
          LocationId = shLocation.First().Id // Change this when we get multiple locations.
        });
      }
      catch (Exception ex)
      {
        throw new Exception("An error was encountered while trying to save product inventory data.", ex);
      }
      
      try
      {
        foreach (KitProduct kp in await _context.KitProducts.ToListAsync())
        {
          if (kp.Product == ProductId)
          {
            kitList.Add((long)kp.Kit);
          }
        }

        foreach (long kitId in kitList)
        {
          lowestStock = long.MaxValue;
          foreach (KitProduct kp in _context.KitProducts.Where(x => x.Kit == kitId))
          {
            product = await _context.Products.FindAsync(kp.Product);
            if (product.Stock < lowestStock)
            {
              lowestStock = (long)product.Stock;
            }
          }
          product = await _context.Products.FindAsync(kitId);
          originalProductStock = (long)product.Stock;
          product.Stock = lowestStock;
          _context.Products.Update(product);
          await _context.SaveChangesAsync();
          shProduct = await shProductService.GetAsync((long)product.ShopifyProductId);
          shProductVariant = await shProductVariantService.GetAsync((long)shProduct.Variants.First().Id);
          shInventoryItem = await shInventoryItemService.GetAsync((long)shProductVariant.InventoryItemId);
          shLocation = (List<ShopifySharp.Location>)await shLocationService.ListAsync();
          await shInventoryLevelService.AdjustAsync(new ShopifySharp.InventoryLevelAdjust()
          {
            AvailableAdjustment = (int?)(product.Stock - originalProductStock),
            InventoryItemId = shInventoryItem.Id,
            LocationId = shLocation.First().Id // Change this when we get multiple locations.
          });
        }
      }
      catch (Exception ex)
      {
        throw new Exception("An error was encountered while trying to save kit product inventory data.", ex);
      }
      
      foreach (Product p in await _context.Products.ToListAsync())
      {
        inventory.Add(new InventoryViewModel()
        {
          CurrentStock = (long)p.Stock,
          NewStock = (long)p.Stock,
          ProductId = p.Id,
          ProductName = p.Name,
          Sku = p.Sku,
          IsKit = p.KitType != null
        });
      }

      return View("Index", inventory);
    }
  }
}