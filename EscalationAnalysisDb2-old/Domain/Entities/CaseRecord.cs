namespace EscalationAnalysisDb2.Domain.Entities
{
    // Representa un caso cargado desde el archivo
    public class CaseRecord
    {
        public int CaseRecordId { get; set; }

        // Número del caso (clave funcional)
        public string CaseNumber { get; set; }

        // Relaciones (FK)
        public int AccountId { get; set; }
        public int CaseOwnerId { get; set; }
        public int SeverityId { get; set; }
        public int StatusId { get; set; }

        // Relación con el reporte de donde viene el dato
        public int? ReportId { get; set; }

        // Navegación a otras entidades
        public Account Account { get; set; }
        public CaseOwner CaseOwner { get; set; }
        public Severity Severity { get; set; }
        public Status Status { get; set; }
        public Report Report { get; set; }

        public string ProductVersion { get; set; }

        // Un caso puede tener varias escalaciones
        public ICollection<Escalation> Escalations { get; set; }
            = new List<Escalation>();
    }
}