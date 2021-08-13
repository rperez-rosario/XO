using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace XOSkinWebApp.Areas.Identity.Models
{
  public class RoleEdit
  {
    public IdentityRole Role { get; set; }
    public IEnumerable<ApplicationUser> Member { get; set; }
    public IEnumerable<ApplicationUser> NonMember { get; set; }
  }
}
