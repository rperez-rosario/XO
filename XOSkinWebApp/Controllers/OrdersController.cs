using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
      private readonly XOSkinContext _context;

      public OrdersController(XOSkinContext context)
      {
        _context = context;
      }

      public IActionResult Index()
      {
        ViewData.Add("Orders.WelcomeText", _context.LocalizedTexts.Where(x => x.PlacementPointCode.Equals("Orders.WelcomeText")).Select(x => x.Text).FirstOrDefault());

        return View();
      }
   }
}
