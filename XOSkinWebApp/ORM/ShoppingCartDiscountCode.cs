using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class ShoppingCartDiscountCode
    {
        public long Id { get; set; }
        public long Code { get; set; }
        public long ShoppingCart { get; set; }

        public virtual DiscountCode CodeNavigation { get; set; }
        public virtual ShoppingCart ShoppingCartNavigation { get; set; }
    }
}
