﻿
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using Gestion_Stagiaire.Data;
using Gestion_Stagiaire.Models;
using Gestion_Stagiaires.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using X.PagedList;
using DocumentFormat.OpenXml.Office2010.Excel;


namespace Gestion_Stagiaire.Controllers
{
    [Authorize]
    public class StagiairesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StagiairesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int? page)
        {
            // Store the search term in ViewData to retain it in the form and pagination links
            ViewData["CurrentFilter"] = searchString;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Get the user's role
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Base query to retrieve all stagiaires
            var stagiaires = _context.Stagiaires.AsQueryable();

            if (userRole == "Stagiaire")
            {
                stagiaires = stagiaires.Where(s => s.Id.ToString() == userId);
            }

            // Apply the search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                stagiaires = stagiaires.Where(s => s.Nom.Contains(searchString)
                                                || s.Prenom.Contains(searchString)
                                                || s.Cin.ToString().Contains(searchString)
                                                || s.Telephone.ToString().Contains(searchString)
                                                || s.Ecole.Contains(searchString)); // Ensure Ecole is compared correctly
            }

            // Pagination parameters
            int pageNumber = page ?? 1; // Default to page 1 if null
            int pageSize = 10;          // Number of items per page

            // Apply pagination
            var paginatedStagiaires = await stagiaires.ToPagedListAsync(pageNumber, pageSize);

            return View(paginatedStagiaires);
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
        public async Task<IActionResult> Create()
        {

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            ViewBag.UserEmail = userEmail;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nom,Prenom,Cin,Telephone,Email, Ecole")] Stagiaire stagiaire, IFormFile Path_Photo, IFormFile Path_CV)
        {
            if (ModelState.IsValid)
            {
                // Ensure the user is authenticated
                if (User.Identity?.IsAuthenticated == true)
                {
                    foreach (var claim in User.Claims)
                    {
                        Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
                    }
                    // Retrieve user information
                    string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    //var userEmail = User.Identity?.Name;
                    var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        Console.WriteLine($"Authenticated User ID: {userId}");

                        // Only check for existing stagiaire email if the user is a "stagiaire" (not "rh")
                        if (User.IsInRole("Stagiaire"))
                        {
                            // Check if a Stagiaire with the same email already exists
                            var existingStagiaire = await _context.Stagiaires
                                .FirstOrDefaultAsync(s => s.Email == userEmail);
                            stagiaire.Email = userEmail;


                            if (existingStagiaire != null)
                            {
                                // If an account already exists for "stagiaire", set an error message in ViewData for the popup
                                ModelState.AddModelError(string.Empty, "You have already created an account.");
                                return View(stagiaire);
                            }
                            else
                            {
                                // Assign the user's ID (stagiaire)
                                stagiaire.Id = Guid.Parse(userId); // User's ID (stagiaire)
                            }
                        }
                        else if (User.IsInRole("RH"))
                        {
                            // If the user is "rh", generate a new GUID for the Stagiaire ID
                            stagiaire.Id = Guid.NewGuid(); // New random ID for "rh"
                        }

                        // Assign the user email to stagiaire (optional, as it's already included)
                    }
                    else
                    {
                        Console.WriteLine("User ID claim is not available.");
                        ModelState.AddModelError(string.Empty, "Failed to retrieve user ID. Please try again.");
                        return View(stagiaire);
                    }
                }
                else
                {
                    Console.WriteLine("User is not authenticated.");
                    ModelState.AddModelError(string.Empty, "User is not authenticated. Please log in.");
                    return View(stagiaire);
                }

                // Handle Photo upload
                if (!await HandleFileUpload(Path_Photo, stagiaire, "photos", "Path_Photo", ".png"))
                {
                    return View(stagiaire);
                }

                // Handle CV upload
                if (!await HandleFileUpload(Path_CV, stagiaire, "cvs", "Path_CV", ".pdf"))
                {
                    return View(stagiaire);
                }

                // Save to the database
                _context.Add(stagiaire);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Return the view with the model if validation fails
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Nom,Prenom,Cin,Telephone,Email,Ecole,Path_Photo,Path_CV")] Stagiaire stagiaire, IFormFile? Path_Photo, IFormFile? Path_CV)
        {
            if (id != stagiaire.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve the existing record from the database
                    var existingStagiaire = await _context.Stagiaires.FindAsync(id);
                    if (existingStagiaire == null)
                    {
                        return NotFound();
                    }

                    // Handle Photo upload if a new one is provided
                    if (Path_Photo != null && Path_Photo.Length > 0)
                    {
                        var fileExtension = Path.GetExtension(Path_Photo.FileName).ToLower();
                        if (fileExtension != ".png")
                        {
                            ModelState.AddModelError("Path_Photo", "La photo doit être un fichier .png.");
                            return View(stagiaire);
                        }

                        var uploadsFolder = Path.Combine("wwwroot/uploads/photos");
                        Directory.CreateDirectory(uploadsFolder);

                        var filePath = Path.Combine(uploadsFolder, $"{stagiaire.Id}.png");  // Append "_photo" to avoid overwriting
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await Path_Photo.CopyToAsync(stream);
                        }

                        stagiaire.Path_Photo = $"{stagiaire.Id}.png"; // Update Path_Photo
                    }
                    else
                    {
                        // Preserve the existing Path_Photo if no new photo is uploaded
                        stagiaire.Path_Photo = existingStagiaire.Path_Photo;
                    }

                    // Handle CV upload if a new one is provided
                    if (Path_CV != null && Path_CV.Length > 0)
                    {
                        var fileExtension = Path.GetExtension(Path_CV.FileName).ToLower();
                        if (fileExtension != ".pdf")
                        {
                            ModelState.AddModelError("Path_CV", "Le CV doit être un fichier .pdf.");
                            return View(stagiaire);
                        }

                        var uploadsFolder = Path.Combine("wwwroot/uploads/cvs");
                        Directory.CreateDirectory(uploadsFolder);

                        var filePath = Path.Combine(uploadsFolder, $"{stagiaire.Id}.pdf");  // Append "_cv" to avoid overwriting
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await Path_CV.CopyToAsync(stream);
                        }

                        stagiaire.Path_CV = $"{stagiaire.Id}.pdf"; // Update Path_CV
                    }
                    else
                    {
                        // Preserve the existing Path_CV if no new CV is uploaded
                        stagiaire.Path_CV = existingStagiaire.Path_CV;
                    }

                    // Ensure no conflicting instances of the same entity are tracked
                    _context.Entry(existingStagiaire).State = EntityState.Detached;

                    // Update the Stagiaire with the new values


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

        // Export to Excel
        public async Task<IActionResult> ExportToExcel()
        {
            var stagiaires = await _context.Stagiaires.ToListAsync();

            // Configure EPPlus to use the non-commercial license
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Stagiaires");

            // Add headers
            worksheet.Cells[1, 1].Value = "Nom";
            worksheet.Cells[1, 2].Value = "Prenom";
            worksheet.Cells[1, 3].Value = "Cin";
            worksheet.Cells[1, 4].Value = "Email";
            worksheet.Cells[1, 5].Value = "Telephone";
            worksheet.Cells[1, 6].Value = "Photo";
            worksheet.Cells[1, 7].Value = "Cv";

            // Add values
            for (int i = 0; i < stagiaires.Count; i++)
            {
                var row = i + 2;
                worksheet.Cells[row, 1].Value = stagiaires[i].Nom;
                worksheet.Cells[row, 2].Value = stagiaires[i].Prenom;
                worksheet.Cells[row, 3].Value = stagiaires[i].Cin;
                worksheet.Cells[row, 4].Value = stagiaires[i].Email;
                worksheet.Cells[row, 5].Value = stagiaires[i].Telephone;
                worksheet.Cells[row, 6].Value = stagiaires[i].Path_Photo;
                worksheet.Cells[row, 7].Value = stagiaires[i].Path_CV;
            }

            var stream = new MemoryStream(package.GetAsByteArray());

            var content = stream.ToArray();
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = "Stagiaires.xlsx";

            return File(content, contentType, fileName);
        }

        private bool StagiaireExists(Guid id)
        {
            return _context.Stagiaires.Any(e => e.Id == id);
        }

        private async Task<bool> HandleFileUpload(IFormFile file, Stagiaire stagiaire, string folder, string propertyName, string requiredExtension)
        {
            if (file != null && file.Length > 0)
            {
                var extension = Path.GetExtension(file.FileName);
                if (extension.ToLower() != requiredExtension)
                {
                    ModelState.AddModelError(propertyName, $"The file must be a {requiredExtension} file.");
                    return false;
                }

                var fileName = $"{stagiaire.Cin}{requiredExtension}";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/uploads/{folder}", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                if (propertyName == "Path_Photo")
                {
                    stagiaire.Path_Photo = fileName;
                }
                else if (propertyName == "Path_CV")
                {
                    stagiaire.Path_CV = fileName;
                }
            }

            return true;
        }
    }
}

