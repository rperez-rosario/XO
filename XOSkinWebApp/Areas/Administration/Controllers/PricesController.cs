using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using XOSkinWebApp.Areas.Administration.Models;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Areas.Administration.Controllers
{
  [Area("Administration")]
  public class PricesController : Controller
  {
    private readonly XOSkinContext _context;

    public PricesController(XOSkinContext context)
    {
      _context = context;
    }

    // GET: Administration/Prices
    public async Task<IActionResult> Index()
    {
      List<PriceViewModel> price = new List<PriceViewModel>();
      
      foreach (Price p in await _context.Prices.ToListAsync())
      {
        price.Add(new PriceViewModel()
        {
          Active = p.Active,
          Amount = p.Amount,
          CreatedBy = _context.AspNetUsers.Where(
            x => x.Id.Equals(p.CreatedBy)).Select(x => x.Email).FirstOrDefault(),
          CreatedOn = p.CreatedOn,
          Id = p.Id
        });
      }

      return View(price);
    }

    // GET: Administration/Prices/Create
    public IActionResult Create()
    {
      return View();
    }

    // POST: Administration/Prices/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Amount")] PriceViewModel priceViewModel)
    {
      Price price = null;

      if (ModelState.IsValid)
      {
        price = new Price()
        {
          Active = true,
          Amount = priceViewModel.Amount,
          CreatedBy = _context.AspNetUsers.Where(
            x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
          CreatedOn = DateTime.UtcNow
        };

        _context.Add(price);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
      }
      return View(priceViewModel);
    }

    // GET: Administration/Prices/Edit/5
    public async Task<IActionResult> Edit(long? id)
    {
      Price price = null;
      PriceViewModel priceViewModel = null;

      if (id == null)
      {
          return NotFound();
      }

      price = await _context.Prices.FindAsync(id);
      
      if (price == null)
      {
        return NotFound();
      }

      priceViewModel = new PriceViewModel()
      {
        Active = price.Active,
        Amount = price.Amount,
        CreatedBy = _context.AspNetUsers.Find(price.CreatedBy).Email,
        CreatedOn = price.CreatedOn,
        Id = price.Id
      };

      return View(priceViewModel);
    }

    // POST: Administration/Prices/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, [Bind("Id,Amount,Active,CreatedBy,CreatedOn")] PriceViewModel priceViewModel)
    {
      Price price = null;

      if (id != priceViewModel.Id)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
          price = new Price() {
            Id = priceViewModel.Id,
            Active = priceViewModel.Active,
            Amount = priceViewModel.Amount,
            CreatedBy = _context.AspNetUsers.Where(
              x => x.Email.Equals(priceViewModel.CreatedBy)).Select(x => x.Id).FirstOrDefault(),
            CreatedOn = priceViewModel.CreatedOn
          };

          _context.Update(price);
          await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!PriceExists(price.Id))
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
      return View(priceViewModel);
    }

    public JsonResult ActiveAmountAvailable(decimal Amount, bool ActionCreate)
    {
      if (ActionCreate)
      {
        return Json(!_context.Prices.Where(x => x.Active).Any(x => x.Amount.Equals(Amount)));
      }
      return Json(true);
    }

    private bool PriceExists(long id)
    {
      return _context.Prices.Any(e => e.Id == id);
    }
  }
}
