using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Controllers
{
    public class ShoppingCartController : Controller
    {
      private readonly XOSkinContext _context;

      public ShoppingCartController(XOSkinContext context)
      {
       _context = context;
      } 
    
      public IActionResult Index()
      {
      ViewData.Add("ShoppingCart.WelcomeText", _context.LocalizedTexts.Where(x => x.PlacementPointCode.Equals("ShoppingCart.WelcomeText")).Select(x => x.Text).FirstOrDefault());

      return View();
      }
  }
}
