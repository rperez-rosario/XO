﻿using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class OrderBillTo
    {
        public long Id { get; set; }
        public long Order { get; set; }
        public string NameOnCreditCard { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string CityName { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
        public string PostalCode { get; set; }
        public DateTime? BillingDate { get; set; }

        public virtual ProductOrder OrderNavigation { get; set; }
    }
}