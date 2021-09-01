using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class CityStateCountryWorld
    {
        public CityStateCountryWorld()
        {
            Addresses = new HashSet<Address>();
        }

        public int Id { get; set; }
        public string CityName { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }

        public virtual ICollection<Address> Addresses { get; set; }
    }
}
