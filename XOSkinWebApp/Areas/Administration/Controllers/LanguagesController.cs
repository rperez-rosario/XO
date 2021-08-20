using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using XOSkinWebApp.ORM;
using XOSkinWebApp.Areas.Administration.Models;

namespace XOSkinWebApp.Areas.Administration.Controllers
{
  [Authorize(Roles = "Administrator")]
  [Area("Administration")]
  public class LanguagesController : Controller
  {
    private readonly XOSkinContext _context;

    public LanguagesController(XOSkinContext context)
    {
      _context = context;
    }

    // GET: Administration/Languages
    public async Task<IActionResult> Index()
    {
      List<LanguageViewModel> language = new List<LanguageViewModel>();
      foreach (Language l in _context.Languages.ToListAsync().Result)
      {
        language.Add(new LanguageViewModel()
        {
          Id = l.Id,
          LanguageName = l.LanguageName,
          Active = l.Active,
          LocalizedImages = l.LocalizedImages,
          LocalizedTexts = l.LocalizedTexts
        });
      }
      return View(language);
    }

    // GET: Administration/Languages/Details/5
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var language = await _context.Languages
          .FirstOrDefaultAsync(m => m.Id == id);
      if (language == null)
      {
        return NotFound();
      }

      return View(new LanguageViewModel() { 
        Id = language.Id,
        LanguageName = language.LanguageName,
        Active = language.Active,
        LocalizedImages = language.LocalizedImages,
        LocalizedTexts = language.LocalizedTexts
      });
    }

    // GET: Administration/Languages/Create
    public IActionResult Create()
    {
      return View();
    }

    // POST: Administration/Languages/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,LanguageName,Active")] LanguageViewModel language)
    {
      if (ModelState.IsValid)
      {
        language.LanguageName = language.LanguageName.Trim();

        _context.Add(new Language()
        {
          Id = language.Id,
          LanguageName = language.LanguageName,
          Active = language.Active,
          LocalizedImages = language.LocalizedImages,
          LocalizedTexts = language.LocalizedTexts
        });

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
      }
      return View(language);
    }

    // GET: Administration/Languages/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var language = await _context.Languages.FindAsync(id);
      if (language == null)
      {
        return NotFound();
      }

      return View(new LanguageViewModel()
      {
        Id = language.Id,
        LanguageName = language.LanguageName,
        Active = language.Active,
        LocalizedImages = language.LocalizedImages,
        LocalizedTexts = language.LocalizedTexts
      });
    }

    // POST: Administration/Languages/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,LanguageName,Active")] LanguageViewModel language)
    {
      if (id != language.Id)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
          language.LanguageName = language.LanguageName.Trim();

          _context.Update(new Language()
          {
            Id = language.Id,
            LanguageName = language.LanguageName,
            Active = language.Active,
            LocalizedImages = language.LocalizedImages,
            LocalizedTexts = language.LocalizedTexts
          });
          await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!LanguageExists(language.Id))
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
      return View(language);
    }

    // GET: Administration/Languages/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var language = await _context.Languages
          .FirstOrDefaultAsync(m => m.Id == id);
      if (language == null)
      {
        return NotFound();
      }

      return View(new LanguageViewModel()
      {
        Id = language.Id,
        LanguageName = language.LanguageName,
        Active = language.Active,
        LocalizedImages = language.LocalizedImages,
        LocalizedTexts = language.LocalizedTexts
      });
    }

    // POST: Administration/Languages/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      var language = await _context.Languages.FindAsync(id);
      _context.Languages.Remove(language);
      await _context.SaveChangesAsync();
      return RedirectToAction(nameof(Index));
    }

    public JsonResult LanguageNameAvailable(String LanguageName, bool ActionCreate, String OriginalLanguageName)
    {
      if (OriginalLanguageName == null)
        OriginalLanguageName = String.Empty;
      if (ActionCreate || (!ActionCreate && !LanguageName.Equals(OriginalLanguageName)))
      {
        return Json(!_context.Languages.Any(x => x.LanguageName.Equals(LanguageName.Trim())));
      }
      return Json(true);
    }

    private bool LanguageExists(int id)
    {
      return _context.Languages.Any(e => e.Id == id);
    }
  }
}
