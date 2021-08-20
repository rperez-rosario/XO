using System;
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
  public partial class LocalizedImageViewModel
  {
    public long Id { get; set; }
    public string Path { get; set; }
    public int Language { get; set; }

    [StringLength(500, ErrorMessage = "Maximum field length is 500.")]
    [Required(ErrorMessage = "Placement code required.")]
    [Remote("PlacementCodeAvailable", "LocalizedImages", ErrorMessage = "Placement code already registered.", AdditionalFields = "ActionCreate, Page, Language, OriginalLanguage, OriginalPlacementCode")]
    public string PlacementPointCode { get; set; }

    public int Page { get; set; }

    public virtual Language LanguageNavigation { get; set; }
    public virtual Page PageNavigation { get; set; }
  }
}
