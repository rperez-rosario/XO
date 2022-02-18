using System;
using System.Collections.Generic;

namespace XOSkinWebApp.ORM
{
    public partial class PossibleAnswer
    {
        public PossibleAnswer()
        {
            AnswerIngredients = new HashSet<AnswerIngredient>();
        }

        public long Id { get; set; }
        public long Question { get; set; }
        public string Answer { get; set; }

        public virtual Question QuestionNavigation { get; set; }
        public virtual ICollection<AnswerIngredient> AnswerIngredients { get; set; }
    }
}
