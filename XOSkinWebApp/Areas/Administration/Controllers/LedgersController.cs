using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using XOSkinWebApp.Areas.Administration.Models;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Areas.Administration.Controllers
{
  [Area("Administration")]
  public class LedgersController : Controller
  {
    private readonly XOSkinContext _context;

    public LedgersController(XOSkinContext context)
    {
      _context = context;
    }

    // GET: Administration/Ledgers
    public async Task<IActionResult> Index()
    {
      List<LedgerViewModel> ledger = new List<LedgerViewModel>();
      decimal ch1productSales = 0.0M;
      decimal ch1productShipping = 0.0M;
      decimal ch1salesTaxes = 0.0M;
      decimal ch1productRefunds = 0.0M;
      String ch2series = String.Empty;
      String ch2data = String.Empty;
      List<DateTime> series = new List<DateTime>();
      List<decimal> data = new List<decimal>();
      DateTime currentSeries = DateTime.MinValue;
      decimal currentData = 0.0M;
      StringBuilder stringBuilder = new StringBuilder();

      foreach (UserLedgerTransaction t in await _context.UserLedgerTransactions.ToListAsync())
      {
        ledger.Add(new LedgerViewModel()
        {
          Amount = t.Amount,
          BalanceAfterTransaction = t.BalanceAfterTransaction,
          BalanceBeforeTransaction = t.BalanceBeforeTransaction,
          CreatedBy = _context.AspNetUsers.FindAsync(t.CreatedBy).Result.Email,
          CreatedOn = t.Created,
          Description = t.Description,
          OrderID = t.ProductOrder == null ? "NONE" : "#XO" + (t.ProductOrder + 10000).ToString(),
          TransactionConcept = _context.TransactionConcepts.FindAsync(t.Concept).Result.Name.ToUpper(),
          TransactionId = t.Id,
          TransactionType = _context.UserLedgerTransactionTypes.FindAsync(t.TransactionType).Result.Name.ToUpper(),
          User = _context.AspNetUsers.FindAsync(t.User).Result.Email
        });

        switch (ledger.Last().TransactionConcept)
        {
          case "PRODUCT":
            ch1productSales += ledger.Last().Amount;
            break;
          case "REFUND":
            ch1productRefunds += ledger.Last().Amount;
            break;
          case "SHIPPING":
            ch1productShipping += ledger.Last().Amount;
            break;
          case "TAXATION":
            ch1salesTaxes += ledger.Last().Amount;
            break;
          default:
            break;
        }

        if (ledger.Last().TransactionConcept.Equals("PRODUCT") && 
          ledger.Last().CreatedOn.Date > currentSeries.Date)
        {
          series.Add(currentSeries);
          data.Add(currentData);
          currentSeries = ledger.Last().CreatedOn;
          currentData = ledger.Last().Amount;
        }
        else if (ledger.Last().TransactionConcept.Equals("PRODUCT"))
        {
          currentData += ledger.Last().Amount;
        }
      }

      series.Add(currentSeries);
      data.Add(currentData);

      if (series.Count > 0)
      {
        series.RemoveAt(0);

        foreach (DateTime t in series)
        {
          stringBuilder.Append("'" + t.Date.ToShortDateString() + "', ");
        }

        ch2series = stringBuilder.ToString();
        ch2series = ch2series.Remove(ch2series.Length - 2, 2);
      }

      stringBuilder = new StringBuilder();

      if (data.Count > 0)
      {
        data.RemoveAt(0);

        foreach (decimal a in data)
        {
          stringBuilder.Append(a.ToString() + ", ");
        }

        ch2data = stringBuilder.ToString();
        ch2data = ch2data.Remove(ch2data.Length - 3, 3);
      }
      
      ViewData["ProductSales"] = ch1productSales;
      ViewData["ProductShipping"] = ch1productShipping;
      ViewData["SalesTaxes"] = ch1salesTaxes;
      ViewData["ProductRefunds"] = ch1productRefunds;
      ViewData["Series"] = ch2series;
      ViewData["Data"] = ch2data;

      return View(ledger);
    }
  }
}
