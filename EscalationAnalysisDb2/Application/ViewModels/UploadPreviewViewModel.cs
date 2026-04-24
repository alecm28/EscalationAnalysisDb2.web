namespace EscalationAnalysisDb2.Application.ViewModels
{
    public class UploadPreviewViewModel
    {
        // representa una fila del archivo antes de guardar en bd

        // numero de caso
        public string CaseNumber { get; set; }

        // nombre o id de escalacion
        public string EscalationTask { get; set; }

        // severidad en texto
        public string Severity { get; set; }

        // estado actual
        public string Status { get; set; }

        // owner responsable
        public string Owner { get; set; }

        // cliente o cuenta
        public string Account { get; set; }

        // region del owner
        public string Region { get; set; }

        // version del producto
        public string ProductVersion { get; set; }

        // fecha de escalacion
        public DateTime? EscalationDate { get; set; }
    }
}