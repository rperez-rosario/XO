using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XOSkinWebApp.ORM;

namespace XOSkinWebApp.Controllers
{
    [Authorize]
    public class QuestionnaireController : Controller
    {
        private readonly XOSkinContext _context;

        public QuestionnaireController(XOSkinContext context)
        {
          _context = context;
        }

        // GET: QuestionnaireController
        public async Task<IActionResult> Index()
        {
          ViewData.Add("Questionnaire.WelcomeText", _context.LocalizedTexts.Where(x => x.PlacementPointCode.Equals("Questionnaire.WelcomeText")).Select(x => x.Text).FirstOrDefault());

          var xOSkinContext = _context.Questions.Where(x => x.QuestionnaireNavigation.Active);
          return View(await xOSkinContext.ToListAsync());
        }

        // GET: QuestionnaireController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: QuestionnaireController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: QuestionnaireController/Create
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

        // GET: QuestionnaireController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: QuestionnaireController/Edit/5
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

        // GET: QuestionnaireController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: QuestionnaireController/Delete/5
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
