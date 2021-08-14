using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using XOSkinWebApp.ORM;
using Microsoft.AspNetCore.Authorization;

namespace XOSkinWebApp.Areas.Administration.Controllers
{
    [Authorize(Roles = "Administrator")]
    [Area("Administration")]
    public class QuestionsController : Controller
    {
        private readonly XOSkinContext _context;

        public QuestionsController(XOSkinContext context)
        {
            _context = context;
        }

        // GET: Administration/Questions
        public async Task<IActionResult> Index()
        {
            var xOSkinContext = _context.Questions
              .Include(q => q.QuestionnaireNavigation);
            return View(await xOSkinContext.ToListAsync());
        }

        // GET: Administration/Questions/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Include(q => q.QuestionnaireNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // GET: Administration/Questions/Create
        public IActionResult Create()
        {
            ViewData["Questionnaire"] = new SelectList(_context.Questionnaires, "Id", "QuestionnaireName");
            return View();
        }

        // POST: Administration/Questions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,QuestionText,Questionnaire,DisplayOrder")] Question question)
        {
            if (ModelState.IsValid)
            {
                _context.Add(question);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Questionnaire"] = new SelectList(_context.Questionnaires, "Id", "QuestionnaireName", question.Questionnaire);
            return View(question);
        }

        // GET: Administration/Questions/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions.FindAsync(id);
            if (question == null)
            {
                return NotFound();
            }
            ViewData["Questionnaire"] = new SelectList(_context.Questionnaires, "Id", "QuestionnaireName", question.Questionnaire);
            return View(question);
        }

        // POST: Administration/Questions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,QuestionText,Questionnaire,DisplayOrder")] Question question)
        {
            if (id != question.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    question.Questionnaire = _context.Questions.Where(x => x.Id == id).Select(x => x.Questionnaire).FirstOrDefault();

                    _context.Update(question);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionExists(question.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Questionnaire"] = new SelectList(_context.Questionnaires, "Id", "QuestionnaireName", question.Questionnaire);
            return View(question);
        }

        // GET: Administration/Questions/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Include(q => q.QuestionnaireNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // POST: Administration/Questions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var question = await _context.Questions.FindAsync(id);
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuestionExists(long id)
        {
            return _context.Questions.Any(e => e.Id == id);
        }
    }
}
