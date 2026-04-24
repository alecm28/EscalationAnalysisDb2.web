namespace EscalationAnalysisDb2.Domain.Entities
{
    // representa archivo cargado al sistema
    public class Report
    {
        // id principal
        public int ReportId { get; set; }

        // usuario que subio archivo
        public int UploadedByUserId { get; set; }

        // fecha de carga
        public DateTime UploadDate { get; set; }

        // nombre del archivo
        public string FileName { get; set; }

        // relacion con usuario
        public AppUser UploadedByUser { get; set; }

        // casos incluidos en este reporte
        public ICollection<CaseRecord> CaseRecords { get; set; }
            = new List<CaseRecord>();
    }
}