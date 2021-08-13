using XOSkinWebApp.Areas.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XOSkinWebApp.TagHelper
{
  [HtmlTargetElement("td", Attributes="i-role")]
  public class RoleUsersTH : Microsoft.AspNetCore.Razor.TagHelpers.TagHelper
  {
    private UserManager<ApplicationUser> userManager;
    private RoleManager<IdentityRole> roleManager;

    [HtmlAttributeName("i-role")]
    public string Role { get; set; }

    public RoleUsersTH(UserManager<ApplicationUser> UserManager, RoleManager<IdentityRole> RoleManager)
    {
      userManager = UserManager;
      roleManager = RoleManager;
    }

    public override async Task ProcessAsync(TagHelperContext Context, TagHelperOutput Output)
    {
      List<string> names = new List<string>();
      IdentityRole role = await roleManager.FindByIdAsync(Role);

      if (role != null)
      {
        foreach (var user in userManager.Users)
        {
          if (user != null && await userManager.IsInRoleAsync(user, role.Name))
            names.Add(user.UserName);
        }
      }
      Output.Content.SetContent(names.Count == 0 ? "No Users" : string.Join(", ", names));
    }
  }
}
