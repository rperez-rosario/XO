﻿using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class LocalizedImage
    {
        public long Id { get; set; }
        public string Path { get; set; }
        public int Language { get; set; }
        public string PlacementPointCode { get; set; }

        public virtual Language LanguageNavigation { get; set; }
    }
}