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
        public string StateName { get; set; }
        public string CountryName { get; set; }
        public string PostalCode { get; set; }

        public virtual AddressType AddressTypeNavigation { get; set; }
        public virtual AspNetUser UserNavigation { get; set; }
    }
}
