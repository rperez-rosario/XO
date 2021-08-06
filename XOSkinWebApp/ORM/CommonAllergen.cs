using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class CommonAllergen
    {
        public CommonAllergen()
        {
            UserCommonAllergens = new HashSet<UserCommonAllergen>();
        }

        public int Id { get; set; }
        public string AlergenName { get; set; }

        public virtual ICollection<UserCommonAllergen> UserCommonAllergens { get; set; }
    }
}
