using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using XOSkinWebApp.Areas.Administration.Models;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Areas.Administration.Controllers
{
  [Area("Administration")]
  [Authorize(Roles = "Administrator")]
  public class ProductController : Controller
  {
    private readonly XOSkinContext _context;

    public ProductController(XOSkinContext context)
    {
      _context = context;
    }

    // GET: Administration/Product
    public async Task<IActionResult> Index()
    {
      List<Product> product = null;
      List<ProductViewModel> productViewModel = null;

      try
      {
        product = await _context.Products.ToListAsync();
      }
      catch (Exception ex)
      {
        throw new Exception("An error was encountered while loading product form data.", ex);
      }

      productViewModel = new List<ProductViewModel>();

      foreach (Product p in product)
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
          KitProduct = p.KitProducts,
          KitType = p.KitType,
          LastUpdated = p.LastUpdated,
          LastUpdatedBy = p.LastUpdatedBy,
          Name = p.Name,
          Ph = p.Ph,
          ProductCategory = p.ProductCategory,
          ProductType = p.ProductType,
          Sku = p.Sku,
          Stock = p.Stock,
          VolumeInFluidOunces = p.VolumeInFluidOunces
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
        //ViewData["ProductCategory"] = new SelectList(_context.ProductCategories, "Id", "Name");
        ViewData["Price"] = new SelectList(_context.Prices.Where(
          x => x.ValidFrom <= DateTime.UtcNow).Where(
          x => x.ValidTo >= DateTime.UtcNow), "Id", "Amount");
        ViewData["Ingredient"] = new MultiSelectList(_context.Ingredients, "Id", "Name");
        ViewData["KitType"] = new SelectList(_context.KitTypes, "Id", "Name");
        ViewData["Product"] = new SelectList(_context.Products.Where(x => x.Active == true), "Id", "Name");
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
    public async Task<IActionResult> Create([Bind("Id,ShopifyProductId,Sku,Name,Description,ProductType,ProductCategory,Kit,KitType,KitProduct,Ingredient,VolumeInFluidOunces,Ph,Stock,CurrentPrice,CurrentPriceId,ImagePathSmall,ImagePathMedium,ImagePathLarge,ImageLarge,Active,CreatedBy,Created,LastUpdatedBy,LastUpdated")] ProductViewModel productViewModel,
      IFormFile ImageLarge, long[] KitProduct, long[] Ingredient)
    {
      if (ModelState.IsValid)
      {
        //_context.Add(productViewModel);
        //await _context.SaveChangesAsync();
        //return RedirectToAction(nameof(Index));
      }
      return View(productViewModel);
    }

    // GET: Administration/Product/Edit/5
    public async Task<IActionResult> Edit(long? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var productViewModel = await _context.Products.FindAsync(id);
      if (productViewModel == null)
      {
        return NotFound();
      }
      return View(productViewModel);
    }

    // POST: Administration/Product/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, [Bind("Id,ShopifyProductId,Sku,Name,Description,ProductType,ProductCategory,KitType,VolumeInFluidOunces,Ph,Stock,CurrentPrice,ImagePathSmall,ImagePathMedium,ImagePathLarge,Active,CreatedBy,Created,LastUpdatedBy,LastUpdated")] ProductViewModel productViewModel)
    {
      if (id != productViewModel.Id)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
          _context.Update(productViewModel);
          await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!ProductViewModelExists(productViewModel.Id))
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
      return View(productViewModel);
    }

    // GET: Administration/Product/Delete/5
    public async Task<IActionResult> Delete(long? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var productViewModel = await _context.Products
          .FirstOrDefaultAsync(m => m.Id == id);
      if (productViewModel == null)
      {
        return NotFound();
      }

      return View(productViewModel);
    }

    // POST: Administration/Product/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
      var productViewModel = await _context.Products.FindAsync(id);
      _context.Products.Remove(productViewModel);
      await _context.SaveChangesAsync();
      return RedirectToAction(nameof(Index));
    }

    private bool ProductViewModelExists(long id)
    {
      return _context.Products.Any(e => e.Id == id);
    }
  }
}
