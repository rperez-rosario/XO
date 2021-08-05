using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Controllers
{
  public class YourFaceController : Controller
  {
    private readonly XOSkinContext _context;

    public YourFaceController(XOSkinContext context)
    {
      _context = context;
    }

    public IActionResult Index()
    {
      ViewData.Add("YourFace.WelcomeText", _context.LocalizedTexts.Where(x => x.PlacementPointCode.Equals("YourFace.WelcomeText")).Select(x => x.Text).FirstOrDefault());

      return View();
    }
  }
}
