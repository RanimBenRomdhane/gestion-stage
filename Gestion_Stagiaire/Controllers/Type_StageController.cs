using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Gestion_Stagiaire.Data;
using Gestion_Stagiaire.Models;
using Microsoft.AspNetCore.Authorization;

namespace Gestion_Stagiaire.Controllers
{
    [Authorize]
    public class Type_StageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public Type_StageController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Type_Stage
        public async Task<IActionResult> Index()
        {
            return View(await _context.TypesStage.ToListAsync());
        }

        // GET: Type_Stage/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var type_Stage = await _context.TypesStage
                .FirstOrDefaultAsync(m => m.Id == id);
            if (type_Stage == null)
            {
                return NotFound();
            }

            return View(type_Stage);
        }

        // GET: Type_Stage/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Type_Stage/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Stage_Type")] Type_Stage type_Stage)
        {
            if (ModelState.IsValid)
            {
                type_Stage.Id = Guid.NewGuid();
                _context.Add(type_Stage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(type_Stage);
        }

        // GET: Type_Stage/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var type_Stage = await _context.TypesStage.FindAsync(id);
            if (type_Stage == null)
            {
                return NotFound();
            }
            return View(type_Stage);
        }

        // POST: Type_Stage/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Stage_Type")] Type_Stage type_Stage)
        {
            if (id != type_Stage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(type_Stage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Type_StageExists(type_Stage.Id))
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
            return View(type_Stage);
        }

        // GET: Type_Stage/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var type_Stage = await _context.TypesStage
                .FirstOrDefaultAsync(m => m.Id == id);
            if (type_Stage == null)
            {
                return NotFound();
            }

            return View(type_Stage);
        }

        // POST: Type_Stage/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var type_Stage = await _context.TypesStage.FindAsync(id);
            if (type_Stage != null)
            {
                _context.TypesStage.Remove(type_Stage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool Type_StageExists(Guid id)
        {
            return _context.TypesStage.Any(e => e.Id == id);
        }
    }
}
