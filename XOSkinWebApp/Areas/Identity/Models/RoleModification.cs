using System.ComponentModel.DataAnnotations;

namespace XOSkinWebApp.Areas.Identity.Models
{
  public class RoleModification
  {
    [Required]
    public string RoleName { get; set; }

    public string RoleId { get; set; }
    public string[] AddId { get; set; }
    public string[] DeleteId { get; set; }
  }
}
