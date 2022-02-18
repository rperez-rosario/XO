using System;
using System.Collections.Generic;

namespace XOSkinWebApp.ORM
{
    public partial class TransactionConcept
    {
        public TransactionConcept()
        {
            UserLedgerTransactions = new HashSet<UserLedgerTransaction>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<UserLedgerTransaction> UserLedgerTransactions { get; set; }
    }
}
