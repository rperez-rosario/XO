using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using XOSkinWebApp.Models;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Controllers
{
  public class HomeController : Controller
  {
    private readonly ILogger<HomeController> _logger;
    private readonly XOSkinContext _context;

    public HomeController(XOSkinContext context, ILogger<HomeController> logger)
    {
      _logger = logger;
      _context = context;
    }

    public IActionResult Index()
    {
      ViewData.Add("Splash.SplashText", _context.LocalizedTexts.Where(x => x.PlacementPointCode.Equals("Splash.SplashText")).Select(x => x.Text).FirstOrDefault());

      ViewData.Add("Home.WelcomeText", _context.LocalizedTexts.Where(x => x.PlacementPointCode.Equals("Home.WelcomeText")).Select(x => x.Text).FirstOrDefault());
      ViewData.Add("Home.PromotionalRibbon", _context.LocalizedTexts.Where(x => x.PlacementPointCode.Equals("Home.PromotionalRibbon")).Select(x => x.Text).FirstOrDefault());
      
      return View();
    }

    public IActionResult Privacy()
    {
      ViewData.Add("Privacy.PrivacyPolicy", _context.LocalizedTexts.Where(x => x.PlacementPointCode.Equals("Privacy.PrivacyPolicy")).Select(x => x.Text).FirstOrDefault());

      return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
  }
}
