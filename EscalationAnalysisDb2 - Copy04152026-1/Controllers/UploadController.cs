using EscalationAnalysisDb2.Application.Services;
using EscalationAnalysisDb2.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EscalationAnalysisDb2.Controllers
{
    [Authorize]
    public class UploadController : Controller
    {
        private readonly CaseService _caseService;
        private readonly UploadService _uploadService;

        public UploadController(CaseService caseService, UploadService uploadService)
        {
            _caseService = caseService;
            _uploadService = uploadService;
        }

        // Vista principal de carga
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            // Validación de archivo
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Seleccione un archivo válido.";
                return RedirectToAction("Index");
            }

            // Validación de formato
            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "El archivo debe estar en formato CSV (.csv).";
                return RedirectToAction("Index");
            }

            List<UploadPreviewViewModel> preview;

            try
            {
                // Procesamiento del archivo
                preview = _uploadService.ProcessFile(file);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al procesar el archivo.";
                return RedirectToAction("Index");
            }

            // Validación de contenido
            if (preview == null || !preview.Any())
            {
                TempData["ErrorMessage"] = "El archivo no contiene datos válidos.";
                return RedirectToAction("Index");
            }

            try
            {
                // Guardado en base de datos
                // userId = 1 temporal (puedes cambiarlo luego por usuario logueado)
                await _caseService.SaveData(preview, 1, file.FileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error guardando la información en la base de datos.";
                return RedirectToAction("Index");
            }

            // Mensaje de éxito
            TempData["SuccessMessage"] = "Archivo cargado correctamente.";

            // Redirección al dashboard
            return RedirectToAction("Index", "Dashboard");
        }
    }
}