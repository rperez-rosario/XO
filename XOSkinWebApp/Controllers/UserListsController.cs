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
  public class UserListsController : Controller
  {
    private readonly XOSkinContext _context;

    public UserListsController(XOSkinContext context)
    {
      _context = context;
    }

    public IActionResult Index()
    {
      ViewData.Add("UserLists.WelcomeText", _context.LocalizedTexts.Where(x => x.PlacementPointCode.Equals("UserLists.WelcomeText")).Select(x => x.Text).FirstOrDefault());

      return View();
    }
  }
}
