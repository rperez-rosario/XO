using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class Language
    {
        public Language()
        {
            LocalizedImages = new HashSet<LocalizedImage>();
            LocalizedTexts = new HashSet<LocalizedText>();
        }

        public int Id { get; set; }
        public string LanguageName { get; set; }
        public bool Active { get; set; }

        public virtual ICollection<LocalizedImage> LocalizedImages { get; set; }
        public virtual ICollection<LocalizedText> LocalizedTexts { get; set; }
    }
}
