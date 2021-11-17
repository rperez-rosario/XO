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
    public class SubscriptionsController : Controller
    {
        private readonly XOSkinContext _context;

        public SubscriptionsController(XOSkinContext context)
        {
            _context = context;
        }

        // GET: Administration/Subscriptions
        public async Task<IActionResult> Index()
        {
            return View(await _context.SubscriptionViewModel.ToListAsync());
        }

        // GET: Administration/Subscriptions/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subscriptionViewModel = await _context.SubscriptionViewModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subscriptionViewModel == null)
            {
                return NotFound();
            }

            return View(subscriptionViewModel);
        }

        // GET: Administration/Subscriptions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Administration/Subscriptions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ImagePathLarge,Subscription,Type,ShipmentFrequencyInDays")] SubscriptionViewModel subscriptionViewModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(subscriptionViewModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(subscriptionViewModel);
        }

        // GET: Administration/Subscriptions/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subscriptionViewModel = await _context.SubscriptionViewModel.FindAsync(id);
            if (subscriptionViewModel == null)
            {
                return NotFound();
            }
            return View(subscriptionViewModel);
        }

        // POST: Administration/Subscriptions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,ImagePathLarge,Subscription,Type,ShipmentFrequencyInDays")] SubscriptionViewModel subscriptionViewModel)
        {
            if (id != subscriptionViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(subscriptionViewModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubscriptionViewModelExists(subscriptionViewModel.Id))
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
            return View(subscriptionViewModel);
        }

        // GET: Administration/Subscriptions/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subscriptionViewModel = await _context.SubscriptionViewModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subscriptionViewModel == null)
            {
                return NotFound();
            }

            return View(subscriptionViewModel);
        }

        // POST: Administration/Subscriptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var subscriptionViewModel = await _context.SubscriptionViewModel.FindAsync(id);
            _context.SubscriptionViewModel.Remove(subscriptionViewModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SubscriptionViewModelExists(long id)
        {
            return _context.SubscriptionViewModel.Any(e => e.Id == id);
        }
    }
}
