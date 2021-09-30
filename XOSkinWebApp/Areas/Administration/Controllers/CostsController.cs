using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XOSkinWebApp.Areas.Administration.Models;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Areas.Administration.Controllers
{
  [Area("Administration")]
  public class CostsController : Controller
  {
    private readonly XOSkinContext _context;

    public CostsController(XOSkinContext context)
    {
      _context = context;
    }

    // GET: Administration/Costs
    public async Task<IActionResult> Index()
    {
      List<CostViewModel> price = new List<CostViewModel>();

      foreach (Cost c in await _context.Costs.ToListAsync())
      {
        price.Add(new CostViewModel()
        {
          Active = c.Active,
          Amount = c.Amount,
          CreatedBy = _context.AspNetUsers.Where(
            x => x.Id.Equals(c.CreatedBy)).Select(x => x.Email).FirstOrDefault(),
          CreatedOn = c.CreatedOn,
          Id = c.Id
        });
      }

      return View(price);
    }

    // GET: Administration/Costs/Create
    public IActionResult Create()
    {
      return View();
    }

    // POST: Administration/Costs/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Amount")] CostViewModel costViewModel)
    {
      Cost cost = null;

      if (ModelState.IsValid)
      {
        cost = new Cost()
        {
          Active = true,
          Amount = costViewModel.Amount,
          CreatedBy = _context.AspNetUsers.Where(
            x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault(),
          CreatedOn = DateTime.UtcNow
        };

        _context.Add(cost);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
      }
      return View(costViewModel);
    }

    // GET: Administration/Prices/Edit/5
    public async Task<IActionResult> Edit(long? id)
    {
      Cost cost = null;
      CostViewModel costViewModel = null;

      if (id == null)
      {
        return NotFound();
      }

      cost = await _context.Costs.FindAsync(id);

      if (cost == null)
      {
        return NotFound();
      }

      costViewModel = new CostViewModel()
      {
        Active = cost.Active,
        Amount = cost.Amount,
        CreatedBy = _context.AspNetUsers.Find(cost.CreatedBy).Email,
        CreatedOn = cost.CreatedOn,
        Id = cost.Id
      };

      return View(costViewModel);
    }

    // POST: Administration/Prices/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, [Bind("Id,Amount,Active,CreatedBy,CreatedOn")] CostViewModel costViewModel)
    {
      Cost cost = null;

      if (id != costViewModel.Id)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
          cost = new Cost()
          {
            Id = costViewModel.Id,
            Active = costViewModel.Active,
            Amount = costViewModel.Amount,
            CreatedBy = _context.AspNetUsers.Where(
              x => x.Email.Equals(costViewModel.CreatedBy)).Select(x => x.Id).FirstOrDefault(),
            CreatedOn = costViewModel.CreatedOn
          };

          _context.Update(cost);
          await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!PriceExists(cost.Id))
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
      return View(costViewModel);
    }

    public JsonResult ActiveAmountAvailable(decimal Amount, bool ActionCreate)
    {
      if (ActionCreate)
      {
        return Json(!_context.Costs.Where(x => x.Active).Any(x => x.Amount.Equals(Amount)));
      }
      return Json(true);
    }

    private bool PriceExists(long id)
    {
      return _context.Costs.Any(e => e.Id == id);
    }
  }
}
