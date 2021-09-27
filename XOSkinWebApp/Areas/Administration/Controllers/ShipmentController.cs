using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XOSkinWebApp.Areas.Administration.Models;
using XOSkinWebApp.ConfigurationHelper;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Areas.Administration.Controllers
{
  public class ShipmentController : Controller
  {
    private readonly XOSkinContext _context;
    private readonly IOptions<Option> _option;

    public ShipmentController(XOSkinContext context, IOptions<Option> option)
    {
      _context = context;
      _option = option;
    }

    // GET: ShipmentController
    public ActionResult Index()
    {
      List<ShipmentViewModel> model = new List<ShipmentViewModel>();
      int numberOfItems;

      foreach (OrderShipTo shipment in _context.OrderShipTos.ToList())
      {
        numberOfItems = 0;

        foreach (ProductOrderLineItem item in _context.ProductOrderLineItems.Where(
          x => x.ProductOrder == shipment.Order))
        {
          numberOfItems += item.Quantity;
        }

        model.Add(new ShipmentViewModel()
        {
          ShipmentStatus = shipment.Shipped == true ? "SHIPPED" : "PENDING",
          DatePlaced = _context.ProductOrders.Where(
            x => x.Id == shipment.Order).Select(x => x.DatePlaced).FirstOrDefault(),
          DateShipped = shipment.ShipDate,
          ActualDateShipped = shipment.ActualShipDate,
          Arrives = shipment.Arrives,
          ActualArrives = shipment.ActualArrives,
          Recipient = shipment.RecipientName,
          NumberOfItems = numberOfItems,
          TrackingNumber = shipment.TrackingNumber,
          ShippingLabelURL = shipment.ShippingLabelUrl
        });
      }

      return View(model);
    }

    // GET: ShipmentController/Details/5
    public ActionResult Details(int id)
    {
      return View();
    }

    // GET: ShipmentController/Create
    public ActionResult Create()
    {
      return View();
    }

    // POST: ShipmentController/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(IFormCollection collection)
    {
      try
      {
        return RedirectToAction(nameof(Index));
      }
      catch
      {
        return View();
      }
    }

    // GET: ShipmentController/Edit/5
    public ActionResult Edit(int id)
    {
      return View();
    }

    // POST: ShipmentController/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(int id, IFormCollection collection)
    {
      try
      {
        return RedirectToAction(nameof(Index));
      }
      catch
      {
        return View();
      }
    }

    // GET: ShipmentController/Delete/5
    public ActionResult Delete(int id)
    {
      return View();
    }

    // POST: ShipmentController/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Delete(int id, IFormCollection collection)
    {
      try
      {
        return RedirectToAction(nameof(Index));
      }
      catch
      {
        return View();
      }
    }
  }
}
