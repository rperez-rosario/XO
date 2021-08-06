using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class AnswerIngredient
    {
        public long Id { get; set; }
        public long Answer { get; set; }
        public long Ingredient { get; set; }
        public bool Recommend { get; set; }
        public bool Prohibit { get; set; }

        public virtual PossibleAnswer AnswerNavigation { get; set; }
    }
}
