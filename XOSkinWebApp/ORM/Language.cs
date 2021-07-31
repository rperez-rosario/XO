using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class Language
    {
        public Language()
        {
            Texts = new HashSet<Text>();
            Users = new HashSet<User>();
        }

        public int Id { get; set; }
        public string LanguageName { get; set; }

        public virtual ICollection<Text> Texts { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
