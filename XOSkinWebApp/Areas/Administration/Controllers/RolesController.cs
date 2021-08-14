using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using XOSkinWebApp.Areas.Identity.Models;

namespace XOSkinWebApp.Areas.Administration.Controllers
{
  //[Authorize(Roles = "Administrator")]
  [Area("Administration")]
  public class RolesController : Controller
  {
    private RoleManager<IdentityRole> roleManager;
    private UserManager<ApplicationUser> userManager;

    public RolesController(RoleManager<IdentityRole> RoleManager, UserManager<ApplicationUser> UserManager)
    {
      roleManager = RoleManager;
      userManager = UserManager;
    }

    public ViewResult Index() => View(roleManager.Roles);

    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create ([Required]string Name)
    {
      if (ModelState.IsValid)
      {
        IdentityResult result = await roleManager.CreateAsync(new IdentityRole(Name));
        if (result.Succeeded)
          return RedirectToAction("Index");
        else
          Errors(result);
      }
      return View(Name);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string Id)
    {
      IdentityRole role = await roleManager.FindByIdAsync(Id);
      if (role != null)
      {
        IdentityResult result = await roleManager.DeleteAsync(role);
        if (result.Succeeded)
          return RedirectToAction("Index");
        else
          Errors(result);
      }
      else
      {
        ModelState.AddModelError("An error has resulted from a role management operation.", "No role found.");
      }
      return View("Index", roleManager.Roles);
    }

    public async Task<IActionResult> Update(string Id)
    {
      IdentityRole role = await roleManager.FindByIdAsync(Id);
      List<ApplicationUser> member = new List<ApplicationUser>();
      List<ApplicationUser> nonMember = new List<ApplicationUser>();

      foreach (ApplicationUser user in userManager.Users)
      {
        var list = await userManager.IsInRoleAsync(user, role.Name) ? member : nonMember;
        list.Add(user);
      }

      return View(new RoleEdit
      {
        Role = role,
        Member = member,
        NonMember = nonMember
      });
    }

    [HttpPost]
    public async Task<IActionResult> Update(RoleModification Model)
    {
      IdentityResult result;

      if (ModelState.IsValid)
      {
        foreach (string userId in Model.AddId ?? new string[] { })
        {
          ApplicationUser user = await userManager.FindByIdAsync(userId);
          
          if (user != null)
          {
            result = await userManager.AddToRoleAsync(user, Model.RoleName);
            if (!result.Succeeded)
              Errors(result);
          }
        }

        foreach (string userId in Model.DeleteId ?? new string[] { })
        {
          ApplicationUser user = await userManager.FindByIdAsync(userId);

          if (user != null)
          {
            result = await userManager.RemoveFromRoleAsync(user, Model.RoleName);
            if (!result.Succeeded)
              Errors(result);
          }
        }
      }
      if (ModelState.IsValid)
        return RedirectToAction(nameof(Index));
      else
        return await Update(Model.RoleId);
    }

    private void Errors(IdentityResult Result)
    {
      foreach (IdentityError error in Result.Errors)
        ModelState.AddModelError("An error has resulted from a role management operation.", error.Description);
    }
  }
}
