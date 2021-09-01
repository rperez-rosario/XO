using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class CityPr
    {
        public CityPr()
        {
            Addresses = new HashSet<Address>();
        }

        public int Id { get; set; }
        public string CityName { get; set; }

        public virtual ICollection<Address> Addresses { get; set; }
    }
}
