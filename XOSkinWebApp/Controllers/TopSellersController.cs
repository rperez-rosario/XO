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
using Microsoft.AspNetCore.Authorization;

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

    [AllowAnonymous]
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
      ViewData.Add("TopSellers.WelcomeText", _context.LocalizedTexts.Where(x => x.PlacementPointCode.Equals("TopSellers.WelcomeText")).Select(x => x.Text).FirstOrDefault());
      return View(productModel);
    }

    [Authorize]
    public IActionResult AddItemToCart(long? id)
    {
      AspNetUser user = null;
      ShoppingCart shoppingCart = null;
      ShoppingCartLineItem cartLineItem = null;
      ShoppingCartHistory cartHistory = null;
      ORM.Product product = null;

      if (id != null)
      {
        try
        {
          user = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).FirstOrDefault();
          product = _context.Products.Where(x => x.ShopifyProductId.Equals(id)).FirstOrDefault();
          cartLineItem = new ShoppingCartLineItem();
          cartHistory = new ShoppingCartHistory();
          shoppingCart = _context.ShoppingCarts.Where(x => x.User.Equals(user.Id)).FirstOrDefault();
        }
        catch (Exception ex)
        {
          throw new Exception("Error while retrieving user, product or shopping cart information.", ex);
        }
        
        cartLineItem.ShoppingCart = shoppingCart.Id;
        cartLineItem.Product = product.Id;
        cartLineItem.Quantity = 1;

        cartHistory.ShoppingCart = shoppingCart.Id;
        cartHistory.Product = product.Id;
        cartHistory.DateAddedToCart = DateTime.UtcNow;
        cartHistory.PromotedToOrder = false;

        _context.ShoppingCartLineItems.Add(cartLineItem);
        _context.ShoppingCartHistories.Add(cartHistory);

        try
        {
          if (_context.SaveChanges() != 2)
            throw new ApplicationException("Error while adding shopping cart product and cart history entry. " + 
              "Created rows not equal to 2.");
        }
        catch (Exception ex)
        {
          throw new Exception("Error while adding shopping cart product.", ex);
        }
      }
      return RedirectToAction("Index", "ShoppingCart");
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
