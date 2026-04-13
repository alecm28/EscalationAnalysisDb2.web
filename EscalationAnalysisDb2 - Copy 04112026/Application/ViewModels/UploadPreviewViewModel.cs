namespace EscalationAnalysisDb2.Application.ViewModels
{
    public class UploadPreviewViewModel
    {
        // este modelo representa cada fila del archivo que se sube
        // básicamente aquí guardo los datos tal como vienen antes de procesarlos

        // número del caso
        public string CaseNumber { get; set; }

        // id o nombre de la escalación
        public string EscalationTask { get; set; }

        // severidad en texto (ej: Critical, High, etc.)
        public string Severity { get; set; }

        // estado del caso
        public string Status { get; set; }

        // persona encargada del caso
        public string Owner { get; set; }

        // cuenta o cliente
        public string Account { get; set; }

        // región, no siempre viene en el archivo
        public string Region { get; set; }

        // versión del producto
        public string ProductVersion { get; set; }

        // fecha de la escalación (puede venir vacía)
        public DateTime? EscalationDate { get; set; }
    }
}