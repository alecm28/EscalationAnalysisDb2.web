using Microsoft.AspNetCore.Http;

namespace EscalationAnalysisDb2.Application.ViewModels
{
    public class UploadPreviewPageViewModel
    {
        public IFormFile? File { get; set; }

        public string? FileName { get; set; }

        public bool ShowPreview { get; set; } = false;

        public List<UploadPreviewViewModel> PreviewRows { get; set; } = new();

        public string? ErrorMessage { get; set; }
    }
}
