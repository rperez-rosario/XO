using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Controllers
{
  public class YourEyesController : Controller
  {
    private readonly XOSkinContext _context;

    public YourEyesController(XOSkinContext context)
    {
      _context = context;
    }

    public IActionResult Index()
    {
      ViewData.Add("YourEyes.WelcomeText", _context.LocalizedTexts.Where(x => x.PlacementPointCode.Equals("YourEyes.WelcomeText")).Select(x => x.Text).FirstOrDefault());

      return View();
    }
  }
}
