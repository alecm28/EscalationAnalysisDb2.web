namespace EscalationAnalysisDb2.Domain.Entities
{
    // representa un caso importado desde archivo
    public class CaseRecord
    {
        // id principal
        public int CaseRecordId { get; set; }

        // numero del caso
        public string CaseNumber { get; set; }

        // llaves foraneas
        public int AccountId { get; set; }
        public int CaseOwnerId { get; set; }
        public int SeverityId { get; set; }
        public int StatusId { get; set; }

        // reporte del que vino el dato
        public int? ReportId { get; set; }

        // relaciones de navegacion
        public Account Account { get; set; }
        public CaseOwner CaseOwner { get; set; }
        public Severity Severity { get; set; }
        public Status Status { get; set; }
        public Report Report { get; set; }

        // version del producto
        public string ProductVersion { get; set; }

        // un caso puede tener varias escalaciones
        public ICollection<Escalation> Escalations { get; set; }
            = new List<Escalation>();
    }
}