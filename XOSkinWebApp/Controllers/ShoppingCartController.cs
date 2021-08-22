using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XOSkinWebApp.ORM;
using XOSkinWebApp.Models;

namespace XOSkinWebApp.Controllers
{
  [Authorize]
  public class ShoppingCartController : Controller
  {
    private readonly XOSkinContext _context;

    public ShoppingCartController(XOSkinContext context)
    {
      _context = context;
    } 
    
    public IActionResult Index()
    {
      List<ShoppingCartLineItemViewModel> lineItem = new List<ShoppingCartLineItemViewModel>();

      foreach (ShoppingCartLineItem li in _context.ShoppingCartLineItems.Where(
        x => x.ShoppingCart == _context.ShoppingCarts.Where(
        x => x.User.Equals(
        _context.AspNetUsers.Where(
        x => x.Email.Equals(
        User.Identity.Name)).Select(
        x => x.Id).FirstOrDefault())).Select(
        x => x.Id).FirstOrDefault()))
      {
        lineItem.Add(new ShoppingCartLineItemViewModel()
        {
          ProductId = li.Id,
          ProductName = _context.Products.Where(x => x.Id == li.Product).Select(x => x.Name).FirstOrDefault(),
          ProductDescription = _context.Products.Where(x => x.Id == li.Product).Select(x => x.Description).FirstOrDefault(),
          Quantity = li.Quantity,
          Total = _context.Prices.Where(x => x.Id.Equals(
            _context.Products.Where(
            x => x.Id == li.Product).Select(
            x => x.CurrentPrice).FirstOrDefault())).Select(
            x => x.Amount).FirstOrDefault()
        });
      }

      ViewData.Add("ShoppingCart.WelcomeText", _context.LocalizedTexts.Where(
        x => x.PlacementPointCode.Equals("ShoppingCart.WelcomeText")).Select(x => x.Text).FirstOrDefault());
      return View(lineItem);
    }
  }
}
