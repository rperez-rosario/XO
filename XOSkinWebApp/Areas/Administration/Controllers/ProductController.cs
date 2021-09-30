using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using XOSkinWebApp.Areas.Administration.Models;
using XOSkinWebApp.ORM;
using ShopifySharp;
using Microsoft.Extensions.Options;
using XOSkinWebApp.ConfigurationHelper;

namespace XOSkinWebApp.Areas.Administration.Controllers
{
  [Area("Administration")]
  [Authorize(Roles = "Administrator")]
  public class ProductController : Controller
  {
    private readonly XOSkinContext _context;
    private readonly IOptions<Option> _option;

    public ProductController(XOSkinContext context, IOptions<Option> option)
    {
      _context = context;
      _option = option;
    }

    // GET: Administration/Product
    public async Task<IActionResult> Index()
    {
      List<ORM.Product> product = null;
      List<ProductViewModel> productViewModel = null;

      try
      {
        product = await _context.Products.OrderByDescending(x => x.Created).ToListAsync();
      }
      catch (Exception ex)
      {
        throw new Exception("An error was encountered while loading product form data.", ex);
      }

      productViewModel = new List<ProductViewModel>();

      foreach (ORM.Product p in product)
      {
        productViewModel.Add(new ProductViewModel()
        {
          Id = p.Id,
          ShopifyProductId = p.ShopifyProductId,
          Active = p.Active,
          Created = p.Created,
          CreatedBy = _context.AspNetUsers.Where(x => x.Id.Equals(p.CreatedBy)).Select(x => x.Email).FirstOrDefault(),
          CurrentPriceId = p.CurrentPrice,
          CurrentPrice = _context.Prices.Where(x => x.Id == p.CurrentPrice).Select(x => x.Amount).FirstOrDefault(),
          Description = p.Description,
          ImagePathLarge = p.ImagePathLarge,
          ImagePathMedium = p.ImagePathMedium,
          ImagePathSmall = p.ImagePathSmall,
          Ingredient = p.ProductIngredients,
          KitType = p.KitType,
          KitTypeName = _context.KitTypes.Where(x => x.Id == p.KitType).Select(x => x.Name).FirstOrDefault(),
          LastUpdated = p.LastUpdated,
          LastUpdatedBy = p.LastUpdatedBy,
          Name = p.Name,
          Ph = p.Ph,
          ProductCategory = p.ProductCategory,
          ProductType = p.ProductType,
          Sku = p.Sku,
          Stock = p.Stock,
          VolumeInFluidOunces = p.VolumeInFluidOunces,
          ShippingWeightLb = p.ShippingWeightLb,
          Sample = (bool)p.Sample
        });
      }

      return View(productViewModel);
    }

    // GET: Administration/Product/Create
    public IActionResult Create()
    {
      try
      {
        ViewData["ProductType"] = new MultiSelectList(_context.ProductTypes, "Id", "Name");
        ViewData["Price"] = new SelectList(_context.Prices.Where(
          x => x.Active), "Id", "Amount");
        ViewData["Cost"] = new SelectList(_context.Costs.Where(
          x => x.Active), "Id", "Amount");
        ViewData["Ingredient"] = new MultiSelectList(_context.Ingredients.OrderBy(x => x.Name), "Id", "Name");
        ViewData["KitType"] = new SelectList(_context.KitTypes, "Id", "Name");
        ViewData["Product"] = new SelectList(_context.Products.Where(
          x => x.Active).Where(
          x => x.KitType == null).OrderBy(x => x.Name), "Id", "Name");
      }
      catch (Exception ex)
      {
        throw new Exception("An error was encountered while loading product creation form data.", ex);
      }
      
      return View();
    }

