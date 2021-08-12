using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class UserProductList
    {
        public UserProductList()
        {
            UserProductListProducts = new HashSet<UserProductListProduct>();
        }

        public long Id { get; set; }
        public string User { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual AspNetUser UserNavigation { get; set; }
        public virtual ICollection<UserProductListProduct> UserProductListProducts { get; set; }
    }
}
