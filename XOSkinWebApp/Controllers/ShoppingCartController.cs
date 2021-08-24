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

      ViewData.Add("ShoppingCart.WelcomeText", _context.LocalizedTexts.Where(
        x => x.PlacementPointCode.Equals("ShoppingCart.WelcomeText"))
        .Select(x => x.Text).FirstOrDefault());
      return View(lineItemViewModel);
    }

    public IActionResult UpdateQuantity(long LineItemId, long ProductId, int Quantity)
    {
      int i = 0;
      int originalQuantity = 0;
      int difference = 0;

      try
      {
        originalQuantity = _context.ShoppingCartLineItems.Where(
          x => x.Id == LineItemId).Select(x => x.Quantity).FirstOrDefault();

        if (originalQuantity != Quantity)
        {
          if (Quantity == 0)
          {
            _context.ShoppingCartLineItems.Remove(_context.ShoppingCartLineItems.Where(
              x => x.Id == LineItemId).FirstOrDefault());
            _context.SaveChanges();
          }
          else
          {
            _context.ShoppingCartLineItems.Where(
            x => x.Id == LineItemId).FirstOrDefault().Quantity = Quantity;
            _context.SaveChanges();
          }

          if (Quantity > originalQuantity)
          {
            for (; i < Quantity - originalQuantity; i++)
            {
              _context.Add(new ShoppingCartHistory()
              {
                ShoppingCart = _context.ShoppingCarts.Where(
                  x => x.User == _context.AspNetUsers.Where(
                  x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault())
                  .Select(x => x.Id).FirstOrDefault(),
                Product = ProductId,
                PromotedToOrder = false,
                DateAddedToCart = DateTime.UtcNow
              });
            }
            _context.SaveChanges();
          }
          else if (Quantity < originalQuantity)
          {
            difference = originalQuantity - Quantity;

            if (difference == 1)
            {
              _context.ShoppingCartHistories.Where(
                x => x.ShoppingCart == _context.ShoppingCarts.Where(
                x => x.User.Equals(_context.AspNetUsers.Where(
                x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault()))
                .Select(x => x.Id).FirstOrDefault())
                .First().DateRemovedFromCart = DateTime.UtcNow;

              _context.ShoppingCartHistories.Where(
                x => x.ShoppingCart == _context.ShoppingCarts.Where(
                x => x.User.Equals(_context.AspNetUsers.Where(
                x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault()))
                .Select(x => x.Id).FirstOrDefault())
                .First().PromotedToOrder = false;
            }
            else
            {
              for (; i < difference; i++)
              {
                _context.ShoppingCartHistories.Where(
                  x => x.ShoppingCart == _context.ShoppingCarts.Where(
                  x => x.User.Equals(_context.AspNetUsers.Where(
                  x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault()))
                  .Select(x => x.Id).FirstOrDefault()).OrderByDescending(x => x.DateAddedToCart)
                  .Skip(i).First().DateRemovedFromCart = DateTime.UtcNow;

                _context.ShoppingCartHistories.Where(
                  x => x.ShoppingCart == _context.ShoppingCarts.Where(
                  x => x.User.Equals(_context.AspNetUsers.Where(
                  x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault()))
                  .Select(x => x.Id).FirstOrDefault()).OrderByDescending(x => x.DateAddedToCart)
                  .Skip(i).First().PromotedToOrder = false;
              }
            }
            _context.SaveChanges();
          }
        }
      }
      catch (Exception ex)
      {
        throw (new Exception(
          "An error resulted while trying to update the quantity on the selected shopping cart line item, or " +
          "while trying to create the corresponding shopping cart history entry(ies).", ex));
      }
      return RedirectToAction("Index", "ShoppingCart");
    }

    public IActionResult DeleteLineItem(long LineItemId)
    {
      int i = 0;
      int quantity = 0;

      try
      {
        quantity = _context.ShoppingCartLineItems.Where(
        x => x.Id == LineItemId).Select(x => x.Quantity).FirstOrDefault();
        
        if (quantity == 1)
        {
          _context.ShoppingCartHistories.Where(
            x => x.ShoppingCart == _context.ShoppingCarts.Where(
            x => x.User.Equals(_context.AspNetUsers.Where(
            x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault()))
            .Select(x => x.Id).FirstOrDefault())
            .First().DateRemovedFromCart = DateTime.UtcNow;

          _context.ShoppingCartHistories.Where(
            x => x.ShoppingCart == _context.ShoppingCarts.Where(
            x => x.User.Equals(_context.AspNetUsers.Where(
            x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault()))
            .Select(x => x.Id).FirstOrDefault())
            .First().PromotedToOrder = false;
        }
        else
        {
          for (; i < quantity; i++)
          {
            _context.ShoppingCartHistories.Where(
              x => x.ShoppingCart == _context.ShoppingCarts.Where(
              x => x.User.Equals(_context.AspNetUsers.Where(
              x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault()))
              .Select(x => x.Id).FirstOrDefault()).OrderByDescending(x => x.DateAddedToCart)
              .Skip(i).First().DateRemovedFromCart = DateTime.UtcNow;

            _context.ShoppingCartHistories.Where(
              x => x.ShoppingCart == _context.ShoppingCarts.Where(
              x => x.User.Equals(_context.AspNetUsers.Where(
              x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault()))
              .Select(x => x.Id).FirstOrDefault()).OrderByDescending(x => x.DateAddedToCart)
              .Skip(i).First().PromotedToOrder = false;
          }
        }
        _context.SaveChanges();

        _context.ShoppingCartLineItems.Remove(_context.ShoppingCartLineItems.Where(
                x => x.Id == LineItemId).FirstOrDefault());
        _context.SaveChanges();
      }
      catch (Exception ex)
      {
        throw new Exception("An error resulted while removing the selected shopping cart line, or " + 
          "while trying to update the corresponding cart history entry(ies).", ex);
      }

      return RedirectToAction("Index", "ShoppingCart");
    }
  }
}
