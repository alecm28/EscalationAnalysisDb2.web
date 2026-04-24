namespace EscalationAnalysisDb2.Domain.Entities
{
    // owner responsable del caso
    public class CaseOwner
    {
        // id principal
        public int CaseOwnerId { get; set; }

        // nombre del owner
        public string CaseOwnerName { get; set; }

        // region asignada
        public string Region { get; set; }

        // casos relacionados a este owner
        public ICollection<CaseRecord> CaseRecords { get; set; }
    }
}