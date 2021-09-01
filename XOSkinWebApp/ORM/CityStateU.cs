using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class CityStateU
    {
        public CityStateU()
        {
            Addresses = new HashSet<Address>();
        }

        public int Id { get; set; }
        public string CityName { get; set; }
        public string StateAbbreviation { get; set; }
        public string StateName { get; set; }

        public virtual ICollection<Address> Addresses { get; set; }
    }
}
