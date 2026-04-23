namespace EscalationAnalysisDb2.Domain.Entities
{
    public class Status
    {
        public int StatusId { get; set; }

        public string StatusName { get; set; }

        public ICollection<CaseRecord> CaseRecords { get; set; } = new List<CaseRecord>();
    }
}