using System;
using System.Collections.Generic;

namespace XOSkinWebApp.ORM
{
    public partial class Questionnaire
    {
        public Questionnaire()
        {
            Questions = new HashSet<Question>();
        }

        public int Id { get; set; }
        public string QuestionnaireName { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public string CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }

        public virtual AspNetUser CreatedByNavigation { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
    }
}
