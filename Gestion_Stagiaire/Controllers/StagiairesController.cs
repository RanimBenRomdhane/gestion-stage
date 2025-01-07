using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Gestion_Stagiaire.Data;
using Gestion_Stagiaires.Models;

namespace Gestion_Stagiaire.Controllers
{
    public class StagiairesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StagiairesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Stagiaires
        public async Task<IActionResult> Index()
        {
            return View(await _context.Stagiaires.ToListAsync());
        }

        // GET: Stagiaires/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stagiaire = await _context.Stagiaires
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stagiaire == null)
            {
                return NotFound();
            }

            return View(stagiaire);
        }

        // GET: Stagiaires/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Stagiaires/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nom,Prenom,Cin,Telephone,Email,Ecole,Path_Photo,Path_CV")] Stagiaire stagiaire)
        {
            if (ModelState.IsValid)
            {
                stagiaire.Id = Guid.NewGuid();
                _context.Add(stagiaire);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(stagiaire);
        }

        // GET: Stagiaires/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stagiaire = await _context.Stagiaires.FindAsync(id);
            if (stagiaire == null)
            {
                return NotFound();
            }
            return View(stagiaire);
        }

        // POST: Stagiaires/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Nom,Prenom,Cin,Telephone,Email,Ecole,Path_Photo,Path_CV")] Stagiaire stagiaire)
        {
            if (id != stagiaire.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stagiaire);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StagiaireExists(stagiaire.Id))
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
            return View(stagiaire);
        }

        // GET: Stagiaires/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stagiaire = await _context.Stagiaires
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stagiaire == null)
            {
                return NotFound();
            }

            return View(stagiaire);
        }

        // POST: Stagiaires/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var stagiaire = await _context.Stagiaires.FindAsync(id);
            if (stagiaire != null)
            {
                _context.Stagiaires.Remove(stagiaire);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StagiaireExists(Guid id)
        {
            return _context.Stagiaires.Any(e => e.Id == id);
        }
    }
}