    // POST: Administration/Product/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
      [Bind("Id,ShopifyProductId,Sku,Name,Description,ProductType,ProductCategory,Kit,KitType,KitProduct,Ingredient," +
      "VolumeInFluidOunces,Ph,ShippingWeightLb,Stock,CurrentPrice,CurentCost,CurrentPriceId,CurrentCostId,ImagePathSmall," +
      "ImagePathMedium,ImagePathLarge,ImageLarge,Active,Sample,CreatedBy,Created,LastUpdatedBy,LastUpdated")] 
      ProductViewModel productViewModel,
      IFormFile ImageLarge, long[] KitProduct, long[] Ingredient)
    {
      ORM.Product product = null;
      ShopifySharp.Product sProduct = null;
      ProductVariant sProductVariant = null;
      List<Location> sLocation = null;
      InventoryItem sInventoryItem = null;
      ProductService sProductService = null;
      ProductVariantService sProductVariantService = null;
      InventoryLevelService sInventoryLevelService = null;
      InventoryItemService sInventoryItemService = null;
      LocationService sLocationService = null;
      decimal weight = 0.0M;
      decimal pH = 0.0M;
      decimal volume = 0.0M;
      List<ProductIngredient> productIngredient = null;
      bool emptyStock = false;
      long minStock = long.MaxValue;
      String filePathPrefix = "wwwroot/img/product/xo-img-pid-";
      String srcPathPrefix = "/img/product/xo-img-pid-";
      FileStream stream = null;
      int i = 0;
      
      if (ModelState.IsValid)
      {
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

          sProduct = new ShopifySharp.Product()
          {
            BodyHtml = productViewModel.Description,
            CreatedAt = productViewModel.Created,
            ProductType = _context.ProductTypes.Where(
              x => x.Id == productViewModel.Id).Select(x => x.Name).FirstOrDefault(),
            Status = productViewModel.Active ? "active" : "draft",
            Title = productViewModel.Name,
            PublishedAt = DateTime.UtcNow
          };

          sProduct = await sProductService.CreateAsync(sProduct);

          for (; i < KitProduct.Length; i++)
          {
            weight += (decimal)_context.Products.Where(
              x => x.Id == KitProduct[i]).Select(x => x.ShippingWeightLb).FirstOrDefault();
          }

          sProductVariant = sProduct.Variants.First();
          sProductVariant.Price = _context.Prices.Where(
            x => x.Id == productViewModel.CurrentPriceId).Select(x => x.Amount).FirstOrDefault();
          sProductVariant.SKU = productViewModel.Sku.Trim();
          sProductVariant.Taxable = true;
          sProductVariant.TaxCode = "92127";
          sProductVariant.Weight = productViewModel.Kit ? weight : productViewModel.ShippingWeightLb;
          sProductVariant.WeightUnit = "lb";
          sProductVariant.RequiresShipping = true;
          sProductVariant.UpdatedAt = DateTime.UtcNow;
          sProductVariant.InventoryQuantity = null;

          await sProductVariantService.UpdateAsync((long)sProductVariant.Id, sProductVariant);

          sLocation = (List<Location>)await sLocationService.ListAsync();

          sInventoryItem = await sInventoryItemService.GetAsync((long)sProductVariant.InventoryItemId);
          
          sInventoryItem.Tracked = true;
          sInventoryItem.Cost = _context.Costs.Where(
            x => x.Id == productViewModel.CurrentCostId).Select(
            x => x.Amount).FirstOrDefault();
          sInventoryItem.CountryCodeOfOrigin = "US"; // Map this if we use products not made in the U.S.

          await sInventoryItemService.UpdateAsync((long)sProductVariant.InventoryItemId, sInventoryItem);

          if (productViewModel.Kit)
          {
            for (i = 0; i < KitProduct.Length; i++)
            {
              if (_context.Products.Where(x => x.Id == KitProduct[i]).Select(x => x.Stock).FirstOrDefault() == 0)
                emptyStock = true;
              if (_context.Products.Where(x => x.Id == KitProduct[i]).Select(x => x.Stock).FirstOrDefault()
                < minStock)
                minStock = (long)_context.Products.Where(x => x.Id == KitProduct[i]).Select(x => x.Stock).FirstOrDefault();
            }
          }

          await sInventoryLevelService.SetAsync(new InventoryLevel()
          {
            Available = productViewModel.Kit ? emptyStock ? 0 : minStock : (long)productViewModel.Stock,
            InventoryItemId = sProductVariant.InventoryItemId,
            LocationId = sLocation.First().Id // Map this to future additional locations.
          });
        }
        catch (Exception ex)
        {
          throw new Exception("An error was encountered while writing the product entity to Shopify.", ex);
        }

        try
        {
          product = new ORM.Product()
          {
            Active = productViewModel.Active,
            Created = DateTime.UtcNow,
            CreatedBy = _context.AspNetUsers.Where(
              x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
            CurrentPrice = productViewModel.CurrentPriceId,
            Cost = productViewModel.CurrentCostId,
            Description = productViewModel.Description,
            LastUpdated = null,
            LastUpdatedBy = null,
            Name = productViewModel.Name.Trim(),
            ProductType = productViewModel.ProductType,
            ProductCategory = 6, // General product category, we can write categories, sub-categories and sub-under-categories.
            ShopifyProductId = sProduct.Id,
            Sku = productViewModel.Sku.Trim(),
            VolumeInFluidOunces = productViewModel.VolumeInFluidOunces,
            ShippingWeightLb = productViewModel.ShippingWeightLb,
            Ph = productViewModel.Ph,
            Stock = productViewModel.Stock,
            Sample = productViewModel.Sample
          };
          _context.Add(product);
          _context.SaveChanges();
        }
        catch (Exception ex)
        {
          throw new Exception("An error was encountered while writing the product entity to the database.", ex);
        }

        try
        {
          if (ImageLarge != null && ImageLarge.Length > 0)
          {
            if (ImageLarge.FileName.LastIndexOf(".jpg") + 4 == ImageLarge.FileName.Length 
              || ImageLarge.FileName.LastIndexOf(".jpeg") + 5 == ImageLarge.FileName.Length)
            {
              using (stream = System.IO.File.Create(filePathPrefix + product.Id.ToString() + ".jpg"))
              {
                await ImageLarge.CopyToAsync(stream);
              }
              product.ImagePathLarge = srcPathPrefix + product.Id.ToString() + ".jpg";
              _context.Update(product);
              _context.SaveChanges();
            }
          }
        }
        catch (Exception ex)
        {
          throw new Exception("An error was encountered while saving the attached image.", ex);
        }

        try
        {
          if (productViewModel.Kit)
          {
            product.KitType = productViewModel.KitType;

            for (i = 0; i < KitProduct.Length; i++)
            {
              _context.KitProducts.Add(new KitProduct()
              {
                Kit = product.Id,
                Product = KitProduct[i]
              });
            }
            await _context.SaveChangesAsync();
          }
        }
        catch (Exception ex)
        {
          throw new Exception("An error was encountered while writing the kit products to the database.", ex);
        }

        try
        {
          if (productViewModel.Kit)
          {
            productIngredient = new List<ProductIngredient>();

            for (i = 0; i < KitProduct.Length; i++)
            {
              volume += (decimal)_context.Products.Where(
                x => x.Id == KitProduct[i]).Select(x => x.VolumeInFluidOunces).FirstOrDefault();
              pH += (decimal)_context.Products.Where(
                x => x.Id == KitProduct[i]).Select(x => x.Ph).FirstOrDefault() *
                (decimal)_context.Products.Where(
                x => x.Id == KitProduct[i]).Select(x => x.VolumeInFluidOunces).FirstOrDefault();
              foreach (ProductIngredient pi in _context.ProductIngredients.Where(x => x.Product == KitProduct[i]))
              {
                if (productIngredient.FindAll(x => x.Ingredient == pi.Ingredient).Count == 0)
                {
                  productIngredient.Add(new ProductIngredient()
                  {
                    Ingredient = pi.Ingredient,
                    Product = product.Id
                  });
                }
              }
            }

            pH = pH / volume;

            product.ShippingWeightLb = weight;
            product.VolumeInFluidOunces = volume;
            product.Ph = pH;
            product.Stock = emptyStock ? 0 : minStock;

            _context.Products.Update(product);
            _context.SaveChanges();
            _context.ProductIngredients.AddRange(productIngredient);
            _context.SaveChanges();
          }
        }
        catch (Exception ex)
        {
          throw new Exception("An error was encountered while calculating kit product data and updating the product.", ex);
        }

        try
        {
          if (!productViewModel.Kit)
          {
            for (i = 0; i < Ingredient.Length; i++)
            {
              _context.ProductIngredients.Add(new ProductIngredient()
              {
                Product = product.Id,
                Ingredient = Ingredient[i]
              });
            }
            await _context.SaveChangesAsync();
          }
        }
        catch (Exception ex)
        {
          throw new Exception("An error was encountered while writing the product ingredients to the database.", ex);
        }

        return RedirectToAction(nameof(Index));
      }

      return View(productViewModel);
    }

