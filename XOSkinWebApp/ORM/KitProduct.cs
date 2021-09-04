using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class KitProduct
    {
        public int Id { get; set; }
        public long? Kit { get; set; }
        public long Product { get; set; }

        public virtual Product KitNavigation { get; set; }
    }
}
