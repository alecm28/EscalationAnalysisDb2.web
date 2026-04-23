using EscalationAnalysisDb2.Application.Services;
using EscalationAnalysisDb2.Application.ViewModels;
using EscalationAnalysisDb2.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EscalationAnalysisDb2.Controllers
{
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

        // vista principal
        public IActionResult Index()
        {
            return View(new List<UploadPreviewViewModel>());
        }

        // preview del archivo
        [HttpPost]
        public IActionResult PreviewFile(IFormFile file)
        {
            // validar archivo seleccionado
            if (file == null)
            {
                TempData["ErrorMessage"] = "Please select a file.";
                return RedirectToAction("Index");
            }

            // validar vacío
            if (file.Length == 0)
            {
                TempData["ErrorMessage"] = "The selected file is empty.";
                return RedirectToAction("Index");
            }

            // validar formato csv
            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Only CSV files are allowed.";
                return RedirectToAction("Index");
            }

            try
            {
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

        // subir archivo definitivo
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            // validar archivo seleccionado
            if (file == null)
            {
                TempData["ErrorMessage"] = "Please select a file.";
                return RedirectToAction("Index");
            }

            // validar vacío
            if (file.Length == 0)
            {
                TempData["ErrorMessage"] = "The selected file is empty.";
                return RedirectToAction("Index");
            }

            // validar formato csv
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

                // obtener usuario logueado
                var userEmail = User.Identity.Name;

                var currentUser = await _context.AppUsers
                    .FirstOrDefaultAsync(x => x.Email == userEmail);

                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index");
                }

                // guardar información
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