﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XOSkinWebApp.Controllers
{
  public class UserListsController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }
  }
}