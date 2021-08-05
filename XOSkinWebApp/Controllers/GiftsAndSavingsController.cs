using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Controllers
{
  public class GiftsAndSavingsController : Controller
  {
    private readonly XOSkinContext _context;

    public GiftsAndSavingsController(XOSkinContext context)
    {
      _context = context;
    }

    public IActionResult Index()
    {
      ViewData.Add("GiftsAndSavings.WelcomeText", _context.LocalizedTexts.Where(x => x.PlacementPointCode.Equals("GiftsAndSavings.WelcomeText")).Select(x => x.Text).FirstOrDefault());
      
      return View();
    }
  }
}