    // GET: Administration/Product/Edit/5
    public async Task<IActionResult> Edit(long? id)
    {
      ORM.Product product = null;
      ProductViewModel productViewModel = null;
      List<Ingredient> ingredientList = null;
      List<ORM.Product> productList = null;

      if (id == null)
      {
        return NotFound();
      }

      try
      {
        product = await _context.Products.FindAsync(id);

        productList = new List<ORM.Product>();

        if (product.KitType != null)
        {
          foreach (KitProduct kp in _context.KitProducts.Where(x => x.Kit == id).ToList())
            productList.Add(_context.Products.Where(x => x.Id == kp.Product).FirstOrDefault());
          productList = productList.OrderBy(x => x.Name).ToList();
        }

        ingredientList = new List<Ingredient>();
        foreach (ProductIngredient ingredient in _context.ProductIngredients.Where(x => x.Product == id).ToList())
          ingredientList.Add(_context.Ingredients.Where(
            x => x.Id == ingredient.Ingredient).FirstOrDefault());
        ingredientList = ingredientList.OrderBy(x => x.Name).ToList();
        
        ViewData["ProductType"] = new MultiSelectList(_context.ProductTypes, "Id", "Name");
        ViewData["Price"] = new SelectList(_context.Prices.Where(
          x => x.Active), "Id", "Amount");
        ViewData["Cost"] = new SelectList(_context.Costs.Where(
          x => x.Active), "Id", "Amount");
        ViewData["Ingredient"] = new MultiSelectList(ingredientList, "Id", "Name");
        ViewData["KitType"] = new SelectList(_context.KitTypes, "Id", "Name");
        ViewData["Product"] = new SelectList(productList, "Id", "Name");
      }
      catch (Exception ex)
      {
        throw new Exception("An error was encountered while loading product update form data.", ex);
      }

      if (product == null)
      {
        return NotFound();
      }

      productViewModel = new ProductViewModel()
      {
        Id = (long)id,
        Active = product.Active,
        Created = product.Created,
        CreatedByName = _context.AspNetUsers.Where(
          x => x.Id.Equals(product.CreatedBy)).Select(x => x.Email).FirstOrDefault(),
        CreatedBy = product.CreatedBy,
        CurrentCostId = (long)product.Cost,
        CurrentPriceId = (long)product.CurrentPrice,
        Description = product.Description,
        ImagePathLarge = product.ImagePathLarge,
        ImagePathMedium = null,
        ImagePathSmall = null,
        Ingredient = await _context.ProductIngredients.Where(
          x => x.Product == id).ToListAsync(),
        Kit = product.KitType != null ? true : false,
        KitProduct = await _context.KitProducts.Where(
          x => x.Kit == id).ToListAsync(),
        KitType = product.KitType,
        LastUpdated = product.LastUpdated,
        LastUpdatedBy = product.LastUpdatedBy,
        LastUpdateByName = _context.AspNetUsers.Where(
          x => x.Id.Equals(product.LastUpdatedBy)).Select(x => x.Email).FirstOrDefault(),
        Name = product.Name,
        Ph = product.Ph,
        ProductCategory = product.ProductCategory,
        ProductType = product.ProductType,
        ShippingWeightLb = product.ShippingWeightLb,
        ShopifyProductId = product.ShopifyProductId,
        Sku = product.Sku,
        Stock = product.Stock,
        VolumeInFluidOunces = product.VolumeInFluidOunces,
        Sample = (bool)product.Sample
      };

      return View(productViewModel);
    }

