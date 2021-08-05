using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class LocalizedImagesController : Controller
    {
        private readonly XOSkinContext _context;

        public LocalizedImagesController(XOSkinContext context)
        {
            _context = context;
        }

        // GET: Administration/LocalizedImages
        public async Task<IActionResult> Index()
        {
            var xOSkinContext = _context.LocalizedImages.Include(l => l.LanguageNavigation).Include(l => l.PageNavigation);
            return View(await xOSkinContext.ToListAsync());
        }

        // GET: Administration/LocalizedImages/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var localizedImage = await _context.LocalizedImages
                .Include(l => l.LanguageNavigation)
                .Include(l => l.PageNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (localizedImage == null)
            {
                return NotFound();
            }

            return View(localizedImage);
        }

        // GET: Administration/LocalizedImages/Create
        public IActionResult Create()
        {
            ViewData["Language"] = new SelectList(_context.Languages.Where(x=> x.Active == true), "Id", "LanguageName");
            ViewData["Page"] = new SelectList(_context.Pages, "Id", "Name");
            return View();
        }

        // POST: Administration/LocalizedImages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Path,Language,PlacementPointCode,Page")] LocalizedImage localizedImage)
        {
            if (ModelState.IsValid)
            {
        localizedImage.PlacementPointCode = _context.Pages.Where(x => x.Id == localizedImage.Page).Select(x => x.Name).FirstOrDefault().Replace(" ", "") + 
                  "." + localizedImage.PlacementPointCode;
                _context.Add(localizedImage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Language"] = new SelectList(_context.Languages, "Id", "LanguageName", localizedImage.Language);
            ViewData["Page"] = new SelectList(_context.Pages, "Id", "Name", localizedImage.Page);
            return View(localizedImage);
        }

        // GET: Administration/LocalizedImages/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var localizedImage = await _context.LocalizedImages.FindAsync(id);
            if (localizedImage == null)
            {
                return NotFound();
            }
            localizedImage.PlacementPointCode = localizedImage.PlacementPointCode.Replace(
              _context.Pages.Where(x => x.Id == localizedImage.Page).Select(x => x.Name).FirstOrDefault() + "."
              , "");
            ViewData["Language"] = new SelectList(_context.Languages.Where(x => x.Active == true), "Id", "LanguageName", localizedImage.Language);
            ViewData["Page"] = new SelectList(_context.Pages, "Id", "Name", localizedImage.Page);
      
            return View(localizedImage);
        }

        // POST: Administration/LocalizedImages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Path,Language,PlacementPointCode,Page")] LocalizedImage localizedImage)
        {
            if (id != localizedImage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    localizedImage.PlacementPointCode = _context.Pages.Where(x => x.Id == localizedImage.Page).Select(x => x.Name).FirstOrDefault().Replace(" ", "") +
                      "." + localizedImage.PlacementPointCode;
                    _context.Update(localizedImage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LocalizedImageExists(localizedImage.Id))
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
            ViewData["Language"] = new SelectList(_context.Languages, "Id", "LanguageName", localizedImage.Language);
            ViewData["Page"] = new SelectList(_context.Pages, "Id", "Name", localizedImage.Page);
            return View(localizedImage);
        }

        // GET: Administration/LocalizedImages/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var localizedImage = await _context.LocalizedImages
                .Include(l => l.LanguageNavigation)
                .Include(l => l.PageNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (localizedImage == null)
            {
                return NotFound();
            }

            return View(localizedImage);
        }

        // POST: Administration/LocalizedImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var localizedImage = await _context.LocalizedImages.FindAsync(id);
            _context.LocalizedImages.Remove(localizedImage);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public JsonResult PlacementCodeAvailable(String PlacementPointCode, bool ActionCreate, int Page, int Language,
          String OriginalPlacementCode, int OriginalLanguage)
        {
          if (ActionCreate || (!ActionCreate && !PlacementPointCode.Equals(OriginalPlacementCode)) ||
            (!ActionCreate && Language != OriginalLanguage))
          {
            String prefixedPlacementCode =
              _context.Pages.Where(x => x.Id == Page).Select(x => x.Name).FirstOrDefault() + "." + PlacementPointCode;
            if (_context.LocalizedImages.Where(x => x.Language == Language).Any(x => x.PlacementPointCode.Equals(prefixedPlacementCode)))
            {
              return Json(false);
            }
          }
          return Json(true);
        }

        private bool LocalizedImageExists(long id)
        {
          return _context.LocalizedImages.Any(e => e.Id == id);
        }
    }
}
