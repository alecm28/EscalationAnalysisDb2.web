using Microsoft.AspNetCore.Http;

namespace EscalationAnalysisDb2.Application.ViewModels
{
    public class UploadPreviewPageViewModel
    {
        // archivo cargado por el usuario
        public IFormFile? File { get; set; }

        // nombre del archivo
        public string? FileName { get; set; }

        // indica si se muestra preview
        public bool ShowPreview { get; set; } = false;

        // filas que se ven en la previsualizacion
        public List<UploadPreviewViewModel> PreviewRows { get; set; } = new();

        // mensaje de error si algo falla
        public string? ErrorMessage { get; set; }
    }
}