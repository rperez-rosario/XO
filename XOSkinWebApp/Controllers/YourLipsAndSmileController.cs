using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Controllers
{
  public class YourLipsAndSmileController : Controller
  {
    private readonly XOSkinContext _context;

    public YourLipsAndSmileController(XOSkinContext context)
    {
      _context = context;
    }

    public IActionResult Index()
    {
      ViewData.Add("YourLipsAndSmile.WelcomeText", _context.LocalizedTexts.Where(x => x.PlacementPointCode.Equals("YourLipsAndSmile.WelcomeText")).Select(x => x.Text).FirstOrDefault());
      return View();
    }
  }
}
