using System;
using System.Collections.Generic;

namespace XOSkinWebApp.ORM
{
    public partial class UserCommonAllergen
    {
        public long Id { get; set; }
        public string User { get; set; }
        public int CommonAllergen { get; set; }

        public virtual CommonAllergen CommonAllergenNavigation { get; set; }
        public virtual AspNetUser UserNavigation { get; set; }
    }
}
