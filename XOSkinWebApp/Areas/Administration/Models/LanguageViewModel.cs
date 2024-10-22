﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using CompareAttribute =
System.ComponentModel.DataAnnotations.CompareAttribute;
using Microsoft.AspNetCore.Mvc;
using XOSkinWebApp.ORM;

#nullable disable

namespace XOSkinWebApp.Areas.Administration.Models
{
  public partial class LanguageViewModel
  {
    public LanguageViewModel()
    {
      LocalizedImages = new HashSet<LocalizedImage>();
      LocalizedTexts = new HashSet<LocalizedText>();
    }

    public int Id { get; set; }

    [StringLength(50, ErrorMessage = "Maximum field length is 50.")]
    [Required(ErrorMessage = "Language name required.")]
    [Remote("LanguageNameAvailable", "Languages", ErrorMessage = "Language name already registered.", 
      AdditionalFields = "ActionCreate, OriginalLanguageName")]
    public string LanguageName { get; set; }

    public bool Active { get; set; }

    public virtual ICollection<LocalizedImage> LocalizedImages { get; set; }
    public virtual ICollection<LocalizedText> LocalizedTexts { get; set; }
  }
}
