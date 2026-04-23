using EscalationAnalysisDb2.Application.Services;
using EscalationAnalysisDb2.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EscalationAnalysisDb2.Controllers
{
    [Authorize]
    public class UploadController : Controller
    {
        // servicio para guardar la información en la base de datos
        private readonly CaseService _caseService;

        // servicio para procesar el archivo que se sube
        private readonly UploadService _uploadService;

        // constructor donde recibo ambos servicios
        public UploadController(CaseService caseService, UploadService uploadService)
        {
            _caseService = caseService;
            _uploadService = uploadService;
        }

        public IActionResult Index()
        {
            // retorna la vista donde se sube el archivo
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            // valido que el archivo exista y tenga contenido
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Seleccione un archivo válido.";
                return RedirectToAction("Index");
            }

            // valido que sea un archivo csv
            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "El archivo debe estar en formato CSV (.csv).";
                return RedirectToAction("Index");
            }

            List<UploadPreviewViewModel> preview;

            try
            {
                // proceso el archivo para obtener los datos
                preview = _uploadService.ProcessFile(file);
            }
            catch (Exception ex)
            {
                // si ocurre un error al leer el archivo lo muestro al usuario
                TempData["ErrorMessage"] = $"Error procesando el archivo: {ex.Message}";
                return RedirectToAction("Index");
            }

            // valido que el archivo tenga datos válidos
            if (preview == null || !preview.Any())
            {
                TempData["ErrorMessage"] = "El archivo no contiene datos válidos o el formato no coincide.";
                return RedirectToAction("Index");
            }

            try
            {
                // guardo la información procesada en la base de datos
                await _caseService.SaveData(preview, 1, file.FileName);
            }
            catch (Exception ex)
            {
                // si falla el guardado, muestro el error
                TempData["ErrorMessage"] = $"Error guardando la información: {ex.Message}";
                return RedirectToAction("Index");
            }

            // mensaje de éxito cuando todo sale bien
            TempData["SuccessMessage"] = "Archivo cargado correctamente";

            // redirijo al dashboard
            return RedirectToAction("Index", "Dashboard");
        }
    }
}