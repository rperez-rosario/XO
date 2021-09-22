using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Controllers
{
  [Authorize]
  public class SubscriptionsAndReferralsController : Controller
  {
    private readonly XOSkinContext _context;

    public SubscriptionsAndReferralsController(XOSkinContext context)
    {
      _context = context;
    }

    public IActionResult Index()
    {
      ViewData.Add("SubscriptionsAndReferrals.WelcomeText", _context.LocalizedTexts.Where(
        x => x.PlacementPointCode.Equals("SubscriptionsAndReferrals.WelcomeText")).Select(x => x.Text).FirstOrDefault());

      return View();
    }
  }
}
