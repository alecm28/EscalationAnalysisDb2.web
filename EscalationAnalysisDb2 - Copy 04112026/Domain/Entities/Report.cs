namespace EscalationAnalysisDb2.Domain.Entities
{
    // Representa el archivo que se sube al sistema
    public class Report
    {
        public int ReportId { get; set; }

        // Usuario que subió el archivo
        public int UploadedByUserId { get; set; }

        // Fecha de carga
        public DateTime UploadDate { get; set; }

        // Nombre del archivo
        public string FileName { get; set; }

        // Relación con usuario
        public AppUser UploadedByUser { get; set; }

        // Casos que vienen en este archivo
        public ICollection<CaseRecord> CaseRecords { get; set; }
            = new List<CaseRecord>();
    }
}