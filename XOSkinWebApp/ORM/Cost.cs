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
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
