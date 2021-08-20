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
  public partial class QuestionViewModel
  {
    public QuestionViewModel()
    {
      PossibleAnswers = new HashSet<PossibleAnswer>();
      UserAnswers = new HashSet<UserAnswer>();
    }

    public long Id { get; set; }

    [Required(ErrorMessage = "Question text required.")]
    public string QuestionText { get; set; }

    public int Questionnaire { get; set; }

    [Required(ErrorMessage = "Question order required.")]
    public long DisplayOrder { get; set; }

    public virtual Questionnaire QuestionnaireNavigation { get; set; }
    public virtual ICollection<PossibleAnswer> PossibleAnswers { get; set; }
    public virtual ICollection<UserAnswer> UserAnswers { get; set; }
  }
}
