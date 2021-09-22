using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using XOSkinWebApp.ORM;
using Microsoft.AspNetCore.Authorization;
using XOSkinWebApp.Areas.Administration.Models;

namespace XOSkinWebApp.Areas.Administration.Controllers
{
    [Area("Administration")]
    [Authorize(Roles = "Administrator")]
    public class LocalizedImagesController : Controller
    {
        private readonly XOSkinContext _context;

        public LocalizedImagesController(XOSkinContext context)
        {
            _context = context;
        }

        // GET: Administration/LocalizedImages
        public IActionResult Index()
        {
            List<LocalizedImageViewModel> localizedImage = new List<LocalizedImageViewModel>();
            
            foreach (LocalizedImage m in 
              _context.LocalizedImages.Include(l => l.LanguageNavigation).Include(l => l.PageNavigation).ToListAsync().Result)
            {
              localizedImage.Add(new LocalizedImageViewModel()
              {
                Id = m.Id,
                Path = m.Path,
                Language = m.Language,
                PlacementPointCode = m.PlacementPointCode,
                Page = m.Page,
                LanguageNavigation = m.LanguageNavigation,
                PageNavigation = m.PageNavigation
              });
            }
            
            return View(localizedImage);
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

            return View(new LocalizedImage() { 
              Id = localizedImage.Id,
              Path = localizedImage.Path,
              Language = localizedImage.Language,
              PlacementPointCode = localizedImage.PlacementPointCode,
              Page = localizedImage.Page,
              LanguageNavigation = localizedImage.LanguageNavigation,
              PageNavigation = localizedImage.PageNavigation
            });
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
        public async Task<IActionResult> Create(
          [Bind("Id,Path,Language,PlacementPointCode,Page")] LocalizedImageViewModel localizedImage)
        {
            if (ModelState.IsValid)
            {
                localizedImage.PlacementPointCode = _context.Pages.Where(
                  x => x.Id == localizedImage.Page).Select(x => x.Name).FirstOrDefault().Replace(" ", "") + 
                  "." + localizedImage.PlacementPointCode;

                _context.Add(new LocalizedImage()
                {
                  Id = localizedImage.Id,
                  Path = localizedImage.Path,
                  Language = localizedImage.Language,
                  PlacementPointCode = localizedImage.PlacementPointCode,
                  Page = localizedImage.Page,
                  LanguageNavigation = localizedImage.LanguageNavigation,
                  PageNavigation = localizedImage.PageNavigation
                });
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
            ViewData["Language"] = new SelectList(_context.Languages.Where(
              x => x.Active == true), "Id", "LanguageName", localizedImage.Language);
            ViewData["Page"] = new SelectList(_context.Pages, "Id", "Name", localizedImage.Page);
            
            return View(new LocalizedImageViewModel()
            {
              Id = localizedImage.Id,
              Path = localizedImage.Path,
              Language = localizedImage.Language,
              PlacementPointCode = localizedImage.PlacementPointCode,
              Page = localizedImage.Page,
              LanguageNavigation = localizedImage.LanguageNavigation,
              PageNavigation = localizedImage.PageNavigation
            });
       }

        // POST: Administration/LocalizedImages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
          long id, [Bind("Id,Path,Language,PlacementPointCode,Page")] LocalizedImageViewModel localizedImage, bool LimitedEntry)
        {
            if (id != localizedImage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                  if (LimitedEntry)
                  {
                    localizedImage.Language = _context.LocalizedImages.Where(
                      x => x.Id == id).Select(x => x.Language).FirstOrDefault();
                    localizedImage.Page = _context.LocalizedImages.Where(
                      x => x.Id == id).Select(x => x.Page).FirstOrDefault();

                    localizedImage.PlacementPointCode =
                      _context.Pages.Where(
                        x => x.Id == localizedImage.Page).Select(x => x.Name).FirstOrDefault().Replace(" ", "") +
                        "." + _context.LocalizedTexts.Where(
                        x => x.Id == id).Select(x => x.PlacementPointCode).FirstOrDefault().Replace(
                     _context.Pages.Where(
                       x => x.Id == localizedImage.Page).Select(x => x.Name).FirstOrDefault().Replace(" ", "") + ".", "");
                   }
                    _context.Update(new LocalizedImage()
                    {
                      Id = localizedImage.Id,
                      Path = localizedImage.Path,
                      Language = localizedImage.Language,
                      PlacementPointCode = localizedImage.PlacementPointCode,
                      Page = localizedImage.Page,
                      LanguageNavigation = localizedImage.LanguageNavigation,
                      PageNavigation = localizedImage.PageNavigation
                    });
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

            return View(new LocalizedImageViewModel()
            {
              Id = localizedImage.Id,
              Path = localizedImage.Path,
              Language = localizedImage.Language,
              PlacementPointCode = localizedImage.PlacementPointCode,
              Page = localizedImage.Page,
              LanguageNavigation = localizedImage.LanguageNavigation,
              PageNavigation = localizedImage.PageNavigation
            });
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
              _context.Pages.Where(x => x.Id == Page).Select(x => x.Name).FirstOrDefault() + "." + PlacementPointCode.Trim();
            if (_context.LocalizedImages.Where(
              x => x.Language == Language).Any(x => x.PlacementPointCode.Equals(prefixedPlacementCode)))
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
