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
    public class AffectationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AffectationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Affectations
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Affectations.Include(a => a.DemandeStage);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Affectations/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var affectation = await _context.Affectations
                .Include(a => a.DemandeStage)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (affectation == null)
            {
                return NotFound();
            }

            return View(affectation);
        }

        // GET: Affectations/Create
        public IActionResult Create()
        {
            ViewData["DemandeStageId"] = new SelectList(_context.DemandesStage, "Id", "Id");
            return View();
        }

        // POST: Affectations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DemandeStageId,Encadrant,Date_Affectation")] Affectation affectation)
        {
            if (ModelState.IsValid)
            {
                affectation.Id = Guid.NewGuid();
                _context.Add(affectation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DemandeStageId"] = new SelectList(_context.DemandesStage, "Id", "Id", affectation.DemandeStageId);
            return View(affectation);
        }

        // GET: Affectations/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var affectation = await _context.Affectations.FindAsync(id);
            if (affectation == null)
            {
                return NotFound();
            }
            ViewData["DemandeStageId"] = new SelectList(_context.DemandesStage, "Id", "Id", affectation.DemandeStageId);
            return View(affectation);
        }

        // POST: Affectations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,DemandeStageId,Encadrant,Date_Affectation")] Affectation affectation)
        {
            if (id != affectation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(affectation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AffectationExists(affectation.Id))
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
            ViewData["DemandeStageId"] = new SelectList(_context.DemandesStage, "Id", "Id", affectation.DemandeStageId);
            return View(affectation);
        }

        // GET: Affectations/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var affectation = await _context.Affectations
                .Include(a => a.DemandeStage)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (affectation == null)
            {
                return NotFound();
            }

            return View(affectation);
        }

        // POST: Affectations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var affectation = await _context.Affectations.FindAsync(id);
            if (affectation != null)
            {
                _context.Affectations.Remove(affectation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AffectationExists(Guid id)
        {
            return _context.Affectations.Any(e => e.Id == id);
        }
    }
}
