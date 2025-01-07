using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Gestion_Stagiaire.Data;
using Gestion_Stagiaire.Models;

namespace Gestion_Stagiaire.Controllers
{
    public class DemandeStagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DemandeStagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DemandeStages
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.DemandesStage.Include(d => d.Stagiaire);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: DemandeStages/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var demandeStage = await _context.DemandesStage
                .Include(d => d.Stagiaire)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (demandeStage == null)
            {
                return NotFound();
            }

            return View(demandeStage);
        }

        // GET: DemandeStages/Create
        public IActionResult Create()
        {
            ViewData["StagiaireId"] = new SelectList(_context.Stagiaires, "Id", "Id");
            return View();
        }

        // POST: DemandeStages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StagiaireId,Type_Stage,Date_Debut,Date_Fin,Status,Path_Demande_Stage,Date_Demande,Affectation,Commentaire")] DemandeStage demandeStage)
        {
            if (ModelState.IsValid)
            {
                demandeStage.Id = Guid.NewGuid();
                _context.Add(demandeStage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["StagiaireId"] = new SelectList(_context.Stagiaires, "Id", "Id", demandeStage.StagiaireId);
            return View(demandeStage);
        }

        // GET: DemandeStages/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var demandeStage = await _context.DemandesStage.FindAsync(id);
            if (demandeStage == null)
            {
                return NotFound();
            }
            ViewData["StagiaireId"] = new SelectList(_context.Stagiaires, "Id", "Id", demandeStage.StagiaireId);
            return View(demandeStage);
        }

        // POST: DemandeStages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,StagiaireId,Type_Stage,Date_Debut,Date_Fin,Status,Path_Demande_Stage,Date_Demande,Affectation,Commentaire")] DemandeStage demandeStage)
        {
            if (id != demandeStage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(demandeStage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DemandeStageExists(demandeStage.Id))
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
            ViewData["StagiaireId"] = new SelectList(_context.Stagiaires, "Id", "Id", demandeStage.StagiaireId);
            return View(demandeStage);
        }

        // GET: DemandeStages/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var demandeStage = await _context.DemandesStage
                .Include(d => d.Stagiaire)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (demandeStage == null)
            {
                return NotFound();
            }

            return View(demandeStage);
        }

        // POST: DemandeStages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var demandeStage = await _context.DemandesStage.FindAsync(id);
            if (demandeStage != null)
            {
                _context.DemandesStage.Remove(demandeStage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DemandeStageExists(Guid id)
        {
            return _context.DemandesStage.Any(e => e.Id == id);
        }
    }
}
