using System;
using System.Collections.Generic;

namespace XOSkinWebApp.ORM
{
    public partial class ProductOrderDiscountCode
    {
        public long Id { get; set; }
        public long Code { get; set; }
        public long ProductOrder { get; set; }

        public virtual DiscountCode CodeNavigation { get; set; }
        public virtual ProductOrder ProductOrderNavigation { get; set; }
    }
}
