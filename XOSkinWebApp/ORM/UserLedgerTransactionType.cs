using System;
using System.Collections.Generic;

#nullable disable

namespace XOSkinWebApp.ORM
{
    public partial class UserLedgerTransactionType
    {
        public UserLedgerTransactionType()
        {
            UserLedgerTransactions = new HashSet<UserLedgerTransaction>();
        }

        public short Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<UserLedgerTransaction> UserLedgerTransactions { get; set; }
    }
}
