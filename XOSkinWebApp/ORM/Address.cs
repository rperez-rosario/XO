using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class Address
    {
        public long Id { get; set; }
        public string User { get; set; }
        public short AddressType { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Apartment { get; set; }
        public int? CityUs { get; set; }
        public int? CityPr { get; set; }
        public int? CityWorld { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
        public string ZipCode5 { get; set; }
        public string ZipCode4 { get; set; }
        public string ForeignPostalCode { get; set; }

        public virtual AddressType AddressTypeNavigation { get; set; }
        public virtual CityPr CityPrNavigation { get; set; }
        public virtual CityStateU CityUsNavigation { get; set; }
        public virtual CityStateCountryWorld CityWorldNavigation { get; set; }
        public virtual AspNetUser UserNavigation { get; set; }
    }
}
