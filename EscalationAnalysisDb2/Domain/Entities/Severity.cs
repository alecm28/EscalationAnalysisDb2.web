namespace EscalationAnalysisDb2.Domain.Entities
{
    public class Severity
    {
        // id principal
        public int SeverityId { get; set; }

        // nombre de severidad
        public string SeverityName { get; set; }

        // casos con esta severidad
        public ICollection<CaseRecord> CaseRecords { get; set; } = new List<CaseRecord>();
    }
}