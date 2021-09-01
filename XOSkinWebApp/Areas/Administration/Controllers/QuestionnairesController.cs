using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using XOSkinWebApp.ORM;
using Microsoft.AspNetCore.Authorization;
using XOSkinWebApp.Areas.Administration.Models;

namespace XOSkinWebApp.Areas.Administration.Controllers
{
    [Authorize(Roles = "Administrator")]
    [Area("Administration")]
    public class QuestionnairesController : Controller
    {
        private readonly XOSkinContext _context;

        public QuestionnairesController(XOSkinContext context)
        {
            _context = context;
        }

        // GET: Administration/Questionnaires
        public async Task<IActionResult> Index()
        {
            List<QuestionnaireViewModel> questionnaire = new List<QuestionnaireViewModel>();
            foreach (Questionnaire q in _context.Questionnaires.Include(q => q.CreatedByNavigation))
            {
              questionnaire.Add(new QuestionnaireViewModel() { 
                Id = q.Id,
                QuestionnaireName = q.QuestionnaireName,
                Description = q.Description,
                Active = q.Active,
                CreatedBy = q.CreatedBy,
                DateCreated = q.DateCreated,
                CreatedByNavigation = q.CreatedByNavigation,
                Questions = q.Questions
              });
            }
            return View(questionnaire);
        }

        // GET: Administration/Questionnaires/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questionnaire = await _context.Questionnaires
                .Include(q => q.CreatedByNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (questionnaire == null)
            {
                return NotFound();
            }

            return View(new QuestionnaireViewModel() { 
              Id = questionnaire.Id,
              QuestionnaireName = questionnaire.QuestionnaireName,
              Description = questionnaire.Description,
              Active = questionnaire.Active,
              CreatedBy = questionnaire.CreatedBy,
              DateCreated = questionnaire.DateCreated,
              CreatedByNavigation = questionnaire.CreatedByNavigation,
              Questions = questionnaire.Questions
            });
        }

        // GET: Administration/Questionnaires/Create
        public IActionResult Create()
        {
            ViewData["CreatedBy"] = new SelectList(_context.AspNetUsers, "Id", "Email");
            return View();
        }

        // POST: Administration/Questionnaires/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
          [Bind("Id,QuestionnaireName,Description,Active,CreatedBy,DateCreated")] QuestionnaireViewModel questionnaire)
        {
            Questionnaire newQuestionnaire = null;
            if (ModelState.IsValid)
            {
                questionnaire.CreatedBy = _context.AspNetUsers.Where(
                  x => x.Email.Equals(User.Identity.Name)).Select(x => x.Id).FirstOrDefault();
                questionnaire.DateCreated = DateTime.UtcNow;
                questionnaire.QuestionnaireName = questionnaire.QuestionnaireName.Trim();
                if (questionnaire.Active)
                {
                  foreach (Questionnaire q in _context.Questionnaires)
                  {
                    q.Active = false;
                  }
                  _context.SaveChanges();
                }
                newQuestionnaire = new Questionnaire()
                {
                  Id = questionnaire.Id,
                  QuestionnaireName = questionnaire.QuestionnaireName,
                  Description = questionnaire.Description,
                  Active = questionnaire.Active,
                  CreatedBy = questionnaire.CreatedBy,
                  DateCreated = questionnaire.DateCreated,
                  CreatedByNavigation = questionnaire.CreatedByNavigation,
                  Questions = questionnaire.Questions
                };
                _context.Add(newQuestionnaire);
                await _context.SaveChangesAsync();
                if (questionnaire.Active)
                {
                  foreach (Questionnaire q in _context.Questionnaires)
                  {
                    if (q.Id != newQuestionnaire.Id)
                      q.Active = false;
                  }
                  await _context.SaveChangesAsync();
                }
              return RedirectToAction(nameof(Index));
            }
            ViewData["CreatedBy"] = new SelectList(_context.AspNetUsers, "Id", "Email", questionnaire.CreatedBy);
            return View(questionnaire);
        }

