﻿using System;
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
  public class DiscountCouponsController: Controller
  {
    private readonly XOSkinContext _context;

    public DiscountCouponsController(XOSkinContext context)
    {
      _context = context;
    }

    // GET: Administration/DiscountCoupons
    public async Task<IActionResult> Index()
    {
      List<DiscountCouponViewModel> discountCoupon = new List<DiscountCouponViewModel>();
      foreach (DiscountCoupon dc in await _context.DiscountCoupons.ToListAsync())
      {
        discountCoupon.Add(new DiscountCouponViewModel()
        {
          Active = dc.Active,
          Name = dc.Name,
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
          Id = dc.Id,
          LastUpdated = dc.LastUpdated,
          LastUpdatedBy = (dc.LastUpdatedBy == null || dc.LastUpdatedBy.Length == 0) ?
            String.Empty : _context.AspNetUsers.FindAsync(dc.LastUpdatedBy).Result.Email,
          ValidFrom = dc.ValidFrom,
          ValidTo = dc.ValidTo,
        });
      }
      return View(discountCoupon);
    }

    // GET: Administration/DiscountCoupons/Create
    public IActionResult Create()
    {
      ViewData["Products"] = new SelectList(_context.Products.Where(
          x => x.Active).OrderBy(x => x.Name), "Id", "Name");
      return View();
    }

    // POST: Administration/DiscountCoupons/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Active,Name,DiscountAsInNProductPercentage," +
      "DiscountNProductPercentage,DiscountAsInNProductDollars,DiscountInNProductDollars,DiscountProductN," +
      "DiscountAsInGlobalOrderPercentage,DiscountGlobalOrderPercentage,DiscountAsInOrderDollars," +
      "DiscountGlobalOrderDollars,ValidFrom,ValidTo,Product," +
      "CreatedBy,Created,LastUpdatedBy,LastUpdated")] DiscountCouponViewModel discountCouponViewModel,
      long[] Product)
    {
      DiscountCoupon discountCoupon = null;
      int i = 0;

      if (ModelState.IsValid)
      {
        discountCoupon = new DiscountCoupon()
        {
          Active = discountCouponViewModel.Active,
          Name = discountCouponViewModel.Name,
          Created = DateTime.UtcNow,
          CreatedBy = _context.AspNetUsers.Where(
            x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
          LastUpdated = DateTime.UtcNow,
          DiscountAsInGlobalOrderDollars = discountCouponViewModel.DiscountAsInGlobalOrderDollars,
          DiscountAsInGlobalOrderPercentage = discountCouponViewModel.DiscountAsInGlobalOrderPercentage,
          DiscountAsInNproductDollars = discountCouponViewModel.DiscountAsInNProductDollars,
          DiscountAsInNproductPercentage = discountCouponViewModel.DiscountAsInNProductPercentage,
          DiscountGlobalOrderDollars = discountCouponViewModel.DiscountGlobalOrderDollars,
          DiscountGlobalOrderPercentage = discountCouponViewModel.DiscountGlobalOrderPercentage,
          DiscountInNproductDollars = discountCouponViewModel.DiscountInNProductDollars,
          DiscountNproductPercentage = discountCouponViewModel.DiscountNProductPercentage,
          DiscountProductN = discountCouponViewModel.DiscountProductN,
          LastUpdatedBy = _context.AspNetUsers.Where(
            x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
          ValidFrom = discountCouponViewModel.ValidFrom,
          ValidTo = discountCouponViewModel.ValidTo
        };
        _context.DiscountCoupons.Add(discountCoupon);
        await _context.SaveChangesAsync();

        for (; i < Product.Length; i++)
        {
          _context.DiscountCouponProducts.Add(new DiscountCouponProduct()
          {
            Coupon = discountCoupon.Id,
            Product = Product[i]
          });
          await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
      }
      return View(discountCouponViewModel);
    }

    // GET: Administration/DiscountCoupons/Edit/5
    public async Task<IActionResult> Edit(long? id)
    {
      DiscountCouponViewModel discountCouponViewModel = null;
      List<DiscountCouponProduct> product = null;
      long[] products = null;
      int i = 0;
      ViewData["Products"] = null;

      if (id == null)
      {
        return NotFound();
      }

      DiscountCoupon discountCoupon = await _context.DiscountCoupons.FindAsync(id);
      if (discountCoupon == null)
      {
        return NotFound();
      }

      product = await _context.DiscountCouponProducts.Where(
        x => x.Coupon == discountCoupon.Id).ToListAsync();

      products = new long[product.Count];

      foreach (DiscountCouponProduct p in product)
      {
        products[i] = p.Product;
        i++;
      }

      ViewData["Products"] = new SelectList(_context.Products.Where(
         x => x.Active).OrderBy(x => x.Name), "Id", "Name");

      discountCouponViewModel = new DiscountCouponViewModel()
      {
        Active = discountCoupon.Active,
        Name = discountCoupon.Name,
        Created = discountCoupon.Created,
        CreatedBy = _context.AspNetUsers.FindAsync(discountCoupon.CreatedBy).Result.Email,
        DiscountAsInGlobalOrderDollars = discountCoupon.DiscountAsInGlobalOrderDollars,
        DiscountAsInGlobalOrderPercentage = discountCoupon.DiscountAsInGlobalOrderPercentage,
        DiscountAsInNProductDollars = discountCoupon.DiscountAsInNproductDollars,
        DiscountAsInNProductPercentage = discountCoupon.DiscountAsInNproductPercentage,
        DiscountGlobalOrderDollars = discountCoupon.DiscountGlobalOrderDollars,
        DiscountGlobalOrderPercentage = discountCoupon.DiscountGlobalOrderPercentage,
        DiscountInNProductDollars = discountCoupon.DiscountInNproductDollars,
        DiscountNProductPercentage = discountCoupon.DiscountNproductPercentage,
        DiscountProductN = discountCoupon.DiscountProductN,
        Id = discountCoupon.Id,
        LastUpdated = discountCoupon.LastUpdated,
        LastUpdatedBy = _context.AspNetUsers.FindAsync(discountCoupon.LastUpdatedBy).Result.Email,
        Product = products,
        ValidFrom = discountCoupon.ValidFrom,
        ValidTo = discountCoupon.ValidTo
      };

      return View(discountCouponViewModel);
    }

    // POST: Administration/DiscountCoupons/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id,
      [Bind("Id,Active")] DiscountCouponViewModel discountCouponViewModel)
    {
      DiscountCoupon discountCoupon = null;

      if (id != discountCouponViewModel.Id)
      {
        return NotFound();
      }

      try
      {
        discountCoupon = await _context.DiscountCoupons.FindAsync(id);
        discountCoupon.Active = discountCouponViewModel.Active;

        _context.DiscountCoupons.Update(discountCoupon);
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!DiscountCouponViewModelExists(discountCouponViewModel.Id))
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

    private bool DiscountCouponViewModelExists(long id)
    {
      return _context.DiscountCoupons.Any(e => e.Id == id);
    }
  }
}