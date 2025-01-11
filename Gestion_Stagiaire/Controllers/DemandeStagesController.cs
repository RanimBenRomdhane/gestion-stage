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
using Microsoft.AspNetCore.Http;

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

            var demandesStage = from d in _context.DemandesStage
                                .Include(d => d.Stagiaire)
                                .Include(d => d.Type_Stage)
                                .Include(d => d.Status)
                                .Include(d => d.Affectation)
                                select d;

            if (!String.IsNullOrEmpty(searchString))
            {
                demandesStage = demandesStage.Where(d =>
                    d.Stagiaire.Nom.Contains(searchString)
                    || d.Stagiaire.Prenom.Contains(searchString)
                    || d.Type_Stage.Stage_Type.Contains(searchString)
                    || d.Status.Reponse.Contains(searchString));
            }

            return View(await demandesStage.ToListAsync());
        }

        // Export to Excel
        public async Task<IActionResult> ExportToExcel()
        {
            var demandeStages = await _context.DemandesStage
                .Include(d => d.Stagiaire)
                .Include(d => d.Type_Stage)
                .Include(d => d.Status)
                .Include(d => d.Affectation)
                .ToListAsync();

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
            worksheet.Cells[1, 10].Value = "Rapport PFE";

            // Add values
            for (int i = 0; i < demandeStages.Count; i++)
            {
                var row = i + 2;
                worksheet.Cells[row, 1].Value = demandeStages[i].Stagiaire.Nom + " " + demandeStages[i].Stagiaire.Prenom;
                worksheet.Cells[row, 2].Value = demandeStages[i].Type_Stage?.Stage_Type;
                worksheet.Cells[row, 3].Value = demandeStages[i].Date_Debut.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 4].Value = demandeStages[i].Date_Fin.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 5].Value = demandeStages[i].Status?.Reponse;
                worksheet.Cells[row, 6].Value = demandeStages[i].Path_Demande_Stage;
                worksheet.Cells[row, 7].Value = demandeStages[i].Date_Demande.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 8].Value = demandeStages[i].Affectation?.Encadrant;
                worksheet.Cells[row, 9].Value = demandeStages[i].Commentaire;
                worksheet.Cells[row, 10].Value = demandeStages[i].Path_Rapport;
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
                .Include(d => d.Type_Stage)
                .Include(d => d.Status)
                .Include(d => d.Affectation)
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

            ViewData["Type_StageId"] = new SelectList(_context.TypesStage, "Id", "Stage_Type");
            ViewData["StatusId"] = new SelectList(_context.Statuses, "Id", "Reponse");
            ViewData["AffectationId"] = new SelectList(_context.Affectations, "Id", "Encadrant");

            return View(demandeStage);
        }

        // POST: DemandeStages/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,StagiaireId,Type_StageId,Date_Debut,Date_Fin,StatusId,Date_Demande,AffectationId,Commentaire")] DemandeStage demandeStage, IFormFile? Path_Demande_Stage, IFormFile? Path_Rapport_PFE)
        {
            if (id != demandeStage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Validate Type_StageId
                    var typeStageExists = await _context.TypesStage.AnyAsync(ts => ts.Id == demandeStage.Type_StageId);
                    if (!typeStageExists)
                    {
                        ModelState.AddModelError("Type_StageId", "The selected type of stage does not exist.");
                        PopulateDropDownLists(demandeStage);
                        return View(demandeStage);
                    }

                    // Fetch Stagiaire
                    var stagiaire = await _context.Stagiaires.FindAsync(demandeStage.StagiaireId);
                    if (stagiaire == null)
                    {
                        ModelState.AddModelError("StagiaireId", "Invalid Stagiaire ID.");
                        PopulateDropDownLists(demandeStage);
                        return View(demandeStage);
                    }

                    // Ensure stagiaire.Cin is not null or empty
                    if (string.IsNullOrWhiteSpace(stagiaire.Cin))
                    {
                        ModelState.AddModelError("", "The CIN for the Stagiaire is not set. Unable to save files.");
                        PopulateDropDownLists(demandeStage);
                        return View(demandeStage);
                    }

                    // Set StatusId to "En cours" by default if not provided
                    if (!demandeStage.StatusId.HasValue)
                    {
                        var status = await _context.Statuses.FirstOrDefaultAsync(s => s.Reponse == "En cours");
                        if (status != null)
                        {
                            demandeStage.StatusId = status.Id; // Set default status to "En cours"
                        }
                    }

                    // Set Date_Demande to current date if not set
                    if (demandeStage.Date_Demande == null)
                    {
                        demandeStage.Date_Demande = DateTime.Now; // Or use DateTime.UtcNow for UTC time
                    }

                    // Handle Demande Stage File upload
                    if (Path_Demande_Stage != null && Path_Demande_Stage.Length > 0)
                    {
                        var fileExtension = Path.GetExtension(Path_Demande_Stage.FileName).ToLower();
                        if (fileExtension != ".pdf")
                        {
                            ModelState.AddModelError("Path_Demande_Stage", "The demande stage file must be a .pdf file.");
                            PopulateDropDownLists(demandeStage);
                            return View(demandeStage);
                        }

                        var uploadsFolder = Path.Combine("wwwroot/uploads/demandes");
                        Directory.CreateDirectory(uploadsFolder); // Ensure folder exists

                        var filePath = Path.Combine(uploadsFolder, $"{stagiaire.Cin}.pdf");
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await Path_Demande_Stage.CopyToAsync(stream);
                        }

                        demandeStage.Path_Demande_Stage = $"{stagiaire.Cin}.pdf"; // Save relative file name
                    }

                    // Handle Rapport PFE File upload
                    if (Path_Rapport_PFE != null && Path_Rapport_PFE.Length > 0)
                    {
                        var fileExtension = Path.GetExtension(Path_Rapport_PFE.FileName).ToLower();
                        if (fileExtension != ".pdf")
                        {
                            ModelState.AddModelError("Path_Rapport_PFE", "The rapport PFE file must be a .pdf file.");
                            PopulateDropDownLists(demandeStage);
                            return View(demandeStage);
                        }

                        var uploadsFolder = Path.Combine("wwwroot/uploads/rapportsPFE");
                        Directory.CreateDirectory(uploadsFolder); // Ensure folder exists

                        var filePath = Path.Combine(uploadsFolder, $"{stagiaire.Cin}.pdf");
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await Path_Rapport_PFE.CopyToAsync(stream);
                        }

                        demandeStage.Path_Rapport = $"{stagiaire.Cin}.pdf"; // Save relative file name
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

            PopulateDropDownLists(demandeStage);
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
            PopulateDropDownLists(demandeStage);
            return View(demandeStage);
        }

        // POST: DemandeStages/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid id, [Bind("Id,StagiaireId,Type_StageId,Date_Debut,Date_Fin,StatusId,Date_Demande,AffectationId,Commentaire")] DemandeStage demandeStage, IFormFile? Path_Demande_Stage, IFormFile? Path_Rapport_PFE)
        {
            if (id != demandeStage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Validate Type_StageId
                    var typeStageExists = await _context.TypesStage.AnyAsync(ts => ts.Id == demandeStage.Type_StageId);
                    if (!typeStageExists)
                    {
                        ModelState.AddModelError("Type_StageId", "The selected type of stage does not exist.");
                        PopulateDropDownLists(demandeStage);
                        return View(demandeStage);
                    }

                    // Fetch Stagiaire
                    var stagiaire = await _context.Stagiaires.FindAsync(demandeStage.StagiaireId);
                    if (stagiaire == null)
                    {
                        ModelState.AddModelError("StagiaireId", "Invalid Stagiaire ID.");
                        PopulateDropDownLists(demandeStage);
                        return View(demandeStage);
                    }

                    // Ensure stagiaire.Cin is not null or empty
                    if (string.IsNullOrWhiteSpace(stagiaire.Cin))
                    {
                        ModelState.AddModelError("", "The CIN for the Stagiaire is not set. Unable to save files.");
                        PopulateDropDownLists(demandeStage);
                        return View(demandeStage);
                    }

                    // Set StatusId to "En cours" by default if not provided
                    if (!demandeStage.StatusId.HasValue)
                    {
                        var status = await _context.Statuses.FirstOrDefaultAsync(s => s.Reponse == "En cours");
                        if (status != null)
                        {
                            demandeStage.StatusId = status.Id; // Set default status to "En cours"
                        }
                    }

                    // Handle Demande Stage File upload
                    if (Path_Demande_Stage != null && Path_Demande_Stage.Length > 0)
                    {
                        var fileExtension = Path.GetExtension(Path_Demande_Stage.FileName).ToLower();
                        if (fileExtension != ".pdf")
                        {
                            ModelState.AddModelError("Path_Demande_Stage", "The demande stage file must be a .pdf file.");
                            PopulateDropDownLists(demandeStage);
                            return View(demandeStage);
                        }

                        var uploadsFolder = Path.Combine("wwwroot/uploads/demandes");
                        Directory.CreateDirectory(uploadsFolder); // Ensure folder exists

                        var filePath = Path.Combine(uploadsFolder, $"{stagiaire.Cin}.pdf");
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await Path_Demande_Stage.CopyToAsync(stream);
                        }

                        demandeStage.Path_Demande_Stage = $"{stagiaire.Cin}.pdf"; // Save relative file name
                    }
                    // Set Date_Demande to current date if not set
                    
                        demandeStage.Date_Demande = DateTime.Now; // Or use DateTime.UtcNow for UTC time
                    
                    // Handle Rapport PFE File upload
                    if (Path_Rapport_PFE != null && Path_Rapport_PFE.Length > 0)
                    {
                        var fileExtension = Path.GetExtension(Path_Rapport_PFE.FileName).ToLower();
                        if (fileExtension != ".pdf")
                        {
                            ModelState.AddModelError("Path_Rapport_PFE", "The rapport PFE file must be a .pdf file.");
                            PopulateDropDownLists(demandeStage);
                            return View(demandeStage);
                        }

                        var uploadsFolder = Path.Combine("wwwroot/uploads/rapportsPFE");
                        Directory.CreateDirectory(uploadsFolder); // Ensure folder exists

                        var filePath = Path.Combine(uploadsFolder, $"{stagiaire.Cin}.pdf");
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await Path_Rapport_PFE.CopyToAsync(stream);
                        }

                        demandeStage.Path_Rapport = $"{stagiaire.Cin}.pdf"; // Save relative file name
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

            PopulateDropDownLists(demandeStage);
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
                .Include(d => d.Type_Stage)
                .Include(d => d.Status)
                .Include(d => d.Affectation)
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

        private void PopulateDropDownLists(DemandeStage demandeStage)
        {
            ViewData["StagiaireId"] = new SelectList(_context.Stagiaires.Select(s => new
            {
                Id = s.Id,
                FullName = s.Nom + " " + s.Prenom
            }), "Id", "FullName", demandeStage.StagiaireId);
            ViewData["Type_StageId"] = new SelectList(_context.TypesStage, "Id", "Stage_Type", demandeStage.Type_StageId);
            ViewData["StatusId"] = new SelectList(_context.Statuses, "Id", "Reponse", demandeStage.StatusId);
            ViewData["AffectationId"] = new SelectList(_context.Affectations, "Id", "Encadrant", demandeStage.AffectationId);
        }
    }
}