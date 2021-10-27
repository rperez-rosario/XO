using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Controllers
{
  public class GiftsAndSavingsController : Controller
  {
    private readonly XOSkinContext _context;

    public GiftsAndSavingsController(XOSkinContext context)
    {
      _context = context;
    }

    public IActionResult Index()
    {
      List<XOSkinWebApp.Areas.Administration.Models.DiscountCouponViewModel> couponModel = null;

      ViewData.Add("GiftsAndSavings.WelcomeText", _context.LocalizedTexts.Where(x => x.PlacementPointCode.Equals(
        "GiftsAndSavings.WelcomeText")).Select(x => x.Text).FirstOrDefault());

      couponModel = new List<Areas.Administration.Models.DiscountCouponViewModel>();

      foreach (DiscountCoupon coupon in _context.DiscountCoupons.Where(x => x.Active).Where(
        x => x.ValidFrom <= DateTime.UtcNow).Where(
        x => x.ValidTo >= DateTime.UtcNow).ToList())
      {
        couponModel.Add(new Areas.Administration.Models.DiscountCouponViewModel()
        {
          Active = coupon.Active,
          Created = coupon.Created,
          CreatedBy = coupon.CreatedBy,
          DiscountAsInGlobalOrderDollars = coupon.DiscountAsInGlobalOrderDollars,
          DiscountAsInGlobalOrderPercentage = coupon.DiscountAsInGlobalOrderPercentage,
          DiscountAsInNProductDollars = coupon.DiscountAsInNproductDollars,
          DiscountAsInNProductPercentage = coupon.DiscountAsInNproductPercentage,
          DiscountGlobalOrderDollars = coupon.DiscountGlobalOrderDollars,
          DiscountGlobalOrderPercentage = coupon.DiscountGlobalOrderPercentage,
          DiscountInNProductDollars = coupon.DiscountInNproductDollars,
          DiscountNProductPercentage = coupon.DiscountNproductPercentage,
          DiscountProductN = coupon.DiscountProductN,
          Id = coupon.Id,
          ImageLarge = null,
          ImagePathLarge = coupon.ImagePathLarge,
          LastUpdated = coupon.LastUpdated,
          LastUpdatedBy = coupon.LastUpdatedBy,
          MinimumPurchase = coupon.MinimumPurchase,
          Name = coupon.Name,
          Product = null,
          ValidFrom = coupon.ValidFrom,
          ValidTo = coupon.ValidTo
        });
      }

      return View(couponModel);
    }
  }
}
