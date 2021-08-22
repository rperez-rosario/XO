using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class LocalizedText
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public int Language { get; set; }
        public string PlacementPointCode { get; set; }
        public int Page { get; set; }

        public virtual Language LanguageNavigation { get; set; }
        public virtual Page PageNavigation { get; set; }
    }
}
