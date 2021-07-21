using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XOSkinWebApp.Models
{
  public class ProductViewModel
  {
    public String ProductName { get; set; }
    public String ProductPrice { get; set; }

    public ProductViewModel()
    {
      this.ProductName = String.Empty;
    }

    public ProductViewModel(String ProductName, decimal ProductPrice)
    {
      this.ProductName = ProductName;
      this.ProductPrice = ProductPrice.ToString();
    }
  }
}
