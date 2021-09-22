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
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;

namespace XOSkinWebApp.Controllers
{
  public class YourLipsAndSmileController : Controller
  {
    private readonly XOSkinContext _context;
    private readonly IOptions<Option> _option;

    public YourLipsAndSmileController(XOSkinContext context, IOptions<Option> option)
    {
      _context = context;
      _option = option;
    }

    public IActionResult Index()
    {
      List<ProductModel> productModel = new List<ProductModel>();
      List<ORM.Product> product = null;

      product = _context.Products.Where(
        x => x.Active == true).Where(
        x => x.ProductType == 1).Where( // 1 = "Lip Cream."
        x => x.KitType == null).ToList<ORM.Product>();

      foreach (ORM.Product p in product)
      {
        productModel.Add(
          new ProductModel(p.Id.ToString(), p.ImagePathLarge, p.Name, p.Description,
          _context.Prices.Where(x => x.Id == p.CurrentPrice).Select(x => x.Amount).FirstOrDefault()));
      }

      ViewData.Add("YourLipsAndSmile.WelcomeText", _context.LocalizedTexts.Where(
        x => x.PlacementPointCode.Equals("YourLipsAndSmile.WelcomeText")).Select(x => x.Text).FirstOrDefault());
      
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
          product = _context.Products.Where(x => x.Id == id).FirstOrDefault();
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

      productListFilter.CollectionId = long.Parse(_option.Value.ShopifyYourLipsAndSmileCollectionId);

      var result = await productService.ListAsync(productListFilter.AsListFilter());
      return result.Items;
    }
  }
}
