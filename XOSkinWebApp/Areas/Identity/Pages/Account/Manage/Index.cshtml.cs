using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using XOSkinWebApp.Areas.Identity.Models;
using XOSkinWebApp.Data;

namespace XOSkinWebApp.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

    public IndexModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
          [Phone]
          [Display(Name = "Phone number")]
          public string PhoneNumber { get; set; }

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
          [Display(Name = "Name")]
          public String FirstName { get; set; }

          [Required]
          [DataType(DataType.Text)]
          [Display(Name = "Last Name")]
          public String LastName { get; set; }
    }

    private async Task LoadAsync(ApplicationUser user)
    {
      var userName = await _userManager.GetUserNameAsync(user);
      var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
      //String homePhoneNumber = await _context.Users.Where(x => x.Id.Equals(user.Id)).Select(x => x.HomePhoneNumber).FirstAsync();
      //String workPhoneNumber = await _context.Users.Where(x => x.Id.Equals(user.Id)).Select(x => x.WorkPhoneNumber).FirstAsync();
      //String additionalPhoneNumber = await _context.Users.Where(x => x.Id.Equals(user.Id)).Select(x => x.AdditionalPhoneNumber).FirstAsync();
      String firstName = await _context.Users.Where(x => x.Id.Equals(user.Id)).Select(x => x.FirstName).FirstAsync();
      String lastName = await _context.Users.Where(x => x.Id.Equals(user.Id)).Select(x => x.LastName).FirstAsync();

      Username = userName;

      Input = new InputModel
      {
        PhoneNumber = phoneNumber,
        //HomePhoneNumber = homePhoneNumber,
        //WorkPhoneNumber = workPhoneNumber,
        //AdditionalPhoneNumber = additionalPhoneNumber,
        FirstName = firstName,
        LastName = lastName
      };
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
      var user = await _userManager.GetUserAsync(User);
      ApplicationUser appUser = null;

      if (user == null)
      {
          return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
      }

      if (ModelState.IsValid)
      {
        try
        {
          appUser = _context.Users.Where(x => x.Id.Equals(user.Id)).FirstOrDefault();
          //appUser.HomePhoneNumber = Input.HomePhoneNumber;
          //appUser.WorkPhoneNumber = Input.WorkPhoneNumber;
          //appUser.AdditionalPhoneNumber = Input.AdditionalPhoneNumber;
          appUser.FirstName = Input.FirstName;
          appUser.LastName = Input.LastName;
          _context.Update(appUser);
          _context.SaveChanges();
        } 
        catch (DbUpdateConcurrencyException)
        {
          return NotFound();
        }
        
      }

      if (!ModelState.IsValid)
      {
        await LoadAsync(user);
        return Page();
      }

      var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
      if (Input.PhoneNumber != phoneNumber)
      {
        var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
        if (!setPhoneResult.Succeeded)
        {
          StatusMessage = "Unexpected error when trying to set phone number.";
          return RedirectToPage();
        }
      }

      await _signInManager.RefreshSignInAsync(user);
      StatusMessage = "Your profile has been updated";
      return RedirectToPage();
    }
  }
}
