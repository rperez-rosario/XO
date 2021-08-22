using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class Question
    {
        public Question()
        {
            PossibleAnswers = new HashSet<PossibleAnswer>();
            UserAnswers = new HashSet<UserAnswer>();
        }

        public long Id { get; set; }
        public string QuestionText { get; set; }
        public int Questionnaire { get; set; }
        public long DisplayOrder { get; set; }

        public virtual Questionnaire QuestionnaireNavigation { get; set; }
        public virtual ICollection<PossibleAnswer> PossibleAnswers { get; set; }
        public virtual ICollection<UserAnswer> UserAnswers { get; set; }
    }
}
