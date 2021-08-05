using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class Page
    {
        public Page()
        {
            LocalizedImages = new HashSet<LocalizedImage>();
            LocalizedTexts = new HashSet<LocalizedText>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<LocalizedImage> LocalizedImages { get; set; }
        public virtual ICollection<LocalizedText> LocalizedTexts { get; set; }
    }
}
