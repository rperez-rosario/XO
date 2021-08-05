using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Controllers
{
  public class TopSellersController : Controller
  {
    private readonly XOSkinContext _context;

    public TopSellersController(XOSkinContext context)
    {
      _context = context;
    }

    public IActionResult Index()
    {
      ViewData.Add("TopSellers.WelcomeText", _context.LocalizedTexts.Where(x => x.PlacementPointCode.Equals("TopSellers.WelcomeText")).Select(x => x.Text).FirstOrDefault());

      return View();
    }
  }
}
