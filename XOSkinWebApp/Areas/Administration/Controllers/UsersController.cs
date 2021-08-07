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
  public class UsersController : Controller
  {
    private readonly XOSkinContext _context;

    public UsersController(XOSkinContext context)
    {
      _context = context;
    }

    // GET: Administration/Users
    public async Task<IActionResult> Index()
    {
      var xOSkinContext = _context.Users.Include(u => u.PreferredLanguageNavigation).Include(u => u.UserGroupNavigation);
      return View(await xOSkinContext.ToListAsync());
    }

    // GET: Administration/Users/Details/5
    public async Task<IActionResult> Details(long? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var user = await _context.Users
          .Include(u => u.PreferredLanguageNavigation)
          .Include(u => u.UserGroupNavigation)
          .FirstOrDefaultAsync(m => m.Id == id);
      if (user == null)
      {
        return NotFound();
      }

      return View(user);
    }

    // GET: Administration/Users/Create
    public IActionResult Create()
    {
      ViewData["PreferredLanguage"] = new SelectList(_context.Languages, "Id", "LanguageName");
      ViewData["UserGroup"] = new SelectList(_context.UserGroups, "Id", "Name");
      return View();
    }

    // POST: Administration/Users/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,EmailAddress,HomePhoneNumber,WorkPhoneNumber,AdditionalPhoneNumber,PasswordHash,FirstName,LastName,PreferredLanguage,IsLocked,UserGroup")] User user)
    {
      if (ModelState.IsValid)
      {
        user.EmailAddress = user.EmailAddress.Trim();
        _context.Add(user);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
      }
      ViewData["PreferredLanguage"] = new SelectList(_context.Languages, "Id", "LanguageName", user.PreferredLanguage);
      ViewData["UserGroup"] = new SelectList(_context.UserGroups, "Id", "Description", user.UserGroup);
      return View(user);
    }

    // GET: Administration/Users/Edit/5
    public async Task<IActionResult> Edit(long? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var user = await _context.Users.FindAsync(id);
      if (user == null)
      {
        return NotFound();
      }
      ViewData["PreferredLanguage"] = new SelectList(_context.Languages, "Id", "LanguageName", user.PreferredLanguage);
      ViewData["UserGroup"] = new SelectList(_context.UserGroups, "Id", "Name", user.UserGroup);
      return View(user);
    }

    // POST: Administration/Users/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, [Bind("Id,EmailAddress,HomePhoneNumber,WorkPhoneNumber,AdditionalPhoneNumber,PasswordHash,FirstName,LastName,PreferredLanguage,IsLocked,UserGroup")] User user)
    {
      if (id != user.Id)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
          user.EmailAddress = user.EmailAddress.Trim();
          _context.Update(user);
          await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!UserExists(user.Id))
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
      ViewData["PreferredLanguage"] = new SelectList(_context.Languages, "Id", "LanguageName", user.PreferredLanguage);
      ViewData["UserGroup"] = new SelectList(_context.UserGroups, "Id", "Name", user.UserGroup);
      return View(user);
    }

    // GET: Administration/Users/Delete/5
    public async Task<IActionResult> Delete(long? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var user = await _context.Users
          .Include(u => u.PreferredLanguageNavigation)
          .Include(u => u.UserGroupNavigation)
          .FirstOrDefaultAsync(m => m.Id == id);
      if (user == null)
      {
        return NotFound();
      }

      return View(user);
    }

    // POST: Administration/Users/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
      var user = await _context.Users.FindAsync(id);
      _context.Users.Remove(user);
      await _context.SaveChangesAsync();
      return RedirectToAction(nameof(Index));
    }

    public JsonResult EmailAddressAvailable(String EmailAddress, bool ActionCreate,
          String OriginalEmailAddress)
    {
      if (ActionCreate || (!ActionCreate && !EmailAddress.Equals(OriginalEmailAddress)))
      {
        if (_context.Users.Any(x => x.EmailAddress.Equals(EmailAddress.Trim())))
        {
          return Json(false);
        }
      }
      return Json(true);
    }

    private bool UserExists(long id)
    {
      return _context.Users.Any(e => e.Id == id);
    }
  }
}