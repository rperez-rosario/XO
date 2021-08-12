using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using CompareAttribute =
System.ComponentModel.DataAnnotations.CompareAttribute;
using Microsoft.AspNetCore.Mvc;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class Questionnaire
    {
        public Questionnaire()
        {
            Questions = new HashSet<Question>();
        }

        public int Id { get; set; }

        [StringLength(200, ErrorMessage = "Maximum field length is 200.")]
        [Required(ErrorMessage = "Questionnaire name required.")]
        [Remote("QuestionnaireNameAvailable", "Questionnaires", ErrorMessage = "Questionnaire name already registered.", AdditionalFields = "ActionCreate, OriginalQuestionnaireName")]
        public string QuestionnaireName { get; set; }
    
        public string Description { get; set; }
        public bool Active { get; set; }
        public string CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }

        public virtual AspNetUser CreatedByNavigation { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
    }
}
