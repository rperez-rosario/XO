using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace XOSkinWebApp.Models.Administration
{
    public class LanguageViewModel
    {
      [Key]
      long Id { get; set; }

      String LanguageName { get; set; }
    }
}