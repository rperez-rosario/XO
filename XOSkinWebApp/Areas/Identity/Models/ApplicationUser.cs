using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace XOSkinWebApp.Areas.Identity.Models
{
  public class ApplicationUser : IdentityUser
  {
    //[DataType(DataType.PhoneNumber)]
    //[Display(Name = "Home Phone Number")]
    //public String HomePhoneNumber { get; set; }

    //[DataType(DataType.PhoneNumber)]
    //[Display(Name = "Work Phone Number")]
    //public String WorkPhoneNumber { get; set; }

    //[DataType(DataType.PhoneNumber)]
    //[Display(Name = "Additional Phone Number")]
    //public String AdditionalPhoneNumber { get; set; }

    [Required]
    [DataType(DataType.Text)]
    [Display(Name = "First Name")]
    public String FirstName { get; set; }

    [Required]
    [DataType(DataType.Text)]
    [Display(Name = "Last Name")]
    public String LastName { get; set; }

    [DataType(DataType.Text)]
    [Display(Name = "Disabled")]
    public bool Disabled { get; set; }
  }
}
