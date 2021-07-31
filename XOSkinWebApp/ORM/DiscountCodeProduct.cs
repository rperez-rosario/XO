using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class DiscountCodeProduct
    {
        public long Id { get; set; }
        public long Code { get; set; }
        public long Product { get; set; }

        public virtual DiscountCode CodeNavigation { get; set; }
        public virtual Product ProductNavigation { get; set; }
    }
}
