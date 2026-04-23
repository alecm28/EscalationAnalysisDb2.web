namespace EscalationAnalysisDb2.Domain.Entities
{
    public class Escalation
    {
        public int EscalationId { get; set; }

        public int CaseRecordId { get; set; }
        public string EscalationTask { get; set; }
        public DateTime EscalationDate { get; set; }

        public int SeverityId { get; set; }
        public int StatusId { get; set; }

        public CaseRecord CaseRecord { get; set; }
        public Severity Severity { get; set; }
        public Status Status { get; set; }
    }
}