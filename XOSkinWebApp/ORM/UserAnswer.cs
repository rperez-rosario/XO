using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class UserAnswer
    {
        public long Id { get; set; }
        public string Answer { get; set; }
        public long Question { get; set; }
        public long User { get; set; }
        public DateTime AnswerDate { get; set; }

        public virtual Question QuestionNavigation { get; set; }
        public virtual User UserNavigation { get; set; }
    }
}
