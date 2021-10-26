using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using XOSkinWebApp.Areas.Administration.Models;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Areas.Administration.Controllers
{
  [Area("Administration")]
  [Authorize(Roles = "Administrator")]
  public class DiscountCodesController : Controller
  {
    private readonly XOSkinContext _context;

    public DiscountCodesController(XOSkinContext context)
    {
      _context = context;
    }

    // GET: Administration/DiscountCodes
    public async Task<IActionResult> Index()
    {
      List<DiscountCodeViewModel> discountCode = new List<DiscountCodeViewModel>();
      foreach (DiscountCode dc in await _context.DiscountCodes.ToListAsync())
      {
        discountCode.Add(new DiscountCodeViewModel()
        {
          Active = dc.Active,
          Code = dc.Code.ToUpper(),
          Created = dc.Created,
          CreatedBy = (dc.CreatedBy == null || dc.CreatedBy.Length == 0) ?
            String.Empty : _context.AspNetUsers.FindAsync(dc.CreatedBy).Result.Email,
          DiscountAsInGlobalOrderPercentage = dc.DiscountAsInGlobalOrderPercentage,
          DiscountAsInNProductDollars = dc.DiscountAsInNproductDollars,
          DiscountAsInNProductPercentage = dc.DiscountAsInNproductPercentage,
          DiscountAsInGlobalOrderDollars = dc.DiscountAsInGlobalOrderDollars,
          DiscountGlobalOrderDollars = dc.DiscountGlobalOrderDollars,
          DiscountGlobalOrderPercentage = dc.DiscountGlobalOrderPercentage,
          DiscountInNProductDollars = dc.DiscountInNproductDollars,
          DiscountNProductPercentage = dc.DiscountNproductPercentage,
          DiscountProductN = dc.DiscountProductN,
          MinimumPurchase = dc.MinimumPurchase,
          Id = dc.Id,
          LastUpdated = dc.LastUpdated,
          LastUpdatedBy = (dc.LastUpdatedBy == null || dc.LastUpdatedBy.Length == 0) ? 
            String.Empty : _context.AspNetUsers.FindAsync(dc.LastUpdatedBy).Result.Email,
          ValidFrom = dc.ValidFrom,
          ValidTo = dc.ValidTo,
        });
      }
      return View(discountCode);
    }

    // GET: Administration/DiscountCodes/Create
    public IActionResult Create()
    {
      ViewData["Products"] = new SelectList(_context.Products.Where(
          x => x.Active).OrderBy(x => x.Name), "Id", "Name");
      return View();
    }

    // POST: Administration/DiscountCodes/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Active,Code,DiscountAsInNProductPercentage," +
      "DiscountNProductPercentage,DiscountAsInNProductDollars,DiscountAsInNProductDollars,DiscountProductN," +
      "MinimumPurchase,DiscountAsInGlobalOrderPercentage,DiscountGlobalOrderPercentage,DiscountAsInOrderDollars," +
      "DiscountGlobalOrderDollars,ValidFrom,ValidTo,Product," +
      "CreatedBy,Created,LastUpdatedBy,LastUpdated," +
      "DiscountAsInGlobalOrderDollars")] DiscountCodeViewModel discountCodeViewModel,
      long [] Product)
    {
      DiscountCode discountCode = null;
      int i = 0;

      if (ModelState.IsValid)
      {
        discountCode = new DiscountCode()
        {
          Active = discountCodeViewModel.Active,
          Code = discountCodeViewModel.Code.ToUpper(),
          Created = DateTime.UtcNow,
          CreatedBy = _context.AspNetUsers.Where(
            x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
          LastUpdated = DateTime.UtcNow,
          DiscountAsInGlobalOrderDollars = discountCodeViewModel.DiscountAsInGlobalOrderDollars,
          DiscountAsInGlobalOrderPercentage = discountCodeViewModel.DiscountAsInGlobalOrderPercentage,
          DiscountAsInNproductDollars = discountCodeViewModel.DiscountAsInNProductDollars,
          DiscountAsInNproductPercentage = discountCodeViewModel.DiscountAsInNProductPercentage,
          DiscountGlobalOrderDollars = discountCodeViewModel.DiscountGlobalOrderDollars,
          DiscountGlobalOrderPercentage = discountCodeViewModel.DiscountGlobalOrderPercentage,
          DiscountInNproductDollars = discountCodeViewModel.DiscountInNProductDollars,
          DiscountNproductPercentage = discountCodeViewModel.DiscountNProductPercentage,
          DiscountProductN = discountCodeViewModel.DiscountProductN,
          MinimumPurchase = discountCodeViewModel.MinimumPurchase,
          LastUpdatedBy = _context.AspNetUsers.Where(
            x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
          ValidFrom = discountCodeViewModel.ValidFrom,
          ValidTo = discountCodeViewModel.ValidTo
        };
        _context.DiscountCodes.Add(discountCode);
        await _context.SaveChangesAsync();

        for (; i < Product.Length; i++)
        {
          _context.DiscountCodeProducts.Add(new DiscountCodeProduct()
          {
            Code = discountCode.Id,
            Product = Product[i]
          });
          await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
      }
      return View(discountCodeViewModel);
    }

    // GET: Administration/DiscountCodes/Edit/5
    public async Task<IActionResult> Edit(long? id)
    {
      DiscountCodeViewModel discountCodeViewModel = null;
      List<DiscountCodeProduct> product = null;
      long[] products = null;
      int i = 0;
      ViewData["Products"] = null;

      if (id == null)
      {
        return NotFound();
      }

      DiscountCode discountCode = await _context.DiscountCodes.FindAsync(id);
      if (discountCode == null)
      {
        return NotFound();
      }

      product = await _context.DiscountCodeProducts.Where(x => x.Code == discountCode.Id).ToListAsync();

      products = new long[product.Count];

      foreach (DiscountCodeProduct p in product)
      {
        products[i] = p.Product;
        i++;
      }

      ViewData["Products"] = new SelectList(_context.Products.Where(
         x => x.Active).OrderBy(x => x.Name), "Id", "Name");

      discountCodeViewModel = new DiscountCodeViewModel()
      {
        Active = discountCode.Active,
        Code = discountCode.Code.ToUpper(),
        Created = discountCode.Created,
        CreatedBy = _context.AspNetUsers.FindAsync(discountCode.CreatedBy).Result.Email,
        DiscountAsInGlobalOrderDollars = discountCode.DiscountAsInGlobalOrderDollars,
        DiscountAsInGlobalOrderPercentage = discountCode.DiscountAsInGlobalOrderPercentage,
        DiscountAsInNProductDollars = discountCode.DiscountAsInNproductDollars,
        DiscountAsInNProductPercentage = discountCode.DiscountAsInNproductPercentage,
        DiscountGlobalOrderDollars = discountCode.DiscountGlobalOrderDollars,
        DiscountGlobalOrderPercentage = discountCode.DiscountGlobalOrderPercentage,
        DiscountInNProductDollars = discountCode.DiscountInNproductDollars,
        DiscountNProductPercentage = discountCode.DiscountNproductPercentage,
        DiscountProductN = discountCode.DiscountProductN,
        MinimumPurchase = discountCode.MinimumPurchase,
        Id = discountCode.Id,
        LastUpdated = discountCode.LastUpdated,
        LastUpdatedBy = _context.AspNetUsers.FindAsync(discountCode.LastUpdatedBy).Result.Email,
        Product = products,
        ValidFrom = discountCode.ValidFrom,
        ValidTo = discountCode.ValidTo
      };

      return View(discountCodeViewModel);
    }

    // POST: Administration/DiscountCodes/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, 
      [Bind("Id,Active")] DiscountCodeViewModel discountCodeViewModel)
    {
      DiscountCode discountCode = null;

      if (id != discountCodeViewModel.Id)
      {
        return NotFound();
      }

      try
      {
        discountCode = await _context.DiscountCodes.FindAsync(id);
        discountCode.Active = discountCodeViewModel.Active;

        _context.DiscountCodes.Update(discountCode);
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!DiscountCodeViewModelExists(discountCodeViewModel.Id))
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

    private bool DiscountCodeViewModelExists(long id)
    {
      return _context.DiscountCodes.Any(e => e.Id == id);
    }
  }
}
