using System;
using System.Collections.Generic;

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
        public string QuestionnaireName { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public long CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }

        public virtual User CreatedByNavigation { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
    }
}
