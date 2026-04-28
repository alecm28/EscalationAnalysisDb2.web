using EscalationAnalysisDb2.Application.Services;
using EscalationAnalysisDb2.Application.ViewModels;
using EscalationAnalysisDb2.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EscalationAnalysisDb2.Controllers
{
    // requiere sesion iniciada
    [Authorize]
    public class UploadController : Controller
    {
        private readonly CaseService _caseService;
        private readonly UploadService _uploadService;
        private readonly EscalationsDbContext _context;

        public UploadController(
            CaseService caseService,
            UploadService uploadService,
            EscalationsDbContext context)
        {
            _caseService = caseService;
            _uploadService = uploadService;
            _context = context;
        }

        // vista principal de carga
        public IActionResult Index()
        {
            return View(new List<UploadPreviewViewModel>());
        }

        // muestra preview antes de guardar
        [HttpPost]
        public IActionResult PreviewFile(IFormFile file)
        {
            // valida archivo
            if (file == null)
            {
                TempData["ErrorMessage"] = "Please select a file.";
                return RedirectToAction("Index");
            }

            if (file.Length == 0)
            {
                TempData["ErrorMessage"] = "The selected file is empty.";
                return RedirectToAction("Index");
            }

            // solo csv
            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Only CSV files are allowed.";
                return RedirectToAction("Index");
            }

            try
            {
                // toma primeras 10 filas para preview
                var preview = _uploadService
                    .ProcessFile(file)
                    .Take(10)
                    .ToList();

                if (!preview.Any())
                {
                    TempData["ErrorMessage"] = "The file does not contain valid data rows.";
                    return RedirectToAction("Index");
                }

                return View("Index", preview);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        // guarda archivo definitivo
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            // valida archivo
            if (file == null)
            {
                TempData["ErrorMessage"] = "Please select a file.";
                return RedirectToAction("Index");
            }

            if (file.Length == 0)
            {
                TempData["ErrorMessage"] = "The selected file is empty.";
                return RedirectToAction("Index");
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Only CSV files are allowed.";
                return RedirectToAction("Index");
            }

            try
            {
                var preview = _uploadService.ProcessFile(file);

                if (!preview.Any())
                {
                    TempData["ErrorMessage"] = "The file does not contain valid data rows.";
                    return RedirectToAction("Index");
                }

                // usuario logueado
                var userEmail = User.Identity.Name;

                var currentUser = await _context.AppUsers
                    .FirstOrDefaultAsync(x => x.Email == userEmail);

                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index");
                }

                // valida reporte duplicado por usuario
                var alreadyExists = await _context.Reports.AnyAsync(x =>
                    x.UploadedByUserId == currentUser.AppUserId &&
                    x.FileName.ToLower() == file.FileName.ToLower());

                if (alreadyExists)
                {
                    TempData["ErrorMessage"] =
                        "You have already uploaded a report with this file name.";

                    return RedirectToAction("Index");
                }

                // guarda datos en bd
                await _caseService.SaveData(
                    preview,
                    currentUser.AppUserId,
                    file.FileName);

                TempData["SuccessMessage"] = "Report uploaded successfully.";

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}