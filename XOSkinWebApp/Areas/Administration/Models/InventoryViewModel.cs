using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace XOSkinWebApp.Areas.Administration.Models
{
  public class InventoryViewModel
  {
    [Key]
    public long ProductId { get; set; }
    public String ProductName { get; set; }
    public String Sku { get; set; }
    public long CurrentStock { get; set; }
    public long NewStock { get; set; }
    public bool IsKit { get; set; }
  }
}
