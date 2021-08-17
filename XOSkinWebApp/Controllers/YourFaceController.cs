using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XOSkinWebApp.ORM;
using XOSkinWebApp.Models;
using XOSkinWebApp.ConfigurationHelper;
using ShopifySharp;
using ShopifySharp.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace XOSkinWebApp.Controllers
{
  public class YourFaceController : Controller
  {
    private readonly XOSkinContext _context;
    private readonly IOptions<Option> _option;

    public YourFaceController(XOSkinContext context, IOptions<Option> option)
    {
      _context = context;
      _option = option;
    }

    public IActionResult Index()
    {
      List<ProductModel> productModel = new List<ProductModel>();

      IEnumerable<ShopifySharp.Product> product = GetShopifyProducts(_option.Value.ShopifyUrl,
        _option.Value.ShopifyStoreFrontAccessToken).Result;

      foreach (ShopifySharp.Product sp in product)
      {
        productModel.Add(
          new ProductModel(sp.Id.ToString(), sp.Images.ElementAt(0).Src, sp.Title, sp.BodyHtml,
          sp.Variants.ElementAt(0).Price));
      }

      ViewData.Add("YourFace.WelcomeText", _context.LocalizedTexts.Where(x => x.PlacementPointCode.Equals("YourFace.WelcomeText")).Select(x => x.Text).FirstOrDefault());

      return View(productModel);
    }

    private async Task<IEnumerable<ShopifySharp.Product>> GetShopifyProducts(
      String ShopifyURL, String ShopifyStoreFrontAccessToken)
    {
      ProductService productService = new ProductService(ShopifyURL, ShopifyStoreFrontAccessToken);
      ProductListFilter productListFilter = new ProductListFilter();
      
      productListFilter.CollectionId = long.Parse(_option.Value.ShopifyYourFaceCollectionId);

      var result = await productService.ListAsync(productListFilter.AsListFilter());
      return result.Items;
    }
  }
}
