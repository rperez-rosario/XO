using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class Cost
    {
        public Cost()
        {
            Products = new HashSet<Product>();
        }

        public long Id { get; set; }
        public decimal Amount { get; set; }
        public bool Active { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }

        public virtual AspNetUser CreatedByNavigation { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
