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
    public class LocalizedTextsController : Controller
    {
        private readonly XOSkinContext _context;

        public LocalizedTextsController(XOSkinContext context)
        {
            _context = context;
        }

        // GET: Administration/LocalizedTexts
        public async Task<IActionResult> Index()
        {
            var xOSkinContext = _context.LocalizedTexts.Include(l => l.LanguageNavigation).Include(l => l.PageNavigation);
            return View(await xOSkinContext.ToListAsync());
        }

        // GET: Administration/LocalizedTexts/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var localizedText = await _context.LocalizedTexts
                .Include(l => l.LanguageNavigation)
                .Include(l => l.PageNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (localizedText == null)
            {
                return NotFound();
            }

            return View(localizedText);
        }

        // GET: Administration/LocalizedTexts/Create
        public IActionResult Create()
        {
            ViewData["Language"] = new SelectList(_context.Languages.Where(x => x.Active == true), "Id", "LanguageName");
            ViewData["Page"] = new SelectList(_context.Pages, "Id", "Name");
            return View();
        }

        // POST: Administration/LocalizedTexts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Text,Language,PlacementPointCode,Page")] LocalizedText localizedText)
        {
            if (ModelState.IsValid)
            {
                localizedText.PlacementPointCode = _context.Pages.Where(x => x.Id == localizedText.Page).Select(x => x.Name).FirstOrDefault().Replace(" ", "") +
                  "." + localizedText.PlacementPointCode;
                _context.Add(localizedText);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Language"] = new SelectList(_context.Languages, "Id", "LanguageName", localizedText.Language);
            ViewData["Page"] = new SelectList(_context.Pages, "Id", "Name", localizedText.Page);
            return View(localizedText);
        }

        // GET: Administration/LocalizedTexts/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var localizedText = await _context.LocalizedTexts.FindAsync(id);
            if (localizedText == null)
            {
                return NotFound();
            }
            localizedText.PlacementPointCode = localizedText.PlacementPointCode.Replace(
              _context.Pages.Where(x => x.Id == localizedText.Page).Select(x => x.Name).FirstOrDefault() + "."
              , "");
            ViewData["Language"] = new SelectList(_context.Languages.Where(x => x.Active == true), "Id", "LanguageName", localizedText.Language);
            ViewData["Page"] = new SelectList(_context.Pages, "Id", "Name", localizedText.Page);
            return View(localizedText);
        }

        // POST: Administration/LocalizedTexts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Text,Language,PlacementPointCode,Page")] LocalizedText localizedText,
          bool LimitedEntry)
        {   
            if (id != localizedText.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
              try
              {
                if (LimitedEntry)
                {
                  localizedText.Language = _context.LocalizedTexts.Where(x => x.Id == id).Select(x => x.Language).FirstOrDefault();
                  localizedText.Page = _context.LocalizedTexts.Where(x => x.Id == id).Select(x => x.Page).FirstOrDefault();

                  localizedText.PlacementPointCode =
                    _context.Pages.Where(x => x.Id == localizedText.Page).Select(x => x.Name).FirstOrDefault().Replace(" ", "") +
                    "." + _context.LocalizedTexts.Where(
                    x => x.Id == id).Select(x => x.PlacementPointCode).FirstOrDefault().Replace(
                   _context.Pages.Where(x => x.Id == localizedText.Page).Select(x => x.Name).FirstOrDefault().Replace(" ", "") + ".", "");
                }
                _context.Update(localizedText);
                await _context.SaveChangesAsync();
              }
              catch (DbUpdateConcurrencyException)
              {
                  if (!LocalizedTextExists(localizedText.Id))
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
            ViewData["Language"] = new SelectList(_context.Languages, "Id", "LanguageName", localizedText.Language);
            ViewData["Page"] = new SelectList(_context.Pages, "Id", "Name", localizedText.Page);
            return View(localizedText);
        }

        // GET: Administration/LocalizedTexts/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var localizedText = await _context.LocalizedTexts
                .Include(l => l.LanguageNavigation)
                .Include(l => l.PageNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (localizedText == null)
            {
                return NotFound();
            }

            return View(localizedText);
        }

        // POST: Administration/LocalizedTexts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var localizedText = await _context.LocalizedTexts.FindAsync(id);
            _context.LocalizedTexts.Remove(localizedText);
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
            if (_context.LocalizedTexts.Where(x => x.Language == Language).Any(x => x.PlacementPointCode.Equals(prefixedPlacementCode)))
            {
              return Json(false);
            }
          }
          return Json(true);
        }

        private bool LocalizedTextExists(long id)
        {
            return _context.LocalizedTexts.Any(e => e.Id == id);
        }
    }
}