        // GET: Administration/Questionnaires/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questionnaire = await _context.Questionnaires.FindAsync(id);
            if (questionnaire == null)
            {
                return NotFound();
            }
            ViewData["CreatedBy"] = new SelectList(_context.AspNetUsers, "Id", "Email", questionnaire.CreatedBy);
            return View(new QuestionnaireViewModel() { 
              Id = questionnaire.Id,
              QuestionnaireName = questionnaire.QuestionnaireName,
              Description = questionnaire.Description,
              Active = questionnaire.Active,
              CreatedBy = questionnaire.CreatedBy,
              DateCreated = questionnaire.DateCreated,
              CreatedByNavigation = questionnaire.CreatedByNavigation,
              Questions = questionnaire.Questions
            });
        }

        // POST: Administration/Questionnaires/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
          int id, [Bind("Id,QuestionnaireName,Description,Active,CreatedBy,DateCreated")] QuestionnaireViewModel questionnaire)
        {
            if (id != questionnaire.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                  questionnaire.QuestionnaireName = questionnaire.QuestionnaireName.Trim();
                  questionnaire.CreatedBy = _context.Questionnaires.Where(x => x.Id == id).Select(x => x.CreatedBy).FirstOrDefault();
                  questionnaire.DateCreated = _context.Questionnaires.Where(x => x.Id == id).Select(x => x.DateCreated).FirstOrDefault();
                  _context.Update(new Questionnaire() {
                    Id = questionnaire.Id,
                    QuestionnaireName = questionnaire.QuestionnaireName,
                    Description = questionnaire.Description,
                    Active = questionnaire.Active,
                    CreatedBy = questionnaire.CreatedBy,
                    DateCreated = questionnaire.DateCreated,
                    CreatedByNavigation = questionnaire.CreatedByNavigation,
                    Questions = questionnaire.Questions
                  });
                  await _context.SaveChangesAsync();
                  if (questionnaire.Active)
                  {
                    foreach (Questionnaire q in _context.Questionnaires)
                    {
                      if (q.Id != questionnaire.Id)
                        q.Active = false;
                    }
                    await _context.SaveChangesAsync();
                  }
        }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionnaireExists(questionnaire.Id))
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
            ViewData["CreatedBy"] = new SelectList(_context.AspNetUsers, "Id", "Email", questionnaire.CreatedBy);
            return View(questionnaire);
        }

        // GET: Administration/Questionnaires/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questionnaire = await _context.Questionnaires
                .Include(q => q.CreatedByNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (questionnaire == null)
            {
                return NotFound();
            }

            return View(new QuestionnaireViewModel() {
              Id = questionnaire.Id,
              QuestionnaireName = questionnaire.QuestionnaireName,
              Description = questionnaire.Description,
              Active = questionnaire.Active,
              CreatedBy = questionnaire.CreatedBy,
              DateCreated = questionnaire.DateCreated,
              CreatedByNavigation = questionnaire.CreatedByNavigation,
              Questions = questionnaire.Questions
            });
        }

        // POST: Administration/Questionnaires/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var questionnaire = await _context.Questionnaires.FindAsync(id);
            _context.Questionnaires.Remove(questionnaire);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public JsonResult QuestionnaireNameAvailable(String QuestionnaireName, bool ActionCreate,
          String OriginalQuestionnaireName)
        {
          if (OriginalQuestionnaireName == null)
            OriginalQuestionnaireName = String.Empty;
          
          if (ActionCreate || (!ActionCreate && !QuestionnaireName.Equals(OriginalQuestionnaireName)))
          {
            if (_context.Questionnaires.Any(x => x.QuestionnaireName.Equals(OriginalQuestionnaireName.Trim())))
            {
              return Json(false);
            }
          }
          return Json(true);
        }

        private bool QuestionnaireExists(int id)
            {
                return _context.Questionnaires.Any(e => e.Id == id);
            }
        }
}
