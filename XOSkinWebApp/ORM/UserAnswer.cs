using System;
using System.Collections.Generic;

namespace XOSkinWebApp.ORM
{
    public partial class UserAnswer
    {
        public long Id { get; set; }
        public string Answer { get; set; }
        public long Question { get; set; }
        public string User { get; set; }
        public DateTime AnswerDate { get; set; }

        public virtual Question QuestionNavigation { get; set; }
        public virtual AspNetUser UserNavigation { get; set; }
    }
}
