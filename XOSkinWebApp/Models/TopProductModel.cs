using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XOSkinWebApp.Models
{
  public class TopProductModel
  { 
    public TopProductModel(String Id, String ImageSource, String Name, String Description, decimal? Price)
    {
      this.Id = Id;
      this.ImageSource = ImageSource;
      this.Name = Name;
      this.Description = Description;
      this.Price = Price;
    }

    public string Id { get; set; }
    public string ImageSource { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal? Price { get; set; }
  }
}