    // POST: Administration/Product/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
      long id, [Bind("Id,ShopifyProductId,Sku,Name,Description,ProductType,ProductCategory,Kit,KitType,KitProduct,Ingredient," +
      "VolumeInFluidOunces,Ph,ShippingWeightLb,Stock,CurrentPrice,CurentCost,CurrentPriceId,CurrentCostId,ImagePathSmall," +
      "ImagePathMedium,ImagePathLarge,ImageLarge,Active,Sample,CreatedBy,Created,LastUpdatedBy,LastUpdated")] 
      ProductViewModel productViewModel, IFormFile ImageLarge)
    {
      ORM.Product product = null;
      String filePathPrefix = "wwwroot/img/product/xo-img-pid-";
      String srcPathPrefix = "/img/product/xo-img-pid-";
      FileStream stream = null;
      ProductService sProductService = null;
      ProductVariantService sProductVariantService = null;
      InventoryItemService sInventoryItemService = null;
      ShopifySharp.Product sProduct = null;
      ProductVariant sProductVariant = null;
      InventoryItem sInventoryItem = null;
      decimal weight = 0.0M;
      decimal pH = 0.0M;
      decimal volume = 0.0M;
      ORM.Product kit = null;
      List<KitProduct> kitProduct = null;

      if (id != productViewModel.Id)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
          product = _context.Products.Where(x => x.Id == id).FirstOrDefault();

          product.LastUpdatedBy = _context.AspNetUsers.Where(
            x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault();
          product.LastUpdated = DateTime.UtcNow;
          product.Active = productViewModel.Active;
          
          if (ImageLarge != null && ImageLarge.Length > 0)
          {
            if (ImageLarge.FileName.LastIndexOf(".jpg") + 4 == ImageLarge.FileName.Length
              || ImageLarge.FileName.LastIndexOf(".jpeg") + 5 == ImageLarge.FileName.Length)
            {
              using (stream = System.IO.File.Create(filePathPrefix + product.Id.ToString() + ".jpg"))
              {
                await ImageLarge.CopyToAsync(stream);
              }
              product.ImagePathLarge = srcPathPrefix + productViewModel.Id.ToString() + ".jpg";
            }
          }
          
          product.Sku = productViewModel.Sku;
          product.Name = productViewModel.Name;
          product.Description = productViewModel.Description;
          product.KitType = productViewModel.KitType;
          product.ProductType = productViewModel.ProductType;
          product.Sample = productViewModel.Sample;

          if (!productViewModel.Kit)
          {
            product.VolumeInFluidOunces = productViewModel.VolumeInFluidOunces;
            product.Ph = productViewModel.Ph;
            product.ShippingWeightLb = productViewModel.ShippingWeightLb;
          }

          product.CurrentPrice = productViewModel.CurrentPriceId;
          product.Cost = productViewModel.CurrentCostId;

          _context.Update(product);
          await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
          if (!ProductViewModelExists(productViewModel.Id))
          {
            return NotFound();
          }
          else
          {
            throw new Exception("An error was encountered while writing the updated product information to the database.", ex);
          }
        }

        try
        {
          foreach(KitProduct kp in _context.KitProducts.ToList())
          {
            if (kp.Product == id)
            {
              kit = _context.Products.Where(x => x.Id == kp.Kit).FirstOrDefault();
              kitProduct = _context.KitProducts.Where(x => x.Kit == kit.Id).ToList();
              weight = 0.0M;
              volume = 0.0M;
              pH = 0.0M;

              foreach (KitProduct kpB in kitProduct)
              {
                weight += (decimal)_context.Products.Where(
                  x => x.Id == kpB.Product).Select(x => x.ShippingWeightLb).FirstOrDefault();
                volume += (decimal)_context.Products.Where(
                  x => x.Id == kpB.Product).Select(x => x.VolumeInFluidOunces).FirstOrDefault();
                pH += (decimal)_context.Products.Where(
                  x => x.Id == kpB.Product).Select(x => x.Ph).FirstOrDefault() *
                  (decimal)_context.Products.Where(
                  x => x.Id == kpB.Product).Select(x => x.VolumeInFluidOunces).FirstOrDefault();
              }

              pH = pH / volume;

              kit.ShippingWeightLb = weight;
              kit.VolumeInFluidOunces = volume;
              kit.Ph = pH;

              _context.Products.Update(kit);
              _context.SaveChanges();
            }
          }
        }
        catch (Exception ex)
        {
          throw new Exception("An error was encountered while updating kits containing this product", ex);
        }

        try
        {
          foreach(ShoppingCartLineItem scli in _context.ShoppingCartLineItems.ToList())
          {
            if (scli.Product == id)
            {
              scli.Total = _context.Prices.Where(x => x.Id == _context.Products.Where(
                x => x.Id == id).Select(x => x.CurrentPrice).FirstOrDefault()).Select(
                x => x.Amount).FirstOrDefault() * scli.Quantity;
              _context.ShoppingCartLineItems.Update(scli);
              _context.SaveChanges();
            }
          }
          foreach (ProductOrderLineItem poli in _context.ProductOrderLineItems.ToList())
          {
            if (poli.Product == id)
            {
              poli.Total = _context.Prices.Where(x => x.Id == _context.Products.Where(
                x => x.Id == id).Select(x => x.CurrentPrice).FirstOrDefault()).Select(
                x => x.Amount).FirstOrDefault() * poli.Quantity;
              _context.ProductOrderLineItems.Update(poli);
              _context.SaveChanges();
            }
          }
        }
        catch (Exception ex)
        {
          throw new Exception("An error was encountered while updating shopping cart and order line item totals.", ex);
        }

        try
        {
          sProductService = new ProductService(_option.Value.ShopifyUrl,
            _option.Value.ShopifyStoreFrontAccessToken);
          sProductVariantService = new ProductVariantService(_option.Value.ShopifyUrl,
            _option.Value.ShopifyStoreFrontAccessToken);
          sInventoryItemService = new InventoryItemService(_option.Value.ShopifyUrl,
            _option.Value.ShopifyStoreFrontAccessToken);

          sProduct = sProductService.GetAsync((long)product.ShopifyProductId).Result;
          
          if (!sProduct.Status.Equals(productViewModel.Active ? "active" : "draft") || 
            !sProduct.Title.Equals(productViewModel.Name) || !sProduct.BodyHtml.Equals(productViewModel.Description))  
          {
            sProduct.Status = productViewModel.Active ? "active" : "draft";
            sProduct.Title = productViewModel.Name;
            sProduct.BodyHtml = productViewModel.Description;
            await sProductService.UpdateAsync((long)product.ShopifyProductId, sProduct);
          }

          sProductVariant = sProductVariantService.GetAsync((long)sProduct.Variants.First().Id).Result;

          if (sProductVariant.Weight != productViewModel.ShippingWeightLb || 
            sProductVariant.Price != _context.Prices.Where(
              x => x.Id == productViewModel.CurrentPriceId).Select(x => x.Amount).FirstOrDefault() ||
            !sProductVariant.SKU.Equals(productViewModel.Sku))
          {
            sProductVariant.Weight = productViewModel.ShippingWeightLb;
            sProductVariant.Price = _context.Prices.Where(
              x => x.Id == productViewModel.CurrentPriceId).Select(x => x.Amount).FirstOrDefault();
            sProductVariant.SKU = productViewModel.Sku;
            sProductVariant.InventoryQuantity = null;
            await sProductVariantService.UpdateAsync((long)sProduct.Variants.First().Id, sProductVariant);
          }

          sInventoryItem = sInventoryItemService.GetAsync((long)sProductVariant.InventoryItemId).Result;

          if (sInventoryItem.Cost != _context.Costs.Where(
            x => x.Id == productViewModel.CurrentCostId).Select(x => x.Amount).FirstOrDefault())
          {
            sInventoryItem.Cost = _context.Costs.Where(
              x => x.Id == productViewModel.CurrentCostId).Select(x => x.Amount).FirstOrDefault();
            await sInventoryItemService.UpdateAsync((long)sProductVariant.InventoryItemId, sInventoryItem);
          }
        }
        catch (Exception ex)
        {
          throw new Exception("An error was encountered while writing the updated product information to Shopify.", ex);
        }
        return RedirectToAction(nameof(Index));
      }
      return View(productViewModel);
    }

    private bool ProductViewModelExists(long id)
    {
      return _context.Products.Any(e => e.Id == id);
    }

    public JsonResult ProductNameAvailable(String Name, bool ActionCreate, String OriginalProductName)
    {
      if (OriginalProductName == null)
        OriginalProductName = String.Empty;
      if (ActionCreate || (!ActionCreate && !Name.Equals(OriginalProductName)))
      {
        return Json(!_context.Products.Any(x => x.Name.Equals(Name.Trim())));
      }
      return Json(true);
    }

    public JsonResult SkuAvailable(String Sku, bool ActionCreate, String OriginalSku)
    {
      if (OriginalSku == null)
        OriginalSku = String.Empty;
      if (ActionCreate || (!ActionCreate && !Sku.Equals(OriginalSku)))
      {
        return Json(!_context.Products.Any(x => x.Sku.Equals(Sku.Trim())));
      }
      return Json(true);
    }
  }
}