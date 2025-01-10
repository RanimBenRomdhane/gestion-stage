using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Gestion_Stagiaire.Data;
using Gestion_Stagiaire.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;
           

            var demandesStage = from d in _context.DemandesStage.Include(d => d.Stagiaire)
                                select d;

            if (!String.IsNullOrEmpty(searchString))
            {
                demandesStage = demandesStage.Where(d =>
                    d.Stagiaire.Nom.Contains(searchString)
                    || d.Stagiaire.Prenom.Contains(searchString)
                    || d.Type_Stage.Contains(searchString)
                    || d.Status.Contains(searchString));
            }



            return View(await demandesStage.ToListAsync());
        }

        // Export to Excel
        public async Task<IActionResult> ExportToExcel()
        {
            var demandeStages = await _context.DemandesStage.Include(d => d.Stagiaire).ToListAsync();

            // Configure EPPlus to use the non-commercial license
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("DemandeStages");

            // Add headers
            worksheet.Cells[1, 1].Value = "Stagiaire";
            worksheet.Cells[1, 2].Value = "Type de Stage";
            worksheet.Cells[1, 3].Value = "Date Debut";
            worksheet.Cells[1, 4].Value = "Date Fin";
            worksheet.Cells[1, 5].Value = "Status";
            worksheet.Cells[1, 6].Value = "Demande Stage";
            worksheet.Cells[1, 7].Value = "Date Demande";
            worksheet.Cells[1, 8].Value = "Affectation";
            worksheet.Cells[1, 9].Value = "Commentaire";

            // Add values
            for (int i = 0; i < demandeStages.Count; i++)
            {
                var row = i + 2;
                worksheet.Cells[row, 1].Value = demandeStages[i].Stagiaire.Nom + " " + demandeStages[i].Stagiaire.Prenom;
                worksheet.Cells[row, 2].Value = demandeStages[i].Type_Stage;
                worksheet.Cells[row, 3].Value = demandeStages[i].Date_Debut.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 4].Value = demandeStages[i].Date_Fin.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 5].Value = demandeStages[i].Status;
                worksheet.Cells[row, 6].Value = demandeStages[i].Path_Demande_Stage;
                worksheet.Cells[row, 7].Value = demandeStages[i].Date_Demande.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 8].Value = demandeStages[i].Affectation;
                worksheet.Cells[row, 9].Value = demandeStages[i].Commentaire;
            }

            var stream = new MemoryStream(package.GetAsByteArray());

            var content = stream.ToArray();
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = "DemandeStages.xlsx";

            return File(content, contentType, fileName);
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
            var demandeStage = new DemandeStage
            {
                Date_Demande = DateTime.Now,
                Date_Debut = DateTime.MinValue,
                Date_Fin = DateTime.MinValue
            };

            ViewData["StagiaireId"] = new SelectList(_context.Stagiaires.Select(s => new
            {
                Id = s.Id,
                FullName = s.Nom + " " + s.Prenom
            }), "Id", "FullName");

            return View(demandeStage);
        }

        // POST: DemandeStages/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StagiaireId,Type_Stage,Date_Debut,Date_Fin,Status,Date_Demande,Affectation,Commentaire")] DemandeStage demandeStage, IFormFile? Path_Demande_Stage)
        {
            if (ModelState.IsValid)
            {
                demandeStage.Id = Guid.NewGuid();

                // Handle Demande Stage File upload
                if (Path_Demande_Stage != null && Path_Demande_Stage.Length > 0)
                {
                    var fileExtension = Path.GetExtension(Path_Demande_Stage.FileName);
                    if (fileExtension.ToLower() != ".pdf")
                    {
                        ModelState.AddModelError("Path_Demande_Stage", "The demande stage file must be a .pdf file.");
                        ViewData["StagiaireId"] = new SelectList(_context.Stagiaires.Select(s => new
                        {
                            Id = s.Id,
                            FullName = s.Nom + " " + s.Prenom
                        }), "Id", "FullName", demandeStage.StagiaireId);
                        return View(demandeStage);
                    }

                    var stagiaire = await _context.Stagiaires.FindAsync(demandeStage.StagiaireId);
                    if (stagiaire == null)
                    {
                        ModelState.AddModelError("StagiaireId", "Invalid Stagiaire ID.");
                        ViewData["StagiaireId"] = new SelectList(_context.Stagiaires.Select(s => new
                        {
                            Id = s.Id,
                            FullName = s.Nom + " " + s.Prenom
                        }), "Id", "FullName", demandeStage.StagiaireId);
                        return View(demandeStage);
                    }

                    var fileName = $"{stagiaire.Cin}.pdf";
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/demandes");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Path_Demande_Stage.CopyToAsync(stream);
                    }

                    demandeStage.Path_Demande_Stage = fileName;
                }

                _context.Add(demandeStage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["StagiaireId"] = new SelectList(_context.Stagiaires.Select(s => new
            {
                Id = s.Id,
                FullName = s.Nom + " " + s.Prenom
            }), "Id", "FullName", demandeStage.StagiaireId);
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
            ViewData["StagiaireId"] = new SelectList(_context.Stagiaires.Select(s => new
            {
                Id = s.Id,
                FullName = s.Nom + " " + s.Prenom
            }), "Id", "FullName", demandeStage.StagiaireId);
            return View(demandeStage);
        }

        // POST: DemandeStages/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,StagiaireId,Type_Stage,Date_Debut,Date_Fin,Status,Date_Demande,Affectation,Commentaire")] DemandeStage demandeStage, IFormFile? Path_Demande_Stage)
        {
            if (id != demandeStage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle Demande Stage File upload
                    if (Path_Demande_Stage != null && Path_Demande_Stage.Length > 0)
                    {
                        var fileExtension = Path.GetExtension(Path_Demande_Stage.FileName);
                        if (fileExtension.ToLower() != ".pdf")
                        {
                            ModelState.AddModelError("Path_Demande_Stage", "The demande stage file must be a .pdf file.");
                            ViewData["StagiaireId"] = new SelectList(_context.Stagiaires.Select(s => new
                            {
                                Id = s.Id,
                                FullName = s.Nom + " " + s.Prenom
                            }), "Id", "FullName", demandeStage.StagiaireId);
                            return View(demandeStage);
                        }

                        var stagiaire = await _context.Stagiaires.FindAsync(demandeStage.StagiaireId);
                        if (stagiaire == null)
                        {
                            ModelState.AddModelError("StagiaireId", "Invalid Stagiaire ID.");
                            ViewData["StagiaireId"] = new SelectList(_context.Stagiaires.Select(s => new
                            {
                                Id = s.Id,
                                FullName = s.Nom + " " + s.Prenom
                            }), "Id", "FullName", demandeStage.StagiaireId);
                            return View(demandeStage);
                        }

                        var fileName = $"{stagiaire.Cin}.pdf";
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/demandes");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await Path_Demande_Stage.CopyToAsync(stream);
                        }

                        demandeStage.Path_Demande_Stage = fileName;
                    }

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
            ViewData["StagiaireId"] = new SelectList(_context.Stagiaires.Select(s => new
            {
                Id = s.Id,
                FullName = s.Nom + " " + s.Prenom
            }), "Id", "FullName", demandeStage.StagiaireId);
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