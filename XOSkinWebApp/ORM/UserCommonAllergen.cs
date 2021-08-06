using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class UserCommonAllergen
    {
        public long Id { get; set; }
        public long User { get; set; }
        public int CommonAllergen { get; set; }

        public virtual CommonAllergen CommonAllergenNavigation { get; set; }
        public virtual User UserNavigation { get; set; }
    }
}
