﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace XOSkinWebApp.Controllers
{
  public class AdministrationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
