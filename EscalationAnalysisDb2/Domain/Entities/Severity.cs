namespace EscalationAnalysisDb2.Domain.Entities
{
    public class Severity
    {
        public int SeverityId { get; set; }

        public string SeverityName { get; set; }

        public ICollection<CaseRecord> CaseRecords { get; set; } = new List<CaseRecord>();
    }
}