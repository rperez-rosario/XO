using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ShopifySharp;
using XOSkinWebApp.ConfigurationHelper;
using XOSkinWebApp.Models;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Controllers
{
  [Authorize]
  public class CheckoutController : Controller
  {
    private readonly XOSkinContext _context;
    private readonly IOptions<Option> _option;

    public CheckoutController(XOSkinContext context)
    {
      _context = context;
    }

    public IActionResult Index()
    {
      CheckoutViewModel checkoutViewModel = new CheckoutViewModel();
      List<ShoppingCartLineItemViewModel> lineItemViewModel = new List<ShoppingCartLineItemViewModel>();
      List<ShoppingCartLineItem> lineItem = _context.ShoppingCartLineItems.Where(
        x => x.ShoppingCart == _context.ShoppingCarts.Where(x => x.User.Equals(_context.AspNetUsers.Where(
        x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault()))
        .Select(x => x.Id).FirstOrDefault()).ToList();

      foreach (ShoppingCartLineItem li in lineItem)
      {
        lineItemViewModel.Add(new ShoppingCartLineItemViewModel()
        {
          Id = li.Id,
          ProductId = li.Product,
          ImageSource = _context.Products.Where(
            x => x.Id.Equals(li.Product)).Select(x => x.ImagePathLarge).FirstOrDefault(),
          ProductName = _context.Products.Where(
            x => x.Id == li.Product)
            .Select(x => x.Name).FirstOrDefault(),
          ProductDescription = _context.Products.Where(
            x => x.Id == li.Product)
            .Select(x => x.Description).FirstOrDefault(),
          Quantity = li.Quantity,
          Total = li.Total
        });
      }

      checkoutViewModel.LineItem = lineItemViewModel;
      checkoutViewModel.CreditCardExpirationDate = DateTime.Now;

      ViewData.Add("Checkout.WelcomeText", _context.LocalizedTexts.Where(
        x => x.PlacementPointCode.Equals("Checkout.WelcomeText"))
        .Select(x => x.Text).FirstOrDefault());

      return View(checkoutViewModel);
    }

    public IActionResult PlaceOrder()
    {
      return RedirectToAction("Index", "Checkout");
    }
  }
}
