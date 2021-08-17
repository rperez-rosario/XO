using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XOSkinWebApp.ORM;
using XOSkinWebApp.Models;
using XOSkinWebApp.ConfigurationHelper;
using ShopifySharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace XOSkinWebApp.Controllers
{
  public class TopSellersController : Controller
  {
    private readonly XOSkinContext _context;
    private readonly IOptions<Option> _option;

    public TopSellersController(XOSkinContext context, IOptions<Option> option)
    {
      _context = context;
      _option = option;
    }

    public IActionResult Index()
    {
      List<TopProductModel> topProductModel = new List<TopProductModel>();

      IEnumerable<ShopifySharp.Product> product = GetShopifyProducts(_option.Value.ShopifyUrl, 
        _option.Value.ShopifyStoreFrontAccessToken).Result;

      foreach (ShopifySharp.Product sp in product)
      {
        topProductModel.Add(
          new TopProductModel(sp.Id.ToString(), sp.Images.ElementAt(0).Src, sp.Title, sp.BodyHtml, 
          sp.Variants.ElementAt(0).Price));
      }

      ViewData.Add("TopSellers.WelcomeText", _context.LocalizedTexts.Where(x => x.PlacementPointCode.Equals("TopSellers.WelcomeText")).Select(x => x.Text).FirstOrDefault());
      return View(topProductModel);
    }

    private async Task<IEnumerable<ShopifySharp.Product>> GetShopifyProducts(
      String ShopifyURL, String ShopifyStoreFrontAccessToken)
    {
      ProductService productService = new ProductService(ShopifyURL, ShopifyStoreFrontAccessToken);
      var result = await productService.ListAsync();
      return result.Items;
    }
  }
}
