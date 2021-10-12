using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace XOSkinWebApp.Areas.Administration.Models
{
  public class LedgerViewModel
  {
    [Key]
    public long TransactionId { get; set; }
    public String User { get; set; }
    public String OrderID { get; set; }
    public String TransactionType { get; set; }
    public String TransactionConcept { get; set; }
    public String Description { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceBeforeTransaction { get; set; }
    public decimal BalanceAfterTransaction { get; set; }
    public String CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
  }
}
