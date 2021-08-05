using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Controllers
{
    public class JobsController : Controller
    {
      private readonly XOSkinContext _context;

      public JobsController(XOSkinContext context)
      {
        _context = context;
      }

      public IActionResult Index()
      {
        ViewData.Add("Jobs.WelcomeText", _context.LocalizedTexts.Where(x => x.PlacementPointCode.Equals("Jobs.WelcomeText")).Select(x => x.Text).FirstOrDefault());
        
        return View();
      }
    }
}
