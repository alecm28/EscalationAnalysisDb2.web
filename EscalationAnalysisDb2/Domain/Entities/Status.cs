namespace EscalationAnalysisDb2.Domain.Entities
{
    public class Status
    {
        // id principal
        public int StatusId { get; set; }

        // nombre del estado
        public string StatusName { get; set; }

        // casos con este estado
        public ICollection<CaseRecord> CaseRecords { get; set; } = new List<CaseRecord>();
    }
}