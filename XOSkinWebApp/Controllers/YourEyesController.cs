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
using Microsoft.AspNetCore.Authorization;

namespace XOSkinWebApp.Controllers
{
  public class YourEyesController : Controller
  {
    private readonly XOSkinContext _context;
    private readonly IOptions<Option> _option;

    public YourEyesController(XOSkinContext context, IOptions<Option> option)
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

      ViewData.Add("YourEyes.WelcomeText", _context.LocalizedTexts.Where(x => x.PlacementPointCode.Equals("YourEyes.WelcomeText")).Select(x => x.Text).FirstOrDefault());

      return View(productModel);
    }

    [Authorize]
    public IActionResult AddItemToCart(long? id)
    {
      AspNetUser user = null;
      ShoppingCart shoppingCart = null;
      ShoppingCartLineItem cartLineItem = null;
      ShoppingCartHistory cartHistory = null;
      List<ShoppingCartLineItem> cartLineItemList = null;
      ORM.Product product = null;

      if (id != null)
      {
        try
        {
          user = _context.AspNetUsers.Where(x => x.Email.Equals(User.Identity.Name)).FirstOrDefault();
          product = _context.Products.Where(x => x.ShopifyProductId.Equals(id)).FirstOrDefault();
          cartHistory = new ShoppingCartHistory();
          shoppingCart = _context.ShoppingCarts.Where(x => x.User.Equals(user.Id)).FirstOrDefault();
          cartLineItemList = _context.ShoppingCartLineItems.Where(x => x.ShoppingCart == shoppingCart.Id).ToList();

          if (cartLineItemList.Any(x => x.Product == product.Id))
          {
            _context.ShoppingCartLineItems.Where(x => x.Product == product.Id).FirstOrDefault().Quantity =
              _context.ShoppingCartLineItems.Where(x => x.Product == product.Id).FirstOrDefault().Quantity + 1;
            _context.SaveChanges();
            _context.ShoppingCartLineItems.Where(x => x.Product == product.Id).FirstOrDefault().Total =
              _context.ShoppingCartLineItems.Where(x => x.Product == product.Id).FirstOrDefault().Quantity *
              _context.Prices.Where(x => x.Id == product.CurrentPrice).Select(x => x.Amount).FirstOrDefault();
            _context.SaveChanges();
          }
          else
          {
            cartLineItem = new ShoppingCartLineItem();
            cartLineItem.ShoppingCart = shoppingCart.Id;
            cartLineItem.Product = product.Id;
            cartLineItem.Quantity = 1;
            cartLineItem.Total = _context.Prices.Where(x => x.Id == product.CurrentPrice).Select(x => x.Amount).FirstOrDefault();
            _context.ShoppingCartLineItems.Add(cartLineItem);
          }
        }
        catch (Exception ex)
        {
          throw new Exception("Error while retrieving user, product or shopping cart information.", ex);
        }

        cartHistory.ShoppingCart = shoppingCart.Id;
        cartHistory.Product = product.Id;
        cartHistory.DateAddedToCart = DateTime.UtcNow;
        cartHistory.PromotedToOrder = false;

        _context.ShoppingCartHistories.Add(cartHistory);

        try
        {
          if (_context.SaveChanges() <= 0)
            throw new ApplicationException("Error while adding shopping cart product and cart history entry.");
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
      ProductListFilter productListFilter = new ProductListFilter();

      productListFilter.CollectionId = long.Parse(_option.Value.ShopifyYourEyesCollectionId);

      var result = await productService.ListAsync(productListFilter.AsListFilter());
      return result.Items;
    }
  }
}
