using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class AddressType
    {
        public AddressType()
        {
            Addresses = new HashSet<Address>();
        }

        public short Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Address> Addresses { get; set; }
    }
}
