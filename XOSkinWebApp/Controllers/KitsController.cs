using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Controllers
{
  public class KitsController : Controller
  {
    private readonly XOSkinContext _context;

    public KitsController(XOSkinContext context)
    {
      _context = context;
    }

    public IActionResult Index()
    {
      ViewData.Add("Kits.WelcomeText", _context.LocalizedTexts.Where(x => x.PlacementPointCode.Equals("Kits.WelcomeText")).Select(x => x.Text).FirstOrDefault());

      return View();
    }
  }
}
